using UnityEngine;

namespace NodeMap.Quiz
{
    /// <summary>
    /// Handles answer validation and feedback logic
    /// </summary>
    public class QuizAnswerHandler
    {
        #region Events
        public delegate void CorrectAnswerHandler();
        public event CorrectAnswerHandler OnCorrectAnswer;

        public delegate void IncorrectAnswerHandler();
        public event IncorrectAnswerHandler OnIncorrectAnswer;

        public delegate void QuizCompletedHandler(int nodeIndex);
        public event QuizCompletedHandler OnQuizCompleted;
        #endregion

        #region Private Fields
        private QuizStateManager stateManager;
        private QuizUIController uiController;
        #endregion

        #region Initialization
        public void Initialize(QuizStateManager stateManager, QuizUIController uiController)
        {
            this.stateManager = stateManager;
            this.uiController = uiController;
        }
        #endregion

        #region Answer Processing
        public void ProcessAnswer(int selectedIndex)
        {
            if (stateManager.CurrentQuestionIndex >= stateManager.CurrentQuizQuestions.Count)
                return;

            var question = stateManager.CurrentQuizQuestions[stateManager.CurrentQuestionIndex];

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
            stateManager.UnlockNextQuestion();
            OnCorrectAnswer?.Invoke();

            uiController.ShowCorrectFeedback(() => {
                stateManager.MoveToNextQuestion();
            });
        }

        private void HandleIncorrectAnswer(int selectedIndex)
        {
            OnIncorrectAnswer?.Invoke();
            uiController.ShowIncorrectFeedback(selectedIndex, () => {
                uiController.ShowFailurePanel();
            });
        }
        #endregion

        #region Completion Handling
        public void HandleQuizCompletion()
        {
            uiController.ShowSuccessPanel();
            NotifyNodeCompletion();
        }

        private void NotifyNodeCompletion()
        {
            OnQuizCompleted?.Invoke(stateManager.CurrentNodeIndex);
            
            NodeMapManager gameManager = Object.FindFirstObjectByType<NodeMapManager>();
            if (gameManager != null)
            {
                gameManager.SendMessage("NodeCompleted", stateManager.CurrentNodeIndex, SendMessageOptions.DontRequireReceiver);
            }
        }
        #endregion
    }
}
