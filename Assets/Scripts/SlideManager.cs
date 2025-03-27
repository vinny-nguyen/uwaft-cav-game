using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class SlideManager : MonoBehaviour
{
    public TextMeshProUGUI slideText; // Assign in Inspector
    public Button leftArrowButton;    // Assign in Inspector
    public Button rightArrowButton;  // Assign in Inspector
    public RectTransform slideContainer; // Assign in Inspector
    public float slideDuration = 0.5f;

    private List<List<string>> topicSlides = new List<List<string>>();
    private int currentTopicIndex = 0;
    private int currentSlideIndex = 0;
    private bool isAnimating = false;

    void Start()
    {
        InitializeSlideContent();
        UpdateSlideContent();

        // Set up button listeners
        leftArrowButton.onClick.AddListener(ShowPreviousSlide);
        rightArrowButton.onClick.AddListener(ShowNextSlide);
    }

    void InitializeSlideContent()
    {
        // Topic 1: Tires
        topicSlides.Add(new List<string>(){
            "Tires are the only contact point between the car and the road.",
            "Electric car tires are designed for low rolling resistance to maximize range.",
            "Special compounds handle the instant torque of electric motors.",
            "Tire pressure monitoring is crucial for efficiency and safety.",
            "Regenerative braking affects tire wear patterns differently."
        });

        // Topic 2: Aerodynamics
        topicSlides.Add(new List<string>(){
            "Aerodynamics significantly impact an EV's range at high speeds.",
            "Smooth underbody panels reduce drag and improve efficiency.",
            "Active grille shutters optimize cooling with minimal drag.",
            "Side mirrors are being replaced with cameras to reduce drag.",
            "Wind tunnel testing helps perfect the car's aerodynamic profile."
        });

        // Topic 3: Suspension
        topicSlides.Add(new List<string>(){
            "EV suspensions must handle heavy battery packs.",
            "Air suspensions are common to adjust for varying loads.",
            "Low center of gravity improves handling and stability.",
            "Regenerative braking affects suspension tuning requirements.",
            "Some EVs feature adaptive damping for different driving modes."
        });

        // Topic 4: Battery
        topicSlides.Add(new List<string>(){
            "Lithium-ion batteries are the current standard for EVs.",
            "Battery cooling systems prevent overheating and maintain efficiency.",
            "The battery pack is the heaviest single component in an EV.",
            "Charging speed decreases as the battery reaches full capacity.",
            "Battery management systems optimize performance and longevity."
        });

        // Topic 5: Electric Motors
        topicSlides.Add(new List<string>(){
            "EVs use AC induction or permanent magnet motors.",
            "Motors deliver instant torque for quick acceleration.",
            "Most EVs don't need multi-speed transmissions.",
            "Motor efficiency often exceeds 90%, far better than ICE engines.",
            "Some EVs use multiple motors for all-wheel drive."
        });

        // Topic 6: Regenerative Braking
        topicSlides.Add(new List<string>(){
            "Converts kinetic energy back into electrical energy.",
            "Reduces wear on traditional friction brakes.",
            "Can often enable 'one-pedal' driving in EVs.",
            "Effectiveness depends on battery state of charge.",
            "Different regeneration levels can usually be selected."
        });
    }

    public void SetCurrentTopic(int topicIndex)
    {
        currentTopicIndex = topicIndex;
        currentSlideIndex = 0;
        UpdateSlideContent();
    }

    void UpdateSlideContent()
    {
        if (slideText != null &&
            currentTopicIndex >= 0 && currentTopicIndex < topicSlides.Count &&
            currentSlideIndex >= 0 && currentSlideIndex < topicSlides[currentTopicIndex].Count)
        {
            slideText.text = topicSlides[currentTopicIndex][currentSlideIndex];
        }
    }

    public void ShowNextSlide()
    {
        if (!isAnimating && currentSlideIndex < topicSlides[currentTopicIndex].Count - 1)
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