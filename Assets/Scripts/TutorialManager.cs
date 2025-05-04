using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [SerializeField] private GameObject _TutorialOver;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        Time.timeScale = 1f;
    }
    private IEnumerator DelayedGameOver()
    {

        yield return new WaitForSecondsRealtime(1.0f);
        _TutorialOver.SetActive(true);
        Time.timeScale = 0f;
    }

    public void GameOver()
    {
        StartCoroutine(DelayedGameOver());
    }

    public void SendtoNodeMap()
    {
        SceneManager.LoadScene("NodeMap");
    }
}
