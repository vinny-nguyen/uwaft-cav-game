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

        [Header("Arrow Settings")]
        [SerializeField] private float arrowBobAmount = 20f;
        [SerializeField] private float arrowBobSpeed = 2f;

        [Header("Target References")]
        [SerializeField] private Transform firstNodeTransform;  // First node to highlight
        [SerializeField] private Transform finalNodeTransform;  // Final node to highlight

        [Header("Tutorial Settings")]
        [SerializeField] private bool showTutorialOnStart = true;
        [SerializeField] private string playerPrefKey = "CompletedTutorial";
        [SerializeField] private float fadeInDuration = 0.5f;

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
                Transform currentTarget = tutorialSteps[currentStep].Target;
                if (currentTarget != null)
                {
                    PositionArrowAtTarget(currentTarget);
                }
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
                firstNodeTransform));

            // Step 2: Show the goal
            tutorialSteps.Add(new TutorialStep(
                "This is your goal. Complete all nodes to reach the final destination!",
                finalNodeTransform));

            // Add more steps as needed
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
                Debug.LogWarning("No tutorial steps have been defined!");
                return;
            }

            tutorialActive = true;
            currentStep = 0;

            // Pre-populate the text before showing the canvas
            if (currentStep < tutorialSteps.Count && messageText != null)
            {
                messageText.text = tutorialSteps[currentStep].Message;
            }

            // Enable the tutorial canvas
            tutorialCanvasGroup.gameObject.SetActive(true);

            // Show first step
            StartCoroutine(FadeInTutorial());
        }

        private IEnumerator FadeInTutorial()
        {
            tutorialCanvasGroup.alpha = 0f;

            // Make sure text is set before showing anything
            if (currentStep < tutorialSteps.Count && messageText != null)
            {
                messageText.text = tutorialSteps[currentStep].Message;
            }

            float timer = 0f;

            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                tutorialCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
                yield return null;
            }

            tutorialCanvasGroup.alpha = 1f;
            ShowCurrentStep();
        }
        /// <summary>
        /// Display the current tutorial step
        /// </summary>
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
            {
                messageText.text = step.Message;
            }

            // Position arrow and panel
            if (step.Target != null)
            {
                PositionArrowAtTarget(step.Target);
                // PositionMessagePanelNearTarget(step.Target);

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
        private void PositionArrowAtTarget(Transform target)
        {
            if (target == null || mainCamera == null || arrowImage == null)
            {
                Debug.LogWarning("Missing reference for positioning arrow!");
                return;
            }

            // Get the RectTransform of the canvas
            Canvas canvas = tutorialCanvasGroup.GetComponentInParent<Canvas>();
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

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

                // Position arrow with some offset
                arrowImage.anchoredPosition = localPosition + new Vector2(100, 200);

                // Debug.Log($"Positioning arrow - World: {target.position}, Screen: {screenPosition}, UI: {localPosition}");
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // For Screen Space - Camera canvas
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, target.position);

                // Convert screen position to local position in canvas
                Vector2 localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform, screenPoint, canvas.worldCamera, out localPosition);

                // Position arrow with some offset
                arrowImage.anchoredPosition = localPosition + new Vector2(100, 200);

                Debug.Log($"Positioning arrow - World: {target.position}, Screen: {screenPoint}, UI: {localPosition}");
            }
            else
            {
                // For World Space canvas (unlikely in this case but included for completeness)
                Vector3 worldPosition = target.position + new Vector3(0, 1f, 0); // Add offset in world space
                arrowImage.position = worldPosition;
            }
        }

        private bool IsTargetVisible(Transform target)
        {
            if (target == null || mainCamera == null)
                return false;

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(target.position);
            return (viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                    viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                    viewportPoint.z > 0);
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
    }

    /// <summary>
    /// Represents a single step in the tutorial sequence
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        public string Message;
        public Transform Target;

        public TutorialStep(string message, Transform target)
        {
            Message = message;
            Target = target;
        }
    }
}