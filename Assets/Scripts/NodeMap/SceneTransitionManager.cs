using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
        }

        // And modify the Start() method:
        private void Start()
        {
            // Start with screen covered
            CoverScreen();

            // Update aspect ratio
            UpdateShaderAspectRatio();

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