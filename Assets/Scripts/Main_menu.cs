using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_menu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string loadingSceneName = "LoadingScreen";
    [SerializeField] private string gameSceneName = "NodeMapFullHD";
    [SerializeField] private string leaderSceneName = "LeaderBoard";
    [SerializeField] private string tutorialSceneName = "GameStart";

    public void PlayGame()
    {
        PlayerPrefs.SetString("TargetScene", gameSceneName);
        SceneManager.LoadScene(loadingSceneName);
    }

    public void OpenTutorial()
    {
        PlayerPrefs.SetString("TargetScene", tutorialSceneName);
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void OpenLeaderboard()
    {
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(leaderSceneName);
    }

    public void ReturnToPreviousScene()
    {
        var previous = PlayerPrefs.GetString("PreviousScene", "MainMenu");
        SceneManager.LoadScene(previous);
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
}