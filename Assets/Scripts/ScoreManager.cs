using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    // scores[levelId][miniGameId] = best score
    private readonly Dictionary<string, Dictionary<string, int>> scores
        = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ReportMiniGameScore(string levelId, string miniGameId, int score)
    {
        if (string.IsNullOrWhiteSpace(levelId) || string.IsNullOrWhiteSpace(miniGameId))
            return;

        if (!scores.TryGetValue(levelId, out var perLevel))
        {
            perLevel = new Dictionary<string, int>();
            scores[levelId] = perLevel;
        }

        if (!perLevel.ContainsKey(miniGameId) || score > perLevel[miniGameId])
            perLevel[miniGameId] = score;
    }

    public int GetLevelTotal(string levelId)
    {
        if (!scores.TryGetValue(levelId, out var perLevel)) return 0;

        int total = 0;
        foreach (var kvp in perLevel)
            total += kvp.Value;
        return total;
    }

    public int GetOverallTotal()
    {
        int total = 0;
        foreach (var kvp in scores)
            total += GetLevelTotal(kvp.Key);
        return total;
    }

    public void ResetAll()
    {
        scores.Clear();
    }
}

