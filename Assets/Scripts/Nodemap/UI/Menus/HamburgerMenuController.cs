using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

namespace Nodemap.UI.Menus
{
    /// <summary>
    /// Controls a popup menu with multiple navigation options.
    /// Spawns buttons dynamically based on configured menu options.
    /// </summary>
    public class HamburgerMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject backgroundOverlay;
        [SerializeField] private Transform buttonContainer;
        
        [Header("Button Prefab")]
        [SerializeField] private GameObject menuButtonPrefab;
        
        [Header("Menu Options")]
        [SerializeField] private List<MenuOption> menuOptions = new List<MenuOption>();
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;
        
        private List<GameObject> spawnedButtons = new List<GameObject>();
        private CanvasGroup menuCanvasGroup;
        private CanvasGroup overlayCanvasGroup;
        private bool isOpen = false;

        private void Awake()
        {
            // Get or add CanvasGroup components for fading
            menuCanvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
                menuCanvasGroup = menuPanel.AddComponent<CanvasGroup>();
            
            if (backgroundOverlay != null)
            {
                overlayCanvasGroup = backgroundOverlay.GetComponent<CanvasGroup>();
                if (overlayCanvasGroup == null)
                    overlayCanvasGroup = backgroundOverlay.AddComponent<CanvasGroup>();
            }
            
            // Close menu by default
            HideImmediate();
        }

        /// <summary>
        /// Opens the menu and spawns buttons for each menu option
        /// </summary>
        public void OpenMenu()
        {
            if (isOpen) return;
            
            isOpen = true;
            
            // Clear any existing buttons
            ClearButtons();
            
            // Spawn buttons for each menu option
            SpawnMenuButtons();
            
            // Show the menu with animation
            ShowMenu();
        }

        /// <summary>
        /// Closes the menu
        /// </summary>
        public void CloseMenu()
        {
            if (!isOpen) return;
            
            isOpen = false;
            HideMenu();
        }

        /// <summary>
        /// Toggle menu open/close
        /// </summary>
        public void ToggleMenu()
        {
            if (isOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        private void SpawnMenuButtons()
        {
            if (menuButtonPrefab == null || buttonContainer == null)
            {
                Debug.LogError("[HamburgerMenuController] Menu button prefab or button container not assigned!");
                return;
            }

            foreach (var option in menuOptions)
            {
                GameObject buttonObj = Instantiate(menuButtonPrefab, buttonContainer);
                spawnedButtons.Add(buttonObj);
                
                // Set button text
                TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = option.label;
                }
                
                // Setup button click handler
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    // Capture the option in a local variable for the lambda
                    MenuOption capturedOption = option;
                    button.onClick.AddListener(() => OnMenuOptionClicked(capturedOption));
                }
            }
        }

        private void OnMenuOptionClicked(MenuOption option)
        {
            // Close menu first
            CloseMenu();
            
            // Load the scene if specified
            if (!string.IsNullOrEmpty(option.sceneName))
            {
                SceneManager.LoadScene(option.sceneName);
            }
        }

        private void ClearButtons()
        {
            foreach (var button in spawnedButtons)
            {
                if (button != null)
                    Destroy(button);
            }
            spawnedButtons.Clear();
        }

        private void ShowMenu()
        {
            menuPanel.SetActive(true);
            if (backgroundOverlay != null)
                backgroundOverlay.SetActive(true);
            
            // Simple fade in (you can replace with tweening library like DOTween if preferred)
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }

        private void HideMenu()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        private void HideImmediate()
        {
            menuPanel.SetActive(false);
            if (backgroundOverlay != null)
                backgroundOverlay.SetActive(false);
            
            menuCanvasGroup.alpha = 0f;
            if (overlayCanvasGroup != null)
                overlayCanvasGroup.alpha = 0f;
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;
                
                menuCanvasGroup.alpha = t;
                if (overlayCanvasGroup != null)
                    overlayCanvasGroup.alpha = t * 0.8f; // Slightly transparent overlay
                
                yield return null;
            }
            
            menuCanvasGroup.alpha = 1f;
            if (overlayCanvasGroup != null)
                overlayCanvasGroup.alpha = 0.8f;
        }

        private System.Collections.IEnumerator FadeOut()
        {
            float elapsed = 0f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - (elapsed / fadeOutDuration);
                
                menuCanvasGroup.alpha = t;
                if (overlayCanvasGroup != null)
                    overlayCanvasGroup.alpha = t * 0.8f;
                
                yield return null;
            }
            
            HideImmediate();
        }

        private void OnDestroy()
        {
            ClearButtons();
        }
    }
}
