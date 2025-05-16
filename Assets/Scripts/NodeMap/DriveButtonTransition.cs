using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace NodeMap
{
    /// <summary>
    /// Handles the drive button's behavior, enabling it when a node is completed
    /// and transitioning to the next scene when clicked
    /// </summary>
    public class DriveButtonTransition : MonoBehaviour, IPointerClickHandler
    {
        [Header("Navigation")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private float transitionDelay = 0.2f;

        [Header("Visual Effects")]
        [SerializeField] private bool useAnimation = true;
        [SerializeField] private float clickScaleAmount = 0.9f;
        [SerializeField] private float animationDuration = 0.1f;
        
        [Header("Disabled State")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        [SerializeField] private float shakeAmount = 0.15f;
        [SerializeField] private float shakeDuration = 0.15f;

        private Button button;
        private Vector3 originalScale;
        private Image[] buttonImages;
        private bool isEnabled = false;
        private bool isAnimating = false;

        private void Awake()
        {
            // Cache components
            button = GetComponent<Button>();
            buttonImages = GetComponentsInChildren<Image>(true);
            originalScale = transform.localScale;
            
            // Remove the button click listener since we'll handle it ourselves
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        private void Start()
        {
            UpdateButtonState();
        }

        private void Update()
        {
            UpdateButtonState();
        }

        /// <summary>
        /// Unified click handler that works for both enabled and disabled states
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isAnimating)
                return;
                
            // Button is enabled - proceed with click action
            if (isEnabled)
            {
                if (useAnimation)
                    StartCoroutine(AnimateButtonClick());
                else
                    LoadTargetScene();
            }
            // Button is disabled - show shake effect
            else
            {
                StartCoroutine(ShakeButton());
            }
        }

        private void UpdateButtonState()
        {
            // Check if current node is completed
            NopeMapManager manager = NopeMapManager.Instance;
            if (manager != null)
            {
                int currentNode = manager.CurrentNodeIndex;
                isEnabled = manager.IsNodeCompleted(currentNode);

                // Update visual state
                foreach (Image img in buttonImages)
                {
                    img.color = isEnabled ? normalColor : disabledColor;
                }

                // We still update interactable for visual state, but handle clicks ourselves
                if (button != null)
                {
                    button.interactable = isEnabled;
                }
            }
        }

        private IEnumerator ShakeButton()
        {
            isAnimating = true;
            RectTransform rt = transform as RectTransform;
            Vector3 startPos = rt != null ? rt.anchoredPosition : transform.localPosition;
            
            float elapsed = 0f;
            float frequency = 60f;
            float amplitude = shakeAmount * 60f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float diminish = 1f - (elapsed / shakeDuration);
                float offsetX = Mathf.Sin(elapsed * frequency) * amplitude * diminish;
                
                // Apply shake
                if (rt != null)
                    rt.anchoredPosition = startPos + new Vector3(offsetX, 0, 0);
                else
                    transform.localPosition = startPos + new Vector3(offsetX, 0, 0);
                
                yield return null;
            }

            // Reset position
            if (rt != null)
                rt.anchoredPosition = startPos;
            else
                transform.localPosition = startPos;
                
            isAnimating = false;
        }

        private IEnumerator AnimateButtonClick()
        {
            isAnimating = true;
            Vector3 currentScale = transform.localScale;
            Vector3 targetScale = currentScale * clickScaleAmount;
            
            // Scale down
            float elapsed = 0;
            while (elapsed < animationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration / 2);
                transform.localScale = Vector3.Lerp(currentScale, targetScale, t);
                yield return null;
            }
            
            // Scale back up
            elapsed = 0;
            while (elapsed < animationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration / 2);
                transform.localScale = Vector3.Lerp(targetScale, currentScale, t);
                yield return null;
            }
            
            transform.localScale = currentScale;
            
            yield return new WaitForSeconds(transitionDelay);
            LoadTargetScene();
            isAnimating = false;
        }

        private void LoadTargetScene()
        {
            if (string.IsNullOrEmpty(targetSceneName))
                return;
                
            // Save current node progress
            NopeMapManager manager = NopeMapManager.Instance;
            if (manager != null)
            {
                // Let the manager handle saving progress
                manager.SaveNodeProgress();
                SceneManager.LoadScene(targetSceneName);
            }
        }

        private void OnDisable()
        {
            // Reset scale when disabled
            transform.localScale = originalScale;
            isAnimating = false;
        }
    }
}