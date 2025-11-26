using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Nodemap.UI;
using UWAFT.UI.Hotspots;
using UnityEngine.Events;

namespace Nodemap.Controllers
{
        public class PopupController : MonoBehaviour
        {
            // Track completed minigame slides by index
            private HashSet<int> _completedMinigameSlides = new HashSet<int>();
        // MapConfig accessed via singleton - no need to assign
        private MapConfig mapConfig;

        [Header("Popup UI References")]
        public GameObject popupPanel;
        public GameObject backgroundOverlay;
        public Button nextSlideButton;
        public Button previousSlideButton;
        public Button closeButton;
        [SerializeField] private Transform slidesContainer;
        [SerializeField] private TMP_Text headerText;

        [Header("Indicators")]
        [SerializeField] private Transform slideIndicators;
        [SerializeField] private GameObject indicatorPrefab;
        [SerializeField] private SpriteRenderer activeIndicatorSprite;
        [SerializeField] private SpriteRenderer inactiveIndicatorSprite;

        [Header("Quiz References")]
        [SerializeField] private GameObject quizPrefab;
        [SerializeField] private Transform quizContainer;

        private readonly List<GameObject> slides = new();
        private readonly List<string> slideKeys = new(); // Store keys from SlideDeck
        private readonly List<GameObject> indicatorObjects = new();
        private int currentSlideIndex = 0;

        // Quiz mode state
        private bool isQuizMode = false;
        private GameObject currentQuizInstance;
        private NodeData currentNodeData;

        // Minigame completion tracking
        private bool _isMinigameSlide = false;
        private bool _minigameCompleted = false;
        // Hotspot group blocking (hotspots are not minigames)
        private HotspotGroup _hotspotGroup;

        void Awake()
        {
            // Initialize config if not assigned
            if (!mapConfig) mapConfig = MapConfig.Instance;

            nextSlideButton.onClick.AddListener(NextSlide);
            previousSlideButton.onClick.AddListener(PrevSlide);
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
            Hide();
        }

        // Configuration Helpers - Cleaner pattern with single method
        private T GetConfigValue<T>(System.Func<MapConfig, T> configGetter, T fallback)
        {
            return mapConfig ? configGetter(mapConfig) : fallback;
        }

        private float GetPopupFadeDuration() => GetConfigValue(c => c.popupFadeDuration, 0.3f);
        private float GetSlideTransitionDuration() => GetConfigValue(c => c.slideTransitionDuration, 0.2f);
        private string GetNodeSpriteFolder() => GetConfigValue(c => c.nodeSpriteFolder, "Sprites/Nodes");

        // Internal method to set header and create slides from slide objects
        private void SetupHeaderAndSlides(string header, List<GameObject> slideObjects, List<string> keys)
        {
            // Set header
            if (headerText != null)
                headerText.text = header;

            // Clear previous content first
            ClearSlides();

            // Add new slides and store their keys
            for (int i = 0; i < slideObjects.Count; i++)
            {
                var slide = Instantiate(slideObjects[i], slidesContainer);
                slide.SetActive(false);
                slides.Add(slide);

                // Store the key from SlideDeck
                if (keys != null && i < keys.Count)
                    slideKeys.Add(keys[i]);
                else
                    slideKeys.Add(""); // Fallback to empty if no key provided
            }

            // Initialize to first slide
            currentSlideIndex = 0;
            UpdateSlides();
            UpdateIndicators();
        }

        // Opens the popup with the specified node data and completion state
        public void Open(NodeData node, bool isCompleted)
        {
            if (!popupPanel || node == null) return;

            // Store node data for quiz mode
            currentNodeData = node;

            // Set header text
            string headerText = string.IsNullOrEmpty(node.title) ? "Lesson" : node.title;

            // Prepare slide objects and keys from the node's slide deck
            List<GameObject> slideObjects = new List<GameObject>();
            List<string> keys = new List<string>();
            if (node.slideDeck != null && node.slideDeck.slides != null)
            {
                foreach (var slideRef in node.slideDeck.slides)
                {
                    if (slideRef?.slidePrefab != null)
                    {
                        slideObjects.Add(slideRef.slidePrefab);
                        keys.Add(slideRef.key); // Store the key from SlideDeck
                    }
                }
            }

            // Setup all popup content
            SetupHeaderAndSlides(headerText, slideObjects, keys);

            // Log warning if no slides found (for debugging)
            if (slides.Count == 0)
            {
                Debug.LogWarning($"[PopupController] No slides found in SlideDeck for node: {node.name}");
            }

            // Ensure we're not in quiz mode when opening
            isQuizMode = false;

            // Show the popup
            Show();
        }

        // Shows the popup panel
        private void Show()
        {
            if (backgroundOverlay != null)
                backgroundOverlay.SetActive(true);
            popupPanel.SetActive(true);
        }

        // Hides the popup panel (public for close button functionality)
        public void Hide()
        {
            // Clean up quiz mode if active
            if (isQuizMode)
            {
                ExitQuizMode();
            }

            popupPanel.SetActive(false);
            if (backgroundOverlay != null)
                backgroundOverlay.SetActive(false);
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

        public void PrevSlide()
        {
            if (slides.Count == 0) return;
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
                bool active = (i == currentSlideIndex);
                if (slides[i]) slides[i].SetActive(active);

                // optional lifecycle hooks
                var sb = slides[i] ? slides[i].GetComponent<SlideBase>() : null;
                if (sb != null)
                {
                    if (active) sb.OnEnter();
                    else sb.OnExit();
                }
            }

            // Check if current slide has a minigame and update navigation
            CheckMinigameOnCurrentSlide();
        }

        public void JumpToSlideByKey(string key)
        {
            if (string.IsNullOrEmpty(key) || slides.Count == 0) return;

            // Search using the keys from SlideDeck instead of SlideBase component
            for (int i = 0; i < slideKeys.Count; i++)
            {
                if (slideKeys[i] == key)
                {
                    currentSlideIndex = i;
                    UpdateSlides();
                    UpdateIndicators();
                    return;
                }
            }
            Debug.LogWarning($"PopupController.JumpToSlideByKey: key '{key}' not found.");
        }

        private void ClearSlides()
        {
            foreach (var s in slides)
                if (s) Destroy(s);
            slides.Clear();
            slideKeys.Clear(); // Clear keys as well

            foreach (var dot in indicatorObjects)
                if (dot) Destroy(dot);
            indicatorObjects.Clear();
        }


        private void UpdateIndicators()
        {
            if (!indicatorPrefab || !slideIndicators) return;

            // rebuild dots
            foreach (var dot in indicatorObjects)
                if (dot) Destroy(dot);
            indicatorObjects.Clear();

            for (int i = 0; i < slides.Count; i++)
            {
                var dot = Instantiate(indicatorPrefab, slideIndicators);
                var spriteRenderer = dot.GetComponent<SpriteRenderer>();
                if (spriteRenderer)
                {
                    var sourceSprite = (i == currentSlideIndex) ? activeIndicatorSprite : inactiveIndicatorSprite;
                    if (sourceSprite != null && sourceSprite.sprite != null)
                        spriteRenderer.sprite = sourceSprite.sprite;
                }

                // Button already exists on prefab, just wire up the click event
                var button = dot.GetComponent<Button>();
                if (button != null)
                {
                    int slideIndex = i; // Capture for lambda
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => JumpToSlide(slideIndex));
                }

                indicatorObjects.Add(dot);
            }
        }

        // Jump to a specific slide by index
        private void JumpToSlide(int index)
        {
            if (index < 0 || index >= slides.Count) return;

            currentSlideIndex = index;
            UpdateSlides();
            UpdateIndicators();
        }

        // Check if the current slide contains a minigame and lock navigation accordingly
        private void CheckMinigameOnCurrentSlide()
        {
            _isMinigameSlide = false;
            // Check if this slide was already completed
            _minigameCompleted = _completedMinigameSlides.Contains(currentSlideIndex);

            if (currentSlideIndex < 0 || currentSlideIndex >= slides.Count)
            {
                UpdateNavigationButtons();
                return;
            }

            var currentSlide = slides[currentSlideIndex];
            if (currentSlide == null)
            {
                UpdateNavigationButtons();
                return;
            }

            // Check for any minigame controller types
            var wordUnscramble = currentSlide.GetComponentInChildren<WordUnscrambleController>();
            var memoryMatch = currentSlide.GetComponentInChildren<MemoryMatchController>();
            var dragDrop = currentSlide.GetComponentInChildren<DragDropController>();

            // Treat hotspot groups as blocking minigames when configured to require all clicks
            // Hotspot groups are NOT minigames; treat them as a separate progression blocker
            // Unsubscribe previous hotspot group if any
            if (_hotspotGroup != null)
            {
                _hotspotGroup.OnAllClicked.RemoveListener(OnHotspotGroupCompleted);
                _hotspotGroup = null;
            }

            var hotspotGroup = currentSlide.GetComponentInChildren<HotspotGroup>();
            if (hotspotGroup != null && hotspotGroup.RequireAllClicked)
            {
                _hotspotGroup = hotspotGroup;
                _hotspotGroup.OnAllClicked.RemoveListener(OnHotspotGroupCompleted);
                _hotspotGroup.OnAllClicked.AddListener(OnHotspotGroupCompleted);
            }

            if (wordUnscramble != null)
            {
                _isMinigameSlide = true;
                // Subscribe to completion event
                wordUnscramble.OnCompleted.RemoveListener(OnMinigameCompleted);
                wordUnscramble.OnCompleted.AddListener(OnMinigameCompleted);
            }
            else if (memoryMatch != null)
            {
                _isMinigameSlide = true;
                memoryMatch.OnCompleted.RemoveListener(OnMinigameCompleted);
                memoryMatch.OnCompleted.AddListener(OnMinigameCompleted);
            }
            else if (dragDrop != null)
            {
                _isMinigameSlide = true;
                dragDrop.OnCompleted.RemoveListener(OnMinigameCompleted);
                dragDrop.OnCompleted.AddListener(OnMinigameCompleted);
            }

            UpdateNavigationButtons();
        }

        // Called when a minigame is completed
        private void OnMinigameCompleted()
        {
            _minigameCompleted = true;
            // Mark this slide as completed
            if (!_completedMinigameSlides.Contains(currentSlideIndex))
                _completedMinigameSlides.Add(currentSlideIndex);
            UpdateNavigationButtons();
        }

        // Called when a hotspot group reports all hotspots viewed
        private void OnHotspotGroupCompleted()
        {
            UpdateNavigationButtons();
        }

        // Update navigation button states based on minigame completion
        private void UpdateNavigationButtons()
        {
            bool minigameLocked = (_isMinigameSlide && !_minigameCompleted);
            bool hotspotLocked = (_hotspotGroup != null && _hotspotGroup.RequireAllClicked && !_hotspotGroup.IsComplete);

            bool allowNext = !(minigameLocked || hotspotLocked) && (currentSlideIndex < slides.Count - 1);

            if (nextSlideButton != null) nextSlideButton.interactable = allowNext;
            if (previousSlideButton != null) previousSlideButton.interactable = (currentSlideIndex > 0);
        }

        #region Quiz Mode

        // Enter quiz mode - hide slide navigation and show quiz UI
        public void EnterQuizMode()
        {
            if (isQuizMode || currentNodeData == null)
            {
                Debug.LogWarning("[PopupController] Cannot enter quiz mode - already in quiz mode or no node data!");
                return;
            }

            if (currentNodeData.quizJson == null)
            {
                Debug.LogError($"[PopupController] No quiz JSON assigned for node: {currentNodeData.name}");
                return;
            }

            if (quizPrefab == null)
            {
                Debug.LogError("[PopupController] Quiz prefab not assigned in PopupController!");
                return;
            }

            // Hide slide navigation buttons and indicators
            if (nextSlideButton != null) nextSlideButton.gameObject.SetActive(false);
            if (previousSlideButton != null) previousSlideButton.gameObject.SetActive(false);
            if (slideIndicators != null) slideIndicators.gameObject.SetActive(false);

            // Hide all slides
            foreach (var slide in slides)
            {
                if (slide != null) slide.SetActive(false);
            }

            // Instantiate quiz UI
            Transform container = quizContainer != null ? quizContainer : slidesContainer;
            currentQuizInstance = Instantiate(quizPrefab, container);

            // Initialize quiz with data
            var quizController = currentQuizInstance.GetComponent<QuizController>();
            if (quizController != null)
            {
                quizController.Initialize(currentNodeData.quizJson, currentNodeData, currentNodeData.id);

                // Auto-wire completion event to QuizCompletionHandler
                var completionHandler = GameServices.Instance?.QuizCompletionHandler;
                if (completionHandler != null)
                {
                    quizController.OnQuizCompleted.AddListener(completionHandler.OnQuizCompleted);
                }
                else
                {
                    Debug.LogWarning("[PopupController] QuizCompletionHandler not found in GameServices! Quiz completion won't trigger node completion.");
                }
            }
            else
            {
                Debug.LogError("[PopupController] QuizController component not found on quiz prefab!");
            }

            isQuizMode = true;
        }

        // Exit quiz mode - restore slide navigation and destroy quiz UI
        public void ExitQuizMode()
        {
            if (!isQuizMode) return;

            // Destroy quiz instance
            if (currentQuizInstance != null)
            {
                Destroy(currentQuizInstance);
                currentQuizInstance = null;
            }

            // Restore slide navigation
            if (nextSlideButton != null) nextSlideButton.gameObject.SetActive(true);
            if (previousSlideButton != null) previousSlideButton.gameObject.SetActive(true);
            if (slideIndicators != null) slideIndicators.gameObject.SetActive(true);

            // Show current slide
            UpdateSlides();

            isQuizMode = false;
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up button listeners to prevent memory leaks
            if (nextSlideButton != null)
                nextSlideButton.onClick.RemoveListener(NextSlide);

            if (previousSlideButton != null)
                previousSlideButton.onClick.RemoveListener(PrevSlide);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(Hide);

            // Unsubscribe hotspot group listener if set
            if (_hotspotGroup != null)
            {
                _hotspotGroup.OnAllClicked.RemoveListener(OnHotspotGroupCompleted);
                _hotspotGroup = null;
            }
        }
    }
}
