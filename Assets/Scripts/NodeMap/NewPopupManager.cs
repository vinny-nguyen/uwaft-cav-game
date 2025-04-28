using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text mainText;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button closeButton;

    [Header("Animation Settings")]
    private RectTransform headerTransform;
    private RectTransform mainTextTransform;

    [Header("Slide Indicator Settings")]
    [SerializeField] private GameObject slideDotPrefab;
    [SerializeField] private Transform slideIndicatorsParent;
    [SerializeField] private Sprite activeDotSprite;
    [SerializeField] private Sprite inactiveDotSprite;
    [SerializeField] private CanvasGroup slideIndicatorsCanvasGroup;


    [Header("Slide Data")]
    [SerializeField] private List<SlideData> slides = new List<SlideData>();


    private int currentSlideIndex = 0;
    private Color enabledColor = Color.white;
    private Color disabledColor = new Color(1f, 1f, 1f, 0.4f); // Slightly transparent white
    private Vector3 textOriginalScale = Vector3.one;
    private List<GameObject> spawnedDots = new List<GameObject>();
    private int lastSlideIndex = -1;
    private Coroutine activeDotBreathing;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 🔥 Reactivate PopupCanvas if disabled in Editor
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.interactable = false;
        popupCanvasGroup.blocksRaycasts = false;

        if (headerText != null)
            headerTransform = headerText.GetComponent<RectTransform>();

        if (mainText != null)
            mainTextTransform = mainText.GetComponent<RectTransform>();

        leftArrowButton.onClick.AddListener(PreviousSlide);
        rightArrowButton.onClick.AddListener(NextSlide);
        closeButton.onClick.AddListener(ClosePopup);
    }


    public void OpenPopup()
    {
        currentSlideIndex = 0;
        GenerateSlideIndicators();
        UpdateSlide(skipAnimation: true);
        StartCoroutine(AnimatePopupOpen());
    }


    public void ClosePopup()
    {
        StartCoroutine(AnimatePopupClose());
    }


    private void NextSlide()
    {
        if (slides.Count == 0) return;

        if (currentSlideIndex >= slides.Count - 1)
        {
            StartCoroutine(ShakePopup());
            return; // No move
        }

        currentSlideIndex++;
        UpdateSlide();
        StartCoroutine(BounceButton(rightArrowButton.transform));
        StartCoroutine(NudgeUIElements()); // 🔥 Nudge!
    }

    private void PreviousSlide()
    {
        if (slides.Count == 0) return;

        if (currentSlideIndex <= 0)
        {
            StartCoroutine(ShakePopup());
            return; // No move
        }

        currentSlideIndex--;
        UpdateSlide();
        StartCoroutine(BounceButton(leftArrowButton.transform));
        StartCoroutine(NudgeUIElements()); // 🔥 Nudge!
    }


    private void UpdateSlide(bool skipAnimation = false)
    {
        if (slides.Count == 0) return;

        if (skipAnimation)
        {
            headerText.text = slides[currentSlideIndex].header;
            mainText.text = slides[currentSlideIndex].body;

            headerText.color = Color.white;
            mainText.color = Color.white;

            if (headerTransform != null)
                headerTransform.localScale = Vector3.one;
            if (mainTextTransform != null)
                mainTextTransform.localScale = Vector3.one;
        }
        else
        {
            StartCoroutine(AnimateSlideSwitch());
        }

        UpdateSlideIndicators(); // ✅ Always update indicators!
        UpdateArrows(); // ✅ Update arrows based on currentSlideIndex
    }


    private void GenerateSlideIndicators()
    {
        // Clear previous if any
        foreach (var dot in spawnedDots)
        {
            Destroy(dot);
        }
        spawnedDots.Clear();

        if (slides.Count == 0 || slideDotPrefab == null) return;

        for (int i = 0; i < slides.Count; i++)
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
            Image img = spawnedDots[i].GetComponent<Image>();
            if (img != null)
            {
                img.sprite = (i == currentSlideIndex) ? activeDotSprite : inactiveDotSprite;
            }

            // Scale immediately to correct size
            if (i == currentSlideIndex)
            {
                if (activeDotBreathing != null)
                    StopCoroutine(activeDotBreathing);

                activeDotBreathing = StartCoroutine(BreatheDot(spawnedDots[i].transform));
            }
            else if (i == lastSlideIndex)
            {
                spawnedDots[i].transform.localScale = Vector3.one;
            }
        }

        lastSlideIndex = currentSlideIndex;
    }


    private void UpdateArrows()
    {
        // Fade and enable/disable arrows based on currentSlideIndex

        if (leftArrowButton != null)
        {
            bool canGoLeft = currentSlideIndex > 0;
            leftArrowButton.interactable = canGoLeft;
            leftArrowButton.image.color = canGoLeft ? enabledColor : disabledColor;
        }

        if (rightArrowButton != null)
        {
            bool canGoRight = currentSlideIndex < slides.Count - 1;
            rightArrowButton.interactable = canGoRight;
            rightArrowButton.image.color = canGoRight ? enabledColor : disabledColor;
        }
    }

    private IEnumerator AnimatePopupOpen()
    {
        popupCanvasGroup.blocksRaycasts = true;

        float duration = 0.4f;
        float time = 0f;

        Transform popupTransform = popupCanvasGroup.transform;
        Vector3 originalScale = Vector3.one;
        Vector3 startScale = Vector3.one * 0.8f;

        popupTransform.localScale = startScale;
        popupCanvasGroup.alpha = 0f;
        backgroundOverlay.color = new Color(0f, 0f, 0f, 0f); // start transparent

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            popupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            popupTransform.localScale = Vector3.Lerp(startScale, originalScale, t);

            if (backgroundOverlay != null)
                backgroundOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.6f, t));

            if (slideIndicatorsCanvasGroup != null)
                slideIndicatorsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        popupCanvasGroup.alpha = 1f;
        popupTransform.localScale = originalScale;

        popupCanvasGroup.interactable = true; // 🔥 Now finally interactable
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
                backgroundOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.6f, 0f, t));

            if (slideIndicatorsCanvasGroup != null)
                slideIndicatorsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        popupCanvasGroup.alpha = 0f;
        popupTransform.localScale = originalScale;

        popupCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator AnimateSlideSwitch()
    {
        float fadeDuration = 0.2f;
        float pulseScale = 0.9f;

        Vector3 smallScale = textOriginalScale * pulseScale;

        // 1. Fade out + shrink
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            if (headerTransform != null)
            {
                headerTransform.localScale = Vector3.Lerp(textOriginalScale, smallScale, t);
                headerText.color = new Color(headerText.color.r, headerText.color.g, headerText.color.b, 1f - t);
            }

            if (mainTextTransform != null)
            {
                mainTextTransform.localScale = Vector3.Lerp(textOriginalScale, smallScale, t);
                mainText.color = new Color(mainText.color.r, mainText.color.g, mainText.color.b, 1f - t);
            }

            yield return null;
        }

        // 2. Actually switch the text content while faded out
        headerText.text = slides[currentSlideIndex].header;
        mainText.text = slides[currentSlideIndex].body;

        // 3. Fade in + expand back
        time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            if (headerTransform != null)
            {
                headerTransform.localScale = Vector3.Lerp(smallScale, textOriginalScale, t);
                headerText.color = new Color(headerText.color.r, headerText.color.g, headerText.color.b, t);
            }

            if (mainTextTransform != null)
            {
                mainTextTransform.localScale = Vector3.Lerp(smallScale, textOriginalScale, t);
                mainText.color = new Color(mainText.color.r, mainText.color.g, mainText.color.b, t);
            }

            yield return null;
        }

        // Reset final states
        if (headerTransform != null)
        {
            headerTransform.localScale = textOriginalScale;
            headerText.color = new Color(headerText.color.r, headerText.color.g, headerText.color.b, 1f);
        }

        if (mainTextTransform != null)
        {
            mainTextTransform.localScale = textOriginalScale;
            mainText.color = new Color(mainText.color.r, mainText.color.g, mainText.color.b, 1f);
        }
    }

    private IEnumerator BounceButton(Transform buttonTransform)
    {
        float bounceDuration = 0.2f;
        float bounceScale = 0.8f;
        float time = 0f;

        Vector3 originalScale = Vector3.one;
        Vector3 smallScale = Vector3.one * bounceScale;

        // Shrink
        while (time < bounceDuration / 2f)
        {
            time += Time.deltaTime;
            float t = time / (bounceDuration / 2f);
            buttonTransform.localScale = Vector3.Lerp(originalScale, smallScale, t);
            yield return null;
        }

        time = 0f;

        // Expand back
        while (time < bounceDuration / 2f)
        {
            time += Time.deltaTime;
            float t = time / (bounceDuration / 2f);
            buttonTransform.localScale = Vector3.Lerp(smallScale, originalScale, t);
            yield return null;
        }

        buttonTransform.localScale = originalScale;
    }

    private IEnumerator ShakePopup()
    {
        float shakeDuration = 0.3f;
        float shakeMagnitude = 5f;
        float time = 0f;

        Vector3 originalPosition = popupCanvasGroup.transform.localPosition;

        while (time < shakeDuration)
        {
            time += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            popupCanvasGroup.transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            yield return null;
        }

        popupCanvasGroup.transform.localPosition = originalPosition;
    }

    private IEnumerator ScaleDot(Transform dotTransform, float targetScale)
    {
        float duration = 0.25f;
        float time = 0f;

        Vector3 startScale = dotTransform.localScale;
        Vector3 endScale = Vector3.one * targetScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            dotTransform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        dotTransform.localScale = endScale;
    }

    private IEnumerator NudgeUIElements()
    {
        float duration = 0.25f;
        float magnitude = 10f;
        float frequency = 20f;

        Vector3 originalPosHeader = headerTransform.localPosition;
        Vector3 originalPosMain = mainTextTransform.localPosition;
        Vector3 originalPosIndicators = slideIndicatorsParent.localPosition;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float offset = Mathf.Sin(time * frequency) * magnitude * (1f - time / duration);

            if (headerTransform != null)
                headerTransform.localPosition = originalPosHeader + new Vector3(offset, 0f, 0f);

            if (mainTextTransform != null)
                mainTextTransform.localPosition = originalPosMain + new Vector3(offset, 0f, 0f);

            if (slideIndicatorsParent != null)
                slideIndicatorsParent.localPosition = originalPosIndicators + new Vector3(offset, 0f, 0f);

            yield return null;
        }

        if (headerTransform != null)
            headerTransform.localPosition = originalPosHeader;
        if (mainTextTransform != null)
            mainTextTransform.localPosition = originalPosMain;
        if (slideIndicatorsParent != null)
            slideIndicatorsParent.localPosition = originalPosIndicators;
    }

    private IEnumerator BreatheDot(Transform dotTransform)
    {
        float breatheDuration = 1.5f;
        float breatheMagnitude = 0.1f; // How much scale difference

        while (dotTransform != null && spawnedDots.Contains(dotTransform.gameObject))
        {
            float timer = 0f;

            while (timer < breatheDuration)
            {
                timer += Time.deltaTime;
                float scale = 1f + Mathf.Sin(timer * Mathf.PI * 2f / breatheDuration) * breatheMagnitude;
                dotTransform.localScale = Vector3.one * scale;

                yield return null;
            }
        }
    }

}

[System.Serializable]
public class SlideData
{
    public string header;
    public string body;
}
