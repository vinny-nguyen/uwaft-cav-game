using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class SlideManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI slideTitle;
    public TextMeshProUGUI slideText;
    public Button leftArrowButton;
    public Button rightArrowButton;
    public RectTransform slideContainer; // Assign EducationPanel here
    public float slideDuration = 0.5f;

    [Header("Completion Settings")]
    public Button completeNodeButton; // Drag your button in Inspector
    public PlayerMovement playerMovement; // Reference to your player script


    // NEW: Add Viewport reference
    public RectTransform viewport; // Assign the Viewport panel (child of BodyPanel)

    [System.Serializable]
    public class Slide { public string content; }

    [System.Serializable]
    public class Topic
    {
        public string topicName;
        public List<Slide> slides = new List<Slide>();
    }

    private List<Topic> topics = new List<Topic>();
    private int currentTopicIndex = 0;
    private int currentSlideIndex = 0;
    private bool isAnimating = false;

    void Start()
    {
        InitializeSlideContent();
        UpdateSlideContent();

        leftArrowButton.onClick.AddListener(() => {
            if (!isAnimating)
            {
                ShowPreviousSlide();
                StartCoroutine(ButtonPressEffect(leftArrowButton));
            }
        });

        rightArrowButton.onClick.AddListener(() => {
            if (!isAnimating)
            {
                ShowNextSlide();
                StartCoroutine(ButtonPressEffect(rightArrowButton));
            }
        });

        completeNodeButton.onClick.AddListener(CompleteCurrentNode);
        completeNodeButton.gameObject.SetActive(false);
    }

    // Moved outside of Start() as a regular class method
    IEnumerator ButtonPressEffect(Button btn)
    {
        RectTransform rt = btn.GetComponent<RectTransform>();
        Vector3 originalScale = rt.localScale;
        float duration = 0.15f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = EaseOutBack(time / duration);
            rt.localScale = originalScale * Mathf.Lerp(0.9f, 1.1f, t);
            yield return null;
        }
        rt.localScale = originalScale;
    }

    void InitializeSlideContent()
    {
        // Topic 1: Tires
        Topic tires = new Topic
        {
            topicName = "Tires",
            slides = {
            new Slide { content = "Tires are the only contact point between the car and the road." },
            new Slide { content = "Electric car tires are designed for low rolling resistance to maximize range." },
            new Slide { content = "Special compounds handle the instant torque of electric motors." },
            new Slide { content = "Tire pressure monitoring is crucial for efficiency and safety." },
            new Slide { content = "Regenerative braking affects tire wear patterns differently." }
        }
        };
        topics.Add(tires);

        // Topic 2: Aerodynamics
        Topic aero = new Topic
        {
            topicName = "Aerodynamics",
            slides = {
            new Slide { content = "Aerodynamics significantly impact an EV's range at high speeds." },
            new Slide { content = "Smooth underbody panels reduce drag and improve efficiency." },
            new Slide { content = "Active grille shutters optimize cooling with minimal drag." },
            new Slide { content = "Side mirrors are being replaced with cameras to reduce drag." },
            new Slide { content = "Wind tunnel testing helps perfect the car's aerodynamic profile." }
        }
        };
        topics.Add(aero);

        // Topic 3: Suspension
        Topic suspension = new Topic
        {
            topicName = "Suspension",
            slides = {
            new Slide { content = "EV suspensions must handle heavy battery packs." },
            new Slide { content = "Air suspensions are common to adjust for varying loads." },
            new Slide { content = "Low center of gravity improves handling and stability." },
            new Slide { content = "Regenerative braking affects suspension tuning requirements." },
            new Slide { content = "Some EVs feature adaptive damping for different driving modes." }
        }
        };
        topics.Add(suspension);

        // Topic 4: Battery
        Topic battery = new Topic
        {
            topicName = "Battery",
            slides = {
            new Slide { content = "Lithium-ion batteries are the current standard for EVs." },
            new Slide { content = "Battery cooling systems prevent overheating and maintain efficiency." },
            new Slide { content = "The battery pack is the heaviest single component in an EV." },
            new Slide { content = "Charging speed decreases as the battery reaches full capacity." },
            new Slide { content = "Battery management systems optimize performance and longevity." }
        }
        };
        topics.Add(battery);

        // Topic 5: Electric Motors
        Topic motors = new Topic
        {
            topicName = "Electric Motors",
            slides = {
            new Slide { content = "EVs use AC induction or permanent magnet motors." },
            new Slide { content = "Motors deliver instant torque for quick acceleration." },
            new Slide { content = "Most EVs don't need multi-speed transmissions." },
            new Slide { content = "Motor efficiency often exceeds 90%, far better than ICE engines." },
            new Slide { content = "Some EVs use multiple motors for all-wheel drive." }
        }
        };
        topics.Add(motors);

        // Topic 6: Regenerative Braking
        Topic regenBraking = new Topic
        {
            topicName = "Regenerative Braking",
            slides = {
            new Slide { content = "Converts kinetic energy back into electrical energy." },
            new Slide { content = "Reduces wear on traditional friction brakes." },
            new Slide { content = "Can often enable 'one-pedal' driving in EVs." },
            new Slide { content = "Effectiveness depends on battery state of charge." },
            new Slide { content = "Different regeneration levels can usually be selected." }
        }
        };
        topics.Add(regenBraking);
    }

    public void SetCurrentTopic(int topicIndex)
    {
        if (topicIndex >= 0 && topicIndex < topics.Count)
        {
            currentTopicIndex = topicIndex;
            currentSlideIndex = 0;
            UpdateSlideContent();

            if (slideContainer != null)
            {
                slideContainer.anchoredPosition = Vector2.zero;
            }
        }
    }

    void UpdateSlideContent()
    {
        if (currentTopicIndex >= 0 && currentTopicIndex < topics.Count &&
       currentSlideIndex >= 0 && currentSlideIndex < topics[currentTopicIndex].slides.Count)
        {
            // Always show the topic name as the title
            slideTitle.text = topics[currentTopicIndex].topicName;
            // Show slide-specific content
            slideText.text = topics[currentTopicIndex].slides[currentSlideIndex].content;

            bool isLastSlide = currentSlideIndex == topics[currentTopicIndex].slides.Count - 1;
            completeNodeButton.gameObject.SetActive(isLastSlide);
        }
    }

    public void ShowNextSlide()
    {
        if (!isAnimating && currentSlideIndex < topics[currentTopicIndex].slides.Count - 1)
        {
            StartCoroutine(SlideAnimation(1));
        }
    }

    public void ShowPreviousSlide()
    {
        if (!isAnimating && currentSlideIndex > 0)
        {
            StartCoroutine(SlideAnimation(-1));
        }
    }

    public void CompleteCurrentNode()
    {
        if (playerMovement != null &&
            currentSlideIndex == topics[currentTopicIndex].slides.Count - 1)
        {
            playerMovement.CompleteNode(playerMovement.CurrentNode); // Now using the property
            FindAnyObjectByType<PopupManager>()?.ClosePopup();
        }
    }

    IEnumerator SlideAnimation(int direction)
    {
        isAnimating = true;

        // Motion blur setup
        CanvasGroup slideCanvas = slideContainer.GetComponent<CanvasGroup>();
        if (slideCanvas == null) slideCanvas = slideContainer.gameObject.AddComponent<CanvasGroup>();

        // Animation setup
        float slideDistance = viewport.rect.width;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(-direction * slideDistance, 0);

        // Slide out current content
        float time = 0f;
        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = time / slideDuration;

            // Motion blur effect
            float blurAmount = Mathf.Abs(slideContainer.anchoredPosition.x) / slideDistance;
            slideCanvas.alpha = Mathf.Lerp(1f, 0.92f, blurAmount * 0.5f);

            // Replace ... with your actual Lerp parameters
            slideContainer.anchoredPosition = Vector2.Lerp(
                startPos,
                endPos,
                EaseOutCubic(t) // Using easing function for smoother motion
            );
            yield return null;
        }

        // Update content
        currentSlideIndex += direction;
        UpdateSlideContent();

        // Slide in new content
        slideContainer.anchoredPosition = new Vector2(direction * slideDistance, 0);
        time = 0f;
        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = time / slideDuration;

            // Motion blur effect
            float blurAmount = Mathf.Abs(slideContainer.anchoredPosition.x) / slideDistance;
            slideCanvas.alpha = Mathf.Lerp(1f, 0.92f, blurAmount * 0.5f);

            slideContainer.anchoredPosition = Vector2.Lerp(
                slideContainer.anchoredPosition,
                startPos,
                EaseOutElastic(t) // Different easing for slide-in
            );
            yield return null;
        }

        // Reset final state
        slideCanvas.alpha = 1f;
        slideContainer.anchoredPosition = startPos;
        isAnimating = false;
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3);
    }

    float EaseOutElastic(float t)
    {
        float c4 = (2f * Mathf.PI) / 3f;
        return t == 0f ? 0f :
               t == 1f ? 1f :
               Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }
}