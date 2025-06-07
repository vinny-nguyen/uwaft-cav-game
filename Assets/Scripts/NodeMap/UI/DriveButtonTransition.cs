using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NodeMap.UI; // Added using directive
using NodeMap.Nodes;

namespace NodeMap
{
    /// <summary>
    /// Handles the drive button's behavior, enabling it when a node is completed
    /// and transitioning to the next scene when clicked.
    /// Also updates its car visuals based on CarUpgradeManager.
    /// </summary>
    public class DriveButtonTransition : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private string targetSceneName;
        // [SerializeField] private float transitionDelay = 0.2f; // Removed, SceneTransitionManager handles duration

        [Header("Disabled State")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);

        private Button button;
        private Image[] buttonImages; // These are for the general button state (color tinting)
        private bool isEnabled = false;

        [Header("Button Car Visuals")]
        [SerializeField] private Image driveButtonCarBodyRenderer;
        [SerializeField] private Image driveButtonFrontWheelRenderer;
        [SerializeField] private Image driveButtonRearWheelRenderer; // Or a single wheel if preferred

        [Header("Error Feedback")]
        [SerializeField] private bool useShakeEffect = true;
        [SerializeField] private float shakeAmount = 0.15f; // This will be used to calculate magnitude
        [SerializeField] private float shakeDuration = 0.15f;
        private float shakeFrequency = 60f; // Define frequency for the shake

        private Coroutine shakeCoroutine;
        private CarUpgradeManager carUpgradeManagerInstance;

        private void Awake()
        {
            // Cache components
            button = GetComponent<Button>();
            buttonImages = GetComponentsInChildren<Image>(true); // This gets ALL images, including car parts.
                                                              // Be mindful if car parts shouldn't be tinted.
                                                              // If car parts have their own Image components,
                                                              // they will be tinted by UpdateButtonState.
                                                              // This might be desired or not.

            // Setup button click handler
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }

            carUpgradeManagerInstance = FindFirstObjectByType<CarUpgradeManager>();
            if (carUpgradeManagerInstance == null)
            {
                Debug.LogWarning("[DriveButtonTransition] CarUpgradeManager instance not found. Button car visuals will not be updated by it.");
            }
        }

        private void OnEnable()
        {
            CarUpgradeManager.OnCarAppearanceUpdated += HandleCarAppearanceUpdated;
            // Attempt to set initial appearance
            if (carUpgradeManagerInstance != null)
            {
                carUpgradeManagerInstance.GetCurrentCarSprites(out Sprite bodySprite, out Sprite wheelSprite);
                UpdateDriveButtonCarVisuals(bodySprite, wheelSprite);
            }
            else
            {
                // Fallback: if no manager, maybe clear sprites or use defaults if any
                UpdateDriveButtonCarVisuals(null, null);
            }
        }

        private void OnDisable()
        {
            CarUpgradeManager.OnCarAppearanceUpdated -= HandleCarAppearanceUpdated;

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

        private void Start()
        {
            UpdateButtonState();
            // Initial car visual update is handled in OnEnable to catch cases where
            // CarUpgradeManager might initialize later or if this object is enabled after CarUpgradeManager.Start.
        }

        private void Update()
        {
            UpdateButtonState();
        }

        private void OnButtonClick()
        {
            if (isEnabled)
            {
                // StartCoroutine(DelayedSceneLoad()); // Old way
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.PlayClosingTransition(LoadTargetScene);
                }
                else
                {
                    // Fallback if SceneTransitionManager is not found
                    LoadTargetScene();
                }
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
            NodeMapManager manager = NodeMapManager.Instance;
            if (manager != null)
            {
                int currentNode = manager.CurrentNodeIndex;
                isEnabled = manager.IsNodeCompleted(currentNode);

                // Update visual state (general button tint)
                foreach (Image img in buttonImages)
                {
                    // Avoid tinting the car part images if they are separate and shouldn't be tinted
                    // This example assumes all child images are part of the button's styleable surface.
                    // If car parts are children and have Image components, they'll be tinted.
                    // To avoid this, you could filter them out:
                    // if (img != driveButtonCarBodyRenderer && img != driveButtonFrontWheelRenderer && img != driveButtonRearWheelRenderer)
                    // {
                    //     img.color = isEnabled ? normalColor : disabledColor;
                    // }
                    // For simplicity, current code tints all child images.
                    img.color = isEnabled ? normalColor : disabledColor;
                }

                // Update button interactable state
                if (button != null)
                {
                    button.interactable = isEnabled;
                }
            }
        }

        private void HandleCarAppearanceUpdated(Sprite carBodySprite, Sprite wheelSprite)
        {
            UpdateDriveButtonCarVisuals(carBodySprite, wheelSprite);
        }

        private void UpdateDriveButtonCarVisuals(Sprite carBodySprite, Sprite wheelSprite)
        {
            if (driveButtonCarBodyRenderer != null)
            {
                driveButtonCarBodyRenderer.sprite = carBodySprite;
                driveButtonCarBodyRenderer.enabled = carBodySprite != null; // Hide if no sprite
            }

            if (driveButtonFrontWheelRenderer != null)
            {
                driveButtonFrontWheelRenderer.sprite = wheelSprite;
                driveButtonFrontWheelRenderer.enabled = wheelSprite != null; // Hide if no sprite
            }

            if (driveButtonRearWheelRenderer != null)
            {
                driveButtonRearWheelRenderer.sprite = wheelSprite; // Assuming same sprite for both wheels on button
                driveButtonRearWheelRenderer.enabled = wheelSprite != null; // Hide if no sprite
            }
        }

        private void LoadTargetScene()
        {
            if (string.IsNullOrEmpty(targetSceneName))
                return;

            // Save current node progress
            NodeMapManager manager = NodeMapManager.Instance;
            if (manager != null)
            {
                // Let the manager handle saving progress
                manager.SaveNodeProgress();
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}