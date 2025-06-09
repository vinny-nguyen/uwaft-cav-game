using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NodeMap.UI;

namespace NodeMap.Quiz
{
    /// <summary>
    /// Main orchestrator for quiz functionality
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        #region Events & Properties
        public delegate void QuizCompletedHandler(int nodeIndex);
        public event QuizCompletedHandler OnQuizCompleted;

        public int CurrentQuestionIndex => stateManager.CurrentQuestionIndex;
        public IReadOnlyList<QuizQuestion> CurrentQuizQuestions => stateManager.CurrentQuizQuestions;
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

        #region Private Components
        private QuizStateManager stateManager;
        private QuizUIController uiController;
        private QuizAnswerHandler answerHandler;
        #endregion

        #region Initialization
        private void Awake()
        {
            InitializeComponents();
            SetupEventHandlers();
        }

        private void InitializeComponents()
        {
            stateManager = new QuizStateManager();
            stateManager.LoadQuizData();

            uiController = gameObject.AddComponent<QuizUIController>();
            uiController.Initialize(quizPanel, successPanel, failurePanel, questionText, 
                optionButtons, correctFeedbackPanel, incorrectFeedbackPanel, feedbackDisplayTime, indicatorManager);

            answerHandler = new QuizAnswerHandler();
            answerHandler.Initialize(stateManager, uiController);
        }

        private void SetupEventHandlers()
        {
            stateManager.OnQuestionChanged += HandleQuestionChanged;
            stateManager.OnQuizCompleted += answerHandler.HandleQuizCompletion;
            uiController.OnOptionSelected += answerHandler.ProcessAnswer;
            answerHandler.OnQuizCompleted += (nodeIndex) => OnQuizCompleted?.Invoke(nodeIndex);
        }
        #endregion

        #region Public Methods
        public void StartQuiz(int nodeIndex)
        {
            stateManager.StartQuiz(nodeIndex);
            SetupUI();
        }

        public void NextQuestion()
        {
            stateManager.NextQuestion();
        }

        public void PreviousQuestion()
        {
            stateManager.PreviousQuestion();
        }

        public bool CanGoToNextQuestion()
        {
            return stateManager.CanGoToNextQuestion();
        }

        public bool CanGoToPreviousQuestion()
        {
            return stateManager.CanGoToPreviousQuestion();
        }

        public void RestartQuiz()
        {
            uiController.ShowQuizPanel();
            stateManager.RestartQuiz();
            uiController.GenerateIndicators(stateManager.CurrentQuizQuestions.Count);
        }

        public void CompleteCurrentNode()
        {
            OnQuizCompleted?.Invoke(stateManager.CurrentNodeIndex);
        }
        #endregion

        #region Event Handlers
        private void HandleQuestionChanged(int newIndex)
        {
            if (newIndex < stateManager.CurrentQuizQuestions.Count)
            {
                uiController.LoadQuestion(stateManager.CurrentQuizQuestions[newIndex], newIndex);
                uiController.UpdateIndicator(newIndex);
            }
        }

        private void SetupUI()
        {
            HandleQuestionChanged(stateManager.CurrentQuestionIndex);
            uiController.GenerateIndicators(stateManager.CurrentQuizQuestions.Count);
        }
        #endregion
    }
}