using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [SerializeField] private GameObject levelOver;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI distanceText;

    private int tryCount = 0;
    private float highestSpeed = 0f;
    private float distanceTravelled = 0f;

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
        levelOver.SetActive(true);
        Time.timeScale = 0f;
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

    public void ShowEndScreen(float speed, float distance)
    {
        tryCount++;
        endScreen.SetActive(true);

        // Track highest speed and distance
        if (speed > highestSpeed) highestSpeed = speed;
        if (distance > distanceTravelled) distanceTravelled = distance;

        resultText.text = (tryCount % 2 == 0) ? "Great Try!" : "Try Again!";
        speedText.text = $"Max Speed: {Mathf.RoundToInt(highestSpeed)} km/h";
        distanceText.text = $"Distance: {Mathf.RoundToInt(distanceTravelled)} m";
        Time.timeScale = 0f;
    }
}
