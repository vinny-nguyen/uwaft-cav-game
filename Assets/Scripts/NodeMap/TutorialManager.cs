using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NodeMap
{
    /// <summary>
    /// Manages the first-time tutorial flow for players
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI References")]
        [SerializeField] private CanvasGroup tutorialCanvasGroup;
        [SerializeField] private RectTransform arrowImage;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI continueText;
        [SerializeField] private Image clickBlocker;
        [SerializeField] private RectTransform messagePanel;

        [Header("Arrow Settings")]
        [SerializeField] private float arrowBobAmount = 20f;
        [SerializeField] private float arrowBobSpeed = 2f;

        [Header("Target References")]
        [SerializeField] private Transform firstNodeTransform;
        [SerializeField] private Transform finalNodeTransform;
        [SerializeField] private Transform driveButton;
        [SerializeField] private Transform homeButton;

        [Header("Tutorial Settings")]
        [SerializeField] private bool showTutorialOnStart = true;
        [SerializeField] private string playerPrefKey = "CompletedTutorial";
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.3f;

        [Header("Spotlight Effect")]
        [SerializeField] private Image dimmerPanel;
        [SerializeField] private float spotlightRadius = 100f;
        [SerializeField] private Material spotlightMaterial;
        #endregion

        #region Private Fields
        private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
        private int currentStep = 0;
        private bool tutorialActive = false;
        private Coroutine arrowAnimationCoroutine;
        private Camera mainCamera;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
            SetupTutorialSteps();
        }

        private void Start()
        {
            // For testing - remove in production
            // ResetTutorialStatus();
        }

        // Removed Update() polling. Arrow position is updated when tutorial step changes or UI changes.
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            mainCamera = Camera.main;

            // Hide tutorial initially
            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.alpha = 0f;
                tutorialCanvasGroup.gameObject.SetActive(false);
            }

            // Setup spotlight material
            if (dimmerPanel != null && spotlightMaterial != null)
            {
                dimmerPanel.material = Instantiate(spotlightMaterial);
                dimmerPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sets up all tutorial steps with messages and targets
        /// </summary>
        private void SetupTutorialSteps()
        {
            tutorialSteps.Clear();

            // Step 1: Introduction to nodes
            tutorialSteps.Add(new TutorialStep(
                "These are nodes. Click on them to learn about cars!",
                firstNodeTransform,
                false,
                Vector2.zero
            ));

            // Step 2: Show the goal
            tutorialSteps.Add(new TutorialStep(
                "This is your goal. Complete all nodes to reach the final destination!",
                finalNodeTransform,
                false,
                Vector2.zero
            ));

            // Step 3: Drive button
            tutorialSteps.Add(new TutorialStep(
                "Click this button to drive when you have completed a node!",
                driveButton,
                true,
                new Vector2(30, 60)
            ));

            // Step 4: Home button
            tutorialSteps.Add(new TutorialStep(
                "Click this button to go back to the main menu.",
                homeButton,
                true,
                new Vector2(50, 60)
            ));
        }
        #endregion

        #region Public API
        /// <summary>
        /// Triggers the tutorial when the player reaches the first node
        /// </summary>
        public void TriggerNodeReachedTutorial()
        {
            if (!HasCompletedTutorial())
                StartTutorial();
        }

        /// <summary>
        /// Shows the tutorial from the beginning
        /// </summary>
        public void StartTutorial()
        {
            if (tutorialSteps.Count == 0)
            {
                Debug.LogWarning("No tutorial steps defined!");
                return;
            }

            tutorialActive = true;
            currentStep = 0;

            // Enable components
            tutorialCanvasGroup.gameObject.SetActive(true);

            if (clickBlocker != null)
            {
                clickBlocker.gameObject.SetActive(true);
                clickBlocker.color = new Color(0, 0, 0, 0.01f); // Invisible but blocks raycasts
            }

            // Start animation
            StartCoroutine(FadeInTutorial());
        }

        /// <summary>
        /// Ends the tutorial and marks it as completed
        /// </summary>
        public void EndTutorial()
        {
            tutorialActive = false;

            if (arrowAnimationCoroutine != null)
                StopCoroutine(arrowAnimationCoroutine);

            // Disable click blocker
            if (clickBlocker != null)
                clickBlocker.gameObject.SetActive(false);

            // Fade out and hide
            StartCoroutine(FadeOutTutorial());

            // Mark tutorial as completed
            PlayerPrefs.SetInt(playerPrefKey, 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Checks if the user has completed the tutorial before
        /// </summary>
        public bool HasCompletedTutorial()
        {
            return PlayerPrefs.GetInt(playerPrefKey, 0) == 1;
        }

        /// <summary>
        /// Forces the tutorial to start (even if completed before)
        /// </summary>
        public void ForceStartTutorial()
        {
            StartTutorial();
        }

        /// <summary>
        /// Resets the tutorial completion status (for testing)
        /// </summary>
        public void ResetTutorialStatus()
        {
            PlayerPrefs.DeleteKey(playerPrefKey);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Checks if the tutorial is currently active
        /// </summary>
        public bool IsTutorialActive()
        {
            return tutorialActive;
        }
        #endregion

        #region Tutorial Step Navigation
        /// <summary>
        /// Advances to the next tutorial step
        /// </summary>
        private void AdvanceToNextStep()
        {
            currentStep++;

            if (currentStep >= tutorialSteps.Count)
                EndTutorial();
            else
                ShowCurrentStep();
        }

        /// <summary>
        /// Display the current tutorial step
        /// </summary>
        private void ShowCurrentStep()
        {
            if (currentStep >= tutorialSteps.Count)
            {
                EndTutorial();
                return;
            }

            TutorialStep step = tutorialSteps[currentStep];

            // Update text
            if (messageText != null)
                messageText.text = step.Message;

            // Position arrow and panel
            if (step.Target != null)
            {
                PositionArrowAtTarget(step.Target, step.IsUIElement, step.ArrowOffset);

                // Stop existing animation if any
                if (arrowAnimationCoroutine != null)
                    StopCoroutine(arrowAnimationCoroutine);

                // Start new animation
                arrowAnimationCoroutine = StartCoroutine(AnimateArrow());
            }

            // Wait for user interaction
            StartCoroutine(WaitForClick());
        }
        #endregion

        #region Animation & UI Positioning
        private void UpdateArrowPosition()
        {
            if (!tutorialActive || currentStep >= tutorialSteps.Count)
                return;

            TutorialStep currentTutorialStep = tutorialSteps[currentStep];
            if (currentTutorialStep?.Target != null)
            {
                PositionArrowAtTarget(
                    currentTutorialStep.Target,
                    currentTutorialStep.IsUIElement,
                    currentTutorialStep.ArrowOffset
                );
            }
        }

        /// <summary>
        /// Positions the arrow to point at a target
        /// </summary>
        private void PositionArrowAtTarget(Transform target, bool isUIElement, Vector2 customOffset)
        {
            if (target == null || mainCamera == null || arrowImage == null)
                return;

            Canvas canvas = tutorialCanvasGroup.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
            if (canvasRectTransform == null) return;

            Vector2 localPosition = GetLocalPositionForTarget(target, isUIElement, canvas, canvasRectTransform);
            Vector2 defaultOffset = isUIElement ? new Vector2(0, 100) : new Vector2(100, 200);

            // Set arrow position
            arrowImage.anchoredPosition = localPosition + defaultOffset + customOffset;

            // Update spotlight effect
            UpdateSpotlight(target, isUIElement);

            // Position message panel if needed
            if (messagePanel != null)
                PositionMessagePanelNearArrow();
        }

        private Vector2 GetLocalPositionForTarget(Transform target, bool isUIElement,
                                                 Canvas canvas, RectTransform canvasRectTransform)
        {
            if (isUIElement)
                return GetLocalPositionForUIElement(target, canvas, canvasRectTransform);
            else
                return GetLocalPositionForWorldObject(target, canvas, canvasRectTransform);
        }

        private Vector2 GetLocalPositionForUIElement(Transform target, Canvas canvas, RectTransform canvasRectTransform)
        {
            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null)
                return Vector2.zero;

            // Get corners in world space
            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            Vector3 targetCenter = (corners[0] + corners[2]) / 2;

            // Convert to screen point
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetCenter);

            // Convert to local position
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform, screenPoint, canvas.worldCamera, out localPosition);

            return localPosition;
        }

        private Vector2 GetLocalPositionForWorldObject(Transform target, Canvas canvas, RectTransform canvasRectTransform)
        {
            Vector2 localPosition = Vector2.zero;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector2 viewportPosition = mainCamera.WorldToViewportPoint(target.position);
                Vector2 screenPosition = new Vector2(
                    viewportPosition.x * Screen.width,
                    viewportPosition.y * Screen.height
                );

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform, screenPosition, null, out localPosition);
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, target.position);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform, screenPoint, canvas.worldCamera, out localPosition);
            }

            return localPosition;
        }

        private void PositionMessagePanelNearArrow()
        {
            // Position message panel below the arrow
            messagePanel.anchoredPosition = arrowImage.anchoredPosition + new Vector2(0, -120);

            // Keep message panel on screen
            KeepRectTransformOnScreen(messagePanel, tutorialCanvasGroup.GetComponent<RectTransform>(), 20);
        }

        private void KeepRectTransformOnScreen(RectTransform rt, RectTransform container, float padding)
        {
            if (rt == null || container == null)
                return;

            Vector2 size = rt.rect.size;
            Vector2 containerSize = container.rect.size;

            float minX = -containerSize.x / 2 + size.x / 2 + padding;
            float maxX = containerSize.x / 2 - size.x / 2 - padding;
            float minY = -containerSize.y / 2 + size.y / 2 + padding;
            float maxY = containerSize.y / 2 - size.y / 2 - padding;

            rt.anchoredPosition = new Vector2(
                Mathf.Clamp(rt.anchoredPosition.x, minX, maxX),
                Mathf.Clamp(rt.anchoredPosition.y, minY, maxY)
            );
        }

        /// <summary>
        /// Updates the spotlight effect
        /// </summary>
        private void UpdateSpotlight(Transform target, bool isUIElement)
        {
            if (dimmerPanel == null || dimmerPanel.material == null || target == null || mainCamera == null)
                return;

            // Get viewport position based on target type
            Vector2 viewportPosition = isUIElement
                ? GetViewportPositionForUIElement(target)
                : mainCamera.WorldToViewportPoint(target.position);

            // Apply to material
            float aspectRatio = (float)Screen.width / Screen.height;
            float normalizedRadius = spotlightRadius / Screen.height;

            dimmerPanel.material.SetVector("_Center", new Vector4(viewportPosition.x, viewportPosition.y, 0, 0));
            dimmerPanel.material.SetFloat("_Radius", normalizedRadius);
            dimmerPanel.material.SetFloat("_SoftEdge", normalizedRadius * 0.2f);
            dimmerPanel.material.SetFloat("_AspectRatio", aspectRatio);
        }

        private Vector2 GetViewportPositionForUIElement(Transform target)
        {
            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null)
                return Vector2.zero;

            // Get corners in world space
            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            Vector3 targetCenter = (corners[0] + corners[2]) / 2;

            // Get screen position
            Canvas canvas = tutorialCanvasGroup.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, targetCenter);

            // Convert to viewport position
            return new Vector2(
                screenPoint.x / Screen.width,
                screenPoint.y / Screen.height
            );
        }

        /// <summary>
        /// Animates the arrow with a bobbing effect
        /// </summary>
        private IEnumerator AnimateArrow()
        {
            Vector2 originalPosition = arrowImage.anchoredPosition;
            float time = 0f;

            while (tutorialActive)
            {
                time += Time.deltaTime;

                // Create bobbing motion
                float yOffset = Mathf.Sin(time * arrowBobSpeed) * arrowBobAmount;
                arrowImage.anchoredPosition = originalPosition + new Vector2(0, yOffset);

                yield return null;
            }
        }

        /// <summary>
        /// Waits for user to click anywhere to continue
        /// </summary>
        private IEnumerator WaitForClick()
        {
            // Delay to avoid accidental progression
            yield return new WaitForSeconds(0.5f);

            while (tutorialActive)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    AdvanceToNextStep();
                    break;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Fade in tutorial UI
        /// </summary>
        private IEnumerator FadeInTutorial()
        {
            // Set initial state
            tutorialCanvasGroup.alpha = 0f;

            if (dimmerPanel != null)
            {
                dimmerPanel.gameObject.SetActive(true);
                dimmerPanel.color = new Color(0, 0, 0, 0);
            }

            // Set text first
            if (currentStep < tutorialSteps.Count && messageText != null)
                messageText.text = tutorialSteps[currentStep].Message;

            // Fade in
            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeInDuration);

                tutorialCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

                if (dimmerPanel != null)
                    dimmerPanel.color = new Color(0, 0, 0, Mathf.Lerp(0, 0.7f, t));

                yield return null;
            }

            // Ensure final state
            tutorialCanvasGroup.alpha = 1f;

            // Show current step
            ShowCurrentStep();
        }

        /// <summary>
        /// Fade out tutorial UI
        /// </summary>
        private IEnumerator FadeOutTutorial()
        {
            float timer = 0f;

            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeOutDuration);

                tutorialCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

                if (dimmerPanel != null)
                    dimmerPanel.color = new Color(0, 0, 0, Mathf.Lerp(0.7f, 0f, t));

                yield return null;
            }

            // Hide UI
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.gameObject.SetActive(false);

            if (dimmerPanel != null)
                dimmerPanel.gameObject.SetActive(false);
        }
        #endregion
    }

    /// <summary>
    /// Represents a single step in the tutorial sequence
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        public string Message;
        public Transform Target;
        public bool IsUIElement;
        public Vector2 ArrowOffset = Vector2.zero;

        public TutorialStep(string message, Transform target, bool isUIElement = false)
            : this(message, target, isUIElement, Vector2.zero) { }

        public TutorialStep(string message, Transform target, bool isUIElement, Vector2 arrowOffset)
        {
            Message = message;
            Target = target;
            IsUIElement = isUIElement;
            ArrowOffset = arrowOffset;
        }
    }
}