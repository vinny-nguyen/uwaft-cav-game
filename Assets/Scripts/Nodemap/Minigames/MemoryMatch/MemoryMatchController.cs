using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MemoryMatchController : MonoBehaviour
{
    [Serializable]
    public class PairEntry
    {
        [Tooltip("Unique ID for matching, e.g., 'tread'")]
        public string key;

        [Header("Learning Content")]
        [Tooltip("The term (short text), shown on one card of the pair.")]
        public string term;

        [Tooltip("The definition (longer text), shown on the matching card.")]
        [TextArea(2, 5)]
        public string definition;

        [Header("Optional visuals (used on the TERM card)")]
        public Sprite icon;   // optional icon to show with the term
    }

    [Header("UI Refs")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private RectTransform gridRoot;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject winBanner;

    [Header("Prefabs")]
    [SerializeField] private MemoryCard cardPrefab;

    [Header("Config")]
    [Tooltip("Define all pairs (term + definition).")]
    [SerializeField] private List<PairEntry> allPairs = new();

    [Tooltip("How many pairs to play this round.")]
    [SerializeField] private int pairsToUse = 6;

    [Tooltip("Shuffle which pairs are used and their positions.")]
    [SerializeField] private bool shufflePairs = true;

    [Tooltip("Columns in the grid (must also match GridLayoutGroup constraint).")]
    [SerializeField] private int gridColumns = 4;

    [Header("Events")]
    public UnityEvent OnCompleted;

    [Header("Scoring (configurable)")]
    [SerializeField] private string levelId = "Mini2";
    [SerializeField] private string miniGameId = "MemoryMatch";
    [SerializeField] private int pointsPerPair = 10;

    // runtime
    private List<MemoryCard> _cards = new();
    private MemoryCard _first;
    private MemoryCard _second;
    private int _matches;
    private int _moves;
    private bool _inputLocked;
    private bool _ended = false;

    void Awake()
    {
        if (winBanner) winBanner.SetActive(false);
        BuildBoard();
        UpdateHUD();
    }

    private void BuildBoard()
    {
        // Validate and pool
        var pool = new List<PairEntry>(
            allPairs.Where(p =>
                !string.IsNullOrWhiteSpace(p.key) &&
                !string.IsNullOrWhiteSpace(p.term) &&
                !string.IsNullOrWhiteSpace(p.definition))
        );

        if (pool.Count == 0)
        {
            Debug.LogWarning("[MemoryMatch] No valid pairs configured.");
            return;
        }

        if (shufflePairs) Shuffle(pool);

        pairsToUse = Mathf.Clamp(pairsToUse, 1, pool.Count);
        var chosen = pool.Take(pairsToUse).ToList();

        // Build deck: for each pair, create a TERM card and a DEF card
        var deck = new List<CardSpec>(pairsToUse * 2);
        foreach (var p in chosen)
        {
            deck.Add(CardSpec.MakeTerm(p.key, p.term, p.icon));
            deck.Add(CardSpec.MakeDef(p.key, p.definition));
        }

        // Shuffle deck positions
        Shuffle(deck);

        Debug.Log($"[MemoryMatch] Deck built. deckCount={deck.Count}, pairsToUse={pairsToUse}, chosenPairs={chosen.Count}");

        // Clear grid & reset state
        foreach (Transform c in gridRoot) Destroy(c.gameObject);
        _cards.Clear();
        _matches = 0;
        _moves = 0;
        _first = _second = null;
        _inputLocked = false;
        _ended = false;

        // Spawn cards
        foreach (var spec in deck)
        {
            var card = Instantiate(cardPrefab, gridRoot);
            card.InitWordDef(spec.key, spec.displayText, spec.icon, spec.isTerm, this);
            _cards.Add(card);
        }

        Debug.Log($"[MemoryMatch] Spawned cards: {_cards.Count}");
    }

    public void OnCardClicked(MemoryCard card)
    {
        if (_inputLocked || card.IsMatched || card.IsFaceUp) return;

        if (_first == null)
        {
            _first = card;
            card.FlipUp();
            return;
        }

        if (_second == null)
        {
            _second = card;
            card.FlipUp();
            _moves++;
            UpdateHUD();

            // Evaluate: keys must match, but also require one TERM + one DEF
            bool keyMatch = _first.Key == _second.Key;
            bool roleMatch = _first.IsTerm != _second.IsTerm;

            if (keyMatch && roleMatch)
            {
                _first.SetMatched();
                _second.SetMatched();
                _matches++;
                Debug.Log($"[MemoryMatch] Matched {_matches}/{pairsToUse} (cardsSpawned={_cards.Count})");
                _first = _second = null;

                // Update score and upload incrementally on each successful match
                UpdateScoreAndUpload();

                if (_matches >= pairsToUse)
                {
                    if (winBanner) winBanner.SetActive(true);
                    OnCompleted?.Invoke();
                    // Final wrap-up
                    EndGame();
                }
            }
            else
            {
                StartCoroutine(FlipBackRoutine());
            }
        }
    }

    private System.Collections.IEnumerator FlipBackRoutine()
    {
        _inputLocked = true;
        yield return new WaitForSeconds(3f);
        _first.FlipDown();
        _second.FlipDown();
        _first = _second = null;
        _inputLocked = false;
    }

    private void UpdateHUD()
    {
        if (movesText) movesText.text = $"Moves: {_moves}";
        if (progressText) progressText.text = $"{_matches}/{pairsToUse} pairs";
    }

    // utils
    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Internal helper to carry the per-card spec
    private struct CardSpec
    {
        public string key;
        public string displayText;
        public Sprite icon;
        public bool isTerm;

        public static CardSpec MakeTerm(string key, string term, Sprite icon)
            => new CardSpec { key = key, displayText = term, icon = icon, isTerm = true };

        public static CardSpec MakeDef(string key, string def)
            => new CardSpec { key = key, displayText = def, icon = null, isTerm = false };
    }

    // Public API (optional)
    public void ResetGame()
    {
        if (winBanner) winBanner.SetActive(false);
        BuildBoard();
        UpdateHUD();
    }

    // scoring / upload
    public void EndGame()
    {
        if (_ended) return;
        _ended = true;

        int finalScore = CalculateFinalScore();
        Debug.Log($"[MemoryMatch] EndGame called finalScore={finalScore}");

        MinigameScoreHelper.ReportAndUpload(levelId, miniGameId, finalScore);
    }

    private int CalculateFinalScore()
    {
        // default scoring: points per matched pair
        return _matches * Mathf.Max(1, pointsPerPair);
    }

    // New: update score and upload on every successful match
    private void UpdateScoreAndUpload()
    {
        int currentScore = CalculateFinalScore();
        MinigameScoreHelper.ReportAndUpload(levelId, miniGameId, currentScore);
    }

    private void OnDestroy()
    {
        // Clean up event listeners to prevent memory leaks
        OnCompleted?.RemoveAllListeners();
    }
}
