using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeMap.Quiz
{
    /// <summary>
    /// Manages quiz state, progression, and question navigation
    /// </summary>
    public class QuizStateManager
    {
        #region Events & Properties
        public delegate void QuestionChangedHandler(int newIndex);
        public event QuestionChangedHandler OnQuestionChanged;

        public delegate void QuizCompletedHandler();
        public event QuizCompletedHandler OnQuizCompleted;

        public int CurrentQuestionIndex { get; private set; }
        public IReadOnlyList<QuizQuestion> CurrentQuizQuestions => currentQuizQuestions;
        public int CurrentNodeIndex { get; private set; } = -1;
        #endregion

        #region Private Fields
        private QuizData quizData;
        private List<QuizQuestion> currentQuizQuestions = new List<QuizQuestion>();
        private HashSet<int> unlockedQuizQuestions = new HashSet<int>();
        #endregion

        #region Initialization
        public void LoadQuizData()
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

        #region Quiz Management
        public void StartQuiz(int nodeIndex)
        {
            ResetQuizState(nodeIndex);
            LoadQuestionsForNode(nodeIndex);
        }

        public void RestartQuiz()
        {
            CurrentQuestionIndex = 0;
            unlockedQuizQuestions.Clear();
            unlockedQuizQuestions.Add(0);
            OnQuestionChanged?.Invoke(CurrentQuestionIndex);
        }

        public void UnlockNextQuestion()
        {
            unlockedQuizQuestions.Add(CurrentQuestionIndex + 1);
        }

        public void MoveToNextQuestion()
        {
            CurrentQuestionIndex++;
            if (CurrentQuestionIndex < currentQuizQuestions.Count)
            {
                OnQuestionChanged?.Invoke(CurrentQuestionIndex);
            }
            else
            {
                OnQuizCompleted?.Invoke();
            }
        }
        #endregion

        #region Navigation
        public void NextQuestion()
        {
            if (CanGoToNextQuestion())
            {
                CurrentQuestionIndex++;
                OnQuestionChanged?.Invoke(CurrentQuestionIndex);
            }
        }

        public void PreviousQuestion()
        {
            if (CanGoToPreviousQuestion())
            {
                CurrentQuestionIndex--;
                OnQuestionChanged?.Invoke(CurrentQuestionIndex);
            }
        }

        public bool CanGoToNextQuestion()
        {
            return unlockedQuizQuestions.Contains(CurrentQuestionIndex + 1);
        }

        public bool CanGoToPreviousQuestion()
        {
            return CurrentQuestionIndex > 0;
        }
        #endregion

        #region Helper Methods
        private void ResetQuizState(int nodeIndex)
        {
            CurrentNodeIndex = nodeIndex;
            CurrentQuestionIndex = 0;
            unlockedQuizQuestions.Clear();
            unlockedQuizQuestions.Add(0);
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
        #endregion
    }
}
