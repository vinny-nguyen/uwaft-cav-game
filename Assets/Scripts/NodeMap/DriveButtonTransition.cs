using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace NodeMap
{
    /// <summary>
    /// Handles transitioning to a different scene when clicked with hover effects
    /// Only allows transition when current node is completed
    /// </summary>
    public class DriveButtonTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Navigation")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private float transitionDelay = 0.2f;

        [Header("Visual Effects")]
        [SerializeField] private bool useClickAnimation = true;
        [SerializeField] private float clickScaleAmount = 0.9f;
        [SerializeField] private float clickAnimationDuration = 0.1f;

        [Header("Hover Effects")]
        [SerializeField] private bool useHoverAnimation = true;
        [SerializeField] private float hoverScaleAmount = 1.05f; // 5% larger on hover
        [SerializeField] private float hoverAnimationDuration = 0.2f;

        [Header("Disabled State")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        [SerializeField] private float shakeAmount = 0.15f;
        [SerializeField] private float shakeDuration = 0.15f;

        [Header("References")]
        [SerializeField] private Transform car; // Direct reference to the car object

        private Button button;
        private Vector3 originalScale;
        private Coroutine hoverAnimationCoroutine;
        private List<Image> allButtonImages = new List<Image>();
        private bool isEnabled = true;
        private bool isShaking = false;
        private Vector3 originalPosition;

        private void Awake()
        {
            button = GetComponent<Button>();

            // Get all images in hierarchy
            allButtonImages.AddRange(GetComponentsInChildren<Image>(true));

            if (allButtonImages.Count == 0)
            {
                // Debug.LogWarning("No images found in button hierarchy!");
            }

            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
            else
            {
                // Debug.LogError("No Button component found on GameObject with DriveButtonTransition script!");
            }

            // Store original scale and position for animations
            originalScale = transform.localScale;
            originalPosition = transform.localPosition;
        }

        private void Start()
        {
            // Check node status on start
            UpdateButtonState();
        }

        private void Update()
        {
            // Regularly check if the node state has changed
            UpdateButtonState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (useHoverAnimation && isEnabled)
            {
                // Stop any existing hover animation
                if (hoverAnimationCoroutine != null)
                {
                    StopCoroutine(hoverAnimationCoroutine);
                }

                // Start new hover animation (scale up)
                hoverAnimationCoroutine = StartCoroutine(AnimateHoverScale(true));
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (useHoverAnimation)
            {
                // Stop any existing hover animation
                if (hoverAnimationCoroutine != null)
                {
                    StopCoroutine(hoverAnimationCoroutine);
                }

                // Start new hover animation (scale down)
                hoverAnimationCoroutine = StartCoroutine(AnimateHoverScale(false));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // This will catch clicks even when the button is disabled
            if (!isEnabled && !isShaking)
            {
                StartCoroutine(ShakeButton());
                // Debug.Log("Shake animation started for disabled button");
            }
        }

        private void UpdateButtonState()
        {
            // TEMPORARY FOR TESTING - always enabled
            // Remove this line in production
            //isEnabled = true;

            // Get from node manager
            NopeMapManager manager = FindFirstObjectByType<NopeMapManager>();
            if (manager != null)
            {
                int currentNode = manager.CurrentNodeIndex;
                isEnabled = manager.IsNodeCompleted(currentNode - 1);

                Debug.Log($"Button state updated: Node {currentNode}, Completed: {isEnabled}");
            }
            else
            {
                // Debug.LogWarning("NodeMapManager not found!");
            }

            // Update visual state for all images
            foreach (Image img in allButtonImages)
            {
                img.color = isEnabled ? normalColor : disabledColor;
            }

            // Enable/disable button functionality
            if (button != null)
            {
                button.interactable = isEnabled;
            }
        }

        private IEnumerator ShakeButton()
        {
            if (isShaking) yield break;

            // Debug.Log("Starting shake animation");
            isShaking = true;
            RectTransform rectTransform = transform as RectTransform;
            Vector3 originalPosition = rectTransform != null ? rectTransform.anchoredPosition : transform.localPosition;

            // Stronger shake parameters
            float amplitude = shakeAmount * 60f; // Increase multiplier for stronger effect
            float frequency = 60f; // Higher frequency for more "nervous" shake

            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float diminish = 1f - (elapsed / shakeDuration); // Shake diminishes over time
                float offsetX = Mathf.Sin(elapsed * frequency) * amplitude * diminish;

                // Apply shake with more visibility
                if (rectTransform != null)
                {
                    // Use anchoredPosition for UI elements (more reliable)
                    rectTransform.anchoredPosition = originalPosition + new Vector3(offsetX, 0, 0);
                }
                else
                {
                    transform.localPosition = originalPosition + new Vector3(offsetX, 0f, 0f);
                }

                // Visual debug
                // Debug.Log($"Shake position: {offsetX} (Original: {originalPosition}, Current: {(rectTransform != null ? rectTransform.anchoredPosition : transform.localPosition)})");

                yield return null;
            }

            // Reset position
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalPosition;
            }
            else
            {
                transform.localPosition = originalPosition;
            }

            isShaking = false;
            // Debug.Log("Shake animation completed");
        }

        private IEnumerator AnimateHoverScale(bool scaleUp)
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = scaleUp ?
                originalScale * hoverScaleAmount :
                originalScale;

            float elapsed = 0f;

            while (elapsed < hoverAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / hoverAnimationDuration);

                // Ease the scaling animation
                float smoothT = t * t * (3f - 2f * t); // Smoothstep easing

                transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
                yield return null;
            }

            // Ensure we end at exactly the target scale
            transform.localScale = targetScale;
            hoverAnimationCoroutine = null;
        }

        private void OnButtonClick()
        {
            if (useClickAnimation && isEnabled)
            {
                StartCoroutine(AnimateButtonClick());
            }
            else if (isEnabled)
            {
                LoadTargetScene();
            }
        }

        private IEnumerator AnimateButtonClick()
        {
            // For click animation, we need to scale relative to current scale
            // which might be the hover scale
            Vector3 currentScale = transform.localScale;
            Vector3 targetScale = currentScale * clickScaleAmount;

            // Scale down
            float elapsed = 0;
            while (elapsed < clickAnimationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (clickAnimationDuration / 2);
                transform.localScale = Vector3.Lerp(currentScale, targetScale, t);
                yield return null;
            }

            // Scale back up
            elapsed = 0;
            while (elapsed < clickAnimationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (clickAnimationDuration / 2);
                transform.localScale = Vector3.Lerp(targetScale, currentScale, t);
                yield return null;
            }

            // Reset to current (possibly hovered) scale
            transform.localScale = currentScale;

            // Load scene after animation
            yield return new WaitForSeconds(transitionDelay);
            LoadTargetScene();
        }

        private void LoadTargetScene()
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
                // Debug.Log($"Loading scene: {targetSceneName}");
            }
            else
            {
                // Debug.LogError("No target scene specified!");
            }
        }

        private void OnDisable()
        {
            // Reset scale and position when disabled
            transform.localScale = originalScale;
            transform.localPosition = originalPosition;
            isShaking = false;
        }
    }
}