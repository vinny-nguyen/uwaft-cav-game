using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [SerializeField] private GameObject levelOver;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI distanceText;
    
    [SerializeField] private LeaderDist playerCar;

    private int tryCount = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (playerCar == null)
        {
            playerCar = FindFirstObjectByType<LeaderDist>();
        }
    }

    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        ShowEndScreen();
        levelOver.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ShowEndScreen()
    {
        tryCount++;
        resultText.text = "Great Try!";
        
        if (playerCar != null)
        {
            speedText.text = $"Max Speed:\n{Mathf.RoundToInt(playerCar.MaxSpeed)} km/h";
            distanceText.text = $"Distance:\n{Mathf.RoundToInt(playerCar.TotalDistance)} m";
        }
        else
        {
            speedText.text = "Max Speed:\n0 km/h";
            distanceText.text = "Distance:\n0 m";
        }
    }

    public void GameOver()
    {
        StartCoroutine(DelayedGameOver());
    }

    public void SendtoNodeMap()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("UpdatedNodemap");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}