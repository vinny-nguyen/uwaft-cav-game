using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NodeMap.UI; // Required for SceneTransitionManager

namespace NodeMap
{
    /// <summary>
    /// Simplified menu button controller that uses Unity's built-in button transitions
    /// </summary>
    public class MenuButtonController : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private string targetSceneName = "MainMenu";

        private Button button;

        private void Awake()
        {
            // Get button component
            button = GetComponent<Button>();

            // Set up button click handler
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        private void OnButtonClick()
        {
            // SaveAndLoadMainMenu(); // Old way
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.PlayClosingTransition(SaveAndLoadMainMenu);
            }
            else
            {
                // Fallback if SceneTransitionManager is not found
                SaveAndLoadMainMenu();
            }
        }

        private void SaveAndLoadMainMenu()
        {
            if (string.IsNullOrEmpty(targetSceneName))
                return;

            // Save progress before scene transition
            if (NopeMapManager.Instance != null)
            {
                NopeMapManager.Instance.SaveNodeProgress();
            }

            // Load main menu scene
            SceneManager.LoadScene(targetSceneName);
        }
    }
}