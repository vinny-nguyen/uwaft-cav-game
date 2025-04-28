// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;
// using System.Collections.Generic;
// using Image = UnityEngine.UI.Image;

// public class SlideManager : MonoBehaviour
// {
//     [Header("Core References")]
//     public TextMeshProUGUI slideTitle;
//     public TextMeshProUGUI slideText;
//     public RectTransform slideContainer;
//     public RectTransform viewport;
//     public Button leftArrowButton;
//     public Button rightArrowButton;
//     public PlayerMovement playerMovement;

//     [Header("Quiz Settings")]
//     public GameObject quizPanel;
//     public TextMeshProUGUI questionText;
//     public Transform answersParent;
//     public GameObject answerButtonPrefab;
//     public Color normalColor = Color.white;
//     public Color correctColor = Color.green;
//     public Color wrongColor = Color.red;
//     public float wrongAnswerShakeIntensity = 10f;

//     [Header("Quiz Navigation")]
//     public float wrongAnswerShakeDuration = 0.5f;
//     public float wrongAnswerShakeMagnitude = 10f;
//     private bool[] answeredQuestions;

//     [Header("Indicators")]
//     public Transform contentIndicatorsParent; // Left side
//     public Transform quizIndicatorsParent;    // Right side
//     public GameObject indicatorPrefab;        // Shared prefab
//     public GameObject lightCirclePrefab;     // For active state reference

//     [Header("Animation Settings")]
//     public float slideAnimationDuration = 0.2f;

//     [Header("Completion UI")]
//     public GameObject congratsPanel;
//     public TextMeshProUGUI congratsText;
//     public Button completeNodeButton; // Make sure this is assigned

//     private int savedQuestionIndex = 0;
//     private bool wasInQuizMode = false;

//     [Header("Debug")]
//     public bool enableDebugLogging = true;

//     private bool _returningFromContent = false;
//     private int _lastQuizQuestionIndex = 0;
//     private bool _isViewingContent = false;
//     private int _lastQuizPosition = 0;

//     // Private state
//     private List<GameObject> contentIndicators = new List<GameObject>();
//     private List<GameObject> quizIndicators = new List<GameObject>();
//     private List<Button> activeAnswerButtons = new List<Button>();
//     private bool isAnimating = false;
//     private bool isInQuizMode = false;
//     private int currentTopicIndex = 0;
//     private int currentSlideIndex = 0;
//     private int currentQuestionIndex = 0;

//     [System.Serializable]
//     public class Slide
//     {
//         public string content;
//     }

//     [System.Serializable]
//     public class QuizQuestion
//     {
//         public string question;
//         public string[] answers; // answers[0] is correct
//     }

//     [System.Serializable]
//     public class Topic
//     {
//         public string topicName;
//         public List<Slide> learningSlides = new List<Slide>();
//         public List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
//         [HideInInspector] public List<bool> answeredCorrectly;
//     }

//     private List<Topic> topics = new List<Topic>();

//     void Start()
//     {
//         congratsPanel.SetActive(false);
//         completeNodeButton.gameObject.SetActive(false);
//         InitializeSlideContent();
//         leftArrowButton.onClick.AddListener(ShowPrevious);
//         rightArrowButton.onClick.AddListener(ShowNext);
//         completeNodeButton.onClick.AddListener(CompleteCurrentNode);

//     }

//     void InitializeSlideContent()
//     {
//         // Topic 1: Tires
//         Topic tires = new Topic
//         {
//             topicName = "Tires",
//             learningSlides = {
//             new Slide { content = "Tires are rubber components mounted on wheels, designed to provide traction, support, and cushioning between a vehicle and the road." },
//             new Slide { content = "Functions of Tires:\n- Traction & Grip\n- Shock Absorption\n- Load Support\n- Steering & Stability\n- Fuel Efficiency" },
//             new Slide { content = "Main types of tires:\n- All-season tires\n- Summer tires\n- Winter tires\n- Off-road tires\n- Touring Tires" },
//             new Slide { content = "Key parts of a tire:\n- Tread\n- Sipes\n- Grooves\n- Sidewall\n- Shoulder\n- Bead\n- Inner Liner\n- Belts" },
//             new Slide { content = "Rolling resistance is the force that opposes the motion of a tire as it rolls on a surface. It accounts for 5-15% of a vehicle's total energy consumption." }
//         },
//             quizQuestions = {
//             new QuizQuestion {
//                 question = "Properly inflated tires:",
//                 answers = new string[] {
//                     "Improve efficiency and safety of the car",
//                     "Improve colour and smell of the car",
//                     "Don't matter",
//                     "Only affect winter performance"
//                 }
//             },
//             new QuizQuestion {
//                 question = "How much total vehicle energy consumption does rolling resistance account for?",
//                 answers = new string[] {
//                     "5-15%",
//                     "1-3%",
//                     "50-54%",
//                     "25-30%"
//                 }
//             },
//             new QuizQuestion {
//                 question = "What is NOT a function of tires?",
//                 answers = new string[] {
//                     "Generating electricity",
//                     "Shock absorption",
//                     "Load support",
//                     "Providing traction"
//                 }
//             },
//             new QuizQuestion {
//                 question = "Low rolling resistance tires reduce energy use in:",
//                 answers = new string[] {
//                     "Both gas and electric cars",
//                     "Only gas cars",
//                     "Only electric cars",
//                     "Neither type of car"
//                 }
//             }
//         }
//         };
//         tires.answeredCorrectly = new List<bool>(new bool[tires.quizQuestions.Count]);
//         topics.Add(tires);

//         // Topic 2: Aerodynamics
//         Topic aero = new Topic
//         {
//             topicName = "Aerodynamics",
//             learningSlides = {
//             new Slide { content = "Drag is a force that pushes against something moving through air or water. We measure it using the drag coefficient (Cd)." },
//             new Slide { content = "Why aerodynamics matter for EVs:\n- Less drag means less energy needed\n- Smoother shape helps car slide through air\n- EVs can have sealed front ends for better aerodynamics" },
//             new Slide { content = "Efficiency boost:\nReducing drag by 10% can improve energy efficiency by up to 5%!" },
//             new Slide { content = "Design tricks to reduce drag:\n- Smooth, flat underbodies\n- Tapered backs\n- Smaller or hidden grilles\n- Pop-out door handles" },
//             new Slide { content = "EVs are typically more aerodynamic than gas cars because:\n- Electric motors generate less waste heat\n- Don't need large air intakes\n- Can have smoother front ends" }
//         },
//             quizQuestions = {
//             new QuizQuestion {
//                 question = "What is Drag?",
//                 answers = new string[] {
//                     "A force that pushes against something moving through air",
//                     "The name of a specific part of the car",
//                     "The ability to create a car that is very light",
//                     "A type of makeup"
//                 }
//             },
//             new QuizQuestion {
//                 question = "The less drag, the ______ the car needs to move.",
//                 answers = new string[] {
//                     "Less energy",
//                     "More energy",
//                     "More time",
//                     "More gas"
//                 }
//             },
//             new QuizQuestion {
//                 question = "If you reduce drag by 10%, it can improve energy efficiency by up to:",
//                 answers = new string[] {
//                     "5%",
//                     "10%",
//                     "1%",
//                     "50%"
//                 }
//             },
//             new QuizQuestion {
//                 question = "Why are EVs typically more aerodynamic than gas cars?",
//                 answers = new string[] {
//                     "They generate less waste heat and need smaller air intakes",
//                     "They are required by law to be more aerodynamic",
//                     "They are always smaller in size",
//                     "They don't have windows"
//                 }
//             }
//         }
//         };
//         aero.answeredCorrectly = new List<bool>(new bool[aero.quizQuestions.Count]);
//         topics.Add(aero);

//         // Topic 3: Suspension
//         Topic suspension = new Topic
//         {
//             topicName = "Suspension",
//             learningSlides = {
//             new Slide { content = "Suspension connects the wheels to the vehicle and helps maintain a smooth and safe ride." },
//             new Slide { content = "Components of suspension:\n- Springs\n- Shock absorbers\n- Control arms and joints\n- Anti-roll bars" },
//             new Slide { content = "How suspension affects cars:\n1. Absorbs bumps\n2. Keeps tires on the road\n3. Supports handling and steering" },
//             new Slide { content = "Suspension affects energy use by:\n- Keeping wheels in contact with ground\n- Reducing energy loss from bouncing\n- Modern adaptive systems optimize energy use" },
//             new Slide { content = "Real-world innovations:\n- Air suspension (like in GM Cadillac)\n- Regenerative suspension systems (in development)" }
//         },
//             quizQuestions = {
//             new QuizQuestion {
//                 question = "What is suspension?",
//                 answers = new string[] {
//                     "The part that connects wheels to the vehicle",
//                     "A type of engine component",
//                     "A braking system",
//                     "An aerodynamic feature"
//                 }
//             },
//             new QuizQuestion {
//                 question = "Which component is NOT part of a suspension system?",
//                 answers = new string[] {
//                     "Steering wheel",
//                     "Springs",
//                     "Shock absorbers",
//                     "Anti-roll bars"
//                 }
//             },
//             new QuizQuestion {
//                 question = "Complete: Poor suspension wastes ______, making the car less ______.",
//                 answers = new string[] {
//                     "Energy, efficient",
//                     "Money, expensive",
//                     "Time, valuable",
//                     "Air, aerodynamic"
//                 }
//             },
//             new QuizQuestion {
//                 question = "What is NOT an example of suspension innovation?",
//                 answers = new string[] {
//                     "Tomatoes",
//                     "Air suspension",
//                     "Regenerative braking",
//                     "Adaptive suspension"
//                 }
//             }
//         }
//         };
//         suspension.answeredCorrectly = new List<bool>(new bool[suspension.quizQuestions.Count]);
//         topics.Add(suspension);

//         // Topic 4: Electric Motors
//         Topic motors = new Topic
//         {
//             topicName = "Electric Motors",
//             learningSlides = {
//             new Slide { content = "An electric motor turns electricity into motion by using electromagnetism." },
//             new Slide { content = "How it works:\n1. Electricity flows into motor\n2. Creates magnetic field\n3. Interacts with magnets\n4. Causes rotor to spin\n5. Turns wheels directly" },
//             new Slide { content = "Efficiency comparison:\n- Gas engines: 25-30% efficient\n- Electric motors: 90-95% efficient" },
//             new Slide { content = "Torque is a measure of rotational force. EVs provide instant torque, resulting in quick acceleration from stop." },
//             new Slide { content = "Key advantages:\n- Much more efficient than gas engines\n- Instant torque for quick acceleration\n- Zero emissions" }
//         },
//             quizQuestions = {
//             new QuizQuestion {
//                 question = "What is an electric motor?",
//                 answers = new string[] {
//                     "A machine that turns electricity into motion",
//                     "A type of battery",
//                     "A suspension component",
//                     "A braking system"
//                 }
//             },
//             new QuizQuestion {
//                 question = "Which statement about electromagnetism is FALSE?",
//                 answers = new string[] {
//                     "Electric vehicles don't need electromagnetism",
//                     "Moving electric charges create magnetic fields",
//                     "Changing magnetic fields can create electric current",
//                     "It's the interaction between electricity and magnetism"
//                 }
//             },
//             new QuizQuestion {
//                 question = "Complete: Gas engines are 20-30% efficient, electric motors are ______.",
//                 answers = new string[] {
//                     "90-95% efficient",
//                     "10% efficient",
//                     "50% efficient",
//                     "5% efficient"
//                 }
//             },
//             new QuizQuestion {
//                 question = "What is torque?",
//                 answers = new string[] {
//                     "A measure of rotational force",
//                     "How fast a car can go from stop to speed",
//                     "When there is no twisting",
//                     "A type of energy"
//                 }
//             }
//         }
//         };
//         motors.answeredCorrectly = new List<bool>(new bool[motors.quizQuestions.Count]);
//         topics.Add(motors);

//         // Topic 5: Battery Pack
//         Topic battery = new Topic
//         {
//             topicName = "Battery Pack",
//             learningSlides = {
//             new Slide { content = "A battery pack is the EV's energy source, storing electric energy to power the motor." },
//             new Slide { content = "Inside a battery pack:\n- Cells (small individual batteries)\n- Modules (groups of cells)\n- Cooling system\n- Wiring\n- Protective casing" },
//             new Slide { content = "How EV batteries work:\n- Use lithium ions storing chemical potential energy\n- Energy measured in kilowatt-hours (kWh)" },
//             new Slide { content = "Charging levels:\n1. Level 1 (120V) - slowest (40+ hours)\n2. Level 2 (240V) - faster (4-12 hours)\n3. Level 3 (400V+) - fastest (20-40 mins)" },
//             new Slide { content = "Battery degradation:\n- Lose 2-3% capacity per year\n- Last 10-15 years\n- Can be recycled for materials" }
//         },
//             quizQuestions = {
//             new QuizQuestion {
//                 question = "What is a battery pack?",
//                 answers = new string[] {
//                     "The EV's energy source",
//                     "A storage for wheels",
//                     "The gas tank",
//                     "The suspension"
//                 }
//             },
//             new QuizQuestion {
//                 question = "Which statement is FALSE?",
//                 answers = new string[] {
//                     "Electric vehicles don't need battery packs",
//                     "Battery packs contain cells and modules",
//                     "Packs include cooling systems",
//                     "You can think of it like LEGO blocks"
//                 }
//             },
//             new QuizQuestion {
//                 question = "How is energy measured?",
//                 answers = new string[] {
//                     "Kilowatt-hours (kWh)",
//                     "Joules",
//                     "Volts",
//                     "Amps"
//                 }
//             },
//             new QuizQuestion {
//                 question = "How many charging levels are there?",
//                 answers = new string[] {
//                     "3",
//                     "1",
//                     "2",
//                     "4"
//                 }
//             }
//         }
//         };
//         battery.answeredCorrectly = new List<bool>(new bool[battery.quizQuestions.Count]);
//         topics.Add(battery);

//         // Topic 6: Regenerative Braking
//         Topic regenBraking = new Topic
//         {
//             topicName = "Regenerative Braking",
//             learningSlides = {
//             new Slide { content = "Regenerative braking captures energy when slowing down and sends it back to the battery instead of wasting it as heat." },
//             new Slide { content = "How it works:\n1. Electric motor runs in reverse\n2. Acts like a generator\n3. Turns kinetic energy into electrical energy\n4. Stores energy in battery" },
//             new Slide { content = "Energy recovery:\n- Can recover up to 70% of braking energy\n- Increases range by 20-30% in city driving" },
//             new Slide { content = "One-pedal driving:\n- Lets you accelerate and slow with just accelerator pedal\n- Reduces brake wear\n- Often comes to full stop without brakes" },
//             new Slide { content = "Key benefits:\n- Extends range\n- Reduces brake wear\n- Important for maximizing energy efficiency" }
//         },
//             quizQuestions = {
//             new QuizQuestion {
//                 question = "What is regenerative braking?",
//                 answers = new string[] {
//                     "A system that captures braking energy for the battery",
//                     "Constant braking",
//                     "A type of drag",
//                     "Only for combustion cars"
//                 }
//             },
//             new QuizQuestion {
//                 question = "How much energy can be recovered?",
//                 answers = new string[] {
//                     "Up to 70%",
//                     "Up to 10%",
//                     "None",
//                     "Up to 50%"
//                 }
//             },
//             new QuizQuestion {
//                 question = "What is NOT true about one-pedal driving?",
//                 answers = new string[] {
//                     "It uses two pedals",
//                     "It reduces brake wear",
//                     "It lets you slow with just accelerator",
//                     "It can bring car to full stop"
//                 }
//             },
//             new QuizQuestion {
//                 question = "Which statement is FALSE?",
//                 answers = new string[] {
//                     "EVs use more gas",
//                     "Regenerative braking converts energy to battery power",
//                     "EVs use it to extend range",
//                     "Controlled braking maximizes efficiency"
//                 }
//             }
//         }
//         };
//         regenBraking.answeredCorrectly = new List<bool>(new bool[regenBraking.quizQuestions.Count]);
//         topics.Add(regenBraking);

//         CreateIndicators();
//         UpdateDisplay();
//     }

//     public void SetCurrentTopic(int nodeIndex)
//     {
//         currentTopicIndex = nodeIndex;
//         currentSlideIndex = 0;
//         isInQuizMode = false;
//         CreateIndicators();
//         UpdateDisplay();
//     }

//     void CreateIndicators()
//     {
//         // Clear old
//         foreach (Transform child in contentIndicatorsParent) Destroy(child.gameObject);
//         foreach (Transform child in quizIndicatorsParent) Destroy(child.gameObject);
//         contentIndicators.Clear();
//         quizIndicators.Clear();

//         // Create new
//         for (int i = 0; i < topics[currentTopicIndex].learningSlides.Count; i++)
//             contentIndicators.Add(Instantiate(indicatorPrefab, contentIndicatorsParent));

//         for (int i = 0; i < topics[currentTopicIndex].quizQuestions.Count; i++)
//             quizIndicators.Add(Instantiate(indicatorPrefab, quizIndicatorsParent));
//     }

//     void UpdateDisplay()
//     {
//         if (enableDebugLogging) Debug.Log($"UpdateDisplay - isInQuizMode: {isInQuizMode}, currentSlideIndex: {currentSlideIndex}, currentQuestionIndex: {currentQuestionIndex}");

//         completeNodeButton.gameObject.SetActive(false);

//         if (isInQuizMode)
//         {
//             if (enableDebugLogging) Debug.Log("Showing quiz mode");
//             quizPanel.SetActive(true);
//             slideContainer.gameObject.SetActive(false);
//             ShowQuizQuestion();
//         }
//         else
//         {
//             if (enableDebugLogging) Debug.Log("Showing content mode");
//             quizPanel.SetActive(false);
//             slideContainer.gameObject.SetActive(true);
//             slideTitle.text = topics[currentTopicIndex].topicName;
//             slideText.text = topics[currentTopicIndex].learningSlides[currentSlideIndex].content;
//         }
//         UpdateIndicators();
//     }

//     void UpdateIndicators()
//     {
//         // Content indicators (left)
//         for (int i = 0; i < contentIndicators.Count; i++)
//         {
//             bool isActive = !isInQuizMode && (i == currentSlideIndex);
//             SetIndicatorState(contentIndicators[i], isActive);
//         }

//         // Quiz indicators (right)
//         for (int i = 0; i < quizIndicators.Count; i++)
//         {
//             bool isActive = isInQuizMode && (i == currentQuestionIndex);
//             SetIndicatorState(quizIndicators[i], isActive);
//         }
//     }

//     void SetIndicatorState(GameObject indicator, bool isActive)
//     {
//         Image img = indicator.GetComponent<Image>();
//         img.sprite = isActive ? lightCirclePrefab.GetComponent<Image>().sprite :
//                               indicatorPrefab.GetComponent<Image>().sprite;

//         if (isActive) StartCoroutine(PulseIndicator(indicator.transform));
//         else indicator.transform.localScale = Vector3.one;
//     }

//     void ShowPrevious()
//     {
//         if (isAnimating) return;

//         if (isInQuizMode)
//         {
//             StartCoroutine(TransitionToContent());
//         }
//         else if (currentSlideIndex > 0)
//         {
//             currentSlideIndex--;
//             StartCoroutine(SlideAnimation(-1));
//         }
//     }

//     void ShowNext()
//     {
//         if (isAnimating) return;

//         if (isInQuizMode)
//         {
//             if (!answeredQuestions[currentQuestionIndex])
//             {
//                 StartCoroutine(ShakeAndHighlight(rightArrowButton.transform));
//                 return;
//             }

//             if (currentQuestionIndex < topics[currentTopicIndex].quizQuestions.Count - 1)
//             {
//                 currentQuestionIndex++;
//                 StartCoroutine(SlideAnimation(1));
//             }
//             else
//             {
//                 CompleteQuiz();
//             }
//         }
//         else if (currentSlideIndex < topics[currentTopicIndex].learningSlides.Count - 1)
//         {
//             currentSlideIndex++;
//             StartCoroutine(SlideAnimation(1));
//         }
//         else
//         {
//             StartQuiz();
//         }
//     }


//     IEnumerator SlideAnimation(int direction)
//     {
//         isAnimating = true;
//         float slideDistance = viewport.rect.width;
//         Vector2 startPos = Vector2.zero;
//         Vector2 endPos = new Vector2(-direction * slideDistance, 0);

//         // Determine which object to move
//         RectTransform movingObject = isInQuizMode ?
//             questionText.rectTransform :
//             slideContainer;

//         // Slide out
//         float time = 0f;
//         while (time < 0.2f)
//         {
//             time += Time.deltaTime;
//             movingObject.anchoredPosition = Vector2.Lerp(startPos, endPos, time / 0.2f);
//             yield return null;
//         }

//         // Update content
//         if (isInQuizMode)
//         {
//             ShowQuizQuestion();
//         }
//         else
//         {
//             UpdateDisplay();
//         }

//         // Slide in
//         movingObject.anchoredPosition = new Vector2(direction * slideDistance, 0);
//         time = 0f;
//         while (time < 0.2f)
//         {
//             time += Time.deltaTime;
//             movingObject.anchoredPosition = Vector2.Lerp(movingObject.anchoredPosition, startPos, time / 0.2f);
//             yield return null;
//         }

//         UpdateIndicators();
//         isAnimating = false;
//     }

//     void StartQuiz()
//     {
//         if (!isAnimating && topics != null && currentTopicIndex < topics.Count)
//         {
//             // Only reset position if starting fresh
//             if (!_isViewingContent)
//             {
//                 _lastQuizPosition = 0;
//                 currentQuestionIndex = 0;
//             }
//             StartCoroutine(TransitionToQuiz());
//         }
//     }

//     void ShowQuizQuestion()
//     {
//         // Clear old buttons
//         foreach (Button btn in activeAnswerButtons) Destroy(btn.gameObject);
//         activeAnswerButtons.Clear();

//         // Set question
//         var question = topics[currentTopicIndex].quizQuestions[currentQuestionIndex];
//         questionText.text = question.question;

//         // Create answers
//         for (int i = 0; i < question.answers.Length; i++)
//         {
//             GameObject btnObj = Instantiate(answerButtonPrefab, answersParent);
//             btnObj.GetComponentInChildren<TextMeshProUGUI>().text = question.answers[i];
//             Button btn = btnObj.GetComponent<Button>();
//             int index = i;
//             btn.onClick.AddListener(() => OnAnswerSelected(index));
//             activeAnswerButtons.Add(btn);
//         }

//         rightArrowButton.interactable = answeredQuestions[currentQuestionIndex];
//         UpdateNavigationButtons();

//     }

//     void OnAnswerSelected(int selectedIndex)
//     {
//         var question = topics[currentTopicIndex].quizQuestions[currentQuestionIndex];
//         bool isCorrect = (selectedIndex == 0);

//         // Visual feedback
//         Image buttonImg = activeAnswerButtons[selectedIndex].GetComponent<Image>();
//         buttonImg.color = isCorrect ? correctColor : wrongColor;

//         if (isCorrect)
//         {
//             answeredQuestions[currentQuestionIndex] = true;
//             _returningFromContent = false; // Clear return flag
//             UpdateNavigationButtons();
//             StartCoroutine(NextQuestionAfterDelay(1f));
//         }
//         else
//         {
//             StartCoroutine(ResetButtonColor(buttonImg, 1f));
//             // Wrong answer - don't modify navigation state
//         }
//     }

//     IEnumerator ShakeButton(Transform button)
//     {
//         Vector3 originalPos = button.localPosition;
//         float elapsed = 0f;

//         while (elapsed < 0.5f)
//         {
//             elapsed += Time.deltaTime;
//             float x = originalPos.x + UnityEngine.Random.Range(-1f, 1f) * wrongAnswerShakeIntensity;
//             float y = originalPos.y + UnityEngine.Random.Range(-1f, 1f) * wrongAnswerShakeIntensity;
//             button.localPosition = new Vector3(x, y, originalPos.z);
//             yield return null;
//         }
//         button.localPosition = originalPos;
//     }

//     IEnumerator ShakeAndHighlight(Transform target)
//     {
//         Image buttonImage = target.GetComponent<Image>();
//         Color originalColor = buttonImage.color;
//         Vector3 originalPos = target.localPosition;
//         float elapsed = 0f;

//         // Flash red and shake
//         while (elapsed < wrongAnswerShakeDuration)
//         {
//             elapsed += Time.deltaTime;
//             float progress = elapsed / wrongAnswerShakeDuration;

//             // Shake effect
//             float x = originalPos.x + Mathf.Sin(Time.time * 50f) * wrongAnswerShakeMagnitude;
//             float y = originalPos.y + Mathf.Sin(Time.time * 45f) * wrongAnswerShakeMagnitude * 0.7f;
//             target.localPosition = new Vector3(x, y, originalPos.z);

//             // Color pulse
//             buttonImage.color = Color.Lerp(originalColor, wrongColor, Mathf.PingPong(progress * 2f, 1f));

//             yield return null;
//         }

//         // Reset to original state
//         target.localPosition = originalPos;
//         buttonImage.color = originalColor;
//     }

//     IEnumerator ResetButtonColor(Image buttonImg, float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         buttonImg.color = normalColor;
//     }

//     IEnumerator NextQuestionAfterDelay(float delay)
//     {
//         yield return new WaitForSeconds(delay);

//         if (currentQuestionIndex < topics[currentTopicIndex].quizQuestions.Count - 1)
//         {
//             currentQuestionIndex++;
//             ShowQuizQuestion();
//             UpdateIndicators();
//         }
//         else
//         {
//             CompleteQuiz();
//         }
//     }

//     void CompleteQuiz()
//     {
//         // Hide quiz elements
//         quizPanel.SetActive(false);
//         slideContainer.gameObject.SetActive(false);

//         // Show congratulations
//         congratsPanel.SetActive(true);
//         congratsText.text = $"Congratulations!\nYou've completed the {topics[currentTopicIndex].topicName} module!";

//         // Position complete button
//         completeNodeButton.gameObject.SetActive(true);
//         completeNodeButton.transform.SetParent(congratsPanel.transform, false);
//         completeNodeButton.onClick.RemoveAllListeners();
//         completeNodeButton.onClick.AddListener(CompleteCurrentNode);
//     }

//     void CompleteCurrentNode()
//     {
//         // Hide congratulations panel
//         congratsPanel.SetActive(false);

//         // Complete the node (existing functionality)
//         playerMovement.CompleteNode(playerMovement.CurrentNode);
//         FindAnyObjectByType<PopupManager>()?.ClosePopup();

//         // Reset quiz state
//         _isViewingContent = false;
//         _lastQuizPosition = 0;
//     }


//     IEnumerator PulseIndicator(Transform indicator)
//     {
//         Vector3 originalScale = indicator.localScale;
//         float elapsed = 0f;

//         while (elapsed < 0.8f)
//         {
//             elapsed += Time.deltaTime;
//             float t = Mathf.PingPong(elapsed * 2f, 1f);
//             indicator.localScale = originalScale * (1f + t * 0.2f);
//             yield return null;
//         }
//         indicator.localScale = originalScale;
//     }

//     IEnumerator TransitionToContent()
//     {
//         isAnimating = true;
//         _lastQuizPosition = currentQuestionIndex; // Remember where we were
//         _isViewingContent = true;

//         // Slide out animation
//         float duration = slideAnimationDuration;
//         float time = 0f;
//         Vector2 startPos = Vector2.zero;
//         Vector2 endPos = new Vector2(viewport.rect.width, 0);

//         while (time < duration)
//         {
//             time += Time.deltaTime;
//             slideContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, time / duration);
//             yield return null;
//         }

//         // Switch to content
//         isInQuizMode = false;
//         currentSlideIndex = topics[currentTopicIndex].learningSlides.Count - 1;
//         slideContainer.anchoredPosition = new Vector2(-viewport.rect.width, 0);
//         UpdateDisplay();

//         // Slide in content
//         time = 0f;
//         while (time < duration)
//         {
//             time += Time.deltaTime;
//             slideContainer.anchoredPosition = Vector2.Lerp(slideContainer.anchoredPosition, Vector2.zero, time / duration);
//             yield return null;
//         }

//         UpdateNavigationButtons();
//         isAnimating = false;
//     }

//     // 2. QUIZ TRANSITION (Restore position)
//     IEnumerator TransitionToQuiz()
//     {
//         isAnimating = true;
//         _isViewingContent = false;

//         // Slide out content
//         float duration = slideAnimationDuration;
//         float time = 0f;
//         Vector2 startPos = Vector2.zero;
//         Vector2 endPos = new Vector2(-viewport.rect.width, 0);

//         while (time < duration)
//         {
//             time += Time.deltaTime;
//             slideContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, time / duration);
//             yield return null;
//         }

//         // Restore quiz position
//         isInQuizMode = true;
//         currentQuestionIndex = _lastQuizPosition; // Return to where we left off

//         // Initialize answer tracking if needed
//         if (answeredQuestions == null || answeredQuestions.Length != topics[currentTopicIndex].quizQuestions.Count)
//         {
//             answeredQuestions = new bool[topics[currentTopicIndex].quizQuestions.Count];
//         }

//         slideContainer.anchoredPosition = new Vector2(viewport.rect.width, 0);
//         UpdateDisplay();

//         // Slide in quiz
//         time = 0f;
//         while (time < duration)
//         {
//             time += Time.deltaTime;
//             slideContainer.anchoredPosition = Vector2.Lerp(slideContainer.anchoredPosition, Vector2.zero, time / duration);
//             yield return null;
//         }

//         UpdateNavigationButtons();
//         isAnimating = false;
//     }

//     // 3. BUTTON CONTROL (Simple rules)
//     void UpdateNavigationButtons()
//     {
//         if (leftArrowButton == null || rightArrowButton == null) return;

//         leftArrowButton.interactable = true;

//         rightArrowButton.interactable =
//             _isViewingContent || // Always enabled in content
//             (answeredQuestions != null &&
//              currentQuestionIndex < answeredQuestions.Length &&
//              answeredQuestions[currentQuestionIndex]); // Only enabled if answered
//     }
// }