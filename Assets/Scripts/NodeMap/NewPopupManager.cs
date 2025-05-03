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

    [SerializeField] private GameObject finalSlidePrefab;
    [SerializeField] private GameObject finalSlideObject; // assign in Inspector

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
        // Deactivate and clear old slides
        foreach (var slide in currentNodeSlides)
        {
            if (slide != null)
                slide.SetActive(false);
        }
        currentNodeSlides.Clear();

        // Find node container
        Transform nodeContainer = slidesParent.Find($"Node{nodeIndex}");
        if (nodeContainer == null)
        {
            Debug.LogWarning($"Node{nodeIndex} slides not found!");
            return;
        }

        // Add learning slides
        foreach (Transform slide in nodeContainer)
        {
            currentNodeSlides.Add(slide.gameObject);
            slide.gameObject.SetActive(false);

            // If this is the FinalSlide, set its header + button
            if (slide.name == "StartQuiz")
            {
                TMP_Text finalHeader = slide.GetComponentInChildren<TMP_Text>();
                if (finalHeader != null)
                {
                    finalHeader.text = $"Congratulations for completing the {nodeHeaders[nodeIndex - 1]} node!";
                }

                Button quizButton = slide.GetComponentInChildren<Button>();
                if (quizButton != null)
                {
                    quizButton.onClick.RemoveAllListeners();
                    quizButton.onClick.AddListener(() => StartQuizForNode(nodeIndex));
                }
            }
        }

        // Set header
        headerText.text = (nodeIndex - 1 >= 0 && nodeIndex - 1 < nodeHeaders.Count)
            ? nodeHeaders[nodeIndex - 1]
            : $"Node {nodeIndex}";

        GenerateSlideIndicators();

        currentSlideIndex = 0;
        ShowSlide(currentSlideIndex);
        StartCoroutine(AnimatePopupOpen());
    }


    private void StartQuizForNode(int nodeIndex)
    {
        Debug.Log($"Starting quiz for Node {nodeIndex}");

        // TODO: Replace this with your actual quiz system call
        // e.g., QuizManager.Instance.StartQuiz(nodeIndex);
    }

    public void ClosePopup()
    {
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
        // Animate out previous slide (if any)
        if (lastSlideIndex >= 0 && lastSlideIndex < currentNodeSlides.Count)
        {
            GameObject lastSlide = currentNodeSlides[lastSlideIndex];
            if (lastSlide != null)
                StartCoroutine(AnimateSlideOut(lastSlide));
        }

        // Activate only the new target slide
        for (int i = 0; i < currentNodeSlides.Count; i++)
        {
            bool isActive = i == index;
            currentNodeSlides[i].SetActive(isActive);
        }

        Transform activeSlide = currentNodeSlides[index].transform;
        CanvasGroup slideCg = activeSlide.GetComponent<CanvasGroup>();
        if (slideCg == null)
            slideCg = activeSlide.gameObject.AddComponent<CanvasGroup>();

        if (lastSlideIndex < 0)
        {
            // First open → skip extra animation; set visible
            activeSlide.localScale = Vector3.one;
            slideCg.alpha = 1f;

            if (slideIndicatorsCanvasGroup != null)
                slideIndicatorsCanvasGroup.alpha = 1f;
        }
        else
        {
            // Normal slide switch → use animation
            StartCoroutine(AnimateSlideIn(activeSlide));
        }

        UpdateSlideIndicators();
        UpdateArrows();

        lastSlideIndex = index;
    }


    private void NextSlide()
    {
        if (currentSlideIndex < currentNodeSlides.Count - 1)
        {
            currentSlideIndex++;
            ShowSlide(currentSlideIndex);
            StartCoroutine(BounceButton(rightArrowButton.transform));
        }
    }

    private void PreviousSlide()
    {
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            ShowSlide(currentSlideIndex);
            StartCoroutine(BounceButton(leftArrowButton.transform));
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
        if (spawnedDots.Count == 0) return;

        for (int i = 0; i < spawnedDots.Count; i++)
        {
            GameObject dotContainer = spawnedDots[i];
            Transform dotVisual = dotContainer.transform.Find("DotVisual");
            if (dotVisual == null)
            {
                Debug.LogWarning("DotVisual child not found!");
                continue;
            }

            Image img = dotVisual.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = (i == currentSlideIndex) ? activeDotSprite : inactiveDotSprite;
            }

            if (i == currentSlideIndex)
            {
                if (activeDotBreathing != null)
                    StopCoroutine(activeDotBreathing);

                activeDotBreathing = StartCoroutine(BreatheDot(dotVisual));
            }
            else if (i == lastSlideIndex)
            {
                dotVisual.localScale = Vector3.one;
            }
        }
    }

    private IEnumerator BreatheDot(Transform dotTransform)
    {
        float breatheDuration = 1f;
        float breatheMagnitude = 0.2f;
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

    private IEnumerator AnimateSlideIn(Transform slideTransform)
    {
        CanvasGroup slideCg = slideTransform.GetComponent<CanvasGroup>();
        if (slideCg == null)
        {
            slideCg = slideTransform.gameObject.AddComponent<CanvasGroup>();
        }

        float fadeDuration = 0.2f;
        float pulseScale = 0.9f;
        Vector3 smallScale = Vector3.one * pulseScale;
        Vector3 originalScale = Vector3.one;

        slideTransform.localScale = smallScale;
        slideCg.alpha = 0f;

        if (slideIndicatorsCanvasGroup != null)
            slideIndicatorsCanvasGroup.alpha = 0f;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            slideTransform.localScale = Vector3.Lerp(smallScale, originalScale, t);
            slideCg.alpha = t;

            if (slideIndicatorsCanvasGroup != null)
                slideIndicatorsCanvasGroup.alpha = t;

            yield return null;
        }

        slideTransform.localScale = originalScale;
        slideCg.alpha = 1f;

        if (slideIndicatorsCanvasGroup != null)
            slideIndicatorsCanvasGroup.alpha = 1f;
    }



    private IEnumerator AnimateSlideOut(GameObject slide)
    {
        Transform slideTransform = slide.transform;
        CanvasGroup slideCg = slide.GetComponent<CanvasGroup>();
        if (slideCg == null)
        {
            slideCg = slide.AddComponent<CanvasGroup>();
        }

        float fadeDuration = 0.2f;
        float pulseScale = 0.9f;
        Vector3 originalScale = Vector3.one;
        Vector3 smallScale = originalScale * pulseScale;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            slideTransform.localScale = Vector3.Lerp(originalScale, smallScale, t);
            slideCg.alpha = 1f - t;

            if (slideIndicatorsCanvasGroup != null)
                slideIndicatorsCanvasGroup.alpha = 1f - t;

            yield return null;
        }

        slideCg.alpha = 0f;
        slideTransform.localScale = smallScale;
        slide.SetActive(false);

        if (slideIndicatorsCanvasGroup != null)
            slideIndicatorsCanvasGroup.alpha = 0f;
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
}
