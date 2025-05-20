using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NodeMap.UI; // Added using directive

namespace NodeMap
{
    /// <summary>
    /// Handles the drive button's behavior, enabling it when a node is completed
    /// and transitioning to the next scene when clicked
    /// </summary>
    public class DriveButtonTransition : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private float transitionDelay = 0.2f;

        [Header("Disabled State")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);

        private Button button;
        private Image[] buttonImages;
        private bool isEnabled = false;

        [Header("Error Feedback")]
        [SerializeField] private bool useShakeEffect = true;
        [SerializeField] private float shakeAmount = 0.15f; // This will be used to calculate magnitude
        [SerializeField] private float shakeDuration = 0.15f;
        private float shakeFrequency = 60f; // Define frequency for the shake

        private Coroutine shakeCoroutine;

        private void Awake()
        {
            // Cache components
            button = GetComponent<Button>();
            buttonImages = GetComponentsInChildren<Image>(true);

            // Setup button click handler
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
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

        private void OnButtonClick()
        {
            if (isEnabled)
            {
                StartCoroutine(DelayedSceneLoad());
            }
            else if (useShakeEffect)
            {
                // Show error feedback if button is disabled but clicked
                if (shakeCoroutine != null)
                    StopCoroutine(shakeCoroutine);

                // Calculate magnitude for the ShakeElement method
                float magnitude = shakeAmount * 60f; 
                shakeCoroutine = StartCoroutine(UIAnimator.ShakeElement(transform, shakeDuration, magnitude, shakeFrequency));
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

                // Update button interactable state
                if (button != null)
                {
                    button.interactable = isEnabled;
                }
            }
        }

        private IEnumerator DelayedSceneLoad()
        {
            yield return new WaitForSeconds(transitionDelay);
            LoadTargetScene();
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
            // Stop shake animation if active
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }

            // Reset position if needed - UIAnimator.ShakeElement should handle this,
            // but this is a fallback if the coroutine was stopped abruptly.
            // This part might be redundant if ShakeElement's cleanup is robust.
            // For now, let's assume ShakeElement's cleanup is sufficient.
            // if (transform is RectTransform rt)
            // {
            //     // This line was rt.anchoredPosition = rt.anchoredPosition; which does nothing.
            //     // If a reset to a known original position is needed, that original position must be stored.
            // }
        }
    }
}