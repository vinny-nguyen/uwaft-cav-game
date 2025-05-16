using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        [SerializeField] private float shakeAmount = 0.15f;
        [SerializeField] private float shakeDuration = 0.15f;

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

                shakeCoroutine = StartCoroutine(ShakeButton());
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

        private IEnumerator ShakeButton()
        {
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

            shakeCoroutine = null;
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

            // Reset position if needed
            if (transform is RectTransform rt)
            {
                rt.anchoredPosition = rt.anchoredPosition;
            }
        }
    }
}