using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NodeMap.Quiz;
using NodeMap.UI;

namespace NodeMap
{
    /// <summary>
    /// Manages quiz functionality for nodes
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Quiz UI")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private GameObject successPanel;
        [SerializeField] private GameObject failurePanel;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private Button[] optionButtons;

        [Header("Components")]
        [SerializeField] private SlideIndicatorManager indicatorManager;
        #endregion

        #region Private Fields
        private QuizData quizData;
        private List<QuizQuestion> currentQuizQuestions = new List<QuizQuestion>();
        private int currentQuizQuestionIndex = 0;
        private HashSet<int> unlockedQuizQuestions = new HashSet<int>();
        private int currentNodeIndex = -1;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the index of the current quiz question
        /// </summary>
        public int CurrentQuestionIndex => currentQuizQuestionIndex;

        /// <summary>
        /// Gets the list of current quiz questions (read-only)
        /// </summary>
        public IReadOnlyList<QuizQuestion> CurrentQuizQuestions => currentQuizQuestions;
        #endregion

        #region Events
        public delegate void QuizCompletedHandler(int nodeIndex);
        public event QuizCompletedHandler OnQuizCompleted;
        #endregion

        #region Initialization
        private void Awake()
        {
            LoadQuizData();
        }

        private void LoadQuizData()
        {
            TextAsset jsonText = Resources.Load<TextAsset>("quiz_data");
            if (jsonText != null)
            {
                quizData = JsonUtility.FromJson<QuizData>(jsonText.text);
                Debug.Log("Quiz data loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to load quiz_data.json from Resources.");
            }
        }
        #endregion

        #region Quiz Management
        /// <summary>
        /// Starts a quiz for the specified node
        /// </summary>
        public void StartQuiz(int nodeIndex)
        {
            // Reset quiz state
            currentNodeIndex = nodeIndex;
            unlockedQuizQuestions.Clear();
            unlockedQuizQuestions.Add(0); // First question always unlocked
            currentQuizQuestionIndex = 0;

            // Load questions for the node
            NodeQuiz nodeQuiz = quizData.FindNodeQuizById(nodeIndex);
            if (nodeQuiz == null)
            {
                Debug.LogError($"No quiz data found for Node {nodeIndex}.");
                return;
            }

            currentQuizQuestions = nodeQuiz.questions.ToList();
            Debug.Log($"[QUIZ] Starting quiz for Node {nodeIndex} with {currentQuizQuestions.Count} questions");

            // Setup UI
            LoadQuizQuestion(currentQuizQuestionIndex);

            // Update indicators
            if (indicatorManager != null)
            {
                indicatorManager.GenerateIndicators(currentQuizQuestions.Count);
                indicatorManager.UpdateActiveIndicator(0);
            }
        }

        /// <summary>
        /// Loads the current question into the UI
        /// </summary>
        private void LoadQuizQuestion(int index)
        {
            var question = currentQuizQuestions[index];
            questionText.text = question.questionText;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int capturedIndex = i;
                optionButtons[i].GetComponentInChildren<TMP_Text>().text = question.options[i];
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(capturedIndex));
            }

            StartCoroutine(UIAnimator.AnimateSlideIn(quizPanel.transform));
        }

        /// <summary>
        /// Called when a quiz answer option is selected
        /// </summary>
        private void OnOptionSelected(int selectedIndex)
        {
            var question = currentQuizQuestions[currentQuizQuestionIndex];

            if (question.IsCorrectAnswer(selectedIndex))
            {
                HandleCorrectAnswer();
            }
            else
            {
                HandleIncorrectAnswer(selectedIndex);
            }
        }

        /// <summary>
        /// Processes a correct answer
        /// </summary>
        private void HandleCorrectAnswer()
        {
            Debug.Log("[QUIZ] Correct!");

            // Unlock next question
            unlockedQuizQuestions.Add(currentQuizQuestionIndex + 1);

            // Move to next question or complete quiz
            currentQuizQuestionIndex++;
            if (currentQuizQuestionIndex < currentQuizQuestions.Count)
            {
                // Move to next question
                LoadQuizQuestion(currentQuizQuestionIndex);
                if (indicatorManager != null)
                {
                    indicatorManager.UpdateActiveIndicator(currentQuizQuestionIndex);
                }
            }
            else
            {
                // Quiz completed
                Debug.Log("[QUIZ] Quiz Complete!");
                ShowSuccessPanel();

                NodeMapManager gameManager = FindFirstObjectByType<NodeMapManager>();
                if (gameManager != null)
                {
                    // Notify via direct method call
                    gameManager.SendMessage("NodeCompleted", currentNodeIndex, SendMessageOptions.DontRequireReceiver);

                    // Or if you've implemented the event system:
                    // gameManager.TriggerNodeCompleted(currentNodeIndex);
                }
            }
        }

        /// <summary>
        /// Processes an incorrect answer
        /// </summary>
        private void HandleIncorrectAnswer(int selectedIndex)
        {
            Debug.Log("[QUIZ] Incorrect — try again!");

            // Shake the selected button
            StartCoroutine(UIAnimator.ShakeElement(optionButtons[selectedIndex].transform));
            ShowFailurePanel();
        }

        /// <summary>
        /// Shows the success panel
        /// </summary>
        private void ShowSuccessPanel()
        {
            StartCoroutine(UIAnimator.TransitionBetweenPanels(quizPanel, successPanel));
        }

        /// <summary>
        /// Shows the failure panel
        /// </summary>
        private void ShowFailurePanel()
        {
            StartCoroutine(UIAnimator.TransitionBetweenPanels(quizPanel, failurePanel));
        }
        #endregion

        #region Navigation
        /// <summary>
        /// Moves to the next question if available
        /// </summary>
        public void NextQuestion()
        {
            if (currentQuizQuestionIndex < currentQuizQuestions.Count - 1)
            {
                currentQuizQuestionIndex++;
                LoadQuizQuestion(currentQuizQuestionIndex);

                if (indicatorManager != null)
                {
                    indicatorManager.UpdateActiveIndicator(currentQuizQuestionIndex);
                }
            }
        }

        /// <summary>
        /// Moves to the previous question if available
        /// </summary>
        public void PreviousQuestion()
        {
            if (currentQuizQuestionIndex > 0)
            {
                currentQuizQuestionIndex--;
                LoadQuizQuestion(currentQuizQuestionIndex);

                if (indicatorManager != null)
                {
                    indicatorManager.UpdateActiveIndicator(currentQuizQuestionIndex);
                }
            }
        }

        /// <summary>
        /// Checks if the next button should be enabled
        /// </summary>
        public bool CanGoToNextQuestion()
        {
            return unlockedQuizQuestions.Contains(currentQuizQuestionIndex + 1);
        }

        /// <summary>
        /// Checks if the previous button should be enabled
        /// </summary>
        public bool CanGoToPreviousQuestion()
        {
            return currentQuizQuestionIndex > 0;
        }
        #endregion

        #region Public Actions
        /// <summary>
        /// Restarts the current quiz
        /// </summary>
        public void RestartQuiz()
        {
            // First, make sure the quiz panel will be active
            quizPanel.SetActive(true);

            // Hide result panels with direct SetActive instead of transitions
            failurePanel.SetActive(false);
            successPanel.SetActive(false);

            // Reset quiz state
            currentQuizQuestionIndex = 0;
            unlockedQuizQuestions.Clear();
            unlockedQuizQuestions.Add(0);

            // Load the first question immediately
            LoadQuizQuestion(currentQuizQuestionIndex);

            // Update indicators
            if (indicatorManager != null)
            {
                indicatorManager.GenerateIndicators(currentQuizQuestions.Count);
                indicatorManager.UpdateActiveIndicator(0);
            }

            // Log that we're restarting
            Debug.Log("[QUIZ] Quiz restarted. Current question index: 0");
        }

        /// <summary>
        /// Completes the current node and triggers completion events
        /// </summary>
        public void CompleteCurrentNode()
        {
            Debug.Log("[QUIZ] Completing current node and advancing.");

            // Notify listeners
            OnQuizCompleted?.Invoke(currentNodeIndex);
        }
        #endregion
    }
}