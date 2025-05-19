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
        #region Singleton & Properties
        public static PopupManager Instance { get; private set; }
        public bool IsPopupActive() => popupCanvasGroup.alpha > 0f && popupCanvasGroup.interactable;
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
        [SerializeField] private Image popupPanel;

        [Header("Content Containers")]
        [SerializeField] private GameObject slidesParent;
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private GameObject failurePanel;
        [SerializeField] private GameObject successPanel;

        [Header("Components")]
        [SerializeField] private SlideIndicatorManager indicatorManager;
        [SerializeField] private QuizManager quizManager;

        [Header("Popup Styles")]
        [SerializeField] private Sprite normalPopupSprite;
        [SerializeField] private Sprite completedPopupSprite;
        #endregion

        #region Private Fields
        private List<GameObject> currentNodeSlides = new List<GameObject>();
        private int currentSlideIndex = 0;
        private int lastSlideIndex = -1;
        private int openedNodeIndex = -1;
        private bool inQuizMode = false;
        private readonly Color enabledColor = Color.white;
        private readonly Color disabledColor = new Color(1f, 1f, 1f, 0.4f);
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Setup singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
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
                quizManager.OnQuizCompleted += HandleQuizCompleted;
        }

        private void Update()
        {
            // Handle keyboard navigation
            if (Input.GetKeyDown(KeyCode.RightArrow)) NextSlide();
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) PreviousSlide();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Opens a popup with content for the specified node
        /// </summary>
        public void OpenPopupForNode(int nodeIndex)
        {
            // Reset state and track
            inQuizMode = false;
            ResetPanels();
            openedNodeIndex = nodeIndex;

            // Set appearance based on completion
            UpdatePopupAppearance(nodeIndex);

            // Load content
            LoadNodeSlides(nodeIndex);
            UpdateNodeHeader(nodeIndex);

            // Setup indicators
            if (indicatorManager != null)
            {
                indicatorManager.GenerateIndicators(currentNodeSlides.Count);
                indicatorManager.SetVisibility(true);
            }

            // Initialize slides
            lastSlideIndex = -1;
            currentSlideIndex = 0;
            UpdateArrows();
            ShowSlide(currentSlideIndex);

            // Animate opening
            StartCoroutine(UIAnimator.AnimatePopupOpen(popupCanvasGroup, backgroundOverlay));
        }

        /// <summary>
        /// Closes the current popup
        /// </summary>
        public IEnumerator ClosePopup()
        {
            // Hide all slides
            foreach (var slide in currentNodeSlides)
            {
                if (slide != null)
                    slide.SetActive(false);
            }

            // Reset state
            inQuizMode = false;
            currentNodeSlides.Clear();

            // Clear indicators
            indicatorManager?.ClearIndicators();

            // Animate closing
            yield return StartCoroutine(UIAnimator.AnimatePopupClose(popupCanvasGroup, backgroundOverlay));
        }

        /// <summary>
        /// Returns to slides view from quiz mode
        /// </summary>
        // <summary>
        /// Returns to slides view from quiz mode
        /// </summary>
        public void ReturnToSlides()
        {
            // Reset state
            inQuizMode = false;
            ResetPanels();

            // Ensure slides are properly loaded
            EnsureSlidesLoaded();

            // Reset to first slide
            currentSlideIndex = 0;

            // Update UI
            ShowSlide(currentSlideIndex);
            // ShowCurrentSlideWithoutAnimation();
            UpdateArrows();
            UpdateIndicators();
        }
        #endregion

        #region Slide Management
        private void LoadNodeSlides(int nodeIndex)
        {
            ClearCurrentSlides();

            // Find node content container
            Transform nodeContainer = slidesParent.transform.Find($"Node{nodeIndex}");
            if (nodeContainer == null)
            {
                LogNodeContainerError(nodeIndex);
                return;
            }

            // Add all slides
            foreach (Transform slide in nodeContainer)
            {
                currentNodeSlides.Add(slide.gameObject);
                slide.gameObject.SetActive(false);

                // Setup quiz button if present
                if (slide.name == "StartQuiz")
                    SetupQuizStartButton(slide, nodeIndex);
            }
        }

        private void ShowSlide(int index)
        {
            // Safety checks
            if (currentNodeSlides.Count == 0 || index < 0 || index >= currentNodeSlides.Count)
            {
                Debug.LogWarning($"Invalid slide index: {index}. Slide count: {currentNodeSlides.Count}");
                return;
            }

            // Hide previous slide if any
            HidePreviousSlide(index);

            // Get current slide
            GameObject currentSlide = currentNodeSlides[index];
            if (currentSlide == null) return;

            // Ensure slide parent is visible
            SetSlidesParentVisibility(true);

            // Prepare the slide
            currentSlide.SetActive(true);
            CanvasGroup slideCg = GetOrAddCanvasGroup(currentSlide);

            // Show slide with or without animation
            if (lastSlideIndex < 0)
            {
                // First slide - no animation
                currentSlide.transform.localScale = Vector3.one;
                slideCg.alpha = 1f;
            }
            else
            {
                // Regular slide - animate
                StartCoroutine(UIAnimator.AnimateSlideIn(currentSlide.transform));
            }

            // Update state
            lastSlideIndex = index;
            UpdateArrows();
        }

        private void HidePreviousSlide(int newIndex)
        {
            if (lastSlideIndex >= 0 && lastSlideIndex < currentNodeSlides.Count && lastSlideIndex != newIndex)
            {
                GameObject lastSlide = currentNodeSlides[lastSlideIndex];
                if (lastSlide != null)
                    StartCoroutine(UIAnimator.AnimateSlideOut(lastSlide));

                indicatorManager?.UpdateActiveIndicator(currentSlideIndex, animate: true);
            }
        }

        private void ClearCurrentSlides()
        {
            foreach (var slide in currentNodeSlides)
            {
                if (slide != null)
                    slide.SetActive(false);
            }
            currentNodeSlides.Clear();
        }

        private void UpdateNodeHeader(int nodeIndex)
        {
            headerText.text = (nodeIndex >= 0 && nodeIndex < nodeHeaders.Count)
                ? nodeHeaders[nodeIndex]
                : $"Node {nodeIndex}";
        }

        private void UpdatePopupAppearance(int nodeIndex)
        {
            if (popupPanel == null) return;

            PlayerSplineMovement playerMover = FindFirstObjectByType<PlayerSplineMovement>();
            if (playerMover != null)
            {
                bool isNodeCompleted = playerMover.IsNodeCompleted(nodeIndex);
                popupPanel.sprite = isNodeCompleted ? completedPopupSprite : normalPopupSprite;
            }
        }

        private void ShowCurrentSlideWithoutAnimation()
        {
            if (currentNodeSlides.Count == 0 || currentSlideIndex >= currentNodeSlides.Count) return;

            // Reset all slides
            foreach (var slide in currentNodeSlides)
            {
                if (slide == null) continue;

                slide.transform.localScale = Vector3.one;
                slide.SetActive(false);

                CanvasGroup cg = GetOrAddCanvasGroup(slide);
                cg.alpha = 0f;
            }

            // Show current slide
            GameObject currentSlide = currentNodeSlides[currentSlideIndex];
            if (currentSlide == null) return;

            currentSlide.SetActive(true);

            CanvasGroup slideCg = GetOrAddCanvasGroup(currentSlide);
            slideCg.alpha = 1f;
            slideCg.interactable = true;
            slideCg.blocksRaycasts = true;

            // Add a debug log to verify
            Debug.Log($"Showing slide {currentSlideIndex} without animation");
        }

        private void EnsureSlidesLoaded()
        {
            if (currentNodeSlides.Count == 0 && openedNodeIndex >= 0)
                LoadNodeSlides(openedNodeIndex);
        }
        #endregion

        #region Navigation & Arrows
        private void NextSlide()
        {
            if (inQuizMode)
            {
                HandleQuizNavigation(true);
            }
            else if (currentSlideIndex < currentNodeSlides.Count)
            {
                currentSlideIndex++;
                ShowSlide(currentSlideIndex);
                StartCoroutine(UIAnimator.BounceElement(rightArrowButton.transform));
            }

            UpdateArrows();
        }

        private void PreviousSlide()
        {
            if (inQuizMode)
            {
                HandleQuizNavigation(false);
            }
            else if (currentSlideIndex > 0)
            {
                currentSlideIndex--;
                ShowSlide(currentSlideIndex);
                StartCoroutine(UIAnimator.BounceElement(leftArrowButton.transform));
            }

            UpdateArrows();
        }

        private void HandleQuizNavigation(bool goNext)
        {
            if (quizManager == null) return;

            if (goNext)
                quizManager.NextQuestion();
            else
                quizManager.PreviousQuestion();

            if (indicatorManager != null)
                indicatorManager.UpdateActiveIndicator(quizManager.CurrentQuestionIndex, animate: true);

            StartCoroutine(UIAnimator.BounceElement(
                goNext ? rightArrowButton.transform : leftArrowButton.transform));
        }

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
                rightArrowButton.interactable = currentSlideIndex < currentNodeSlides.Count;
            }

            // Update colors
            leftArrowButton.image.color = leftArrowButton.interactable ? enabledColor : disabledColor;
            rightArrowButton.image.color = rightArrowButton.interactable ? enabledColor : disabledColor;
        }

        private void UpdateIndicators()
        {
            if (indicatorManager == null) return;

            indicatorManager.GenerateIndicators(currentNodeSlides.Count);
            indicatorManager.UpdateActiveIndicator(0, animate: false);
            indicatorManager.SetVisibility(true);
        }
        #endregion

        #region Quiz Management
        private void SetupQuizStartButton(Transform slide, int nodeIndex)
        {
            // Set header text
            TMP_Text finalHeader = slide.GetComponentInChildren<TMP_Text>();
            if (finalHeader != null)
            {
                string headerText = (nodeIndex >= 0 && nodeIndex < nodeHeaders.Count)
                    ? nodeHeaders[nodeIndex]
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

        private void HandleQuizCompleted(int nodeIndex)
        {
            PlayerSplineMovement playerMover = FindFirstObjectByType<PlayerSplineMovement>();
            int completedNodeIndex = openedNodeIndex;

            if (playerMover != null && playerMover.IsNodeCompleted(completedNodeIndex))
            {
                // Already completed — just close the popup
                successPanel.SetActive(false);
                StartCoroutine(ClosePopup());
                return;
            }

            // Mark node as complete and close
            successPanel.SetActive(false);
            StartCoroutine(CompleteNodeAfterClosing(completedNodeIndex));
        }

        private IEnumerator CompleteNodeAfterClosing(int nodeIndexToComplete)
        {
            yield return StartCoroutine(ClosePopup());

            PlayerSplineMovement playerMover = FindFirstObjectByType<PlayerSplineMovement>();
            playerMover?.SetNodeToComplete(nodeIndexToComplete);
        }

        private IEnumerator TransitionToQuizMode()
        {
            // Fade between panels
            yield return StartCoroutine(UIAnimator.TransitionBetweenPanels(slidesParent, quizPanel));

            // Update navigation
            UpdateArrows();

            // Setup quiz indicators
            if (indicatorManager != null && quizManager != null)
            {
                indicatorManager.GenerateIndicators(quizManager.CurrentQuizQuestions.Count);
                indicatorManager.UpdateActiveIndicator(0);
                indicatorManager.SetVisibility(true);
            }
        }
        #endregion

        #region Utility Methods
        private void ResetPanels()
        {
            quizPanel.SetActive(false);
            failurePanel.SetActive(false);
            successPanel.SetActive(false);
            slidesParent.SetActive(true);
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
        {
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = obj.AddComponent<CanvasGroup>();
            return cg;
        }

        private void SetSlidesParentVisibility(bool visible)
        {
            CanvasGroup slidesParentCG = slidesParent.GetComponent<CanvasGroup>();
            if (slidesParentCG != null)
            {
                slidesParentCG.alpha = visible ? 1f : 0f;
                slidesParentCG.interactable = visible;
                slidesParentCG.blocksRaycasts = visible;
            }
        }

        private void LogNodeContainerError(int nodeIndex)
        {
            Debug.LogWarning($"Node{nodeIndex} slides not found!");
            foreach (Transform child in slidesParent.transform)
            {
                Debug.Log($"Found node container: {child.name}");
            }
        }
        #endregion
    }
}