using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class SlideManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI slideTitle;  // Assign a TMP text element for the title
    public TextMeshProUGUI slideText;
    public Button leftArrowButton;
    public Button rightArrowButton;
    public RectTransform slideContainer;
    public float slideDuration = 0.5f;

    [System.Serializable]
    public class Slide
    {
        public string title;
        public string content;
    }

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

        leftArrowButton.onClick.AddListener(ShowPreviousSlide);
        rightArrowButton.onClick.AddListener(ShowNextSlide);
    }

    void InitializeSlideContent()
    {
        // Topic 1: Tires
        Topic tires = new Topic
        {
            topicName = "Tires",
            slides = {
            new Slide { title = "Primary Contact Point", content = "Tires are the only contact point between the car and the road." },
            new Slide { title = "Low Rolling Resistance", content = "Electric car tires are designed for low rolling resistance to maximize range." },
            new Slide { title = "Torque Handling", content = "Special compounds handle the instant torque of electric motors." },
            new Slide { title = "Pressure Monitoring", content = "Tire pressure monitoring is crucial for efficiency and safety." },
            new Slide { title = "Regenerative Braking Effects", content = "Regenerative braking affects tire wear patterns differently." }
        }
        };
        topics.Add(tires);

        // Topic 2: Aerodynamics
        Topic aero = new Topic
        {
            topicName = "Aerodynamics",
            slides = {
            new Slide { title = "Range Impact", content = "Aerodynamics significantly impact an EV's range at high speeds." },
            new Slide { title = "Underbody Design", content = "Smooth underbody panels reduce drag and improve efficiency." },
            new Slide { title = "Active Grille Shutters", content = "Active grille shutters optimize cooling with minimal drag." },
            new Slide { title = "Camera Mirrors", content = "Side mirrors are being replaced with cameras to reduce drag." },
            new Slide { title = "Wind Tunnel Testing", content = "Wind tunnel testing helps perfect the car's aerodynamic profile." }
        }
        };
        topics.Add(aero);

        // Topic 3: Suspension
        Topic suspension = new Topic
        {
            topicName = "Suspension",
            slides = {
            new Slide { title = "Battery Load Capacity", content = "EV suspensions must handle heavy battery packs." },
            new Slide { title = "Air Suspension", content = "Air suspensions are common to adjust for varying loads." },
            new Slide { title = "Low Center of Gravity", content = "Low center of gravity improves handling and stability." },
            new Slide { title = "Braking System Effects", content = "Regenerative braking affects suspension tuning requirements." },
            new Slide { title = "Adaptive Damping", content = "Some EVs feature adaptive damping for different driving modes." }
        }
        };
        topics.Add(suspension);

        // Topic 4: Battery
        Topic battery = new Topic
        {
            topicName = "Battery",
            slides = {
            new Slide { title = "Lithium-Ion Standard", content = "Lithium-ion batteries are the current standard for EVs." },
            new Slide { title = "Cooling Systems", content = "Battery cooling systems prevent overheating and maintain efficiency." },
            new Slide { title = "Weight Consideration", content = "The battery pack is the heaviest single component in an EV." },
            new Slide { title = "Charging Speed", content = "Charging speed decreases as the battery reaches full capacity." },
            new Slide { title = "Management Systems", content = "Battery management systems optimize performance and longevity." }
        }
        };
        topics.Add(battery);

        // Topic 5: Electric Motors
        Topic motors = new Topic
        {
            topicName = "Electric Motors",
            slides = {
            new Slide { title = "Motor Types", content = "EVs use AC induction or permanent magnet motors." },
            new Slide { title = "Instant Torque", content = "Motors deliver instant torque for quick acceleration." },
            new Slide { title = "Simplified Transmissions", content = "Most EVs don't need multi-speed transmissions." },
            new Slide { title = "High Efficiency", content = "Motor efficiency often exceeds 90%, far better than ICE engines." },
            new Slide { title = "AWD Configurations", content = "Some EVs use multiple motors for all-wheel drive." }
        }
        };
        topics.Add(motors);

        // Topic 6: Regenerative Braking
        Topic regenBraking = new Topic
        {
            topicName = "Regenerative Braking",
            slides = {
            new Slide { title = "Energy Conversion", content = "Converts kinetic energy back into electrical energy." },
            new Slide { title = "Brake Wear Reduction", content = "Reduces wear on traditional friction brakes." },
            new Slide { title = "One-Pedal Driving", content = "Can often enable 'one-pedal' driving in EVs." },
            new Slide { title = "Battery Dependency", content = "Effectiveness depends on battery state of charge." },
            new Slide { title = "Adjustable Levels", content = "Different regeneration levels can usually be selected." }
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
            var currentSlide = topics[currentTopicIndex].slides[currentSlideIndex];
            slideTitle.text = currentSlide.title;
            slideText.text = currentSlide.content;
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

    IEnumerator SlideAnimation(int direction)
    {
        isAnimating = true;
        Vector2 startPos = slideContainer.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(-direction * slideContainer.rect.width, 0);

        // Slide out
        float time = 0f;
        while (time < slideDuration)
        {
            time += Time.deltaTime;
            slideContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, time / slideDuration);
            yield return null;
        }

        // Update content
        currentSlideIndex += direction;
        UpdateSlideContent();

        // Slide in from opposite direction
        slideContainer.anchoredPosition = startPos + new Vector2(direction * slideContainer.rect.width, 0);
        time = 0f;
        while (time < slideDuration)
        {
            time += Time.deltaTime;
            slideContainer.anchoredPosition = Vector2.Lerp(slideContainer.anchoredPosition, startPos, time / slideDuration);
            yield return null;
        }

        slideContainer.anchoredPosition = startPos;
        isAnimating = false;
    }
}