using System.Threading.Tasks;
using UnityEngine;

// Utility class to handle minigame score reporting and uploading
public static class MinigameScoreHelper
{
    // Reports the minigame score to ScoreManager and uploads the total score
    public static void ReportAndUpload(string levelId, string miniGameId, int score)
    {
        ReportScore(levelId, miniGameId, score);
        UploadTotalScore();
    }

    // Reports the minigame score to ScoreManager
    private static void ReportScore(string levelId, string miniGameId, int score)
    {
        try
        {
            var scoreManager = GameServices.Instance?.ScoreManager;
            if (scoreManager != null)
            {
                scoreManager.ReportMiniGameScore(levelId, miniGameId, score);
                Debug.Log($"[MinigameScoreHelper] Reported score for {miniGameId}: {score} (Level: {levelId})");
            }
            else
            {
                Debug.LogWarning("[MinigameScoreHelper] ScoreManager not found in GameServices. Skipping score report.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MinigameScoreHelper] Failed to report score: {ex.Message}");
        }
    }

    // Uploads the total score using TotalScoreUploader (fire-and-forget async)
    private static void UploadTotalScore()
    {
        try
        {
            var uploader = GameServices.Instance?.ScoreUploader;
            if (uploader != null)
            {
                _ = UploadAsync(uploader);
            }
            else
            {
                Debug.LogWarning("[MinigameScoreHelper] TotalScoreUploader not found in GameServices. Skipping upload.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MinigameScoreHelper] Failed to upload score: {ex.Message}");
        }
    }

    // Async upload helper method
    private static async Task UploadAsync(TotalScoreUploader uploader)
    {
        try
        {
            await uploader.UploadScoreAsync();
            Debug.Log("[MinigameScoreHelper] Total score uploaded successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MinigameScoreHelper] Upload failed: {ex.Message}");
        }
    }
}
