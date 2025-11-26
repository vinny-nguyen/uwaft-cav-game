using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapTutorialManager : MonoBehaviour
{
    public static MapTutorialManager Instance;

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea]
        public string message;

        // Optional highlight: manually placed GameObject (image, outline, etc.)
        public GameObject highlightObject;
    }

    [Header("Config")]
    public string playerPrefsKey = "HasSeenTutorial";
    public List<TutorialStep> steps = new List<TutorialStep>();

    [Header("UI References")]
    public CanvasGroup panelGroup;          // TutorialPanel
    public TextMeshProUGUI messageText;     // MessageText
    public Image backgroundOverlay;           // HighlightImage

    [Header("Navigation UI")]
    public Button prevArrow;            // PrevArrow button GameObject
    public Button nextArrow;            // NextArrow button GameObject
    public Button completeButton;       // CompleteButton GameObject

    private int currentIndex = -1;
    private bool isRunning = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (panelGroup != null)
            panelGroup.gameObject.SetActive(false);
        if (backgroundOverlay != null)
            backgroundOverlay.gameObject.SetActive(false);
        // Tutorial is now only started when triggered externally (e.g., after car arrives at node)
    }

    public void StartTutorial()
    {
        if (steps.Count == 0)
        {
            Debug.LogWarning("MapTutorialManager: No steps configured.");
            return;
        }

        isRunning = true;
        currentIndex = 0;

        if (panelGroup != null)
            panelGroup.gameObject.SetActive(true);

        ShowStep(steps[currentIndex]);
        RefreshNavigationUI();
    }

    private void ShowStep(TutorialStep step)
    {
        // Turn off all highlights
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i].highlightObject != null)
                steps[i].highlightObject.SetActive(false);
        }

        if (messageText != null)
            messageText.text = step.message;

        if (step.highlightObject != null)
        {
            step.highlightObject.SetActive(true);

            // Make sure all children are active
            foreach (Transform child in step.highlightObject.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }


    private void RefreshNavigationUI()
    {
        bool hasPrev = currentIndex > 0;
        bool hasNext = currentIndex < steps.Count - 1;

        if (prevArrow != null)
            prevArrow.gameObject.SetActive(hasPrev);

        if (nextArrow != null)
            nextArrow.gameObject.SetActive(hasNext);

        // Complete button only on last step
        if (completeButton != null)
        {
            completeButton.gameObject.SetActive(!hasNext);
            completeButton.interactable = true;
        }
    }

    private void GoToStep(int index)
    {
        if (!isRunning) return;
        if (index < 0 || index >= steps.Count) return;

        currentIndex = index;
        ShowStep(steps[currentIndex]);
        RefreshNavigationUI();
    }

    // Hook these up to button OnClick events

    public void OnNextClicked()
    {
        if (!isRunning) return;
        if (currentIndex < steps.Count - 1)
            GoToStep(currentIndex + 1);
    }

    public void OnPrevClicked()
    {
        if (!isRunning) return;
        if (currentIndex > 0)
            GoToStep(currentIndex - 1);
    }

    public void OnCompleteClicked()
    {
        EndTutorial();
    }

    private void EndTutorial()
    {
        isRunning = false;

        // Turn off all highlights
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i].highlightObject != null)
                steps[i].highlightObject.SetActive(false);
        }

        if (panelGroup != null)
            panelGroup.gameObject.SetActive(false);

        if (backgroundOverlay != null)
            backgroundOverlay.gameObject.SetActive(false);

        PlayerPrefs.SetInt(playerPrefsKey, 1);
        PlayerPrefs.Save();

        Debug.Log("MapTutorialManager: Tutorial finished.");
    }

    // Optional: external "Skip Tutorial" call
    public void SkipTutorial()
    {
        OnCompleteClicked();
    }
}
