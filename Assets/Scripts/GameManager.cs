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

    public void GameOver() {
        _Game_Over_Canvas.SetActive(true);
        Time.timeScale = 0f;

    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
