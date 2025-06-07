using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NodeMap.Tutorial
{
    /// <summary>
    /// Manages the first-time tutorial flow for players
    /// </summary>
    public class NodeMapTutorialManager : MonoBehaviour
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
        [SerializeField] private float spotlightPulseSpeed = 1f;
        [SerializeField] private float spotlightPulseMagnitude = 0.1f; // Percentage of base radius


        [Header("Engagement Enhancements")]
        [SerializeField] private float typewriterSpeed = 0.1f; // Time between characters
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip tutorialStartSound;
        [SerializeField] private AudioClip stepAdvanceSound;
        [SerializeField] private AudioClip tutorialEndSound;
        #endregion

        #region Private Fields
        private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
        private int currentStep = 0;
        private bool tutorialActive = false;
        private Coroutine arrowAnimationCoroutine;
        private Coroutine typeMessageCoroutine;
        private Camera mainCamera;
        private float pulseTimer = 0f;
        private bool isTypingMessage = false;
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

        private void Update()
        {
            UpdateArrowPosition();

            if (tutorialActive && dimmerPanel != null && dimmerPanel.gameObject.activeInHierarchy)
            {
                pulseTimer += Time.deltaTime;
                // UpdateSpotlight is called via UpdateArrowPosition -> PositionArrowAtTarget,
                // so the pulse effect will be updated.
            }
        }
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

            if (continueText != null)
                continueText.gameObject.SetActive(false);

            // Setup AudioSource
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioSource == null) // If still null, add one
                audioSource = gameObject.AddComponent<AudioSource>();


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
                new Vector2(16.13f, 7.28f),
                0f // Default rotation
            ));

            // Step 2: Show the goal
            tutorialSteps.Add(new TutorialStep(
                "This is your goal. Complete all nodes to reach the final destination!",
                finalNodeTransform,
                false,
                new Vector2(-233.94f, -15.61f),
                78.79f // Default rotation
            ));

            // Step 3: Drive button
            tutorialSteps.Add(new TutorialStep(
                "Click this button to drive when you have completed a node!",
                driveButton,
                true,
                new Vector2(-105.51f, 52.11f),
                80.61f
            ));

            // Step 4: Home button
            tutorialSteps.Add(new TutorialStep(
                "Click this button to go back to the main menu.",
                homeButton,
                true,
                new Vector2(137.57f, -216.42f),
                270.95f
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
            pulseTimer = 0f; // Reset pulse timer for a consistent start

            // Enable components
            tutorialCanvasGroup.gameObject.SetActive(true);

            if (clickBlocker != null)
            {
                clickBlocker.gameObject.SetActive(true);
                clickBlocker.color = new Color(0, 0, 0, 0.01f); // Invisible but blocks raycasts
            }

            PlaySound(tutorialStartSound);
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
            if (typeMessageCoroutine != null)
                StopCoroutine(typeMessageCoroutine);

            // Disable click blocker
            if (clickBlocker != null)
                clickBlocker.gameObject.SetActive(false);

            // Fade out and hide
            StartCoroutine(FadeOutTutorial());

            PlaySound(tutorialEndSound);

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
            PlaySound(stepAdvanceSound);
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

            // Hide continue text initially
            if (continueText != null)
                continueText.gameObject.SetActive(false);

            // Animate text
            if (messageText != null)
            {
                if (typeMessageCoroutine != null)
                    StopCoroutine(typeMessageCoroutine);
                typeMessageCoroutine = StartCoroutine(AnimateTextMessage(step.Message));
            }
            else if (continueText != null) // If no message text, show continue immediately
            {
                continueText.gameObject.SetActive(true);
            }


            // Position arrow and panel
            if (step.Target != null)
            {
                PositionArrowAtTarget(step.Target, step.IsUIElement, step.ArrowOffset, step.ArrowRotation);

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
                    currentTutorialStep.ArrowOffset,
                    currentTutorialStep.ArrowRotation
                );
            }
        }

        /// <summary>
        /// Positions the arrow to point at a target
        /// </summary>
        private void PositionArrowAtTarget(Transform target, bool isUIElement, Vector2 customOffset, float arrowRotation)
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
            arrowImage.localEulerAngles = new Vector3(0, 0, arrowRotation);


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

            // Common pulse factor
            float sinPulse = Mathf.Sin(pulseTimer * spotlightPulseSpeed); // Ranges from -1 to 1

            // --- Radius Pulse ---
            float baseNormalizedRadius = spotlightRadius / Screen.height;
            float radiusPulseOffset = sinPulse * baseNormalizedRadius * spotlightPulseMagnitude;
            float actualNormalizedRadius = baseNormalizedRadius + radiusPulseOffset;
            // Clamp to avoid negative or excessively small radius
            actualNormalizedRadius = Mathf.Max(actualNormalizedRadius, baseNormalizedRadius * (1.0f - spotlightPulseMagnitude * 0.9f));
            actualNormalizedRadius = Mathf.Max(actualNormalizedRadius, 0.01f); // Absolute minimum

            dimmerPanel.material.SetFloat("_Radius", actualNormalizedRadius);
            dimmerPanel.material.SetFloat("_SoftEdge", actualNormalizedRadius * 0.2f); // Soft edge pulses with radius

            // --- Alpha Pulse for "Glow" REMOVED ---
            // The alpha of the material's _Color property is now controlled by 
            // dimmerPanel.color.a (Image component's alpha), which is animated 
            // during FadeInTutorial/FadeOutTutorial and remains constant during a step.
            // We don't need to set _Color here if FadeIn/Out correctly sets dimmerPanel.color
            // and that propagates to the material's _Color property.

            // --- Other Shader Properties ---
            float aspectRatio = (float)Screen.width / Screen.height;
            dimmerPanel.material.SetVector("_Center", new Vector4(viewportPosition.x, viewportPosition.y, 0, 0));
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
            // Store the initial Y to bob around, in case UpdateArrowPosition changes it
            float initialY = originalPosition.y;
            // Store the initial X as well if arrow rotation might affect perceived bobbing axis
            float initialX = originalPosition.x;


            while (tutorialActive)
            {
                time += Time.deltaTime;
                // Bob relative to the current X, but the initial Y plus offset
                float yOffset = Mathf.Sin(time * arrowBobSpeed) * arrowBobAmount;

                // If arrow is rotated, bobbing might need to be adjusted based on rotation
                // For simplicity, we'll keep bobbing on the local Y axis of the arrow's parent (Canvas)
                // If the arrow itself is rotated, this yOffset will still be along the canvas's Y.
                // If you want bobbing along the arrow's rotated "up", that's more complex.
                arrowImage.anchoredPosition = new Vector2(arrowImage.anchoredPosition.x, initialY + yOffset);


                yield return null;
            }
        }

        /// <summary>
        /// Animates the tutorial message text using a typewriter effect.
        /// </summary>
        private IEnumerator AnimateTextMessage(string message)
        {
            if (messageText == null) yield break;

            isTypingMessage = true;
            messageText.text = "";
            foreach (char letter in message.ToCharArray())
            {
                messageText.text += letter;
                yield return new WaitForSeconds(typewriterSpeed);
            }

            // Show continue text after message is typed
            if (continueText != null)
                continueText.gameObject.SetActive(true);

            isTypingMessage = false;
        }

        /// <summary>
        /// Waits for user to click anywhere to continue
        /// </summary>
        private IEnumerator WaitForClick()
        {
            // Delay to avoid accidental progression if click initiated the tutorial step
            // or to give user time to start reading.
            yield return new WaitForSeconds(0.2f);

            while (tutorialActive)
            {
                // Wait until message is fully typed out before accepting click to advance
                if (Input.GetMouseButtonDown(0) && !isTypingMessage)
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
            if (continueText != null) continueText.gameObject.SetActive(false);
            if (messageText != null) messageText.text = ""; // Clear text before fade-in


            if (dimmerPanel != null)
            {
                dimmerPanel.gameObject.SetActive(true);
                dimmerPanel.color = new Color(0, 0, 0, 0);
            }

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

            if (continueText != null)
                continueText.gameObject.SetActive(false);

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

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
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
        public float ArrowRotation = 0f; // Added for arrow rotation

        public TutorialStep(string message, Transform target, bool isUIElement = false)
            : this(message, target, isUIElement, Vector2.zero, 0f) { }

        public TutorialStep(string message, Transform target, bool isUIElement, Vector2 arrowOffset)
            : this(message, target, isUIElement, arrowOffset, 0f) { }


        public TutorialStep(string message, Transform target, bool isUIElement, Vector2 arrowOffset, float arrowRotation)
        {
            Message = message;
            Target = target;
            IsUIElement = isUIElement;
            ArrowOffset = arrowOffset;
            ArrowRotation = arrowRotation;
        }
    }
}