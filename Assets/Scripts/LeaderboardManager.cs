//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using Unity.Services.Core;
//using UnityEngine;
//using System.Threading.Tasks;
//using Unity.Services.Leaderboards;
//using Unity.Services.Leaderboards.Models;

//public class LeaderboardManager : MonoBehaviour
//{
//    // Player score script
//    // [HideInInspector] public playerControls playerControls;

//    [SerializeField] private GameObject leaderboardParent;
//    [SerializeField] private Transform leaderboardContentParent;
//    [SerializeField] private Transform leaderboardItemPrefab;

//    private string leaderboardURL = "UWAFT_CAV_Game"; // Replace with your actual leaderboard URL

//    private async void Start()
//    {
//        await UnityServices.InitializeAsync();
//        // need to sign in - already handeled by the AuthManager
//        // await AuthenticatiinService.Instance.SignInAnonymouslyAsync();

//        LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardURL, 0); // Replace with your actual score
//        leaderboardParent.SetActive(false);
//    }

//    private async void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Escape))
//        {
//            if (leaderboardParent.activeInHierarchy)
//            {
//                leaderboardParent.SetActive(false);
//            }
//            else
//            {
//                leaderboardParent.SetActive(true);
//                UpdateLeaderboard();
//            }
//        }
//    }

//    private async void UpdateLeaderboard()
//    {
//        while (Application.isPlaying && leaderboardParent.activeInHierarchy)
//        {
//            LeaderboardScoresPage leaderboardScoresPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardURL); // Replace with your actual leaderboard URL
//            foreach (Transform t in leaderboardContentParent)
//            {
//                Destroy(t.gameObject);  
//            }
//            foreach (LeaderboardEntry entry in leaderboardScoresPage.Results)
//            {
//                Transform leaderboardItem = Instantiate(leaderboardItemPrefab, leaderboardContentParent);
//                leaderboardItem.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = entry.PlayerId;
//                leaderboardItem.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = entry.Score.ToString();
//                leaderboardItem.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = entry.Rank.ToString();
//            }
//            await Task.Delay(500);
//        }
//    }
//}

using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using TMPro;
using System;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardParent;
    [SerializeField] private Transform leaderboardContentParent;
    [SerializeField] private GameObject leaderboardItemPrefab;

    [Header("Settings")]
    [SerializeField] private string leaderboardID = "UWAFT_CAV_Game";
    [SerializeField] private int entriesToShow = 10;

    private bool isInitialized = false;
    private bool isUpdating = false;

    private async void Start()
    {
       if(isInitialized)
        {
            await AddScoreAsync(100);
            await UpdateLeaderboard();
        }
    }
    private async void Awake()
    {
        Debug.Log("Initializing Unity Services...");

        try
        {
            // Initialize Unity Services
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services initialized successfully");

            // Setup authentication event handlers
            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;

            // Sign in anonymously if not already signed in
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Attempting anonymous sign in...");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                isInitialized = true;
                Debug.Log("Already signed in as: " + AuthenticationService.Instance.PlayerId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Initialization failed: {e.Message}");
        }
    }

    private void OnSignedIn()
    {
        isInitialized = true;
        Debug.Log($"Signed in successfully as: {AuthenticationService.Instance.PlayerId}");
    }

    private void OnSignInFailed(RequestFailedException error)
    {
        Debug.LogError($"Sign in failed: {error.Message}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleLeaderboard();
        }
    }

    private async void ToggleLeaderboard()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Cannot toggle leaderboard - services not initialized");
            return;
        }

        if (leaderboardParent.activeSelf)
        {
            leaderboardParent.SetActive(false);
        }
        else
        {
            leaderboardParent.SetActive(true);
            await UpdateLeaderboard();
        }
    }

    public async Task AddScoreAsync(double score)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Cannot add score - services not initialized");
            return;
        }

        try
        {
            Debug.Log($"Attempting to add score: {score}");
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, score);
            Debug.Log("Score added successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to add score: {e.Message}");
        }
    }

    private async Task UpdateLeaderboard()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Cannot update leaderboard - services not initialized");
            return;
        }

        if (isUpdating)
        {
            Debug.LogWarning("Leaderboard update already in progress");
            return;
        }

        isUpdating = true;
        Debug.Log("Updating leaderboard...");

        try
        {
            // Clear existing entries
            foreach (Transform child in leaderboardContentParent)
            {
                Destroy(child.gameObject);
            }

            // Get scores with options
            var options = new GetScoresOptions { Limit = entriesToShow };
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID, options);
            Debug.Log($"Received {scoresResponse.Results.Count} leaderboard entries");

            // Create new entries
            foreach (LeaderboardEntry entry in scoresResponse.Results)
            {
                var leaderboardItem = Instantiate(leaderboardItemPrefab, leaderboardContentParent);

                // Set the text components - adjust indices if needed
                var texts = leaderboardItem.GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 3)
                {
                    texts[0].text = entry.Rank.ToString();
                    texts[1].text = string.IsNullOrEmpty(entry.PlayerName) ? "Anonymous" : entry.PlayerName;
                    texts[2].text = entry.Score.ToString();
                }
                else
                {
                    Debug.LogError("Leaderboard item prefab doesn't have enough text components");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update leaderboard: {e.Message}");

            // Show error message
            var errorItem = Instantiate(leaderboardItemPrefab, leaderboardContentParent);
            var errorTexts = errorItem.GetComponentsInChildren<TMP_Text>();
            if (errorTexts.Length > 0)
            {
                errorTexts[0].text = "Error";
                if (errorTexts.Length > 1) errorTexts[1].text = "Failed to load";
                if (errorTexts.Length > 2) errorTexts[2].text = e.Message;
            }
        }
        finally
        {
            isUpdating = false;
        }
    }

    private void OnDestroy()
    {
        // Clean up event handlers
        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
        }
    }
}