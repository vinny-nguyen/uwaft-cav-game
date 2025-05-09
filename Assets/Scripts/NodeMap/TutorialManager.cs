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
        [Header("UI References")]
        [SerializeField] private CanvasGroup tutorialCanvasGroup;
        [SerializeField] private RectTransform arrowImage;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI continueText;
        [SerializeField] private Image clickBlocker;

        [Header("Arrow Settings")]
        [SerializeField] private float arrowBobAmount = 20f;
        [SerializeField] private float arrowBobSpeed = 2f;

        [Header("Target References")]
        [SerializeField] private Transform firstNodeTransform;  // First node to highlight
        [SerializeField] private Transform finalNodeTransform;  // Final node to highlight
        [SerializeField] private Transform driveButton;    // UI button to highlight
        [SerializeField] private Transform homeButton;     // UI button to highlight

        [Header("Tutorial Settings")]
        [SerializeField] private bool showTutorialOnStart = true;
        [SerializeField] private string playerPrefKey = "CompletedTutorial";
        [SerializeField] private float fadeInDuration = 0.5f;

        [Header("Spotlight Effect")]
        [SerializeField] private Image dimmerPanel;
        [SerializeField] private float spotlightRadius = 100f;
        [SerializeField] private Material spotlightMaterial;

        [SerializeField] private RectTransform messagePanel;

        private int currentStep = 0;
        private bool tutorialActive = false;
        private Coroutine arrowAnimationCoroutine;
        private Camera mainCamera;

        // Tutorial step definitions
        private List<TutorialStep> tutorialSteps = new List<TutorialStep>();

        private void Awake()
        {
            mainCamera = Camera.main;

            // Make sure tutorial is hidden initially
            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.alpha = 0f;
                tutorialCanvasGroup.gameObject.SetActive(false);
            }

            SetupTutorialSteps();

            if (dimmerPanel != null && spotlightMaterial != null)
            {
                dimmerPanel.material = Instantiate(spotlightMaterial);
                dimmerPanel.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            // For testing: Uncomment to always show tutorial regardless of previous completion
            ResetTutorialStatus();
        }

        private void Update()
        {
            if (tutorialActive && currentStep < tutorialSteps.Count)
            {
                // Update arrow position every frame if the target exists
                TutorialStep currentTutorialStep = tutorialSteps[currentStep];
                if (currentTutorialStep.Target != null)
                {
                    PositionArrowAtTarget(
                        currentTutorialStep.Target,
                        currentTutorialStep.IsUIElement,
                        currentTutorialStep.ArrowOffset
                    );
                }
            }
        }

        /// <summary>
        /// Sets up all tutorial steps with messages and targets
        /// </summary>
        private void SetupTutorialSteps()
        {
            tutorialSteps.Clear();

            // Step 1: Introduction to nodes with default positioning
            tutorialSteps.Add(new TutorialStep(
                "These are nodes. Click on them to learn about cars!",
                firstNodeTransform,
                false,
                new Vector2(0, 0))); // Offset arrow 50 pixels up

            // Step 2: Show the goal with custom offset
            tutorialSteps.Add(new TutorialStep(
                "This is your goal. Complete all nodes to reach the final destination!",
                finalNodeTransform,
                false,
                new Vector2(0, 0))); // Position arrow to the left and higher

            // Step 3: UI Button with custom offset
            tutorialSteps.Add(new TutorialStep(
                "Click this button to drive when you have completed a node!",
                driveButton,
                true,
                new Vector2(30, 60))); // Position arrow higher above the UI button

            tutorialSteps.Add(new TutorialStep(
            "Click this button to go back to the main menu.",
            homeButton,
            true,
            new Vector2(50, 60))); // Position arrow higher above the UI button
        }

        /// <summary>
        /// Triggers the tutorial when the player reaches the first node
        /// Call this method from PlayerSplineMovement when reaching a node
        /// </summary>
        public void TriggerNodeReachedTutorial()
        {
            if (HasCompletedTutorial())
                return;

            // Start tutorial
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

            // Enable the tutorial canvas
            tutorialCanvasGroup.gameObject.SetActive(true);

            // Enable click blocker to prevent clicking underlying UI elements
            if (clickBlocker != null)
            {
                clickBlocker.gameObject.SetActive(true);
                // Make it invisible but raycast blocking
                clickBlocker.color = new Color(0, 0, 0, 0.01f);
            }

            // Show first step
            StartCoroutine(FadeInTutorial());
        }

        private IEnumerator FadeInTutorial()
        {
            tutorialCanvasGroup.alpha = 0f;
            float timer = 0f;

            if (dimmerPanel != null)
            {
                dimmerPanel.gameObject.SetActive(true);
                dimmerPanel.color = new Color(0, 0, 0, 0);
            }

            // Make sure text is set before showing anything
            if (currentStep < tutorialSteps.Count && messageText != null)
            {
                messageText.text = tutorialSteps[currentStep].Message;
            }

            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeInDuration;
                tutorialCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

                // Also fade in the dimmer
                if (dimmerPanel != null)
                {
                    dimmerPanel.color = new Color(0, 0, 0, Mathf.Lerp(0, 0.7f, t));
                }
                yield return null;
            }

            tutorialCanvasGroup.alpha = 1f;
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
            messageText.text = step.Message;

            // Position arrow and panel
            if (step.Target != null)
            {
                PositionArrowAtTarget(step.Target, step.IsUIElement, step.ArrowOffset);

                if (arrowAnimationCoroutine != null)
                {
                    StopCoroutine(arrowAnimationCoroutine);
                }
                arrowAnimationCoroutine = StartCoroutine(AnimateArrow());
            }

            // Listen for click to continue
            StartCoroutine(WaitForClick());
        }

        /// <summary>
        /// Positions the arrow to point at a world target
        /// </summary>
        private void PositionArrowAtTarget(Transform target, bool isUIElement = false, Vector2 customOffset = default)
        {
            if (target == null || mainCamera == null || arrowImage == null)
            {
                Debug.LogWarning("Missing reference for positioning arrow!");
                return;
            }

            // Get the RectTransform of the canvas
            Canvas canvas = tutorialCanvasGroup.GetComponentInParent<Canvas>();
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

            if (isUIElement)
            {
                // UI Element positioning
                RectTransform targetRect = target.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    // Get the center position of the UI element in canvas space
                    Vector3[] corners = new Vector3[4];
                    targetRect.GetWorldCorners(corners);
                    Vector3 targetCenter = (corners[0] + corners[2]) / 2; // Center of the UI element

                    // Convert world position to screen position
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetCenter);

                    // Convert screen position to local position in tutorial canvas
                    Vector2 localPosition;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRectTransform, screenPoint, canvas.worldCamera, out localPosition);

                    // Position arrow with default offset + custom offset
                    Vector2 defaultOffset = new Vector2(0, 100); // Default position above the UI element
                    arrowImage.anchoredPosition = localPosition + defaultOffset + customOffset;

                    // Debug.Log($"Positioning arrow at UI element: {target.name}, Position: {localPosition}, With offset: {customOffset}");
                }
            }
            else
            {
                // World object positioning
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // For Screen Space - Overlay canvas
                    Vector2 viewportPosition = mainCamera.WorldToViewportPoint(target.position);
                    Vector2 screenPosition = new Vector2(
                        viewportPosition.x * Screen.width,
                        viewportPosition.y * Screen.height
                    );

                    // Convert screen position to local position in canvas
                    Vector2 localPosition;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRectTransform, screenPosition, null, out localPosition);

                    // Position arrow with default offset + custom offset
                    Vector2 defaultOffset = new Vector2(100, 200);
                    arrowImage.anchoredPosition = localPosition + defaultOffset + customOffset;
                }
                else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    // For Screen Space - Camera canvas
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, target.position);

                    // Convert screen position to local position in canvas
                    Vector2 localPosition;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRectTransform, screenPoint, canvas.worldCamera, out localPosition);

                    // Position arrow with default offset + custom offset
                    Vector2 defaultOffset = new Vector2(100, 200);
                    arrowImage.anchoredPosition = localPosition + defaultOffset + customOffset;
                }
                else
                {
                    // For World Space canvas
                    Vector3 worldPosition = target.position + new Vector3(0, 1f, 0);
                    arrowImage.position = worldPosition + new Vector3(customOffset.x, customOffset.y, 0);
                }
            }

            UpdateSpotlight(target, isUIElement);
        }

        private void UpdateSpotlight(Transform target, bool isUIElement = false)
        {
            if (dimmerPanel == null || dimmerPanel.material == null || target == null || mainCamera == null)
                return;

            Vector2 viewportPosition;

            if (isUIElement)
            {
                // For UI elements, we need to convert UI position to viewport position
                RectTransform targetRect = target.GetComponent<RectTransform>();
                if (targetRect == null) return;

                // Get the center position of the UI element in world space
                Vector3[] corners = new Vector3[4];
                targetRect.GetWorldCorners(corners);
                Vector3 targetCenter = (corners[0] + corners[2]) / 2; // Center of the UI element

                // Get screen position
                Canvas canvas = tutorialCanvasGroup.GetComponentInParent<Canvas>();
                Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, targetCenter);

                // Convert screen position to viewport position (0-1 range)
                viewportPosition = new Vector2(
                    screenPoint.x / Screen.width,
                    screenPoint.y / Screen.height
                );

                // Debug.Log($"UI Element spotlight at viewport: {viewportPosition}");
            }
            else
            {
                // For world objects, use the existing approach
                viewportPosition = mainCamera.WorldToViewportPoint(target.position);
            }

            // Calculate the aspect ratio correction
            float aspectRatio = (float)Screen.width / Screen.height;

            // Update shader properties with the viewport position
            Vector4 correctedCenter = new Vector4(
                viewportPosition.x,
                viewportPosition.y,
                0,
                0
            );

            dimmerPanel.material.SetVector("_Center", correctedCenter);

            // Calculate radius based on screen height
            float screenHeight = Screen.height;
            float normalizedRadius = spotlightRadius / screenHeight;

            // Set the radius with aspect ratio correction factor
            dimmerPanel.material.SetFloat("_Radius", normalizedRadius);
            dimmerPanel.material.SetFloat("_SoftEdge", normalizedRadius * 0.2f);

            // Set the aspect ratio as a shader property
            dimmerPanel.material.SetFloat("_AspectRatio", aspectRatio);
        }

        private bool IsTargetVisible(Transform target)
        {
            if (target == null || mainCamera == null)
                return false;

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(target.position);
            return viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
        }

        /// <summary>
        /// Positions the arrow at the edge of the screen pointing toward offscreen targets
        /// </summary>
        private void PositionArrowAtScreenEdge(Transform target)
        {
            Vector3 targetViewportPos = mainCamera.WorldToViewportPoint(target.position);

            // Calculate direction to offscreen target
            Vector2 direction = new Vector2(
                targetViewportPos.x - 0.5f,
                targetViewportPos.y - 0.5f
            ).normalized;

            // Position at screen edge with some padding
            float padding = 50f;
            float canvasWidth = tutorialCanvasGroup.GetComponent<RectTransform>().rect.width;
            float canvasHeight = tutorialCanvasGroup.GetComponent<RectTransform>().rect.height;

            Vector2 screenEdgePosition = new Vector2(
                Mathf.Clamp(direction.x * (canvasWidth / 2 - padding), -canvasWidth / 2 + padding, canvasWidth / 2 - padding),
                Mathf.Clamp(direction.y * (canvasHeight / 2 - padding), -canvasHeight / 2 + padding, canvasHeight / 2 - padding)
            );

            // Set position and rotation to point at the offscreen target
            arrowImage.anchoredPosition = screenEdgePosition;

            // Rotate arrow to point toward target
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            arrowImage.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void PositionMessagePanelNearTarget(Transform target)
        {
            if (messagePanel == null || target == null)
                return;

            // Position message panel below the arrow
            messagePanel.anchoredPosition = arrowImage.anchoredPosition + new Vector2(0, -120);

            // Keep message panel on screen
            Vector2 panelSize = messagePanel.rect.size;
            Vector2 canvasSize = tutorialCanvasGroup.GetComponent<RectTransform>().rect.size;

            float minX = -canvasSize.x / 2 + panelSize.x / 2 + 20;
            float maxX = canvasSize.x / 2 - panelSize.x / 2 - 20;
            float minY = -canvasSize.y / 2 + panelSize.y / 2 + 20;
            float maxY = canvasSize.y / 2 - panelSize.y / 2 - 20;

            messagePanel.anchoredPosition = new Vector2(
                Mathf.Clamp(messagePanel.anchoredPosition.x, minX, maxX),
                Mathf.Clamp(messagePanel.anchoredPosition.y, minY, maxY)
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
            // Wait for initial click up to avoid accidental progression
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
        /// Advances to the next tutorial step
        /// </summary>
        private void AdvanceToNextStep()
        {
            currentStep++;

            if (currentStep >= tutorialSteps.Count)
            {
                EndTutorial();
            }
            else
            {
                ShowCurrentStep();
            }
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
            {
                clickBlocker.gameObject.SetActive(false);
            }

            // Fade out and hide
            StartCoroutine(FadeOutTutorial());

            // Mark tutorial as completed
            PlayerPrefs.SetInt(playerPrefKey, 1);
            PlayerPrefs.Save();
        }

        private IEnumerator FadeOutTutorial()
        {
            float timer = 0f;
            float duration = 0.3f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                tutorialCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
                yield return null;
            }

            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.gameObject.SetActive(false);
        }

        /// <summary>
        /// Checks if the user has completed the tutorial before
        /// </summary>
        public bool HasCompletedTutorial()
        {
            return PlayerPrefs.GetInt(playerPrefKey, 0) == 1;
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
        /// Forces the tutorial to start (even if completed before)
        /// </summary>
        public void ForceStartTutorial()
        {
            StartTutorial();
        }

        public bool IsTutorialActive()
        {
            return tutorialActive;
        }
    }

    // Modify the TutorialStep class at the bottom of the file

    /// <summary>
    /// Represents a single step in the tutorial sequence
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        public string Message;
        public Transform Target;
        public bool IsUIElement; // Flag to indicate if the target is a UI element
        public Vector2 ArrowOffset = Vector2.zero; // Custom offset for arrow positioning

        public TutorialStep(string message, Transform target, bool isUIElement = false)
        {
            Message = message;
            Target = target;
            IsUIElement = isUIElement;
            ArrowOffset = Vector2.zero;
        }

        public TutorialStep(string message, Transform target, bool isUIElement, Vector2 arrowOffset)
        {
            Message = message;
            Target = target;
            IsUIElement = isUIElement;
            ArrowOffset = arrowOffset;
        }
    }
}