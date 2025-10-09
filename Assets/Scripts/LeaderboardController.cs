using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BoardTab { Weekly, Friends, Overall }

public class LeaderboardController : MonoBehaviour
{
    [Header("UGS / Boards")]
    [SerializeField] private string weeklyBoardId = "UWAFT_CAV_Game";
    [SerializeField] private string friendsBoardId = "UWAFT_CAV_Game";
    [SerializeField] private string overallBoardId = "UWAFT_CAV_Game";
    [SerializeField] private int fetchLimit = 50;

    [Header("Tabs")]
    [SerializeField] private Button weeklyBtn;
    [SerializeField] private Button friendsBtn;
    [SerializeField] private Button overallBtn;
    [SerializeField] private Image weeklyBg;
    [SerializeField] private Image friendsBg;
    [SerializeField] private Image overallBg;
    [SerializeField] private TMP_Text weeklyLabel;
    [SerializeField] private TMP_Text friendsLabel;
    [SerializeField] private TMP_Text overallLabel;
    [SerializeField] private Sprite chipActive;
    [SerializeField] private Sprite chipInactive;
    [SerializeField] private Color activeLabel = new(1f, 0.93f, 0.55f, 1f);
    [SerializeField] private Color idleLabel = Color.white;

    [Header("Podium (Top 3)")]
    [SerializeField] private TMP_Text firstName;
    [SerializeField] private TMP_Text firstScore;
    [SerializeField] private TMP_Text secondName;
    [SerializeField] private TMP_Text secondScore;
    [SerializeField] private TMP_Text thirdName;
    [SerializeField] private TMP_Text thirdScore;

    [Header("Scroll List")]
    [SerializeField] private Transform content;           // List/Viewport/Content
    [SerializeField] private LeaderboardRow rowPrefab;    // the prefab
    [SerializeField] private TMP_Text statusText;         // optional "Loading…"

    private readonly List<GameObject> _spawned = new();
    private ILeaderboardData _data;
    private BoardTab _current = BoardTab.Weekly;
    private CancellationTokenSource _cts;

    void Awake()
    {
        _data = new UGSLeaderboardData();
        if (weeklyBtn) weeklyBtn.onClick.AddListener(() => SetTab(BoardTab.Weekly));
        if (friendsBtn) friendsBtn.onClick.AddListener(() => SetTab(BoardTab.Friends));
        if (overallBtn) overallBtn.onClick.AddListener(() => SetTab(BoardTab.Overall));
    }

    async void OnEnable()
    {
        ApplyTabVisuals();
        await RefreshAsync();
    }

    string CurrentBoardId() => _current switch
    {
        BoardTab.Weekly => weeklyBoardId,
        BoardTab.Friends => friendsBoardId,
        _ => overallBoardId
    };

    void SetTab(BoardTab t)
    {
        if (_current == t) return;
        _current = t;
        ApplyTabVisuals();
        _ = RefreshAsync();
    }

    void ApplyTabVisuals()
    {
        SetChip(weeklyBg, weeklyLabel, _current == BoardTab.Weekly);
        SetChip(friendsBg, friendsLabel, _current == BoardTab.Friends);
        SetChip(overallBg, overallLabel, _current == BoardTab.Overall);
    }

    void SetChip(Image bg, TMP_Text label, bool active)
    {
        if (bg) bg.sprite = active ? chipActive : chipInactive;
        if (label) label.color = active ? activeLabel : idleLabel;
    }

    public async Task RefreshAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            SetStatus("Loading...");
            ClearList();
            BindPodium(null);

            var list = await _data.GetTopAsync(CurrentBoardId(), fetchLimit, ct);
            if (list == null || list.Count == 0) { SetStatus("No scores yet."); return; }
            SetStatus("");

            // Podium
            BindPodium(list);

            // Rows (from index 3)
            for (int i = 3; i < list.Count; i++)
            {
                var dto = list[i];
                var row = Instantiate(rowPrefab, content);
                row.Bind(dto.Rank, dto.Name, Mathf.RoundToInt((float)dto.Score));
                _spawned.Add(row.gameObject);
            }

            var sr = GetComponent<ScrollRect>();
            if (sr) sr.verticalNormalizedPosition = 1f; // top
        }
        catch (TaskCanceledException) { }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            SetStatus("Connection error");
        }
    }

    private static string TrimName(string s, int max = 12)
    {
        if (string.IsNullOrWhiteSpace(s)) return "Anonymous";
        s = s.Trim();
        return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
    }

    void BindPodium(List<ScoreEntryDTO> l)
    {
        if (l != null && l.Count > 0)
        {
            firstName.text = TrimName(l[0].Name, 12);
            firstScore.text = Mathf.RoundToInt((float)l[0].Score).ToString("N0");
        }
        else { firstName.text = firstScore.text = ""; }

        if (l != null && l.Count > 1)
        {
            secondName.text = TrimName(l[1].Name, 12);
            secondScore.text = Mathf.RoundToInt((float)l[1].Score).ToString("N0");
        }
        else { secondName.text = secondScore.text = ""; }

        if (l != null && l.Count > 2)
        {
            thirdName.text = TrimName(l[2].Name, 12);
            thirdScore.text = Mathf.RoundToInt((float)l[2].Score).ToString("N0");
        }
        else { thirdName.text = thirdScore.text = ""; }
    }

    void ClearList()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
            Destroy(_spawned[i]);
        _spawned.Clear();
    }

    void SetStatus(string s) { if (statusText) statusText.text = s; }
}