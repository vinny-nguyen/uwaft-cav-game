using UnityEngine;
using UnityEngine.SceneManagement;

public class BackBtn : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void Go()
    {
        // If you store a previous scene, use that; otherwise just go to menu.
        // var prev = PlayerPrefs.GetString("PreviousScene", mainMenuScene);
        // SceneManager.LoadScene(prev);

        SceneManager.LoadScene(mainMenuScene);
    }
}
