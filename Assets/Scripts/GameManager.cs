using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    
    [SerializeField] private GameObject _GameOver;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        Time.timeScale = 1f;
    }

    private IEnumerator DelayedGameOver() {

        yield return new WaitForSecondsRealtime(1.0f);
        _GameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    public void GameOver() {
        Debug.Log("GameOver method called");
        StartCoroutine(DelayedGameOver());
    }

    public void SendtoNodeMap() {
        Debug.Log("SendtoNodeMap method called");
        Time.timeScale = 1f;
        SceneManager.LoadScene("NodeMapFullHD");
    }

    // public void RestartGame() {
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    // }
}
