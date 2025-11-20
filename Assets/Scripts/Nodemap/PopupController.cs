using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class PopupController : MonoBehaviour
{
    // MapConfig accessed via singleton - no need to assign
    private MapConfig mapConfig;
    
    [Header("Popup Background References")]
    public GameObject popupPanel;
    public Image popupBackground; // Assign the Image component for the popup background
    public Sprite defaultBackgroundSprite; // Assign your default background sprite
    public Sprite completedBackgroundSprite; // Assign your green completed background sprite

    /// <summary>
    /// Sets the background sprite based on completion state.
    /// </summary>
    private void SetBackground(bool isCompleted)
    {
        if (popupBackground != null)
            popupBackground.sprite = isCompleted ? completedBackgroundSprite : defaultBackgroundSprite;
    }

    [Header("Popup UI References")]
    public Button nextSlideButton;
    public Button previousSlideButton;
    public Button closeButton;
    [SerializeField] private Transform slidesContainer;
    [SerializeField] private TMP_Text headerText;

    [Header("Indicators")]
    [SerializeField] private Transform slideIndicators;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Sprite activeIndicatorSprite;
    [SerializeField] private Sprite inactiveIndicatorSprite;

    [Header("Quiz References")]
    [SerializeField] private GameObject quizPrefab;
    [SerializeField] private Transform quizContainer;

    private readonly List<GameObject> slides = new();
    private readonly List<string> slideKeys = new(); // Store keys from SlideDeck
    private readonly List<GameObject> indicatorObjects = new();
    private int currentSlideIndex = 0;
    
    // Quiz mode state
    private bool isQuizMode = false;
    private GameObject currentQuizInstance;
    private NodeData currentNodeData;

    void Awake()
    {
        // Initialize config if not assigned
        if (!mapConfig) mapConfig = MapConfig.Instance;
        
        nextSlideButton.onClick.AddListener(NextSlide);
        previousSlideButton.onClick.AddListener(PrevSlide);
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        Hide();
    }
    
    // Configuration Helpers - Cleaner pattern with single method
    private T GetConfigValue<T>(System.Func<MapConfig, T> configGetter, T fallback)
    {
        return mapConfig ? configGetter(mapConfig) : fallback;
    }
    
    private float GetPopupFadeDuration() => GetConfigValue(c => c.popupFadeDuration, 0.3f);
    private float GetSlideTransitionDuration() => GetConfigValue(c => c.slideTransitionDuration, 0.2f);
    private string GetNodeSpriteFolder() => GetConfigValue(c => c.nodeSpriteFolder, "Sprites/Nodes");

    /// <summary>
    /// Internal method to set header and create slides from slide objects.
    /// </summary>
    private void SetupHeaderAndSlides(string header, List<GameObject> slideObjects, List<string> keys)
    {
        // Set header
        if (headerText != null)
            headerText.text = header;

        // Clear previous content first
        ClearSlides();

        // Add new slides and store their keys
        for (int i = 0; i < slideObjects.Count; i++)
        {
            var slide = Instantiate(slideObjects[i], slidesContainer);
            slide.SetActive(false);
            slides.Add(slide);
            
            // Store the key from SlideDeck
            if (keys != null && i < keys.Count)
                slideKeys.Add(keys[i]);
            else
                slideKeys.Add(""); // Fallback to empty if no key provided
        }
        
        // Initialize to first slide
        currentSlideIndex = 0;
        UpdateSlides();
        UpdateIndicators();
    }

    /// <summary>
    /// Opens the popup with the specified node data and completion state.
    /// This is the main public entry point for opening popups.
    /// </summary>
    public void Open(NodeData node, bool isCompleted)
    {
        if (!popupPanel || node == null) return;

        // Store node data for quiz mode
        currentNodeData = node;

        // Set background based on completion state
        SetBackground(isCompleted);

        // Set header text
        string headerText = string.IsNullOrEmpty(node.title) ? "Lesson" : node.title;
        
        // Prepare slide objects and keys from the node's slide deck
        List<GameObject> slideObjects = new List<GameObject>();
        List<string> keys = new List<string>();
        if (node.slideDeck != null && node.slideDeck.slides != null)
        {
            foreach (var slideRef in node.slideDeck.slides)
            {
                if (slideRef?.slidePrefab != null)
                {
                    slideObjects.Add(slideRef.slidePrefab);
                    keys.Add(slideRef.key); // Store the key from SlideDeck
                }
            }
        }

        // Setup all popup content
        SetupHeaderAndSlides(headerText, slideObjects, keys);

        // Log warning if no slides found (for debugging)
        if (slides.Count == 0)
        {
            Debug.LogWarning($"[PopupController] No slides found in SlideDeck for node: {node.name}");
        }

        // Ensure we're not in quiz mode when opening
        isQuizMode = false;

        // Show the popup
        Show();
    }

    /// <summary>
    /// Shows the popup panel.
    /// </summary>
    private void Show()
    {
        popupPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the popup panel. Public for close button functionality.
    /// </summary>
    public void Hide()
    {
        // Clean up quiz mode if active
        if (isQuizMode)
        {
            ExitQuizMode();
        }
        
        popupPanel.SetActive(false);
    }

    public void NextSlide()
    {
        if (currentSlideIndex < slides.Count - 1)
        {
            currentSlideIndex++;
            UpdateSlides();
            UpdateIndicators();
        }
    }

    public void PrevSlide()
    {
        if (slides.Count == 0) return;
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            UpdateSlides();
            UpdateIndicators();
        }
    }

    private void UpdateSlides()
    {
        for (int i = 0; i < slides.Count; i++)
        {
            bool active = (i == currentSlideIndex);
            if (slides[i]) slides[i].SetActive(active);

            // optional lifecycle hooks
            var sb = slides[i] ? slides[i].GetComponent<SlideBase>() : null;
            if (sb != null)
            {
                if (active) sb.OnEnter();
                else sb.OnExit();
            }
        }
    }

    public void JumpToSlideByKey(string key)
    {
        if (string.IsNullOrEmpty(key) || slides.Count == 0) return;

        // Search using the keys from SlideDeck instead of SlideBase component
        for (int i = 0; i < slideKeys.Count; i++)
        {
            if (slideKeys[i] == key)
            {
                currentSlideIndex = i;
                UpdateSlides();
                UpdateIndicators();
                return;
            }
        }
        Debug.LogWarning($"PopupController.JumpToSlideByKey: key '{key}' not found.");
    }

    private void ClearSlides()
    {
        foreach (var s in slides)
            if (s) Destroy(s);
        slides.Clear();
        slideKeys.Clear(); // Clear keys as well

        foreach (var dot in indicatorObjects)
            if (dot) Destroy(dot);
        indicatorObjects.Clear();
    }


    private void UpdateIndicators()
    {
        if (!indicatorPrefab || !slideIndicators) return;

        // rebuild dots
        foreach (var dot in indicatorObjects)
            if (dot) Destroy(dot);
        indicatorObjects.Clear();

        for (int i = 0; i < slides.Count; i++)
        {
            var dot = Instantiate(indicatorPrefab, slideIndicators);
            var img = dot.GetComponent<Image>();
            if (img)
                img.sprite = (i == currentSlideIndex) ? activeIndicatorSprite : inactiveIndicatorSprite;
            indicatorObjects.Add(dot);
        }
    }

    #region Quiz Mode

    /// <summary>
    /// Enter quiz mode - hide slide navigation and show quiz UI.
    /// </summary>
    public void EnterQuizMode()
    {
        if (isQuizMode || currentNodeData == null)
        {
            Debug.LogWarning("[PopupController] Cannot enter quiz mode - already in quiz mode or no node data!");
            return;
        }

        if (currentNodeData.quizJson == null)
        {
            Debug.LogError($"[PopupController] No quiz JSON assigned for node: {currentNodeData.name}");
            return;
        }

        if (quizPrefab == null)
        {
            Debug.LogError("[PopupController] Quiz prefab not assigned in PopupController!");
            return;
        }

        // Hide slide navigation buttons and indicators
        if (nextSlideButton != null) nextSlideButton.gameObject.SetActive(false);
        if (previousSlideButton != null) previousSlideButton.gameObject.SetActive(false);
        if (slideIndicators != null) slideIndicators.gameObject.SetActive(false);

        // Hide all slides
        foreach (var slide in slides)
        {
            if (slide != null) slide.SetActive(false);
        }

        // Instantiate quiz UI
        Transform container = quizContainer != null ? quizContainer : slidesContainer;
        currentQuizInstance = Instantiate(quizPrefab, container);

        // Initialize quiz with data
        var quizController = currentQuizInstance.GetComponent<QuizController>();
        if (quizController != null)
        {
            quizController.Initialize(currentNodeData.quizJson, currentNodeData);
            
            // Auto-wire completion event to QuizCompletionHandler
            var completionHandler = FindFirstObjectByType<QuizCompletionHandler>();
            if (completionHandler != null)
            {
                quizController.OnQuizCompleted.AddListener(completionHandler.OnQuizCompleted);
            }
            else
            {
                Debug.LogWarning("[PopupController] QuizCompletionHandler not found! Quiz completion won't trigger node completion.");
            }
        }
        else
        {
            Debug.LogError("[PopupController] QuizController component not found on quiz prefab!");
        }

        isQuizMode = true;
    }

    /// <summary>
    /// Exit quiz mode - restore slide navigation and destroy quiz UI.
    /// </summary>
    public void ExitQuizMode()
    {
        if (!isQuizMode) return;

        // Destroy quiz instance
        if (currentQuizInstance != null)
        {
            Destroy(currentQuizInstance);
            currentQuizInstance = null;
        }

        // Restore slide navigation
        if (nextSlideButton != null) nextSlideButton.gameObject.SetActive(true);
        if (previousSlideButton != null) previousSlideButton.gameObject.SetActive(true);
        if (slideIndicators != null) slideIndicators.gameObject.SetActive(true);

        // Show current slide
        UpdateSlides();

        isQuizMode = false;
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up button listeners to prevent memory leaks
        if (nextSlideButton != null)
            nextSlideButton.onClick.RemoveListener(NextSlide);
            
        if (previousSlideButton != null)
            previousSlideButton.onClick.RemoveListener(PrevSlide);
            
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Hide);
    }
}
