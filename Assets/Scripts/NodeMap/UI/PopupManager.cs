using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using NodeMap.UI;
using NodeMap.Quiz;

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
        private bool isTransitioningSlides = false; // Flag to track slide animation state
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
            if (isTransitioningSlides) return; // Don't open if already transitioning

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
            currentSlideIndex = 0;
            lastSlideIndex = -1; // Signifies no previous slide for the first animation

            // Animate opening of the popup itself
            StartCoroutine(UIAnimator.AnimatePopupOpen(popupCanvasGroup, backgroundOverlay));
            // Show the first slide
            StartCoroutine(ProcessSlideChangeInternal(currentSlideIndex, lastSlideIndex));
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
            int oldSlideForAnimation = currentSlideIndex; // Or -1 if a fresh display is desired
            currentSlideIndex = 0;

            StartCoroutine(ProcessSlideChangeInternal(currentSlideIndex, oldSlideForAnimation));

            // Update UI
            // ShowSlide(currentSlideIndex);
            // ShowCurrentSlideWithoutAnimation();
            // UpdateArrows();
            // UpdateIndicators();
        }
        #endregion

        #region Slide Management
        private IEnumerator ProcessSlideChangeInternal(int newSlideIndex, int oldSlideIndex)
        {
            if (newSlideIndex < 0 || newSlideIndex >= currentNodeSlides.Count)
            {
                Debug.LogWarning($"ProcessSlideChangeInternal: Invalid newSlideIndex {newSlideIndex}");
                yield break;
            }

            isTransitioningSlides = true;
            bool originalLeftArrowState = leftArrowButton.interactable;
            bool originalRightArrowState = rightArrowButton.interactable;
            leftArrowButton.interactable = false;
            rightArrowButton.interactable = false;

            try
            {
                // Update the current slide index state for PopupManager
                this.currentSlideIndex = newSlideIndex;

                // Start indicator animation concurrently.
                // SlideIndicatorManager uses its own lastActiveIndex to animate from.
                indicatorManager?.UpdateActiveIndicator(this.currentSlideIndex, animate: true);

                Coroutine hideCoroutine = null;
                // Animate out the old slide if there is one and it's different from the new one
                if (oldSlideIndex != newSlideIndex && oldSlideIndex >= 0 && oldSlideIndex < currentNodeSlides.Count)
                {
                    hideCoroutine = StartCoroutine(HideSlideCoroutine(oldSlideIndex, newSlideIndex));
                }

                // Start the show animation for the new slide
                Coroutine showCoroutine = StartCoroutine(ShowNewSlideCoroutine(newSlideIndex, oldSlideIndex));

                // Wait for slide animations to complete.
                if (hideCoroutine != null)
                {
                    yield return hideCoroutine;
                }
                yield return showCoroutine;

                // Update PopupManager's record of the last slide index for the *next* transition
                this.lastSlideIndex = oldSlideIndex;

                UpdateArrows();
                // Indicator update was moved up to run concurrently.
                // If an immediate final state check is needed for indicators after all animations,
                // it could be done here, but UpdateActiveIndicator should handle the final state.
            }
            finally
            {
                isTransitioningSlides = false;
                // UpdateArrows() should have set the correct interactability.
                // If not, restore:
                // leftArrowButton.interactable = originalLeftArrowState;
                // rightArrowButton.interactable = originalRightArrowState;
            }
        }


        private IEnumerator ShowNewSlideCoroutine(int slideToShowIndex, int previousActualSlideIndex)
        {
            if (slideToShowIndex < 0 || slideToShowIndex >= currentNodeSlides.Count)
            {
                Debug.LogWarning($"ShowNewSlideCoroutine: Invalid slideToShowIndex {slideToShowIndex}");
                yield break;
            }

            GameObject currentSlide = currentNodeSlides[slideToShowIndex];
            if (currentSlide == null) yield break;

            SetSlidesParentVisibility(true);
            currentSlide.SetActive(true);
            CanvasGroup slideCg = GetOrAddCanvasGroup(currentSlide);

            if (previousActualSlideIndex < 0 && slideToShowIndex == 0) // First slide shown for this popup
            {
                currentSlide.transform.localPosition = Vector3.zero;
                currentSlide.transform.localScale = Vector3.one;
                slideCg.alpha = 1f;
            }
            else
            {
                // Determine entry direction:
                // If new slide index > old, it's a "next" slide, enters from right (moves left).
                // If new slide index < old, it's a "previous" slide, enters from left (moves right).
                Vector3 entryDirection = (slideToShowIndex > previousActualSlideIndex) ? Vector3.left : Vector3.right;
                yield return StartCoroutine(UIAnimator.AnimateSlideInFromSide(currentSlide.transform, entryDirection));
            }
        }

        private IEnumerator HideSlideCoroutine(int slideToHideIndex, int nextSlideIndex)
        {
            if (slideToHideIndex < 0 || slideToHideIndex >= currentNodeSlides.Count)
            {
                yield break;
            }

            if (slideToHideIndex == nextSlideIndex)
            {
                yield break;
            }

            GameObject slideToHideObject = currentNodeSlides[slideToHideIndex];
            if (slideToHideObject != null && slideToHideObject.activeSelf)
            {
                // Determine exit direction:
                // If moving to a "next" slide (nextSlideIndex > slideToHideIndex), current slide exits to the LEFT.
                // If moving to a "previous" slide (nextSlideIndex < slideToHideIndex), current slide exits to the RIGHT.
                Vector3 exitDirection = (nextSlideIndex > slideToHideIndex) ? Vector3.left : Vector3.right;
                yield return StartCoroutine(UIAnimator.AnimateSlideOutToSide(slideToHideObject.transform, exitDirection));
            }
        }

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

            // Hide previous slide if any - this will now use the new animation
            HidePreviousSlide(index);

            // Get current slide
            GameObject currentSlide = currentNodeSlides[index];
            if (currentSlide == null) return;

            // Ensure slide parent is visible
            SetSlidesParentVisibility(true);

            // Prepare the slide
            currentSlide.SetActive(true); // Ensure active before animation
            CanvasGroup slideCg = GetOrAddCanvasGroup(currentSlide); // GetOrAddCanvasGroup is in PopupManager

            // Show slide with or without animation
            if (lastSlideIndex < 0) // First slide shown
            {
                // First slide - no animation, just set to final state
                currentSlide.transform.localPosition = Vector3.zero; // Center it
                currentSlide.transform.localScale = Vector3.one;
                slideCg.alpha = 1f;
            }
            else
            {
                // Regular slide - animate with new jab animation
                Vector3 entryDirection = (index > lastSlideIndex) ? Vector3.left : Vector3.right; // Left means enters from Right
                StartCoroutine(UIAnimator.AnimateSlideInFromSide(currentSlide.transform, entryDirection));
            }

            // Update state
            lastSlideIndex = index;
            UpdateArrows();
            // Update indicator after the new slide is supposed to be visible or animating in
            indicatorManager?.UpdateActiveIndicator(currentSlideIndex, animate: true);
        }

        private void HidePreviousSlide(int newIndex)
        {
            if (lastSlideIndex >= 0 && lastSlideIndex < currentNodeSlides.Count && lastSlideIndex != newIndex)
            {
                GameObject lastSlideObject = currentNodeSlides[lastSlideIndex];
                if (lastSlideObject != null)
                {
                    // Determine exit direction based on whether we are moving to the next or previous slide
                    Vector3 exitDirection = (newIndex > lastSlideIndex) ? Vector3.left : Vector3.right; // Left means exits towards Left
                    StartCoroutine(UIAnimator.AnimateSlideOutToSide(lastSlideObject.transform, exitDirection));
                }
                // Indicator update was here, moved to ShowSlide to reflect the new active slide
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
            if (isTransitioningSlides || inQuizMode) return;

            int targetIndex = currentSlideIndex + 1;
            if (targetIndex < currentNodeSlides.Count)
            {
                StartCoroutine(ProcessSlideChangeInternal(targetIndex, currentSlideIndex));
                StartCoroutine(UIAnimator.BounceElement(rightArrowButton.transform));
            }
        }

        private void PreviousSlide()
        {
            if (isTransitioningSlides || inQuizMode) return;

            int targetIndex = currentSlideIndex - 1;
            if (targetIndex >= 0)
            {
                StartCoroutine(ProcessSlideChangeInternal(targetIndex, currentSlideIndex));
                StartCoroutine(UIAnimator.BounceElement(leftArrowButton.transform));
            }
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