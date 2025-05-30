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
        #region Events & Properties
        public delegate void QuizCompletedHandler(int nodeIndex);
        public event QuizCompletedHandler OnQuizCompleted;

        /// <summary>
        /// Gets the index of the current quiz question
        /// </summary>
        public int CurrentQuestionIndex => currentQuizQuestionIndex;

        /// <summary>
        /// Gets the list of current quiz questions (read-only)
        /// </summary>
        public IReadOnlyList<QuizQuestion> CurrentQuizQuestions => currentQuizQuestions;
        #endregion

        #region Inspector Fields
        [Header("Quiz UI")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private GameObject successPanel;
        [SerializeField] private GameObject failurePanel;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private Button[] optionButtons;

        [Header("Feedback UI")]
        [SerializeField] private GameObject correctFeedbackPanel;
        [SerializeField] private GameObject incorrectFeedbackPanel;
        [SerializeField] private float feedbackDisplayTime = 1.5f;

        [Header("Components")]
        [SerializeField] private SlideIndicatorManager indicatorManager;
        #endregion

        #region Private Fields
        private QuizData quizData;
        private List<QuizQuestion> currentQuizQuestions = new List<QuizQuestion>();
        private int currentQuizQuestionIndex = 0;
        private HashSet<int> unlockedQuizQuestions = new HashSet<int>();
        private int currentNodeIndex = -1;

        // Animation constants
        private const float fadeInTime = 0.3f;
        private const float fadeOutTime = 0.3f;
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
            }
            else
            {
                Debug.LogError("Failed to load quiz_data.json from Resources.");
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts a quiz for the specified node
        /// </summary>
        public void StartQuiz(int nodeIndex)
        {
            ResetQuizState(nodeIndex);
            LoadQuestionsForNode(nodeIndex);
            SetupUI();
        }

        /// <summary>
        /// Moves to the next question if available
        /// </summary>
        public void NextQuestion()
        {
            if (currentQuizQuestionIndex < currentQuizQuestions.Count - 1)
            {
                currentQuizQuestionIndex++;
                LoadQuizQuestion(currentQuizQuestionIndex);
                UpdateIndicator();
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
                UpdateIndicator();
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

        /// <summary>
        /// Restarts the current quiz
        /// </summary>
        public void RestartQuiz()
        {
            // Show quiz panel and hide results
            quizPanel.SetActive(true);
            failurePanel.SetActive(false);
            successPanel.SetActive(false);

            // Reset quiz state
            currentQuizQuestionIndex = 0;
            unlockedQuizQuestions.Clear();
            unlockedQuizQuestions.Add(0);

            // Update UI
            LoadQuizQuestion(0);
            GenerateIndicators();
        }

        /// <summary>
        /// Completes the current node and triggers completion events
        /// </summary>
        public void CompleteCurrentNode()
        {
            OnQuizCompleted?.Invoke(currentNodeIndex);
        }
        #endregion

        #region Quiz Question Display
        private void ResetQuizState(int nodeIndex)
        {
            currentNodeIndex = nodeIndex;
            currentQuizQuestionIndex = 0;
            unlockedQuizQuestions.Clear();
            unlockedQuizQuestions.Add(0); // First question always unlocked
        }

        private void LoadQuestionsForNode(int nodeIndex)
        {
            NodeQuiz nodeQuiz = quizData.FindNodeQuizById(nodeIndex);
            if (nodeQuiz == null)
            {
                Debug.LogError($"No quiz data found for Node {nodeIndex}.");
                return;
            }

            currentQuizQuestions = nodeQuiz.questions.ToList();
        }

        private void SetupUI()
        {
            LoadQuizQuestion(currentQuizQuestionIndex);
            GenerateIndicators();
        }

        private void LoadQuizQuestion(int index)
        {
            // Ensure quiz panel is active
            quizPanel.SetActive(true);

            // Safety check
            if (index < 0 || index >= currentQuizQuestions.Count) return;

            // Get current question
            var question = currentQuizQuestions[index];

            // Set question text
            questionText.text = question.questionText;

            // Setup option buttons
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < question.options.Length)
                {
                    int capturedIndex = i;
                    optionButtons[i].gameObject.SetActive(true);
                    optionButtons[i].GetComponentInChildren<TMP_Text>().text = question.options[i];
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnOptionSelected(capturedIndex));
                }
                else
                {
                    // Hide extra buttons if there aren't enough options
                    optionButtons[i].gameObject.SetActive(false);
                }
            }

            // Animate panel
            StartCoroutine(UIAnimator.AnimateSlideIn(quizPanel.transform));
        }

        private void GenerateIndicators()
        {
            if (indicatorManager != null)
            {
                indicatorManager.GenerateIndicators(currentQuizQuestions.Count);
                indicatorManager.UpdateActiveIndicator(0, animate: false);
            }
        }

        private void UpdateIndicator()
        {
            if (indicatorManager != null)
            {
                indicatorManager.UpdateActiveIndicator(currentQuizQuestionIndex);
            }
        }
        #endregion

        #region Answer Handling
        private void OnOptionSelected(int selectedIndex)
        {
            if (currentQuizQuestionIndex >= currentQuizQuestions.Count)
                return;

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

        private void HandleCorrectAnswer()
        {
            // Unlock next question
            unlockedQuizQuestions.Add(currentQuizQuestionIndex + 1);

            if (correctFeedbackPanel != null)
            {
                StartCoroutine(ShowTemporaryFeedback(correctFeedbackPanel, ProceedAfterCorrectAnswer));
            }
            else
            {
                ProceedAfterCorrectAnswer();
            }
        }

        private void HandleIncorrectAnswer(int selectedIndex)
        {
            // Visual feedback - shake the button
            StartCoroutine(UIAnimator.ShakeElement(optionButtons[selectedIndex].transform));

            if (incorrectFeedbackPanel != null)
            {
                StartCoroutine(ShowTemporaryFeedback(incorrectFeedbackPanel, ShowFailurePanel));
            }
            else
            {
                ShowFailurePanel();
            }
        }

        private void ProceedAfterCorrectAnswer()
        {
            // Move to next question or complete quiz
            currentQuizQuestionIndex++;
            if (currentQuizQuestionIndex < currentQuizQuestions.Count)
            {
                // Move to next question
                LoadQuizQuestion(currentQuizQuestionIndex);
                UpdateIndicator();
            }
            else
            {
                // Quiz completed
                ShowSuccessPanel();
                NotifyNodeCompletion();
            }
        }

        private void NotifyNodeCompletion()
        {
            NopeMapManager gameManager = FindFirstObjectByType<NopeMapManager>();
            if (gameManager != null)
            {
                gameManager.SendMessage("NodeCompleted", currentNodeIndex, SendMessageOptions.DontRequireReceiver);
            }
        }
        #endregion

        #region UI Transitions
        private IEnumerator ShowTemporaryFeedback(GameObject feedbackPanel, System.Action onComplete)
        {
            // Hide quiz panel, show feedback
            quizPanel.SetActive(false);
            feedbackPanel.SetActive(true);

            // Get or add canvas group
            CanvasGroup feedbackCanvasGroup = feedbackPanel.GetComponent<CanvasGroup>();
            if (feedbackCanvasGroup == null)
                feedbackCanvasGroup = feedbackPanel.AddComponent<CanvasGroup>();

            // Fade in
            yield return FadeCanvasGroup(feedbackCanvasGroup, 0f, 1f, fadeInTime);

            // Hold
            yield return new WaitForSeconds(feedbackDisplayTime - (fadeInTime + fadeOutTime));

            // Fade out
            yield return FadeCanvasGroup(feedbackCanvasGroup, 1f, 0f, fadeOutTime);

            // Cleanup and callback
            feedbackPanel.SetActive(false);
            onComplete?.Invoke();
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = to; // Ensure final value
        }

        private void ShowSuccessPanel()
        {
            StartCoroutine(UIAnimator.TransitionBetweenPanels(quizPanel, successPanel));
        }

        private void ShowFailurePanel()
        {
            StartCoroutine(UIAnimator.TransitionBetweenPanels(quizPanel, failurePanel));
        }
        #endregion
    }
}