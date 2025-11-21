using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Nodemap.Controllers;

// Quiz controller that displays questions, validates answers, and allows slide review
public class QuizController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private Button nextQuestionButton;
    [SerializeField] private Button reviewSlideButton;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private Button closeButton;

    [Header("Upgrade Display")]
    [SerializeField] private GameObject upgradeContainer;
    [SerializeField] private TMP_Text upgradeText;
    [SerializeField] private UnityEngine.UI.Image beforeFrameImage;
    [SerializeField] private UnityEngine.UI.Image afterFrameImage;
    [SerializeField] private UnityEngine.UI.Image beforeTireImage;
    [SerializeField] private UnityEngine.UI.Image afterTireImage;

    [Header("Events")]
    public UnityEvent<int> OnQuizCompleted; // Passes the node index

    [Header("Colors")]
    [SerializeField] private Color correctColor = new Color(0.11f, 0.73f, 0.33f); // #1BBB55
    [SerializeField] private Color incorrectColor = new Color(0.85f, 0.33f, 0.31f); // #D9534F

    private QuizData quizData;
    private PopupController popupController;
    private NodeData currentNodeData;
    private int currentNodeIndex = -1; // Store which node this quiz is for
    private int currentQuestionIndex = 0;
    private readonly List<Button> optionButtons = new();
    private bool questionAnswered = false;

    private void Awake()
    {
        popupController = GameServices.Instance?.PopupController;
        if (popupController == null)
        {
            Debug.LogWarning("[QuizController] PopupController not found in GameServices!");
        }

        if (nextQuestionButton != null)
            nextQuestionButton.onClick.AddListener(OnNextQuestionClicked);

        if (reviewSlideButton != null)
            reviewSlideButton.onClick.AddListener(OnReviewSlideClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        if (completionPanel != null)
            completionPanel.SetActive(false);

        HideActionButtons();
    }

    // Initialize the quiz with data from a TextAsset JSON file
    public void Initialize(TextAsset quizJsonAsset, NodeData nodeData, int nodeIndex)
    {
        if (quizJsonAsset == null)
        {
            Debug.LogError("[QuizController] No quiz JSON asset provided!");
            return;
        }

        currentNodeData = nodeData;
        currentNodeIndex = nodeIndex;

        try
        {
            quizData = JsonUtility.FromJson<QuizData>(quizJsonAsset.text);
            if (quizData == null || quizData.questions == null || quizData.questions.Count == 0)
            {
                Debug.LogError("[QuizController] Quiz data is empty or invalid!");
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuizController] Failed to parse quiz JSON: {e.Message}");
            return;
        }

        currentQuestionIndex = 0;
        DisplayCurrentQuestion();
    }

    private void DisplayCurrentQuestion()
    {
        if (quizData == null || currentQuestionIndex >= quizData.questions.Count)
        {
            ShowCompletion();
            return;
        }

        questionAnswered = false;
        QuizQuestion question = quizData.questions[currentQuestionIndex];

        // Update question text
        if (questionText != null)
            questionText.text = question.question;

        // Update progress
        if (progressText != null)
            progressText.text = $"Question {currentQuestionIndex + 1}/{quizData.questions.Count}";

        // Clear feedback
        SetFeedback("", Color.white);

        // Clear existing option buttons
        ClearOptionButtons();

        // Create option buttons
        for (int i = 0; i < question.options.Count; i++)
        {
            int optionIndex = i; // Capture for lambda
            Button btn = Instantiate(optionButtonPrefab, optionsContainer);

            // Set button text
            TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = question.options[i];

            // Add click listener
            btn.onClick.AddListener(() => OnOptionSelected(optionIndex));

            optionButtons.Add(btn);
        }

        HideActionButtons();
    }

    private void OnOptionSelected(int selectedIndex)
    {
        if (questionAnswered) return;

        questionAnswered = true;
        QuizQuestion question = quizData.questions[currentQuestionIndex];

        // Disable all option buttons
        foreach (var btn in optionButtons)
            btn.interactable = false;

        if (selectedIndex == question.correctIndex)
        {
            // Correct answer
            SetFeedback("Correct!", correctColor);

            if (nextQuestionButton != null)
            {
                nextQuestionButton.gameObject.SetActive(true);
                nextQuestionButton.interactable = true;
            }
        }
        else
        {
            // Incorrect answer
            SetFeedback("Incorrect. Review the material and try again.", incorrectColor);

            if (reviewSlideButton != null)
            {
                reviewSlideButton.gameObject.SetActive(true);
                reviewSlideButton.interactable = true;
            }
        }
    }

    private void OnNextQuestionClicked()
    {
        currentQuestionIndex++;
        DisplayCurrentQuestion();
    }

    private void OnReviewSlideClicked()
    {
        if (popupController == null)
        {
            Debug.LogError("[QuizController] PopupController not found!");
            return;
        }

        QuizQuestion question = quizData.questions[currentQuestionIndex];

        if (string.IsNullOrEmpty(question.relatedSlideKey))
        {
            Debug.LogWarning($"[QuizController] No related slide key for question {currentQuestionIndex}");
            return;
        }

        // Exit quiz mode and jump to the related slide
        popupController.ExitQuizMode();
        popupController.JumpToSlideByKey(question.relatedSlideKey);
    }

    private void OnCloseClicked()
    {
        if (popupController != null)
        {
            Debug.Log($"[QuizController] Completing node {currentNodeIndex} and closing popup.");
            
            // Trigger the quiz completion event (which completes the node)
            OnQuizCompleted?.Invoke(currentNodeIndex);
            
            // Close the popup
            popupController.Hide();
        }
    }

    private void ShowCompletion()
    {
        // Hide quiz UI
        if (questionText != null) questionText.gameObject.SetActive(false);
        if (progressText != null) progressText.gameObject.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (optionsContainer != null) optionsContainer.gameObject.SetActive(false);
        HideActionButtons();

        // Show completion panel
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
            
            if (completionText != null)
            {
                completionText.text = "Congratulations! You've completed the quiz!";
            }

            // Display upgrade visuals if node data has upgrade info
            DisplayUpgrade();
        }

        // Don't trigger OnQuizCompleted here anymore - wait for user to click "Complete Node" button
    }    private void DisplayUpgrade()
    {
        if (currentNodeData == null || upgradeContainer == null)
            return;

        bool hasFrameUpgrade = currentNodeData.upgradeFrame != null;
        bool hasTireUpgrade = currentNodeData.upgradeTire != null;

        if (!hasFrameUpgrade && !hasTireUpgrade)
        {
            // No upgrades to show
            if (upgradeContainer != null)
                upgradeContainer.SetActive(false);
            return;
        }

        upgradeContainer.SetActive(true);

        // Set upgrade text
        if (upgradeText != null && !string.IsNullOrEmpty(currentNodeData.upgradeText))
        {
            upgradeText.text = currentNodeData.upgradeText;
        }

        // Get current sprites from the car to show "before"
        var carVisual = GameServices.Instance?.CarVisual;

        // Display frame upgrade
        if (hasFrameUpgrade)
        {
            if (beforeFrameImage != null && carVisual != null)
            {
                beforeFrameImage.sprite = carVisual.GetCurrentFrameSprite();
                beforeFrameImage.gameObject.SetActive(true);
            }
            if (afterFrameImage != null)
            {
                afterFrameImage.sprite = currentNodeData.upgradeFrame;
                afterFrameImage.gameObject.SetActive(true);
            }
        }
        else
        {
            if (beforeFrameImage != null) beforeFrameImage.gameObject.SetActive(false);
            if (afterFrameImage != null) afterFrameImage.gameObject.SetActive(false);
        }

        // Display tire upgrade
        if (hasTireUpgrade)
        {
            if (beforeTireImage != null && carVisual != null)
            {
                beforeTireImage.sprite = carVisual.GetCurrentTireSprite();
                beforeTireImage.gameObject.SetActive(true);
            }
            if (afterTireImage != null)
            {
                afterTireImage.sprite = currentNodeData.upgradeTire;
                afterTireImage.gameObject.SetActive(true);
            }
        }
        else
        {
            if (beforeTireImage != null) beforeTireImage.gameObject.SetActive(false);
            if (afterTireImage != null) afterTireImage.gameObject.SetActive(false);
        }
    }

    private void ClearOptionButtons()
    {
        foreach (var btn in optionButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        optionButtons.Clear();
    }

    private void SetFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }

    private void HideActionButtons()
    {
        if (nextQuestionButton != null)
            nextQuestionButton.gameObject.SetActive(false);

        if (reviewSlideButton != null)
            reviewSlideButton.gameObject.SetActive(false);
    }

    // Reset the quiz to the first question (for when re-entering quiz mode)
    public void ResetQuiz()
    {
        currentQuestionIndex = 0;

        // Show quiz UI elements again
        if (questionText != null) questionText.gameObject.SetActive(true);
        if (progressText != null) progressText.gameObject.SetActive(true);
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);
        if (optionsContainer != null) optionsContainer.gameObject.SetActive(true);
        if (completionPanel != null) completionPanel.SetActive(false);

        DisplayCurrentQuestion();
    }

    private void OnDestroy()
    {
        if (nextQuestionButton != null)
            nextQuestionButton.onClick.RemoveListener(OnNextQuestionClicked);

        if (reviewSlideButton != null)
            reviewSlideButton.onClick.RemoveListener(OnReviewSlideClicked);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);

        ClearOptionButtons();
    }
}
