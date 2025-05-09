using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace NodeMap
{
    /// <summary>
    /// Handles UI and animations for the Main Menu button with a house icon
    /// </summary>
    public class MenuButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Navigation")]
        [SerializeField] private string targetSceneName = "MainMenu";
        [SerializeField] private float transitionDelay = 0.2f;

        [Header("Visual Effects")]
        [SerializeField] private bool useClickAnimation = true;
        [SerializeField] private float clickScaleAmount = 0.9f;
        [SerializeField] private float clickAnimationDuration = 0.1f;

        [Header("Hover Effects")]
        [SerializeField] private bool useHoverAnimation = true;
        [SerializeField] private float hoverScaleAmount = 1.05f;
        [SerializeField] private float hoverAnimationDuration = 0.2f;
        [SerializeField] private Color normalIconColor = Color.white;
        [SerializeField] private Color hoverIconColor = new Color(0.9f, 0.9f, 1f);

        [Header("Background Animation")]
        [SerializeField] private bool useBackgroundPulse = true;
        [SerializeField] private float pulseAmount = 0.2f;
        [SerializeField] private float pulseDuration = 2f;

        [Header("Icon References")]
        [SerializeField] private Image houseIcon;

        private Button button;
        private Image backgroundImage;
        private Vector3 originalScale;
        private Coroutine hoverCoroutine;
        private Coroutine pulseCoroutine;

        private void Awake()
        {
            // Get component references
            button = GetComponent<Button>();
            backgroundImage = GetComponent<Image>();
            
            // If house icon isn't assigned, try to find it
            if (houseIcon == null)
            {
                // Try to find it among children
                houseIcon = GetComponentInChildren<Image>();
                
                // If the image we found is actually our background, look for the second image
                if (houseIcon == backgroundImage)
                {
                    Image[] images = GetComponentsInChildren<Image>();
                    foreach (Image img in images)
                    {
                        if (img != backgroundImage)
                        {
                            houseIcon = img;
                            break;
                        }
                    }
                }
            }

            // Set up button click handler
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }

            // Store original scale for animations
            originalScale = transform.localScale;
        }

        private void Start()
        {
            // Set initial icon color
            if (houseIcon != null)
            {
                houseIcon.color = normalIconColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (useHoverAnimation)
            {
                // Stop existing hover animation if any
                if (hoverCoroutine != null)
                {
                    StopCoroutine(hoverCoroutine);
                }

                // Start hover animation (scale up)
                hoverCoroutine = StartCoroutine(AnimateHoverScale(true));

                // Change house icon color
                if (houseIcon != null)
                {
                    houseIcon.color = hoverIconColor;
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (useHoverAnimation)
            {
                // Stop existing hover animation if any
                if (hoverCoroutine != null)
                {
                    StopCoroutine(hoverCoroutine);
                }

                // Start hover animation (scale down)
                hoverCoroutine = StartCoroutine(AnimateHoverScale(false));

                // Reset house icon color
                if (houseIcon != null)
                {
                    houseIcon.color = normalIconColor;
                }
            }
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

                // Smoothstep easing
                float smoothT = t * t * (3f - 2f * t);

                transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
                yield return null;
            }

            transform.localScale = targetScale;
            hoverCoroutine = null;
        }

        private void OnButtonClick()
        {
            if (useClickAnimation)
            {
                StartCoroutine(AnimateButtonClick());
            }
            else
            {
                LoadMainMenu();
            }
        }

        private IEnumerator AnimateButtonClick()
        {
            // Store current scale which might be the hover scale
            Vector3 currentScale = transform.localScale;
            Vector3 clickScale = currentScale * clickScaleAmount;

            // Scale down
            float elapsed = 0f;
            while (elapsed < clickAnimationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (clickAnimationDuration / 2);
                transform.localScale = Vector3.Lerp(currentScale, clickScale, t);
                yield return null;
            }

            // Scale back up
            elapsed = 0f;
            while (elapsed < clickAnimationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (clickAnimationDuration / 2);
                transform.localScale = Vector3.Lerp(clickScale, currentScale, t);
                yield return null;
            }

            // Reset scale
            transform.localScale = currentScale;

            // Wait before scene transition
            yield return new WaitForSeconds(transitionDelay);

            // Load the main menu scene
            LoadMainMenu();
        }

        private void LoadMainMenu()
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
            // Cleanup coroutines
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }

            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
                hoverCoroutine = null;
            }

            // Reset scale
            transform.localScale = originalScale;
        }
    }
}