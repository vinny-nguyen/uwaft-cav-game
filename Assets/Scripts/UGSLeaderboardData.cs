using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

public struct ScoreEntryDTO
{
    public int Rank;
    public string Name;
    public double Score;
}

public interface ILeaderboardData
{
    Task<List<ScoreEntryDTO>> GetTopAsync(string boardId, int limit, CancellationToken ct);
}

public class UGSLeaderboardData : ILeaderboardData
{
    bool _ready;

    async Task EnsureInitAsync()
    {
        if (_ready) return;
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsAuthorized)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _ready = true;
    }

    public async Task<List<ScoreEntryDTO>> GetTopAsync(string boardId, int limit, CancellationToken ct)
    {
        await EnsureInitAsync();

        // Call without cancellation token
        var res = await LeaderboardsService.Instance.GetScoresAsync(
            boardId,
            new GetScoresOptions { Limit = limit }
        );

        var list = new List<ScoreEntryDTO>(res.Results.Count);
        foreach (var e in res.Results)
        {
            list.Add(new ScoreEntryDTO
            {
                Rank = e.Rank,
                Name = string.IsNullOrEmpty(e.PlayerName) ? "Anonymous" : e.PlayerName,
                Score = e.Score
            });
        }
        return list;
    }
}