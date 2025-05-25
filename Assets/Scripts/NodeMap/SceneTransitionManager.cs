using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Splines; // Required for SplineContainer
using System.Linq; // Added to use .Count() on IEnumerable

namespace NodeMap.UI
{
    /// <summary>
    /// Manages scene entrance and exit transitions with animated effects
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        #region Singleton
        public static SceneTransitionManager Instance { get; private set; }
        #endregion

        #region Inspector Fields
        [Header("Transition Settings")]
        [SerializeField] private float openingDuration = 1.0f;
        [SerializeField] private float closingDuration = 0.8f;
        [SerializeField] private AnimationCurve openingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve closingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Effect Type")]
        [SerializeField] private TransitionEffectType effectType = TransitionEffectType.RadialWipe;
        [SerializeField] private Color transitionColor = Color.black;

        [Header("References")]
        [SerializeField] private Material radialWipeMaterial;
        [SerializeField] private CanvasGroup transitionCanvasGroup;
        [SerializeField] private Image transitionImage;

        [Header("Camera Control (Tutorial Zoom)")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private NodeMap.PlayerSplineMovement playerSplineMovement;
        [SerializeField] private NodeMap.TutorialManager tutorialManager;
        [SerializeField] private float zoomedInOrthographicSize = 2.5f;
        [SerializeField] private float defaultMapOrthographicSize = 5.0f;
        [SerializeField] private Vector3 defaultMapCameraPosition = new Vector3(0, 0, -10);
        [SerializeField] private float zoomAnimationDuration = 1.0f;
        [SerializeField] private Vector2 worldBoundsMin = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 worldBoundsMax = new Vector2(10f, 5f);

        public enum TransitionEffectType
        {
            RadialWipe,
            Fade,
            FadeAndScale
        }
        #endregion

        #region Private Fields
        private Coroutine activeTransition;
        private bool isTransitioning = false;
        private bool isCameraZoomedForTutorial = false;
        private Coroutine cameraAnimationCoroutine;
        #endregion

        #region Public Properties
        public bool IsOpeningTransitionComplete { get; private set; } = false;
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
            InitializeComponents();

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        // And modify the Start() method:
        private void Start()
        {
            // Start with screen covered
            CoverScreen();

            // Update aspect ratio
            UpdateShaderAspectRatio();

            // Camera setup based on tutorial state
            if (mainCamera != null && tutorialManager != null && playerSplineMovement != null && playerSplineMovement.GetSpline() != null)
            {
                if (!tutorialManager.HasCompletedTutorial())
                {
                    isCameraZoomedForTutorial = true;
                    mainCamera.orthographicSize = zoomedInOrthographicSize;

                    SplineContainer playerSpline = playerSplineMovement.GetSpline();
                    // Ensure spline has knots before evaluating
                    if (playerSpline != null && playerSpline.Spline != null && playerSpline.Spline.Knots.Count() > 0)
                    {
                        Vector3 playerInitialWorldPos = playerSpline.transform.TransformPoint(playerSpline.Spline.EvaluatePosition(0f));
                        Vector3 cameraTargetPos = new Vector3(playerInitialWorldPos.x, playerInitialWorldPos.y, defaultMapCameraPosition.z);
                        mainCamera.transform.position = ClampCameraPosition(cameraTargetPos, zoomedInOrthographicSize);
                    }
                    else
                    {
                        Debug.LogWarning("SceneTransitionManager: Player's spline is null or empty. Falling back to default camera for tutorial zoom.");
                        mainCamera.orthographicSize = defaultMapOrthographicSize;
                        mainCamera.transform.position = defaultMapCameraPosition; // Fallback
                        isCameraZoomedForTutorial = false; // Cannot perform special zoom
                    }
                }
                else
                {
                    isCameraZoomedForTutorial = false;
                    mainCamera.orthographicSize = defaultMapOrthographicSize;
                    mainCamera.transform.position = defaultMapCameraPosition;
                }
            }
            else
            {
                isCameraZoomedForTutorial = false;
                if (mainCamera != null) {
                    mainCamera.orthographicSize = defaultMapOrthographicSize;
                    mainCamera.transform.position = defaultMapCameraPosition;
                }
                // Log warnings for missing dependencies
                if (mainCamera == null) Debug.LogWarning("SceneTransitionManager: Main Camera not assigned or found.");
                if (tutorialManager == null) Debug.LogWarning("SceneTransitionManager: TutorialManager not assigned.");
                if (playerSplineMovement == null) Debug.LogWarning("SceneTransitionManager: PlayerSplineMovement not assigned.");
                else if (playerSplineMovement.GetSpline() == null) Debug.LogWarning("SceneTransitionManager: PlayerSplineMovement has no spline assigned.");
            }

            // Start the opening transition
            PlayOpeningTransition();
        }

        private void OnEnable()
        {
            // Subscribe to screen resolution changes
            if (Application.isPlaying)
            {
                UpdateShaderAspectRatio();
            }
        }

        private void Update()
        {
            // Check if screen resolution has changed
            if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height)
            {
                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;
                UpdateShaderAspectRatio();
            }

            // Camera follow logic for tutorial zoom
            if (isCameraZoomedForTutorial && cameraAnimationCoroutine == null && mainCamera != null && playerSplineMovement != null)
            {
                Vector3 playerPos = playerSplineMovement.transform.position;
                // Use the camera's current Z or the default Z for consistency
                Vector3 cameraTargetPos = new Vector3(playerPos.x, playerPos.y, defaultMapCameraPosition.z); 
                mainCamera.transform.position = ClampCameraPosition(cameraTargetPos, mainCamera.orthographicSize);
            }
        }

        private int _lastScreenWidth;
        private int _lastScreenHeight;
        #endregion

        #region Public Methods
        /// <summary>
        /// Plays the scene opening transition
        /// </summary>
        public void PlayOpeningTransition()
        {
            if (isTransitioning) return;
            StopActiveTransition();

            activeTransition = StartCoroutine(PlayTransition(true));
        }

        /// <summary>
        /// Plays the scene closing transition
        /// </summary>
        public void PlayClosingTransition(System.Action onComplete = null)
        {
            if (isTransitioning) return;
            StopActiveTransition();

            activeTransition = StartCoroutine(PlayTransition(false, onComplete));
        }

        public void StartZoomOutMapTransition()
        {
            Debug.Log($"[SceneTransitionManager] StartZoomOutMapTransition called. isCameraZoomedForTutorial: {isCameraZoomedForTutorial}, mainCamera != null: {mainCamera != null}");
            if (isCameraZoomedForTutorial && mainCamera != null)
            {
                if (cameraAnimationCoroutine != null)
                {
                    Debug.Log("[SceneTransitionManager] Stopping existing camera animation coroutine.");
                    StopCoroutine(cameraAnimationCoroutine);
                }
                Debug.Log("[SceneTransitionManager] Starting AnimateCameraToDefaultView coroutine (no tutorial callback).");
                cameraAnimationCoroutine = StartCoroutine(AnimateCameraToDefaultView(null)); // Call with null callback
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] Conditions not met to start zoom out animation.");
            }
        }

        public void InitiateZoomOutAndTutorialSequence()
        {
            Debug.Log($"[SceneTransitionManager] InitiateZoomOutAndTutorialSequence called. isCameraZoomedForTutorial: {isCameraZoomedForTutorial}");
            if (isCameraZoomedForTutorial && mainCamera != null)
            {
                if (cameraAnimationCoroutine != null)
                {
                    Debug.Log("[SceneTransitionManager] Stopping existing camera animation coroutine before starting zoom for tutorial.");
                    StopCoroutine(cameraAnimationCoroutine);
                }
                Debug.Log("[SceneTransitionManager] Starting AnimateCameraToDefaultView coroutine with _StartTutorial callback.");
                cameraAnimationCoroutine = StartCoroutine(AnimateCameraToDefaultView(_StartTutorial));
            }
            else
            {
                // Camera is already in default state or not applicable, start tutorial directly.
                Debug.Log("[SceneTransitionManager] Camera already in default state or not applicable. Starting tutorial directly.");
                _StartTutorial();
            }
        }
        #endregion

        #region Private Methods
        private void InitializeComponents()
        {
            // Create components if not provided
            if (transitionCanvasGroup == null)
            {
                GameObject transitionObj = new GameObject("TransitionCanvas");
                transitionObj.transform.SetParent(transform);

                Canvas canvas = transitionObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999; // Make sure it's on top of everything

                transitionCanvasGroup = transitionObj.AddComponent<CanvasGroup>();

                RectTransform rectTransform = transitionObj.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                GameObject imageObj = new GameObject("TransitionImage");
                imageObj.transform.SetParent(transitionObj.transform);

                transitionImage = imageObj.AddComponent<Image>();
                transitionImage.color = transitionColor;

                RectTransform imageRect = imageObj.GetComponent<RectTransform>();
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = Vector2.zero;
                imageRect.offsetMax = Vector2.zero;
            }

            // Create radial wipe material if needed
            if (radialWipeMaterial == null && effectType == TransitionEffectType.RadialWipe)
            {
                Shader shader = Shader.Find("UI/RadialWipe");
                if (shader != null)
                {
                    radialWipeMaterial = new Material(shader);
                    radialWipeMaterial.SetColor("_Color", transitionColor);
                }
                else
                {
                    // Fall back to fade transition
                    Debug.LogWarning("RadialWipe shader not found. Falling back to fade transition.");
                    effectType = TransitionEffectType.Fade;
                }
            }

            // Set initial state
            if (transitionImage != null && effectType == TransitionEffectType.RadialWipe && radialWipeMaterial != null)
            {
                transitionImage.material = radialWipeMaterial;
            }
        }

        private void CoverScreen()
        {
            // Make sure transition objects are ready
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = 1f;
                transitionCanvasGroup.blocksRaycasts = true;

                if (effectType == TransitionEffectType.RadialWipe && radialWipeMaterial != null)
                {
                    radialWipeMaterial.SetFloat("_Progress", 0f);
                }
                else if (effectType == TransitionEffectType.FadeAndScale && transitionImage != null)
                {
                    transitionImage.rectTransform.localScale = Vector3.one;
                }
            }
        }

        private void StopActiveTransition()
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
                activeTransition = null;
            }
        }

        private IEnumerator PlayTransition(bool isOpening, System.Action onComplete = null)
        {
            isTransitioning = true;

            // Prepare canvas
            transitionCanvasGroup.gameObject.SetActive(true);
            transitionCanvasGroup.blocksRaycasts = true;

            // Ensure the canvas group is opaque for RadialWipe effect to be visible.
            // For Fade effects, alpha is animated in the loop.
            if (effectType == TransitionEffectType.RadialWipe)
            {
                transitionCanvasGroup.alpha = 1f;
            }
            // If opening, and not RadialWipe, alpha will be set in the loop.
            // If opening and RadialWipe, it starts at 1 (from CoverScreen or this set) and material animates.
            // If closing and RadialWipe, it was 0, now set to 1, and material animates.

            float duration = isOpening ? openingDuration : closingDuration;
            AnimationCurve curve = isOpening ? openingCurve : closingCurve;

            float startTime = Time.time;

            while (Time.time - startTime < duration)
            {
                float elapsedTime = Time.time - startTime;
                float normalizedTime = elapsedTime / duration;
                float curvedProgress = curve.Evaluate(normalizedTime);

                // For opening, progress goes 0 to 1. For closing, progress goes 1 to 0.
                float progress = isOpening ? curvedProgress : 1 - curvedProgress;

                switch (effectType)
                {
                    case TransitionEffectType.RadialWipe:
                        if (radialWipeMaterial != null)
                            radialWipeMaterial.SetFloat("_Progress", progress);
                        // Note: transitionCanvasGroup.alpha should be 1 here for RadialWipe, set above.
                        break;

                    case TransitionEffectType.Fade:
                        // Alpha for fade: opening (1 to 0), closing (0 to 1)
                        // progress for opening (0 to 1), progress for closing (1 to 0)
                        // So, alpha = 1 - progress works for both.
                        transitionCanvasGroup.alpha = 1 - progress;
                        break;

                    case TransitionEffectType.FadeAndScale:
                        transitionCanvasGroup.alpha = 1 - progress; // Same alpha logic as Fade
                        // Original scale logic:
                        // float scale = isOpening ? 1 - progress * 0.25f : 1 + progress * 0.25f;
                        // Opening: progress (0->1), scale (1 -> 0.75) - shrinks
                        // Closing: progress (1->0), scale (1.25 -> 1) - shrinks from larger
                        // Let's keep original scale logic unless specified otherwise.
                        float scaleValue = isOpening ? (1.0f - progress * 0.25f) : (1.0f + progress * 0.25f);
                        if (transitionImage != null)
                        {
                            transitionImage.transform.localScale = new Vector3(scaleValue, scaleValue, 1);
                        }
                        break;
                }

                yield return null;
            }

            // Ensure final values are set exactly
            if (isOpening)
            {
                transitionCanvasGroup.alpha = 0f; // Fully transparent after opening
                transitionCanvasGroup.blocksRaycasts = false;

                if (effectType == TransitionEffectType.RadialWipe && radialWipeMaterial != null)
                    radialWipeMaterial.SetFloat("_Progress", 1f); // Fully revealed

                if (effectType == TransitionEffectType.FadeAndScale && transitionImage != null)
                    transitionImage.transform.localScale = Vector3.one; // Reset scale

                transitionCanvasGroup.gameObject.SetActive(false); // Hide after opening
                IsOpeningTransitionComplete = true; // Signal completion
                Debug.Log("[SceneTransitionManager] Opening transition complete.");
            }
            else // Closing
            {
                transitionCanvasGroup.alpha = 1f; // Fully opaque after closing

                if (effectType == TransitionEffectType.RadialWipe && radialWipeMaterial != null)
                    radialWipeMaterial.SetFloat("_Progress", 0f); // Fully covered
                
                if (effectType == TransitionEffectType.FadeAndScale && transitionImage != null)
                {
                    // Final scale for closing based on its logic (e.g., 1.0f or 1.25f depending on interpretation)
                    // The loop ends with progress = 0 for closing.
                    // scaleValue = 1.0f + 0 * 0.25f = 1.0f;
                    transitionImage.transform.localScale = Vector3.one; 
                }
            }

            isTransitioning = false;
            activeTransition = null;

            // Call completion callback if provided
            onComplete?.Invoke();
        }

        private IEnumerator AnimateCameraToDefaultView(System.Action onComplete = null)
        {
            Debug.Log($"[SceneTransitionManager] AnimateCameraToDefaultView started. Has onComplete callback: {onComplete != null}");
            float elapsedTime = 0f;
            Vector3 startingPosition = mainCamera.transform.position;
            float startingOrthoSize = mainCamera.orthographicSize;

            // Ensure target position is clamped if it's the default map position and could be out of bounds
            // However, typically defaultMapCameraPosition should be a safe, central view.
            // For this animation, we lerp towards defaultMapCameraPosition.
            // Clamping during the animation might be jittery; better to ensure defaultMapCameraPosition is valid.
            Vector3 targetPosition = defaultMapCameraPosition;
            float targetOrthoSize = defaultMapOrthographicSize;

            while (elapsedTime < zoomAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / zoomAnimationDuration);
                // Optional: Add easing (e.g., SmoothStep)
                // float easedProgress = progress * progress * (3f - 2f * progress);
                // mainCamera.orthographicSize = Mathf.Lerp(startingOrthoSize, targetOrthoSize, easedProgress);
                // mainCamera.transform.position = Vector3.Lerp(startingPosition, targetPosition, easedProgress);

                mainCamera.orthographicSize = Mathf.Lerp(startingOrthoSize, targetOrthoSize, progress);
                // Lerp towards the target, then clamp. Or, ensure targetPosition is already valid.
                // For simplicity, we lerp directly. If defaultMapCameraPosition is out of bounds, that's a setup issue.
                mainCamera.transform.position = Vector3.Lerp(startingPosition, targetPosition, progress);

                yield return null;
            }

            mainCamera.orthographicSize = targetOrthoSize;
            mainCamera.transform.position = targetPosition; 
            // Optionally, clamp final position if defaultMapCameraPosition might be out of bounds with defaultMapOrthographicSize
            // This ensures the final state is valid, even if the animation path wasn't clamped per frame.
            mainCamera.transform.position = ClampCameraPosition(mainCamera.transform.position, mainCamera.orthographicSize);


            isCameraZoomedForTutorial = false; // Crucial: set this before onComplete
            cameraAnimationCoroutine = null;
            Debug.Log("[SceneTransitionManager] AnimateCameraToDefaultView finished.");

            onComplete?.Invoke();
        }

        private void _StartTutorial()
        {
            Debug.Log("[SceneTransitionManager] _StartTutorial called.");
            if (tutorialManager != null)
            {
                // We call StartTutorial directly, assuming if this sequence is initiated, the tutorial should run.
                // HasCompletedTutorial check is implicitly handled by PlayerSplineMovement before calling InitiateZoomOutAndTutorialSequence.
                if (!tutorialManager.HasCompletedTutorial()) // Still good to double check here
                {
                    Debug.Log("[SceneTransitionManager] Calling tutorialManager.StartTutorial().");
                    tutorialManager.StartTutorial();
                }
                else
                {
                    Debug.Log("[SceneTransitionManager] Tutorial already completed, not starting via _StartTutorial.");
                }
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] TutorialManager reference is null in _StartTutorial.");
            }
        }

        private Vector3 ClampCameraPosition(Vector3 targetPosition, float orthoSize)
        {
            if (mainCamera == null) return targetPosition;

            float camHeight = orthoSize; // Orthographic size is half the height
            float camWidth = orthoSize * mainCamera.aspect; // Half the width

            // Calculate camera view boundaries
            float cameraMinX = worldBoundsMin.x + camWidth;
            float cameraMaxX = worldBoundsMax.x - camWidth;
            float cameraMinY = worldBoundsMin.y + camHeight;
            float cameraMaxY = worldBoundsMax.y - camHeight;
            
            Vector3 clampedPosition = targetPosition;

            // If the viewport is larger than the bounds, center it or handle as desired.
            // For now, Mathf.Clamp will ensure it picks one of the bounds if min > max.
            clampedPosition.x = Mathf.Clamp(targetPosition.x, cameraMinX, cameraMaxX);
            clampedPosition.y = Mathf.Clamp(targetPosition.y, cameraMinY, cameraMaxY);
            // Z position is maintained from targetPosition (which should have the correct Z)
            clampedPosition.z = targetPosition.z; 

            return clampedPosition;
        }
        #endregion

        private void UpdateShaderAspectRatio()
        {
            if (radialWipeMaterial != null)
            {
                float aspectRatio = (float)Screen.width / Screen.height;
                radialWipeMaterial.SetFloat("_AspectRatio", aspectRatio);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateShaderAspectRatio();
        }

    }
}