using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
            SaveAndLoadMainMenu();
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