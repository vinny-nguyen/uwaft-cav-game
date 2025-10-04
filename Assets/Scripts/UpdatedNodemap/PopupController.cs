using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class PopupController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private MapConfig mapConfig;
    
    [Header("Popup Background References")]
    public GameObject popupPanel;
    public Image popupBackground; // Assign the Image component for the popup background
    public Sprite defaultBackgroundSprite; // Assign your default background sprite
    public Sprite completedBackgroundSprite; // Assign your green completed background sprite

    /// <summary>
    /// Call this to set which background is visible.
    /// </summary>
    public void SetBackground(bool isCompleted)
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
    
    // Configuration Helpers
    private float GetPopupFadeDuration() => mapConfig ? mapConfig.popupFadeDuration : 0.3f;
    private float GetSlideTransitionDuration() => mapConfig ? mapConfig.slideTransitionDuration : 0.2f;
    private string GetNodeSpriteFolder() => mapConfig ? mapConfig.nodeSpriteFolder : "Sprites/Nodes";

    public void SetHeaderAndSlides(string header, List<GameObject> slideObjects)
    {
        // Set header
        if (headerText != null)
            headerText.text = header;

        // Remove old slides
        foreach (var slide in slides)
        {
            if (slide != null) Destroy(slide);
        }
        slides.Clear();

        // Add new slides
        foreach (var slideObj in slideObjects)
        {
            var slide = Instantiate(slideObj, slidesContainer);
            slide.SetActive(false);
            slides.Add(slide);
        }
        currentSlideIndex = 0;
        UpdateSlides();
        UpdateIndicators();
        Show();
    }

    public void Open(NodeData node, bool isCompleted)
    {
        if (!popupPanel) return;

        // Optional: set a different background if completed
        if (popupBackground && defaultBackgroundSprite && completedBackgroundSprite)
            popupBackground.sprite = isCompleted ? completedBackgroundSprite : defaultBackgroundSprite;

        if (headerText) headerText.text = string.IsNullOrEmpty(node.title) ? "Lesson" : node.title;

        // Clear previous content
        ClearSlides();

        // Instantiate slides from the SlideDeck (if any)
        if (node.slideDeck != null && node.slideDeck.slides != null)
        {
            foreach (var sr in node.slideDeck.slides)
            {
                if (sr == null || sr.slidePrefab == null) continue;
                var go = Instantiate(sr.slidePrefab, slidesContainer);
                go.SetActive(false);
                slides.Add(go);
            }
        }

        // Fall-back if the deck is empty (not required, but nice for testing)
        if (slides.Count == 0)
        {
            Debug.LogWarning($"PopupController.Open: No slides found in SlideDeck for node: {node.name}");
        }

        // Start at slide 0
        currentSlideIndex = 0;
        UpdateSlides();
        UpdateIndicators();

        // Show popup
        popupPanel.SetActive(true);
    }

    public void Show()
    {
        popupPanel.SetActive(true);
    }

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

}
