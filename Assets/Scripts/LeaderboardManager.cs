// using UnityEngine;
// using TMPro;
// using Unity.Services.Leaderboards;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine.UI;

// public class LeaderboardManager : MonoBehaviour
// {
//     [Header("UI References")]
//     [SerializeField] Transform leaderboardContent;
//     [SerializeField] GameObject scoreRowPrefab;

//     [Header("Settings")]
//     [SerializeField] int maxEntries = 10;
//     [SerializeField] string leaderboardId = "high_scores";

//     async void Start()
//     {
//         await LoadAndDisplayLeaderboard();
//     }

//     public async Task SubmitScore(int score)
//     {
//         try
//         {
//             string username = PlayerPrefs.GetString("GeneratedUsername", "Anonymous");

//             await LeaderboardsService.Instance.AddPlayerScoreAsync(
//                 leaderboardId,
//                 score,
//                 new AddPlayerScoreOptions
//                 {
//                     Metadata = new Dictionary<string, string> { { "username", username } }
//                 }
//             );

//             await LoadAndDisplayLeaderboard();
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError($"Score submission failed: {e.Message}");
//         }
//     }

//     public async Task LoadAndDisplayLeaderboard()
//     {
//         try
//         {
//             // Clear existing entries
//             foreach (Transform child in leaderboardContent)
//             {
//                 Destroy(child.gameObject);
//             }

//             // Correct leaderboard fetch with current API
//             var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
//                 leaderboardId,
//                 new GetScoresOptions { Limit = maxEntries }
//             );

//             // Process scores
//             int rank = 1;
//             foreach (var score in scoresResponse.Results.OrderByDescending(s => s.Score))
//             {
//                 var row = Instantiate(scoreRowPrefab, leaderboardContent);
//                 TextMeshProUGUI[] columns = row.GetComponentsInChildren<TextMeshProUGUI>();

//                 columns[0].text = rank.ToString();
//                 columns[1].text = score.Metadata['username'];
//                 columns[2].text = score.Score.ToString();

//                 rank++;
//             }
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError($"Leaderboard load failed: {e.Message}");
//         }
//     }

//     public void RefreshLeaderboard() => _ = LoadAndDisplayLeaderboard();
// }