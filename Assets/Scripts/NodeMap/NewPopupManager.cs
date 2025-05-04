using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


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
    // [SerializeField] private Transform slidesParent; // Points to "Slides" container in hierarchy

    [Header("Slide Indicators")]
    [SerializeField] private GameObject slideDotPrefab;
    [SerializeField] private Transform slideIndicatorsParent;
    [SerializeField] private Sprite activeDotSprite;
    [SerializeField] private Sprite inactiveDotSprite;
    [SerializeField] private CanvasGroup slideIndicatorsCanvasGroup;

    [SerializeField] private GameObject finalSlidePrefab;
    [SerializeField] private GameObject finalSlideObject; // assign in Inspector

    private QuizData quizData;
    private List<QuizQuestion> currentQuizQuestions;
    private int currentQuizQuestionIndex = 0;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private GameObject slidesParent;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject failurePanel;
    [SerializeField] private GameObject successPanel;
    private int openedNodeIndex = -1;



    private HashSet<int> unlockedQuizQuestions = new HashSet<int>();

    private List<GameObject> currentNodeSlides = new List<GameObject>();
    private List<GameObject> spawnedDots = new List<GameObject>();
    private int currentSlideIndex = 0;
    private int lastSlideIndex = -1;
    private Coroutine activeDotBreathing;

    private Color enabledColor = Color.white;
    private Color disabledColor = new Color(1f, 1f, 1f, 0.4f);
    private bool inQuizMode = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gameObject.SetActive(true);
        LoadQuizData();
    }

    private void Start()
    {
        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.interactable = false;
        popupCanvasGroup.blocksRaycasts = false;

        leftArrowButton.onClick.AddListener(PreviousSlide);
        rightArrowButton.onClick.AddListener(NextSlide);
        closeButton.onClick.AddListener(() => StartCoroutine(ClosePopup()));
    }

    // -------------------------------
    // Public entry point per node
    // -------------------------------
    public void OpenPopupForNode(int nodeIndex)
    {

        openedNodeIndex = nodeIndex; // ✅ Track which node the popup is for

        // Deactivate and clear old slides
        foreach (var slide in currentNodeSlides)
        {
            if (slide != null)
                slide.SetActive(false);
        }
        currentNodeSlides.Clear();

        // Find node container
        Transform nodeContainer = slidesParent.transform.Find($"Node{nodeIndex}");
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

        GenerateSlideIndicators(currentNodeSlides.Count);

        lastSlideIndex = -1;
        currentSlideIndex = 0;
        ShowSlide(currentSlideIndex);
        StartCoroutine(AnimatePopupOpen());
    }

    public IEnumerator ClosePopup()
    {

        foreach (var slide in currentNodeSlides)
        {
            if (slide != null)
                slide.SetActive(false);
        }

        // Start closing animation
        yield return StartCoroutine(AnimatePopupClose());
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

        UpdateSlideIndicators(currentSlideIndex);
        UpdateArrows();

        lastSlideIndex = index;
    }


    private void NextSlide()
    {
        if (inQuizMode)
        {
            if (currentQuizQuestionIndex < currentQuizQuestions.Count - 1)
            {
                currentQuizQuestionIndex++;
                LoadQuizQuestion(currentQuizQuestionIndex);
                UpdateSlideIndicators(currentQuizQuestionIndex);
                StartCoroutine(BounceButton(rightArrowButton.transform));
            }
        }
        else
        {
            if (currentSlideIndex < currentNodeSlides.Count - 1)
            {
                currentSlideIndex++;
                ShowSlide(currentSlideIndex);
                StartCoroutine(BounceButton(rightArrowButton.transform));
            }
        }
    }

    private void PreviousSlide()
    {
        if (inQuizMode)
        {
            if (currentQuizQuestionIndex > 0)
            {
                currentQuizQuestionIndex--;
                LoadQuizQuestion(currentQuizQuestionIndex);
                UpdateSlideIndicators(currentQuizQuestionIndex);
                StartCoroutine(BounceButton(leftArrowButton.transform));
            }
        }
        else
        {
            if (currentSlideIndex > 0)
            {
                currentSlideIndex--;
                ShowSlide(currentSlideIndex);
                StartCoroutine(BounceButton(leftArrowButton.transform));
            }
        }
    }

    private void UpdateArrows()
    {
        if (inQuizMode)
        {
            leftArrowButton.interactable = currentQuizQuestionIndex > 0;
            leftArrowButton.image.color = leftArrowButton.interactable ? enabledColor : disabledColor;

            rightArrowButton.interactable = unlockedQuizQuestions.Contains(currentQuizQuestionIndex + 1);
            rightArrowButton.image.color = rightArrowButton.interactable ? enabledColor : disabledColor;
        }
        else
        {
            leftArrowButton.interactable = currentSlideIndex > 0;
            leftArrowButton.image.color = leftArrowButton.interactable ? enabledColor : disabledColor;

            rightArrowButton.interactable = currentSlideIndex < currentNodeSlides.Count - 1;
            rightArrowButton.image.color = rightArrowButton.interactable ? enabledColor : disabledColor;
        }
    }

    // -------------------------------
    // Slide Indicators
    // -------------------------------
    private void GenerateSlideIndicators(int count)
    {
        foreach (var dot in spawnedDots)
        {
            Destroy(dot);
        }
        spawnedDots.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject dot = Instantiate(slideDotPrefab, slideIndicatorsParent);
            spawnedDots.Add(dot);
        }

        UpdateSlideIndicators(0);
    }

    private void UpdateSlideIndicators(int activeIndex)
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
                img.sprite = (i == activeIndex) ? activeDotSprite : inactiveDotSprite;
            }

            if (i == activeIndex)
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
    // Quiz Controller
    // -------------------------------

    private void LoadQuizData()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("quiz_data");
        if (jsonText != null)
        {
            quizData = JsonUtility.FromJson<QuizData>(jsonText.text);
            Debug.Log("Quiz data loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load quiz_data.json from Resources.");
        }
    }

    public void StartQuizForNode(int nodeIndex)
    {
        inQuizMode = true;
        unlockedQuizQuestions.Clear();
        unlockedQuizQuestions.Add(0); // first question always unlocked

        foreach (var slide in currentNodeSlides)
        {
            if (slide != null)
                slide.SetActive(false);
        }

        Debug.Log($"[QUIZ] Starting quiz for Node {nodeIndex}");

        NodeQuiz nodeQuiz = quizData.nodes.FirstOrDefault(n => n.nodeId == nodeIndex);
        if (nodeQuiz == null)
        {
            Debug.LogError($"No quiz data found for Node {nodeIndex}.");
            return;
        }

        currentQuizQuestions = nodeQuiz.questions.ToList();
        currentQuizQuestionIndex = 0;

        StartCoroutine(TransitionToQuiz());

        GenerateSlideIndicators(currentQuizQuestions.Count);
        UpdateSlideIndicators(0);

        LoadQuizQuestion(currentQuizQuestionIndex);
    }

    private void LoadQuizQuestion(int index)
    {
        var question = currentQuizQuestions[index];
        questionText.text = question.questionText;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int capturedIndex = i;
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = question.options[i];
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(capturedIndex));
        }

        StartCoroutine(AnimateSlideIn(quizPanel.transform));
        UpdateArrows();
    }

    private void OnOptionSelected(int selectedIndex)
    {
        var question = currentQuizQuestions[currentQuizQuestionIndex];

        if (selectedIndex == question.correctAnswerIndex)
        {
            Debug.Log("[QUIZ] Correct!");

            unlockedQuizQuestions.Add(currentQuizQuestionIndex + 1);

            UpdateArrows();

            currentQuizQuestionIndex++;
            if (currentQuizQuestionIndex < currentQuizQuestions.Count)
            {
                LoadQuizQuestion(currentQuizQuestionIndex);
            }
            else
            {
                Debug.Log("[QUIZ] Quiz Complete!");
                inQuizMode = false;
                StartCoroutine(TransitionToSuccessPanel());
            }
        }
        else
        {
            Debug.Log("[QUIZ] Incorrect — try again!");

            // Find the clicked button and shake it
            StartCoroutine(ShakeButton(optionButtons[selectedIndex].transform));
            StartCoroutine(TransitionToFailurePanel());
        }
    }

    private IEnumerator ShakeButton(Transform buttonTransform)
    {
        float duration = 0.3f;
        float magnitude = 10f;
        float elapsed = 0f;
        Vector3 originalPos = buttonTransform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Sin(elapsed * 40f) * magnitude * (1f - elapsed / duration);
            buttonTransform.localPosition = originalPos + new Vector3(x, 0f, 0f);
            yield return null;
        }

        buttonTransform.localPosition = originalPos;
    }

    private IEnumerator TransitionToFailurePanel()
    {
        float duration = 0.5f;
        CanvasGroup quizGroup = quizPanel.GetComponent<CanvasGroup>();
        CanvasGroup failureGroup = failurePanel.GetComponent<CanvasGroup>();

        // Fade out quiz
        if (quizGroup != null)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                quizGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
                yield return null;
            }
            quizGroup.alpha = 0f;
            quizPanel.SetActive(false);
        }

        // Hide indicators and arrows
        slideIndicatorsParent.gameObject.SetActive(false);
        leftArrowButton.interactable = false;
        rightArrowButton.interactable = false;

        // Fade in failure panel
        failurePanel.SetActive(true);
        if (failureGroup != null)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                failureGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
                yield return null;
            }
            failureGroup.alpha = 1f;
        }
    }

    public void RestartQuizForNode()
    {
        Debug.Log("[QUIZ] Restarting quiz.");

        failurePanel.SetActive(false);
        slideIndicatorsParent.gameObject.SetActive(true);

        inQuizMode = true;
        currentQuizQuestionIndex = 0;
        unlockedQuizQuestions.Clear();
        unlockedQuizQuestions.Add(0);

        GenerateSlideIndicators(currentQuizQuestions.Count);
        UpdateSlideIndicators(0);

        quizPanel.SetActive(true);
        LoadQuizQuestion(currentQuizQuestionIndex);
    }

    public void ReturnToSlides()
    {
        Debug.Log("[QUIZ] Returning to educational slides.");

        failurePanel.SetActive(false);
        slideIndicatorsParent.gameObject.SetActive(true);

        inQuizMode = false;
        currentSlideIndex = 0;

        ShowSlide(currentSlideIndex);
        slidesParent.SetActive(true);

        GenerateSlideIndicators(currentNodeSlides.Count);
        UpdateSlideIndicators(0);
        UpdateArrows();
    }

    private IEnumerator TransitionToSuccessPanel()
    {
        float duration = 0.5f;
        CanvasGroup quizGroup = quizPanel.GetComponent<CanvasGroup>();
        CanvasGroup successGroup = successPanel.GetComponent<CanvasGroup>();

        // Fade out quiz
        if (quizGroup != null)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                quizGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
                yield return null;
            }
            quizGroup.alpha = 0f;
            quizPanel.SetActive(false);
        }

        // Hide indicators and arrows
        slideIndicatorsParent.gameObject.SetActive(false);
        leftArrowButton.interactable = false;
        rightArrowButton.interactable = false;

        // Fade in success panel
        successPanel.SetActive(true);
        if (successGroup != null)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                successGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
                yield return null;
            }
            successGroup.alpha = 1f;
        }
    }

    public void CompleteCurrentNode()
    {
        Debug.Log("[QUIZ] Completing current node and advancing.");

        PlayerSplineMovement playerMover = FindFirstObjectByType<PlayerSplineMovement>();
        int completedNodeIndex = openedNodeIndex - 1; // ✅ Use opened node, not global current

        if (playerMover != null && playerMover.IsNodeCompleted(completedNodeIndex))
        {
            // ✅ Already completed — just close the popup
            Debug.Log("[QUIZ] Node already completed. Closing popup only.");
            successPanel.SetActive(false);
            StartCoroutine(ClosePopup());
            return;
        }

        // ✅ Node not yet completed — mark as complete
        successPanel.SetActive(false);
        StartCoroutine(CompleteAfterPopupCloses(completedNodeIndex));
        
    }



    private IEnumerator CompleteAfterPopupCloses(int nodeIndexToComplete)
    {
        yield return StartCoroutine(ClosePopup());

        PlayerSplineMovement playerMover = FindFirstObjectByType<PlayerSplineMovement>();
        if (playerMover != null)
        {
            playerMover.SetNodeToComplete(nodeIndexToComplete);
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

    private IEnumerator TransitionToQuiz()
    {
        float duration = 0.5f;
        CanvasGroup slidesGroup = slidesParent.GetComponent<CanvasGroup>();
        CanvasGroup quizGroup = quizPanel.GetComponent<CanvasGroup>();

        // Fade out slides
        if (slidesGroup != null)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                slidesGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
                yield return null;
            }
            slidesGroup.alpha = 0f;
            slidesParent.SetActive(false);
        }

        // Fade in quiz
        quizPanel.SetActive(true);
        if (quizGroup != null)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                quizGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
                yield return null;
            }
            quizGroup.alpha = 1f;
        }
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
