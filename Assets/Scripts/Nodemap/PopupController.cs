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

    private readonly List<GameObject> slides = new();
    private readonly List<GameObject> indicatorObjects = new();
    private int currentSlideIndex = 0;

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
    private void SetupHeaderAndSlides(string header, List<GameObject> slideObjects)
    {
        // Set header
        if (headerText != null)
            headerText.text = header;

        // Clear previous content first
        ClearSlides();

        // Add new slides
        foreach (var slideObj in slideObjects)
        {
            var slide = Instantiate(slideObj, slidesContainer);
            slide.SetActive(false);
            slides.Add(slide);
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

        // Set background based on completion state
        SetBackground(isCompleted);

        // Set header text
        string headerText = string.IsNullOrEmpty(node.title) ? "Lesson" : node.title;
        
        // Prepare slide objects from the node's slide deck
        List<GameObject> slideObjects = new List<GameObject>();
        if (node.slideDeck != null && node.slideDeck.slides != null)
        {
            foreach (var slideRef in node.slideDeck.slides)
            {
                if (slideRef?.slidePrefab != null)
                {
                    slideObjects.Add(slideRef.slidePrefab);
                }
            }
        }

        // Setup all popup content
        SetupHeaderAndSlides(headerText, slideObjects);

        // Log warning if no slides found (for debugging)
        if (slides.Count == 0)
        {
            Debug.LogWarning($"[PopupController] No slides found in SlideDeck for node: {node.name}");
        }

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

        for (int i = 0; i < slides.Count; i++)
        {
            var sb = slides[i].GetComponent<SlideBase>();
            if (sb != null && sb.Key == key)
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
