using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_menu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string loadingSceneName = "LoadingScreen";
    [SerializeField] private string gameSceneName = "NodeMap";
    [SerializeField] private string settingsSceneName = "SettingsMenu";

    public void PlayGame()
    {
        // Tell the loading screen what scene to load next
        PlayerPrefs.SetString("TargetScene", gameSceneName);
        SceneManager.LoadScene(loadingSceneName);
    }

    public void OpenSettings()
    {
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(settingsSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ReturnToPreviousScene()
    {
        string previousScene = PlayerPrefs.GetString("PreviousScene", "MainMenu");
        SceneManager.LoadScene(previousScene);
    }
}

