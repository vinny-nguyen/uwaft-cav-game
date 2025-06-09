using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NodeMap.UI;

namespace NodeMap.Quiz
{
    /// <summary>
    /// Handles quiz UI display, animations, and transitions
    /// </summary>
    public class QuizUIController : MonoBehaviour
    {
        #region Events
        public delegate void OptionSelectedHandler(int optionIndex);
        public event OptionSelectedHandler OnOptionSelected;
        #endregion

        #region Private Fields
        private GameObject quizPanel;
        private GameObject successPanel;
        private GameObject failurePanel;
        private TMP_Text questionText;
        private Button[] optionButtons;
        private GameObject correctFeedbackPanel;
        private GameObject incorrectFeedbackPanel;
        private float feedbackDisplayTime;
        private SlideIndicatorManager indicatorManager;
        
        private int lastLoadedQuestionIndex = -1;
        private const float fadeInTime = 0.3f;
        private const float fadeOutTime = 0.3f;
        #endregion

        #region Initialization
        public void Initialize(GameObject quizPanel, GameObject successPanel, GameObject failurePanel,
            TMP_Text questionText, Button[] optionButtons, GameObject correctFeedbackPanel,
            GameObject incorrectFeedbackPanel, float feedbackDisplayTime, SlideIndicatorManager indicatorManager)
        {
            this.quizPanel = quizPanel;
            this.successPanel = successPanel;
            this.failurePanel = failurePanel;
            this.questionText = questionText;
            this.optionButtons = optionButtons;
            this.correctFeedbackPanel = correctFeedbackPanel;
            this.incorrectFeedbackPanel = incorrectFeedbackPanel;
            this.feedbackDisplayTime = feedbackDisplayTime;
            this.indicatorManager = indicatorManager;
        }
        #endregion

        #region Question Display
        public void LoadQuestion(QuizQuestion question, int questionIndex)
        {
            quizPanel.SetActive(true);
            questionText.text = question.questionText;
            SetupOptionButtons(question);
            AnimateQuestionTransition(questionIndex);
            lastLoadedQuestionIndex = questionIndex;
        }

        private void SetupOptionButtons(QuizQuestion question)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < question.options.Length)
                {
                    int capturedIndex = i;
                    optionButtons[i].gameObject.SetActive(true);
                    optionButtons[i].GetComponentInChildren<TMP_Text>().text = question.options[i];
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnOptionSelected?.Invoke(capturedIndex));
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void AnimateQuestionTransition(int questionIndex)
        {
            Vector3 entryDirection = questionIndex < lastLoadedQuestionIndex ? Vector3.right : Vector3.left;
            StartCoroutine(UIAnimator.AnimateSlideInFromSide(quizPanel.transform, entryDirection));
        }
        #endregion

        #region Indicator Management
        public void GenerateIndicators(int questionCount)
        {
            if (indicatorManager != null)
            {
                indicatorManager.GenerateIndicators(questionCount);
                indicatorManager.UpdateActiveIndicator(0, animate: false);
            }
        }

        public void UpdateIndicator(int currentIndex)
        {
            if (indicatorManager != null)
            {
                indicatorManager.UpdateActiveIndicator(currentIndex);
            }
        }
        #endregion

        #region Feedback & Transitions
        public void ShowCorrectFeedback(System.Action onComplete)
        {
            if (correctFeedbackPanel != null)
            {
                StartCoroutine(ShowTemporaryFeedback(correctFeedbackPanel, onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void ShowIncorrectFeedback(int selectedIndex, System.Action onComplete)
        {
            StartCoroutine(UIAnimator.ShakeElement(optionButtons[selectedIndex].transform, 
                UIAnimator.ShakeDuration, UIAnimator.ShakeMagnitude));

            if (incorrectFeedbackPanel != null)
            {
                StartCoroutine(ShowTemporaryFeedback(incorrectFeedbackPanel, onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void ShowSuccessPanel()
        {
            StartCoroutine(UIAnimator.TransitionBetweenPanels(quizPanel, successPanel));
        }

        public void ShowFailurePanel()
        {
            StartCoroutine(UIAnimator.TransitionBetweenPanels(quizPanel, failurePanel));
        }

        public void ShowQuizPanel()
        {
            quizPanel.SetActive(true);
            failurePanel.SetActive(false);
            successPanel.SetActive(false);
        }

        private IEnumerator ShowTemporaryFeedback(GameObject feedbackPanel, System.Action onComplete)
        {
            yield return UIAnimator.ShowTemporaryPanel(
                feedbackPanel,
                feedbackDisplayTime,
                fadeInTime,
                fadeOutTime,
                onComplete,
                quizPanel
            );
        }
        #endregion
    }
}
