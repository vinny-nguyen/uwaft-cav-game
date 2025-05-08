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

        // Add these fields to the Inspector Fields region
        [Header("Feedback UI")]
        [SerializeField] private GameObject correctFeedbackPanel;
        [SerializeField] private GameObject incorrectFeedbackPanel;
        [SerializeField] private float feedbackDisplayTime = 1.5f; // Time in seconds to show feedback
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
            quizPanel.SetActive(true);
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
        /// <summary>
        /// Processes a correct answer
        /// </summary>
        private void HandleCorrectAnswer()
        {
            Debug.Log("[QUIZ] Correct!");

            // Unlock next question
            unlockedQuizQuestions.Add(currentQuizQuestionIndex + 1);

            // Show "Correct!" feedback temporarily
            if (correctFeedbackPanel != null)
            {
                Debug.Log("[QUIZ] Showing correct feedback panel.");
                StartCoroutine(ShowTemporaryFeedback(correctFeedbackPanel, () =>
                {
                    ProceedAfterCorrectAnswer();
                }));
            }
            else
            {
                // If no feedback panel, proceed immediately
                ProceedAfterCorrectAnswer();
            }
        }

        /// <summary>
        /// Processes an incorrect answer
        /// </summary>
        // Replace the HandleCorrectAnswer method with this version

        /// <summary>
        /// Processes a correct answer
        /// </summary>

        // Add this new helper method
        private void ProceedAfterCorrectAnswer()
        {
            // Move to next question or complete quiz
            currentQuizQuestionIndex++;
            if (currentQuizQuestionIndex < currentQuizQuestions.Count)
            {
                // Move to next question
                Debug.Log($"[QUIZ] Moving to question {currentQuizQuestionIndex}");
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

                NopeMapManager gameManager = FindFirstObjectByType<NopeMapManager>();
                if (gameManager != null)
                {
                    // Notify via direct method call
                    gameManager.SendMessage("NodeCompleted", currentNodeIndex, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        // Replace the HandleIncorrectAnswer method with this version
        /// <summary>
        /// Processes an incorrect answer
        /// </summary>
        private void HandleIncorrectAnswer(int selectedIndex)
        {
            Debug.Log("[QUIZ] Incorrect — try again!");

            // Shake the selected button
            StartCoroutine(UIAnimator.ShakeElement(optionButtons[selectedIndex].transform));

            // Show "Incorrect!" feedback temporarily
            if (incorrectFeedbackPanel != null)
            {
                Debug.Log("[QUIZ] Showing incorrect feedback panel.");
                StartCoroutine(ShowTemporaryFeedback(incorrectFeedbackPanel, () =>
                {
                    ShowFailurePanel();
                }));
            }
            else
            {
                // If no feedback panel, proceed immediately
                ShowFailurePanel();
            }
        }

        // Add this new coroutine for showing temporary feedback
        /// <summary>
        /// Shows a feedback panel temporarily and then executes a callback
        /// </summary>
        // Fix the ShowTemporaryFeedback method

        private IEnumerator ShowTemporaryFeedback(GameObject feedbackPanel, System.Action onComplete)
        {
            // Hide quiz panel
            quizPanel.SetActive(false);

            // Show feedback panel
            feedbackPanel.SetActive(true);

            // Animate panel in
            CanvasGroup feedbackCanvasGroup = feedbackPanel.GetComponent<CanvasGroup>();
            if (feedbackCanvasGroup != null)
            {
                // Fade in
                feedbackCanvasGroup.alpha = 0f;
                float elapsed = 0f;
                float fadeInTime = 0.3f;

                while (elapsed < fadeInTime)
                {
                    elapsed += Time.deltaTime;
                    feedbackCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeInTime);
                    yield return null;
                }

                // Hold at full visibility
                yield return new WaitForSeconds(feedbackDisplayTime - 0.6f); // Adjust for fade times

                // Fade out - FIXED THIS SECTION
                elapsed = 0f;  // Reset elapsed time for fade out
                float fadeOutTime = 0.3f;

                while (elapsed < fadeOutTime)
                {
                    elapsed += Time.deltaTime;  // This line wasn't incrementing elapsed time
                    feedbackCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeOutTime);
                    yield return null;
                }
            }
            else
            {
                // No canvas group, just wait
                yield return new WaitForSeconds(feedbackDisplayTime);
            }

            // Hide feedback panel
            feedbackPanel.SetActive(false);

            // Execute callback
            onComplete?.Invoke();
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