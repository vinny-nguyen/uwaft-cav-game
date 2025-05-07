using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using TMPro;
using System;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private RectTransform entriesParent; // Changed to RectTransform for better UI control
    [SerializeField] private GameObject entryPrefab;

    [Header("Settings")]
    [SerializeField] private string leaderboardID = "UWAFT_CAV_Game";
    [SerializeField] private int maxEntries = 10;
    [SerializeField] private float refreshCooldown = 2f;

    private bool isReady = false;
    private float lastRefreshTime;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-find prefab if missing in editor
        if (entryPrefab == null)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab Rows");
            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                entryPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (entryPrefab != null)
                {
                    UnityEditor.EditorUtility.SetDirty(this);
                    Debug.LogWarning("Auto-assigned missing prefab reference. Please save this change.");
                }
            }
        }
    }
#endif

    private async void Awake()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            isReady = true;

            // Initialize UI components
            EnsureLayoutComponents();
            ClearEntries();
            await RefreshLeaderboard();

            // Highlight parent
            var img = entriesParent.gameObject.AddComponent<Image>();
            img.color = new Color(1, 0, 0, 0.2f);

            Debug.Log("Leaderboard services ready");
        }
        catch (Exception e)
        {
            Debug.LogError($"Initialization failed: {e.Message}");
        }
    }

    private void EnsureLayoutComponents()
    {
        // Ensure parent has required layout components
        if (!entriesParent.TryGetComponent(out VerticalLayoutGroup _))
        {
            var vlg = entriesParent.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;
        }

        if (!entriesParent.TryGetComponent(out ContentSizeFitter _))
        {
            var csf = entriesParent.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    public async void ToggleLeaderboard()
    {
        if (!isReady)
        {
            Debug.LogWarning("Services not ready");
            return;
        }

        leaderboardPanel.SetActive(!leaderboardPanel.activeSelf);

        if (leaderboardPanel.activeSelf && Time.time > lastRefreshTime + refreshCooldown)
        {
            await RefreshLeaderboard();
        }
    }

    private async Task RefreshLeaderboard()
    {
        if (!VerifyPrefabReference()) return;

        ClearEntries();
        lastRefreshTime = Time.time;

        try
        {
            var scores = await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardID,
                new GetScoresOptions { Limit = maxEntries }
            );

            Debug.Log($"Received {scores.Results.Count} entries");

            if (scores.Results.Count == 0)
            {
                CreatePlaceholderEntry("No scores yet!");
                return;
            }

            foreach (var entry in scores.Results)
            {
                CreateLeaderboardEntry(entry);
            }

            // Force UI update
            LayoutRebuilder.ForceRebuildLayoutImmediate(entriesParent);
            await Task.Yield();
        }
        catch (Exception e)
        {
            Debug.LogError($"Refresh failed: {e.Message}");
            CreatePlaceholderEntry("Connection error");
        }
    }

    private bool VerifyPrefabReference()
    {
        if (entryPrefab != null) return true;

        Debug.LogError("Prefab reference is null! Attempting recovery...");

#if UNITY_EDITOR
        OnValidate(); // Try to auto-recover in editor
#endif

        if (entryPrefab == null)
        {
            Debug.LogError("Prefab recovery failed. Please reassign in Inspector.");
            return false;
        }
        return true;
    }

    private void CreateLeaderboardEntry(LeaderboardEntry entry)
    {
        if (!VerifyPrefabReference()) return;

        try
        {
            var row = Instantiate(entryPrefab, entriesParent, false);
            row.SetActive(true);
            row.name = $"Entry_{entry.Rank}";

            // Add debug background
            var debugBg = row.AddComponent<Image>();
            debugBg.color = new Color(0, 1, 0, 0.1f);

            // SAFE COMPONENT LOOKUP
            TMP_Text FindTextComponent(string name)
            {
                var child = row.transform.Find(name);
                return child?.GetComponent<TMP_Text>();
            }

            var positionText = FindTextComponent("Position");
            var playerText = FindTextComponent("Player");
            var scoreText = FindTextComponent("Score");

            if (positionText == null || playerText == null || scoreText == null)
            {
                debugBg.color = Color.red;
                Debug.LogError("Missing text components in prefab!");
                DestroyImmediate(row);
                return;
            }

            positionText.text = entry.Rank.ToString();
            playerText.text = string.IsNullOrEmpty(entry.PlayerName) ? "Anonymous" : entry.PlayerName;
            scoreText.text = $"{entry.Score:F2}m";
        }
        catch (Exception e)
        {
            Debug.LogError($"Entry creation failed: {e.Message}");
        }
    }

    private void CreatePlaceholderEntry(string message)
    {
        if (!VerifyPrefabReference()) return;

        var entry = Instantiate(entryPrefab, entriesParent);
        var text = entry.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = message;
    }

    private void ClearEntries()
    {
        for (int i = entriesParent.childCount - 1; i >= 0; i--)
        {
            Destroy(entriesParent.GetChild(i).gameObject);
        }
    }

    [ContextMenu("Debug Leaderboard Data")]
    public async void DebugLeaderboardData()
    {
        try
        {
            var scores = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID);
            Debug.Log($"Fetched {scores.Results.Count} entries from cloud:");

            foreach (var entry in scores.Results)
            {
                Debug.Log($"{entry.Rank}. {entry.PlayerName}: {entry.Score}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Fetch failed: {e.Message}");
        }
    }
    [ContextMenu("Visual Test")]
    public async Task VisualTest()
    {
        ClearEntries();
        await RefreshLeaderboard();

        // Highlight parent
        var img = entriesParent.gameObject.AddComponent<Image>();
        img.color = new Color(1, 0, 0, 0.2f);
    }

    [ContextMenu("Test Prefab Reference")]
    public void TestPrefabReference()
    {
        if (VerifyPrefabReference())
        {
            Debug.Log($"Prefab reference valid: {entryPrefab.name}");
        }
    }
}