using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private List<string> nodeHeaders; // Fill in from Inspector

    [Header("Slide Container")]
    [SerializeField] private Transform slidesParent; // Points to "Slides" container in hierarchy

    [Header("Slide Indicators")]
    [SerializeField] private GameObject slideDotPrefab;
    [SerializeField] private Transform slideIndicatorsParent;
    [SerializeField] private Sprite activeDotSprite;
    [SerializeField] private Sprite inactiveDotSprite;
    [SerializeField] private CanvasGroup slideIndicatorsCanvasGroup;

    private List<GameObject> currentNodeSlides = new List<GameObject>();
    private List<GameObject> spawnedDots = new List<GameObject>();
    private int currentSlideIndex = 0;
    private int lastSlideIndex = -1;
    private Coroutine activeDotBreathing;

    private Color enabledColor = Color.white;
    private Color disabledColor = new Color(1f, 1f, 1f, 0.4f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gameObject.SetActive(true);
    }

    private void Start()
    {
        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.interactable = false;
        popupCanvasGroup.blocksRaycasts = false;

        leftArrowButton.onClick.AddListener(PreviousSlide);
        rightArrowButton.onClick.AddListener(NextSlide);
        closeButton.onClick.AddListener(ClosePopup);
    }

    // -------------------------------
    // Public entry point per node
    // -------------------------------
    public void OpenPopupForNode(int nodeIndex)
    {
        currentNodeSlides.Clear();

        Transform nodeContainer = slidesParent.Find($"Node{nodeIndex}");
        if (nodeContainer == null)
        {
            Debug.LogWarning($"Node{nodeIndex} slides not found!");
            return;
        }

        foreach (Transform slide in nodeContainer)
        {
            currentNodeSlides.Add(slide.gameObject);
            slide.gameObject.SetActive(false);
        }

        if (currentNodeSlides.Count == 0)
        {
            Debug.LogWarning($"Node{nodeIndex} has no slides!");
            return;
        }

        headerText.text = (nodeIndex - 1 >= 0 && nodeIndex - 1 < nodeHeaders.Count)
            ? nodeHeaders[nodeIndex - 1]
            : $"Node {nodeIndex}";

        GenerateSlideIndicators();

        currentSlideIndex = 0;
        ShowSlide(currentSlideIndex);
        StartCoroutine(AnimatePopupOpen());
    }

    public void ClosePopup()
    {
        // Deactivate all current slides
        foreach (var slide in currentNodeSlides)
        {
            if (slide != null)
                slide.SetActive(false);
        }

        StartCoroutine(AnimatePopupClose());
    }

    // -------------------------------
    // Slide Navigation
    // -------------------------------
    private void ShowSlide(int index)
    {
        for (int i = 0; i < currentNodeSlides.Count; i++)
        {
            currentNodeSlides[i].SetActive(i == index);
        }

        UpdateSlideIndicators();
        UpdateArrows();
    }

    private void NextSlide()
    {
        if (currentSlideIndex < currentNodeSlides.Count - 1)
        {
            currentSlideIndex++;
            ShowSlide(currentSlideIndex);
            StartCoroutine(BounceButton(rightArrowButton.transform));
            StartCoroutine(NudgeUIElements());
        }
    }

    private void PreviousSlide()
    {
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            ShowSlide(currentSlideIndex);
            StartCoroutine(BounceButton(leftArrowButton.transform));
            StartCoroutine(NudgeUIElements());
        }
    }

    private void UpdateArrows()
    {
        leftArrowButton.interactable = currentSlideIndex > 0;
        leftArrowButton.image.color = leftArrowButton.interactable ? enabledColor : disabledColor;

        rightArrowButton.interactable = currentSlideIndex < currentNodeSlides.Count - 1;
        rightArrowButton.image.color = rightArrowButton.interactable ? enabledColor : disabledColor;
    }

    // -------------------------------
    // Slide Indicators
    // -------------------------------
    private void GenerateSlideIndicators()
    {
        foreach (var dot in spawnedDots)
        {
            Destroy(dot);
        }
        spawnedDots.Clear();

        if (currentNodeSlides.Count == 0 || slideDotPrefab == null) return;

        for (int i = 0; i < currentNodeSlides.Count; i++)
        {
            GameObject dot = Instantiate(slideDotPrefab, slideIndicatorsParent);
            spawnedDots.Add(dot);
        }

        UpdateSlideIndicators();
    }

    private void UpdateSlideIndicators()
    {
        Debug.Log($"[DEBUG] Running UpdateSlideIndicators, currentSlideIndex: {currentSlideIndex}");

        if (spawnedDots.Count == 0) return;

        for (int i = 0; i < spawnedDots.Count; i++)
        {
            GameObject dotContainer = spawnedDots[i];
            Transform dotVisual = dotContainer.transform.Find("DotVisual");
            if (dotVisual == null)
                Debug.LogWarning("DotVisual child not found!");

            Image img = dotVisual?.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = (i == currentSlideIndex) ? activeDotSprite : inactiveDotSprite;
            }

            if (i == currentSlideIndex)
            {
                if (activeDotBreathing != null)
                    StopCoroutine(activeDotBreathing);

                if (dotVisual != null)
                {
                    activeDotBreathing = StartCoroutine(BreatheDot(dotVisual));
                }
            }
            else if (i == lastSlideIndex)
            {
                if (dotVisual != null)
                    dotVisual.localScale = Vector3.one;
            }
        }

        lastSlideIndex = currentSlideIndex;
    }

    private IEnumerator BreatheDot(Transform dotTransform)
    {
        float breatheDuration = 1.5f;
        float breatheMagnitude = 0.1f;
        float timer = 0f;
        while (dotTransform != null && spawnedDots.Contains(dotTransform.parent.gameObject))
        {
            timer += Time.deltaTime;
            float scale = 1f + Mathf.Sin(timer * Mathf.PI * 2f / breatheDuration) * breatheMagnitude;
            dotTransform.localScale = Vector3.one * scale;
            yield return null;
        }
    }




    // -------------------------------
    // UI Animations
    // -------------------------------
    private IEnumerator AnimatePopupOpen()
    {
        popupCanvasGroup.blocksRaycasts = true;
        popupCanvasGroup.interactable = true;

        float duration = 0.4f;
        float time = 0f;

        Transform popupTransform = popupCanvasGroup.transform;
        Vector3 originalScale = Vector3.one;
        Vector3 startScale = Vector3.one * 0.8f;

        popupTransform.localScale = startScale;
        popupCanvasGroup.alpha = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            popupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            popupTransform.localScale = Vector3.Lerp(startScale, originalScale, t);

            if (backgroundOverlay != null)
                backgroundOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.6f, t)); // 20% opacity black

            if (slideIndicatorsCanvasGroup != null)
                slideIndicatorsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        popupCanvasGroup.alpha = 1f;
        popupTransform.localScale = originalScale;
    }


    private IEnumerator AnimatePopupClose()
    {
        popupCanvasGroup.interactable = false;

        float duration = 0.3f;
        float time = 0f;

        Transform popupTransform = popupCanvasGroup.transform;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 0.8f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            popupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            popupTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            if (backgroundOverlay != null)
                backgroundOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.2f, 0f, t)); // Fade back to transparent

            if (slideIndicatorsCanvasGroup != null)
                slideIndicatorsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        popupCanvasGroup.alpha = 0f;
        popupTransform.localScale = originalScale;
        popupCanvasGroup.blocksRaycasts = false;
    }


    private IEnumerator BounceButton(Transform buttonTransform)
    {
        float bounceDuration = 0.2f;
        float bounceScale = 0.8f;
        float time = 0f;

        Vector3 originalScale = Vector3.one;
        Vector3 smallScale = Vector3.one * bounceScale;

        while (time < bounceDuration / 2f)
        {
            time += Time.deltaTime;
            float t = time / (bounceDuration / 2f);
            buttonTransform.localScale = Vector3.Lerp(originalScale, smallScale, t);
            yield return null;
        }

        time = 0f;

        while (time < bounceDuration / 2f)
        {
            time += Time.deltaTime;
            float t = time / (bounceDuration / 2f);
            buttonTransform.localScale = Vector3.Lerp(smallScale, originalScale, t);
            yield return null;
        }

        buttonTransform.localScale = originalScale;
    }

    private IEnumerator NudgeUIElements()
    {
        float duration = 0.25f;
        float magnitude = 10f;
        float frequency = 20f;

        Vector3 originalPosIndicators = slideIndicatorsParent.localPosition;
        Transform activeSlide = currentNodeSlides.Count > currentSlideIndex ? currentNodeSlides[currentSlideIndex].transform : null;
        Vector3 originalPosSlide = activeSlide != null ? activeSlide.localPosition : Vector3.zero;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float offset = Mathf.Sin(time * frequency) * magnitude * (1f - time / duration);

            if (slideIndicatorsParent != null)
                slideIndicatorsParent.localPosition = originalPosIndicators + new Vector3(offset, 0f, 0f);

            if (activeSlide != null)
                activeSlide.localPosition = originalPosSlide + new Vector3(offset, 0f, 0f);

            yield return null;
        }

        if (slideIndicatorsParent != null)
            slideIndicatorsParent.localPosition = originalPosIndicators;

        if (activeSlide != null)
            activeSlide.localPosition = originalPosSlide;
    }
}
