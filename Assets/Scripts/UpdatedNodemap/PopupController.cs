using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class PopupController : MonoBehaviour
{
    [Header("Popup UI References")]
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
    public Transform slidesContainer;
    public Button nextSlideButton;
    public Button previousSlideButton;
    public Transform slideIndicators;
    public TextMeshProUGUI headerText;
    [Header("Close Button")]
    public Button closeButton; // Assign your X button here

    [Header("Indicator Sprites")]
    public Sprite activeIndicatorSprite;
    public Sprite inactiveIndicatorSprite;
    public GameObject indicatorPrefab; // Prefab with Image component

    private List<GameObject> slides = new List<GameObject>();
    private int currentSlideIndex = 0;
    private List<GameObject> indicatorObjects = new List<GameObject>();

    void Awake()
    {
        nextSlideButton.onClick.AddListener(NextSlide);
        previousSlideButton.onClick.AddListener(PreviousSlide);
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        Hide();
    }

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

    public void PreviousSlide()
    {
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
            slides[i].SetActive(i == currentSlideIndex);
        }
        previousSlideButton.interactable = currentSlideIndex > 0;
        nextSlideButton.interactable = currentSlideIndex < slides.Count - 1;
    }

    private void UpdateIndicators()
    {
        // Destroy old indicators
        foreach (var obj in indicatorObjects)
        {
            if (obj != null) Destroy(obj);
        }
        indicatorObjects.Clear();

        // Create new indicators
        for (int i = 0; i < slides.Count; i++)
        {
            var indicator = Instantiate(indicatorPrefab, slideIndicators);
            var img = indicator.GetComponent<Image>();
            if (img != null)
                img.sprite = (i == currentSlideIndex) ? activeIndicatorSprite : inactiveIndicatorSprite;
                Debug.Log($"Setting indicator {i} to {(i == currentSlideIndex ? "active" : "inactive")}");
            indicatorObjects.Add(indicator);
        }

        Debug.Log($"Updated indicators: {indicatorObjects.Count} indicators for {slides.Count} slides.");
    }
}
