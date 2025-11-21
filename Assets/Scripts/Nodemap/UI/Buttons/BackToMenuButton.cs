using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nodemap.UI
{
    // Simple back button that returns to the main menu
    [RequireComponent(typeof(Button))]
    public class BackToMenuButton : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnBackClicked);
        }
        
        private void OnBackClicked()
        {
            // Optional: Save current progress before going back
            // PlayerPrefs.Save();
            
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
