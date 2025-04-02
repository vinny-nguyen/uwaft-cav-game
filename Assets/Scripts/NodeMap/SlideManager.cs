using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Image = UnityEngine.UI.Image;

public class SlideManager : MonoBehaviour
{
    [Header("Core References")]
    public TextMeshProUGUI slideTitle;
    public TextMeshProUGUI slideText;
    public RectTransform slideContainer;
    public RectTransform viewport;
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Button completeNodeButton;
    public PlayerMovement playerMovement;

    [Header("Quiz Settings")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public Transform answersParent;
    public GameObject answerButtonPrefab;
    public Color normalColor = Color.white;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public float wrongAnswerShakeIntensity = 10f;

    [Header("Quiz Navigation")]
    public float wrongAnswerShakeDuration = 0.5f;
    public float wrongAnswerShakeMagnitude = 10f;
    private bool[] answeredQuestions;

    [Header("Indicators")]
    public Transform contentIndicatorsParent; // Left side
    public Transform quizIndicatorsParent;    // Right side
    public GameObject indicatorPrefab;        // Shared prefab
    public GameObject lightCirclePrefab;     // For active state reference

    [Header("Animation Settings")]
    public float slideAnimationDuration = 0.2f;

    private int savedQuestionIndex = 0;
    private bool wasInQuizMode = false;

    [Header("Debug")]
    public bool enableDebugLogging = true;

    // Private state
    private List<GameObject> contentIndicators = new List<GameObject>();
    private List<GameObject> quizIndicators = new List<GameObject>();
    private List<Button> activeAnswerButtons = new List<Button>();
    private bool isAnimating = false;
    private bool isInQuizMode = false;
    private int currentTopicIndex = 0;
    private int currentSlideIndex = 0;
    private int currentQuestionIndex = 0;

    [System.Serializable]
    public class Slide
    {
        public string content;
    }

    [System.Serializable]
    public class QuizQuestion
    {
        public string question;
        public string[] answers; // answers[0] is correct
    }

    [System.Serializable]
    public class Topic
    {
        public string topicName;
        public List<Slide> learningSlides = new List<Slide>();
        public List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        [HideInInspector] public List<bool> answeredCorrectly;
    }

    private List<Topic> topics = new List<Topic>();

    void Start()
    {
        InitializeSlideContent();
        leftArrowButton.onClick.AddListener(ShowPrevious);
        rightArrowButton.onClick.AddListener(ShowNext);
        completeNodeButton.onClick.AddListener(CompleteCurrentNode);
    }

    void InitializeSlideContent()
    {

        // Topic 1: Tires
        Topic tires = new Topic
        {
            topicName = "Tires",
            learningSlides = {
            new Slide { content = "Tires are the only contact point between the car and the road." },
            new Slide { content = "EV tires are designed for low rolling resistance to maximize range." },
            new Slide { content = "Special compounds handle instant torque without wearing quickly." }
        },
            quizQuestions = {
            new QuizQuestion {
                question = "What's special about EV tires?",
                answers = new string[] {
                    "Optimized for weight and instant torque",
                    "They're identical to regular tires",
                    "Made from recycled materials",
                    "Only need replacing every 100,000 miles"
                },
                
            },
            new QuizQuestion {
                question = "Why do EV tires have low rolling resistance?",
                answers = new string[] {
                    "To maximize battery range",
                    "To make them cheaper",
                    "For better winter performance",
                    "It's required by law"
                },
                
            }
        }
        };
        tires.answeredCorrectly = new List<bool>(new bool[tires.quizQuestions.Count]);
        topics.Add(tires);

        // Topic 2: Aerodynamics
        Topic aero = new Topic
        {
            topicName = "Aerodynamics",
            learningSlides = {
            new Slide { content = "Aerodynamics significantly impact range at high speeds." },
            new Slide { content = "Smooth underbodies and covered wheels reduce drag." },
            new Slide { content = "Active grille shutters optimize cooling with minimal drag." }
        },
            quizQuestions = {
            new QuizQuestion {
                question = "When does aerodynamics matter most?",
                answers = new string[] {
                    "At highway speeds (45+ mph)",
                    "During city driving",
                    "Only when accelerating",
                    "All speeds equally"
                },
                
            },
            new QuizQuestion {
                question = "What feature helps reduce drag while cooling?",
                answers = new string[] {
                    "Active grille shutters",
                    "Larger side mirrors",
                    "Open wheel wells",
                    "Roof-mounted spoilers"
                },
                
            }
        }
        };
        aero.answeredCorrectly = new List<bool>(new bool[aero.quizQuestions.Count]);
        topics.Add(aero);

        // Topic 3: Battery
        Topic battery = new Topic
        {
            topicName = "Battery",
            learningSlides = {
            new Slide { content = "Lithium-ion batteries are the current standard for EVs." },
            new Slide { content = "Battery cooling systems prevent overheating and maintain efficiency." },
            new Slide { content = "The battery pack is the heaviest single component in an EV." }
        },
            quizQuestions = {
            new QuizQuestion {
                question = "Why do EV batteries need cooling?",
                answers = new string[] {
                    "To maintain efficiency and prevent overheating",
                    "To make them charge slower",
                    "Because they work better when cold",
                    "It's just for looks"
                },
                
            },
            new QuizQuestion {
                question = "What's true about EV battery weight?",
                answers = new string[] {
                    "It's the heaviest single component",
                    "It weighs less than the seats",
                    "Weight doesn't affect performance",
                    "Most weight is in the casing"
                },
                
            }
        }
        };
        battery.answeredCorrectly = new List<bool>(new bool[battery.quizQuestions.Count]);
        topics.Add(battery);

        // Topic 4: Regenerative Braking
        Topic regenBraking = new Topic
        {
            topicName = "Regenerative Braking",
            learningSlides = {
            new Slide { content = "Converts kinetic energy back into electrical energy." },
            new Slide { content = "Reduces wear on traditional friction brakes." },
            new Slide { content = "Can enable 'one-pedal' driving in EVs." }
        },
            quizQuestions = {
            new QuizQuestion {
                question = "What does regenerative braking do?",
                answers = new string[] {
                    "Converts motion to electricity",
                    "Makes brakes last forever",
                    "Only works at high speeds",
                    "Is just for emergency stops"
                },
                
            },
            new QuizQuestion {
                question = "What's 'one-pedal' driving?",
                answers = new string[] {
                    "Using accelerator to both speed up and slow down",
                    "A special type of brake pedal",
                    "Only using the left foot",
                    "A racing technique"
                },
                
            }
        }
        };
        regenBraking.answeredCorrectly = new List<bool>(new bool[regenBraking.quizQuestions.Count]);
        topics.Add(regenBraking);

        // Topic 5: Electric Motors
        Topic motors = new Topic
        {
            topicName = "Electric Motors",
            learningSlides = {
            new Slide { content = "EVs use AC induction or permanent magnet motors." },
            new Slide { content = "Motors deliver instant torque for quick acceleration." },
            new Slide { content = "Most EVs don't need multi-speed transmissions." }
        },
            quizQuestions = {
            new QuizQuestion {
                question = "Why don't most EVs need multi-speed transmissions?",
                answers = new string[] {
                    "Electric motors have wide power bands",
                    "They're too expensive",
                    "EVs don't go fast enough",
                    "It's illegal"
                },
                
            },
            new QuizQuestion {
                question = "What's special about EV torque?",
                answers = new string[] {
                    "Available instantly from zero RPM",
                    "Only at high speeds",
                    "Less than gas engines",
                    "Hard to control"
                },
                
            }
        }
        };
        motors.answeredCorrectly = new List<bool>(new bool[motors.quizQuestions.Count]);
        topics.Add(motors);

        // Topic 6: Charging
        Topic charging = new Topic
        {
            topicName = "Charging",
            learningSlides = {
            new Slide { content = "Level 1 charging uses standard 120V outlets." },
            new Slide { content = "Level 2 chargers (240V) are most common for home use." },
            new Slide { content = "DC fast charging can charge to 80% in 30-45 minutes." }
        },
            quizQuestions = {
            new QuizQuestion {
                question = "What's true about DC fast charging?",
                answers = new string[] {
                    "Charges to 80% in 30-45 minutes",
                    "Can safely charge to 100% quickly",
                    "Works on all EVs the same",
                    "Is the cheapest option"
                },
                
            },
            new QuizQuestion {
                question = "Which charger type is best for home use?",
                answers = new string[] {
                    "Level 2 (240V)",
                    "Level 1 (120V)",
                    "DC fast charging",
                    "Wireless charging"
                },
                
            }
        }
        };
        charging.answeredCorrectly = new List<bool>(new bool[charging.quizQuestions.Count]);
        topics.Add(charging);

        CreateIndicators();
        UpdateDisplay();
    }

    public void SetCurrentTopic(int nodeIndex)
    {
        currentTopicIndex = nodeIndex;
        currentSlideIndex = 0;
        isInQuizMode = false;
        CreateIndicators();
        UpdateDisplay();
    }

    void CreateIndicators()
    {
        // Clear old
        foreach (Transform child in contentIndicatorsParent) Destroy(child.gameObject);
        foreach (Transform child in quizIndicatorsParent) Destroy(child.gameObject);
        contentIndicators.Clear();
        quizIndicators.Clear();

        // Create new
        for (int i = 0; i < topics[currentTopicIndex].learningSlides.Count; i++)
            contentIndicators.Add(Instantiate(indicatorPrefab, contentIndicatorsParent));

        for (int i = 0; i < topics[currentTopicIndex].quizQuestions.Count; i++)
            quizIndicators.Add(Instantiate(indicatorPrefab, quizIndicatorsParent));
    }

    void UpdateDisplay()
    {
        if (enableDebugLogging) Debug.Log($"UpdateDisplay - isInQuizMode: {isInQuizMode}, currentSlideIndex: {currentSlideIndex}, currentQuestionIndex: {currentQuestionIndex}");

        completeNodeButton.gameObject.SetActive(false);

        if (isInQuizMode)
        {
            if (enableDebugLogging) Debug.Log("Showing quiz mode");
            quizPanel.SetActive(true);
            slideContainer.gameObject.SetActive(false);
            ShowQuizQuestion();
        }
        else
        {
            if (enableDebugLogging) Debug.Log("Showing content mode");
            quizPanel.SetActive(false);
            slideContainer.gameObject.SetActive(true);
            slideTitle.text = topics[currentTopicIndex].topicName;
            slideText.text = topics[currentTopicIndex].learningSlides[currentSlideIndex].content;
        }
        UpdateIndicators();
    }

    void UpdateIndicators()
    {
        // Content indicators (left)
        for (int i = 0; i < contentIndicators.Count; i++)
        {
            bool isActive = !isInQuizMode && (i == currentSlideIndex);
            SetIndicatorState(contentIndicators[i], isActive);
        }

        // Quiz indicators (right)
        for (int i = 0; i < quizIndicators.Count; i++)
        {
            bool isActive = isInQuizMode && (i == currentQuestionIndex);
            SetIndicatorState(quizIndicators[i], isActive);
        }
    }

    void SetIndicatorState(GameObject indicator, bool isActive)
    {
        Image img = indicator.GetComponent<Image>();
        img.sprite = isActive ? lightCirclePrefab.GetComponent<Image>().sprite :
                              indicatorPrefab.GetComponent<Image>().sprite;

        if (isActive) StartCoroutine(PulseIndicator(indicator.transform));
        else indicator.transform.localScale = Vector3.one;
    }

    void ShowPrevious()
    {
        if (isAnimating) return;

        if (isInQuizMode)
        {
            StartCoroutine(TransitionToContent());
        }
        else if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            StartCoroutine(SlideAnimation(-1));
        }
    }

    void ShowNext()
    {
        if (isAnimating) return;

        if (isInQuizMode)
        {
            if (!answeredQuestions[currentQuestionIndex])
            {
                StartCoroutine(ShakeAndHighlight(rightArrowButton.transform));
                return;
            }

            if (currentQuestionIndex < topics[currentTopicIndex].quizQuestions.Count - 1)
            {
                currentQuestionIndex++;
                StartCoroutine(SlideAnimation(1));
            }
            else
            {
                CompleteQuiz();
            }
        }
        else if (currentSlideIndex < topics[currentTopicIndex].learningSlides.Count - 1)
        {
            currentSlideIndex++;
            StartCoroutine(SlideAnimation(1));
        }
        else
        {
            StartQuiz();
        }
    }


    IEnumerator SlideAnimation(int direction)
    {
        isAnimating = true;
        float slideDistance = viewport.rect.width;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(-direction * slideDistance, 0);

        // Determine which object to move
        RectTransform movingObject = isInQuizMode ?
            questionText.rectTransform :
            slideContainer;

        // Slide out
        float time = 0f;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            movingObject.anchoredPosition = Vector2.Lerp(startPos, endPos, time / 0.2f);
            yield return null;
        }

        // Update content
        if (isInQuizMode)
        {
            ShowQuizQuestion();
        }
        else
        {
            UpdateDisplay();
        }

        // Slide in
        movingObject.anchoredPosition = new Vector2(direction * slideDistance, 0);
        time = 0f;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            movingObject.anchoredPosition = Vector2.Lerp(movingObject.anchoredPosition, startPos, time / 0.2f);
            yield return null;
        }

        UpdateIndicators();
        isAnimating = false;
    }

    void StartQuiz()
    {
        if (!isAnimating)
        {
            // Only reset if this is a fresh start (not just returning to quiz)
            if (!wasInQuizMode || currentQuestionIndex >= topics[currentTopicIndex].quizQuestions.Count)
            {
                savedQuestionIndex = 0;
                wasInQuizMode = true;
            }
            StartCoroutine(TransitionToQuiz());
        }
    }



    void ShowQuizQuestion()
    {
        // Clear old buttons
        foreach (Button btn in activeAnswerButtons) Destroy(btn.gameObject);
        activeAnswerButtons.Clear();

        // Set question
        var question = topics[currentTopicIndex].quizQuestions[currentQuestionIndex];
        questionText.text = question.question;

        // Create answers
        for (int i = 0; i < question.answers.Length; i++)
        {
            GameObject btnObj = Instantiate(answerButtonPrefab, answersParent);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = question.answers[i];
            Button btn = btnObj.GetComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() => OnAnswerSelected(index));
            activeAnswerButtons.Add(btn);
        }
    }

    void OnAnswerSelected(int selectedIndex)
    {

        var question = topics[currentTopicIndex].quizQuestions[currentQuestionIndex];
        bool isCorrect = (selectedIndex == 0);

        answeredQuestions[currentQuestionIndex] = true; // Mark as answer

        // Visual feedback
        Image buttonImg = activeAnswerButtons[selectedIndex].GetComponent<Image>();
        buttonImg.color = isCorrect ? correctColor : wrongColor;

        if (isCorrect)
        {
            StartCoroutine(NextQuestionAfterDelay(1f));
        }
        else
        {
            StartCoroutine(ShakeButton(activeAnswerButtons[selectedIndex].transform));
            StartCoroutine(ResetButtonColor(buttonImg, 1f));
        }
    }

    IEnumerator ShakeButton(Transform button)
    {
        Vector3 originalPos = button.localPosition;
        float elapsed = 0f;

        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float x = originalPos.x + UnityEngine.Random.Range(-1f, 1f) * wrongAnswerShakeIntensity;
            float y = originalPos.y + UnityEngine.Random.Range(-1f, 1f) * wrongAnswerShakeIntensity;
            button.localPosition = new Vector3(x, y, originalPos.z);
            yield return null;
        }
        button.localPosition = originalPos;
    }

    IEnumerator ShakeAndHighlight(Transform target)
    {
        Image buttonImage = target.GetComponent<Image>();
        Color originalColor = buttonImage.color;
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        // Flash red and shake
        while (elapsed < wrongAnswerShakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / wrongAnswerShakeDuration;

            // Shake effect
            float x = originalPos.x + Mathf.Sin(Time.time * 50f) * wrongAnswerShakeMagnitude;
            float y = originalPos.y + Mathf.Sin(Time.time * 45f) * wrongAnswerShakeMagnitude * 0.7f;
            target.localPosition = new Vector3(x, y, originalPos.z);

            // Color pulse
            buttonImage.color = Color.Lerp(originalColor, wrongColor, Mathf.PingPong(progress * 2f, 1f));

            yield return null;
        }

        // Reset to original state
        target.localPosition = originalPos;
        buttonImage.color = originalColor;
    }

    IEnumerator ResetButtonColor(Image buttonImg, float delay)
    {
        yield return new WaitForSeconds(delay);
        buttonImg.color = normalColor;
    }

    IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentQuestionIndex < topics[currentTopicIndex].quizQuestions.Count - 1)
        {
            currentQuestionIndex++;
            ShowQuizQuestion();
            UpdateIndicators();
        }
        else
        {
            CompleteQuiz();
        }
    }

    void CompleteQuiz()
    {
        // Check if all questions were answered
        bool allAnswered = true;
        for (int i = 0; i < answeredQuestions.Length; i++)
        {
            if (!answeredQuestions[i])
            {
                allAnswered = false;
                break;
            }
        }

        if (allAnswered)
        {
            quizPanel.SetActive(false);
            completeNodeButton.gameObject.SetActive(true);
            wasInQuizMode = false; // Reset for next time
        }
        else
        {
            // Find first unanswered question
            for (int i = 0; i < answeredQuestions.Length; i++)
            {
                if (!answeredQuestions[i])
                {
                    currentQuestionIndex = i;
                    ShowQuizQuestion();
                    break;
                }
            }
        }
    }

    void CompleteCurrentNode()
    {
        playerMovement.CompleteNode(playerMovement.CurrentNode);
        FindAnyObjectByType<PopupManager>()?.ClosePopup();
    }

    IEnumerator PulseIndicator(Transform indicator)
    {
        Vector3 originalScale = indicator.localScale;
        float elapsed = 0f;

        while (elapsed < 0.8f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 2f, 1f);
            indicator.localScale = originalScale * (1f + t * 0.2f);
            yield return null;
        }
        indicator.localScale = originalScale;
    }

    IEnumerator TransitionToQuiz()
    {
        isAnimating = true;

        // Slide out content
        float duration = slideAnimationDuration;
        float time = 0f;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(-viewport.rect.width, 0);

        while (time < duration)
        {
            time += Time.deltaTime;
            slideContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, time / duration);
            yield return null;
        }

        // Initialize quiz mode
        isInQuizMode = true;
        wasInQuizMode = true;

        // Restore position if resuming, otherwise start fresh
        currentQuestionIndex = (wasInQuizMode && savedQuestionIndex < topics[currentTopicIndex].quizQuestions.Count)
            ? savedQuestionIndex
            : 0;

        // Initialize answer tracking if needed
        if (answeredQuestions == null || answeredQuestions.Length != topics[currentTopicIndex].quizQuestions.Count)
        {
            answeredQuestions = new bool[topics[currentTopicIndex].quizQuestions.Count];
        }

        completeNodeButton.gameObject.SetActive(false);
        slideContainer.anchoredPosition = new Vector2(viewport.rect.width, 0);
        UpdateDisplay();

        // Slide in quiz
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            slideContainer.anchoredPosition = Vector2.Lerp(slideContainer.anchoredPosition, Vector2.zero, time / duration);
            yield return null;
        }

        isAnimating = false;
    }

    IEnumerator TransitionToContent()
    {
        isAnimating = true;

        // Save current quiz position
        savedQuestionIndex = currentQuestionIndex;

        // Slide out quiz
        float duration = slideAnimationDuration;
        float time = 0f;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(viewport.rect.width, 0);

        while (time < duration)
        {
            time += Time.deltaTime;
            slideContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, time / duration);
            yield return null;
        }

        // Switch to content
        isInQuizMode = false;
        currentSlideIndex = topics[currentTopicIndex].learningSlides.Count - 1;
        slideContainer.anchoredPosition = new Vector2(-viewport.rect.width, 0);
        UpdateDisplay();

        // Slide in content
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            slideContainer.anchoredPosition = Vector2.Lerp(slideContainer.anchoredPosition, Vector2.zero, time / duration);
            yield return null;
        }

        isAnimating = false;
    }
}