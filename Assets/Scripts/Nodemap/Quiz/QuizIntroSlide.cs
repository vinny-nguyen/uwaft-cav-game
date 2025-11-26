using UnityEngine;
using UnityEngine.UI;
using Nodemap.UI;
using Nodemap.Controllers;

namespace Nodemap
{
    // Final slide that displays a message and "Take Quiz" button to transition to quiz mode
    public class QuizIntroSlide : SlideBase
{
    [Header("UI References")]
    [SerializeField] private Button takeQuizButton;

    private PopupController popupController;

    private void Awake()
    {
        // Get PopupController from GameServices
        popupController = GameServices.Instance?.PopupController;
        if (popupController == null)
        {
            Debug.LogWarning("[QuizTransitionSlide] PopupController not found in GameServices!");
        }

        if (takeQuizButton != null)
        {
            takeQuizButton.onClick.AddListener(OnTakeQuizClicked);
        }
        else
        {
            Debug.LogWarning("[QuizTransitionSlide] Take Quiz button not assigned!");
        }
    }

    public override void OnEnter()
    {
        base.OnEnter();
        // Ensure button is interactable when slide is shown
        if (takeQuizButton != null)
        {
            takeQuizButton.interactable = true;
        }
    }

    private void OnTakeQuizClicked()
    {
        if (popupController != null)
        {
            popupController.EnterQuizMode();
        }
        else
        {
            Debug.LogError("[QuizTransitionSlide] PopupController not found!");
        }
    }

    private void OnDestroy()
    {
        if (takeQuizButton != null)
        {
            takeQuizButton.onClick.RemoveListener(OnTakeQuizClicked);
        }
    }
}
}
