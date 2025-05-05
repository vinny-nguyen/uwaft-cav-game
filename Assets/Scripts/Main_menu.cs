using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_menu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string loadingSceneName = "LoadingScreen";
    [SerializeField] private string gameSceneName = "NodeMap";
    [SerializeField] private string leaderSceneName = "LeaderBoard";
    [SerializeField] private string tutorialSceneName = "GameStart";

    public void PlayGame()
    {
        // Tell the loading screen what scene to load next
        PlayerPrefs.SetString("TargetScene", gameSceneName);
        SceneManager.LoadScene(loadingSceneName);
    }

    public void OpenTutorial()
    {
        PlayerPrefs.SetString("TargetScene", gameSceneName);
        SceneManager.LoadScene(tutorialSceneName);
    }
    public void OpenLeaderboard()
    {
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(leaderSceneName);
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

