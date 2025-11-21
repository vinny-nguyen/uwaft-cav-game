using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Utility class to handle minigame score reporting and uploading.
/// Eliminates code duplication across all minigame controllers.
/// </summary>
public static class MinigameScoreHelper
{
    /// <summary>
    /// Reports the minigame score to ScoreManager and uploads the total score.
    /// </summary>
    /// <param name="levelId">The level identifier (e.g., "Mini1", "Mini2")</param>
    /// <param name="miniGameId">The minigame identifier (e.g., "DragDrop", "MemoryMatch")</param>
    /// <param name="score">The score to report</param>
    public static void ReportAndUpload(string levelId, string miniGameId, int score)
    {
        ReportScore(levelId, miniGameId, score);
        UploadTotalScore();
    }

    /// <summary>
    /// Reports the minigame score to ScoreManager.
    /// </summary>
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

    /// <summary>
    /// Uploads the total score using TotalScoreUploader.
    /// Fire-and-forget async operation.
    /// </summary>
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

    /// <summary>
    /// Async upload helper method.
    /// </summary>
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
