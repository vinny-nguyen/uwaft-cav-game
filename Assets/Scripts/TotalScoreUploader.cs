using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class TotalScoreUploader : MonoBehaviour
{
    [Header("Leaderboard IDs")]
    [SerializeField] private string overallBoardId = "UWAFT_CAV_Overall";
    [SerializeField] private string weeklyBoardId  = "UWAFT_CAV_Weekly";

    private async Task EnsureUGSAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task UploadScoreAsync()
    {
        try
        {
            await EnsureUGSAsync();

            int miniGamesTotal = ScoreManager.Instance != null
                ? ScoreManager.Instance.GetOverallTotal()
                : 0;

            int distanceBest = PlayerProfile.BestDistance;

            int total = miniGamesTotal + distanceBest;

            await LeaderboardsService.Instance.AddPlayerScoreAsync(overallBoardId, total);
            await LeaderboardsService.Instance.AddPlayerScoreAsync(weeklyBoardId, total);

            Debug.Log($"Uploaded overall score: {total} (minis={miniGamesTotal}, distance={distanceBest})");
        }
        catch (Exception ex)
        {
            Debug.LogError("Upload failed: " + ex);
        }
    }
}

