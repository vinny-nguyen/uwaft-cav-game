using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using NodeMap.UI;

namespace NodeMap
{
    /// <summary>
    /// Manages educational popups for nodes including slides and quizzes
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        #region Singleton
        public static PopupManager Instance { get; private set; }
        #endregion

        #region Inspector Fields
        [Header("UI References")]
        [SerializeField] private CanvasGroup popupCanvasGroup;
        [SerializeField] private Image backgroundOverlay;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private Button leftArrowButton;
        [SerializeField] private Button rightArrowButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private List<string> nodeHeaders;

        [Header("Content Containers")]
        [SerializeField] private GameObject slidesParent;
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private GameObject failurePanel;
        [SerializeField] private GameObject successPanel;

        [Header("Components")]
        [SerializeField] private SlideIndicatorManager indicatorManager;
        [SerializeField] private QuizManager quizManager;

        // Add this to the Inspector Fields region
        [Header("Popup Styles")]
        [SerializeField] private Image popupPanel; // Reference to the popup panel image
        [SerializeField] private Sprite normalPopupSprite; // Purple sprite (default)
        [SerializeField] private Sprite completedPopupSprite; // Green sprite for completed nodes
        #endregion

        #region Private Fields
        // Track current state
        private List<GameObject> currentNodeSlides = new List<GameObject>();
        private int currentSlideIndex = 0;
        private int lastSlideIndex = -1;
        private int openedNodeIndex = -1;

        // Mode tracking
        private bool inQuizMode = false;

        // UI state colors
        private Color enabledColor = Color.white;
        private Color disabledColor = new Color(1f, 1f, 1f, 0.4f);
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            SetupSingleton();
        }

        private void Start()
        {
            InitializeUI();
        }

        private void Update()
        {
            HandleKeyboardInput();
        }
        #endregion

        #region Initialization
        private void SetupSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            gameObject.SetActive(true);
        }

        private void InitializeUI()
        {
            // Start hidden
            popupCanvasGroup.alpha = 0f;
            popupCanvasGroup.interactable = false;
            popupCanvasGroup.blocksRaycasts = false;

            // Setup navigation buttons
            leftArrowButton.onClick.AddListener(PreviousSlide);
            rightArrowButton.onClick.AddListener(NextSlide);
            closeButton.onClick.AddListener(() => StartCoroutine(ClosePopup()));

            // Setup quiz manager events
            if (quizManager != null)
            {
                quizManager.OnQuizCompleted += HandleQuizCompleted;
            }
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                NextSlide();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PreviousSlide();
            }
        }
        #endregion

        #region Public Entry Points
        /// <summary>
        /// Opens a popup with content for the specified node
        /// </summary>
        public void OpenPopupForNode(int nodeIndex)
        {
            // Reset state
            inQuizMode = false;
            ResetPanels();

            // Track the opened node
            openedNodeIndex = nodeIndex;

            // Check if the node is completed and set appropriate panel sprite
            PlayerSplineMovement playerMover = FindFirstObjectByType<PlayerSplineMovement>();
            bool isNodeCompleted = false;

            if (playerMover != null)
            {
                // Convert to zero-based index for the player movement system
                int zeroBasedNodeIndex = nodeIndex - 1;
                isNodeCompleted = playerMover.IsNodeCompleted(zeroBasedNodeIndex);

                // Set the appropriate sprite based on completion status
                if (popupPanel != null)
                {
                    popupPanel.sprite = isNodeCompleted ? completedPopupSprite : normalPopupSprite;
                }
            }

            // Load node slides
            LoadNodeSlides(nodeIndex);

            // Set node header
            UpdateNodeHeader(nodeIndex);

            // Rest of existing code...
            // Initialize indicators
            if (indicatorManager != null)
            {
                indicatorManager.GenerateIndicators(currentNodeSlides.Count);
                indicatorManager.SetVisibility(true);
            }

            // Reset and show first slide
            lastSlideIndex = -1;
            currentSlideIndex = 0;
            UpdateArrows();
            ShowSlide(currentSlideIndex);

            // Animate open
            StartCoroutine(UIAnimator.AnimatePopupOpen(popupCanvasGroup, backgroundOverlay));
        }

        /// <summary>
        /// Closes the current popup
        /// </summary>
        public IEnumerator ClosePopup()
        {
            // Clean up current slides
            foreach (var slide in currentNodeSlides)
            {
                if (slide != null)
                    slide.SetActive(false);
            }

            // Reset state
            inQuizMode = false;
            currentNodeSlides.Clear();

            // Clear indicators
            if (indicatorManager != null)
            {
                indicatorManager.ClearIndicators();
            }

            // Animate close
            yield return StartCoroutine(UIAnimator.AnimatePopupClose(popupCanvasGroup, backgroundOverlay));
        }
        #endregion

        #region Slide Management
        /// <summary>
        /// Shows the slide at the specified index
        /// </summary>
        private void ShowSlide(int index)
        {
            // LogSlidesState(); // Debug log for slide state
            Debug.Log("Current Node Slides:");
            foreach (var slide in currentNodeSlides)
            {
                Debug.Log(slide != null ? slide.name : "NULL");
            }
            // Safety check
            if (currentNodeSlides.Count == 0 || index < 0 || index >= currentNodeSlides.Count)
            {
                Debug.LogWarning($"Attempted to show slide at invalid index {index}. Slide count: {currentNodeSlides.Count}");
                return;
            }

            // Hide previous slide if any
            if (lastSlideIndex >= 0 && lastSlideIndex < currentNodeSlides.Count && lastSlideIndex != index)
            {
                GameObject lastSlide = currentNodeSlides[lastSlideIndex];
                if (lastSlide != null)
                    StartCoroutine(UIAnimator.AnimateSlideOut(lastSlide));
                indicatorManager.UpdateActiveIndicator(currentSlideIndex, animate: true);
            }

            // Make sure target slide exists
            GameObject currentSlide = currentNodeSlides[index];
            if (currentSlide == null)
            {
                Debug.LogError($"Slide at index {index} is null!");
                return;
            }

            // Always ensure it's active first
            currentSlide.SetActive(true);

            CanvasGroup slidesParentCG = slidesParent.GetComponent<CanvasGroup>();
            if (slidesParentCG != null)
            {
                slidesParentCG.alpha = 1f;
                slidesParentCG.interactable = true;
                slidesParentCG.blocksRaycasts = true;
            }

            Transform activeSlide = currentSlide.transform;
            CanvasGroup slideCg = currentSlide.GetComponent<CanvasGroup>();

            if (slideCg == null)
                slideCg = currentSlide.gameObject.AddComponent<CanvasGroup>();

            // First opening or regular transition
            if (lastSlideIndex < 0)
            {
                // First open → skip animation
                activeSlide.localScale = Vector3.one;
                slideCg.alpha = 1f;
                Debug.Log($"Showing first slide (index {index}) without animation");
            }
            else
            {
                // Normal transition → animate
                Debug.Log($"Animating slide transition to index {index}");
                StartCoroutine(UIAnimator.AnimateSlideIn(activeSlide));
            }

            UpdateArrows();
            lastSlideIndex = index;
        }
        /// <summary>
        /// Debug method to log info about all slides - call from ReturnToSlides or other places
        /// </summary>
        private void LogSlidesState()
        {
            Debug.Log($"-------- Slides State ({currentNodeSlides.Count} slides) --------");
            for (int i = 0; i < currentNodeSlides.Count; i++)
            {
                if (currentNodeSlides[i] != null)
                {
                    GameObject slide = currentNodeSlides[i];
                    CanvasGroup cg = slide.GetComponent<CanvasGroup>();
                    string cgInfo = cg != null ? $"CanvasGroup alpha: {cg.alpha}, interactable: {cg.interactable}" : "No CanvasGroup";

                    Debug.Log($"Slide {i}: {slide.name}, Active: {slide.activeInHierarchy}, Scale: {slide.transform.localScale}, {cgInfo}");
                }
                else
                {
                    Debug.Log($"Slide {i}: NULL");
                }
            }
            Debug.Log("---------------------------------------");
        }

        /// <summary>
        /// Loads slide content for the specified node
        /// </summary>
        private void LoadNodeSlides(int nodeIndex)
        {
            // Clear any existing slides
            ClearCurrentSlides();

            // Find node content container
            Transform nodeContainer = slidesParent.transform.Find($"Node{nodeIndex}");
            if (nodeContainer == null)
            {
                Debug.LogWarning($"Node{nodeIndex} slides not found!");
                return;
            }

            // Add all slides
            foreach (Transform slide in nodeContainer)
            {
                currentNodeSlides.Add(slide.gameObject);
                slide.gameObject.SetActive(false);

                // Setup quiz start button if present
                if (slide.name == "StartQuiz")
                {
                    SetupQuizStartButton(slide, nodeIndex);
                }
            }
        }

        /// <summary>
        /// Sets up the quiz start button on the final slide
        /// </summary>
        private void SetupQuizStartButton(Transform slide, int nodeIndex)
        {
            // Set header text
            TMP_Text finalHeader = slide.GetComponentInChildren<TMP_Text>();
            if (finalHeader != null)
            {
                string headerText = (nodeIndex - 1 >= 0 && nodeIndex - 1 < nodeHeaders.Count)
                    ? nodeHeaders[nodeIndex - 1]
                    : $"Node {nodeIndex}";

                finalHeader.text = $"Congratulations for completing the {headerText} node!";
            }

            // Setup button click
            Button quizButton = slide.GetComponentInChildren<Button>();
            if (quizButton != null)
            {
                quizButton.onClick.RemoveAllListeners();
                quizButton.onClick.AddListener(() => StartQuizForNode(nodeIndex));
            }
        }

        /// <summary>
        /// Clears all currently loaded slides
        /// </summary>
        private void ClearCurrentSlides()
        {
            foreach (var slide in currentNodeSlides)
            {
                if (slide != null)
                    slide.SetActive(false);
            }
            currentNodeSlides.Clear();
        }

        /// <summary>
        /// Updates the header text for the current node
        /// </summary>
        private void UpdateNodeHeader(int nodeIndex)
        {
            headerText.text = (nodeIndex - 1 >= 0 && nodeIndex - 1 < nodeHeaders.Count)
                ? nodeHeaders[nodeIndex - 1]
                : $"Node {nodeIndex}";
        }
        #endregion

        #region Navigation
        /// <summary>
        /// Moves to the next slide or quiz question
        /// </summary>
        private void NextSlide()
        {
            if (inQuizMode)
            {
                quizManager?.NextQuestion();
                if (indicatorManager != null && quizManager != null)
                {
                    // Use animation for indicator transition
                    indicatorManager.UpdateActiveIndicator(quizManager.CurrentQuestionIndex, true);
                }
                StartCoroutine(UIAnimator.BounceElement(rightArrowButton.transform));
            }
            else
            {
                if (currentSlideIndex < currentNodeSlides.Count - 1)
                {
                    currentSlideIndex++;
                    ShowSlide(currentSlideIndex);
                    StartCoroutine(UIAnimator.BounceElement(rightArrowButton.transform));
                }
            }

            UpdateArrows();
        }

        /// <summary>
        /// Moves to the previous slide or quiz question
        /// </summary>
        private void PreviousSlide()
        {
            if (inQuizMode)
            {
                quizManager?.PreviousQuestion();
                if (indicatorManager != null && quizManager != null)
                {
                    // Use animation for indicator transition
                    indicatorManager.UpdateActiveIndicator(quizManager.CurrentQuestionIndex, true);
                }
                StartCoroutine(UIAnimator.BounceElement(leftArrowButton.transform));
            }
            else
            {
                if (currentSlideIndex > 0)
                {
                    currentSlideIndex--;
                    ShowSlide(currentSlideIndex);
                    StartCoroutine(UIAnimator.BounceElement(leftArrowButton.transform));
                }
            }

            UpdateArrows();
        }

        /// <summary>
        /// Updates the navigation arrow state based on current context
        /// </summary>
        private void UpdateArrows()
        {
            if (inQuizMode)
            {
                if (quizManager != null)
                {
                    leftArrowButton.interactable = quizManager.CanGoToPreviousQuestion();
                    rightArrowButton.interactable = quizManager.CanGoToNextQuestion();
                }
            }
            else
            {
                leftArrowButton.interactable = currentSlideIndex > 0;
                rightArrowButton.interactable = currentSlideIndex < currentNodeSlides.Count - 1;
            }

            // Update colors
            leftArrowButton.image.color = leftArrowButton.interactable ? enabledColor : disabledColor;
            rightArrowButton.image.color = rightArrowButton.interactable ? enabledColor : disabledColor;
        }
        #endregion

        #region Quiz Management
        /// <summary>
        /// Starts a quiz for the specified node
        /// </summary>
        private void StartQuizForNode(int nodeIndex)
        {
            inQuizMode = true;

            // Hide all slides
            foreach (var slide in currentNodeSlides)
            {
                if (slide != null)
                    slide.SetActive(false);
            }

            // Transition to quiz mode
            StartCoroutine(TransitionToQuizMode());

            // Start the quiz
            quizManager?.StartQuiz(nodeIndex);
        }

        /// <summary>
        /// Handles quiz completion
        /// </summary>
        private void HandleQuizCompleted(int nodeIndex)
        {
            PlayerSplineMovement playerMover = FindFirstObjectByType<PlayerSplineMovement>();
            int completedNodeIndex = openedNodeIndex - 1; // Use zero-based index

            if (playerMover != null && playerMover.IsNodeCompleted(completedNodeIndex))
            {
                // Already completed — just close the popup
                Debug.Log("[QUIZ] Node already completed. Closing popup only.");
                successPanel.SetActive(false);
                StartCoroutine(ClosePopup());
                return;
            }

            // Node not yet completed — mark as complete and update panel color
            successPanel.SetActive(false);

            // Update the popup panel to the completed color

            StartCoroutine(CompleteNodeAfterClosing(completedNodeIndex));
        }

        /// <summary>
        /// Returns to slides view from quiz mode
        /// </summary>
        public void ReturnToSlides()
        {
            // Reset state
            inQuizMode = false;

            // Make sure slides parent is active with proper settings
            slidesParent.SetActive(true);

            // Reset any potential canvas group issues on the slides parent
            CanvasGroup slidesParentCG = slidesParent.GetComponent<CanvasGroup>();
            if (slidesParentCG != null)
            {
                slidesParentCG.alpha = 1f;
                slidesParentCG.interactable = true;
                slidesParentCG.blocksRaycasts = true;
            }

            // Reset panels
            ResetPanels();

            // Ensure slides are properly loaded if they aren't already
            if (currentNodeSlides.Count == 0 && openedNodeIndex > 0)
            {
                LoadNodeSlides(openedNodeIndex);
                Debug.Log($"Reloaded slides for node {openedNodeIndex}, count: {currentNodeSlides.Count}");
            }

            // Initialize slides view
            currentSlideIndex = 0;

            // Update indicators
            if (indicatorManager != null)
            {
                indicatorManager.GenerateIndicators(currentNodeSlides.Count);
                indicatorManager.UpdateActiveIndicator(0, false); // Using false to avoid animation
                indicatorManager.SetVisibility(true);
            }

            // Make sure we have slides to show
            if (currentNodeSlides.Count > 0)
            {
                // Force reset any slide scales - this is likely the issue
                foreach (var slide in currentNodeSlides)
                {
                    slide.transform.localScale = Vector3.one; // Reset scale

                    // Reset any canvas group issues
                    CanvasGroup slideCG = slide.GetComponent<CanvasGroup>();
                    if (slideCG != null)
                    {
                        slideCG.alpha = 0f; // We'll set active slide to 1 later
                    }
                }

                // Force show the current slide without animation
                ShowSlide(currentSlideIndex); // Show the slide to ensure it's visible

                UpdateArrows();
                Debug.Log($"Returning to slides. Count: {currentNodeSlides.Count}, Current slide active: {currentNodeSlides[currentSlideIndex].activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("ReturnToSlides called but no slides are available!");
            }
        }

        private void ForceShowSlide(int index)
        {
            // Make sure this is a valid index
            if (index < 0 || index >= currentNodeSlides.Count)
                return;

            // Deactivate all slides first
            for (int i = 0; i < currentNodeSlides.Count; i++)
            {
                if (currentNodeSlides[i] != null)
                {
                    currentNodeSlides[i].SetActive(i == index); // Only activate the target

                    // Reset any canvas group on non-active slides
                    if (i != index)
                    {
                        CanvasGroup cg = currentNodeSlides[i].GetComponent<CanvasGroup>();
                        if (cg != null)
                        {
                            cg.alpha = 0f;
                        }
                    }
                }
            }

            // Now forcefully show the current slide
            GameObject currentSlide = currentNodeSlides[index];
            currentSlide.SetActive(true);

            // Set proper transform
            currentSlide.transform.localScale = Vector3.one;

            // Ensure canvas group is visible
            CanvasGroup slideCG = currentSlide.GetComponent<CanvasGroup>();
            if (slideCG == null)
                slideCG = currentSlide.AddComponent<CanvasGroup>();


            slideCG.alpha = 1f;
            slideCG.interactable = true;
            slideCG.blocksRaycasts = true;

            // Update tracking state
            lastSlideIndex = index;

            Debug.Log($"Force showed slide {index}, active: {currentSlide.activeInHierarchy}, scale: {currentSlide.transform.localScale}, alpha: {slideCG.alpha}");
        }
        #endregion

        #region Transitions and Animations
        /// <summary>
        /// Transitions from slides to quiz mode
        /// </summary>
        private IEnumerator TransitionToQuizMode()
        {
            // Fade between panels
            yield return StartCoroutine(UIAnimator.TransitionBetweenPanels(slidesParent, quizPanel));

            // Update navigation
            UpdateArrows();

            // Set up indicators for quiz questions
            if (indicatorManager != null && quizManager != null)
            {
                // Show indicators for quiz questions
                indicatorManager.GenerateIndicators(quizManager.CurrentQuizQuestions.Count);
                indicatorManager.UpdateActiveIndicator(0);
                indicatorManager.SetVisibility(true);
                Debug.Log("Generated indicators for quiz questions: " + quizManager.CurrentQuizQuestions.Count);
            }
        }

        /// <summary>
        /// Completes a node after closing the popup
        /// </summary>
        private IEnumerator CompleteNodeAfterClosing(int nodeIndexToComplete)
        {
            yield return StartCoroutine(ClosePopup());

            PlayerSplineMovement playerMover = FindFirstObjectByType<PlayerSplineMovement>();
            if (playerMover != null)
            {
                playerMover.SetNodeToComplete(nodeIndexToComplete);
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Resets all panels to default state
        /// </summary>
        private void ResetPanels()
        {
            quizPanel.SetActive(false);
            failurePanel.SetActive(false);
            successPanel.SetActive(false);
            slidesParent.SetActive(true);
        }
        #endregion

        #region Public Status Checks
        /// <summary>
        /// Returns whether a popup is currently active
        /// </summary>
        public bool IsPopupActive()
        {
            return popupCanvasGroup.alpha > 0f && popupCanvasGroup.interactable;
        }
        #endregion
    }
}
