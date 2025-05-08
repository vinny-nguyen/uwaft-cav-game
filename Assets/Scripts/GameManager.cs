using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    
    [SerializeField] private GameObject _Game_Over_Canvas;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        Time.timeScale = 1f;
    }

    private IEnumerator DelayedGameOver() {

        yield return new WaitForSecondsRealtime(3.0f);
        _Game_Over_Canvas.SetActive(true);
        Time.timeScale = 0f;
    }

    public void GameOver() {
        StartCoroutine(DelayedGameOver());
    }

    public void SendtoNodeMap() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("NodeMapFullHD");
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
