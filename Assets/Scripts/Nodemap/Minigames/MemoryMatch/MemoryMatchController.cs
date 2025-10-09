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
        [Tooltip("Unique ID for matching e.g., 'tread'")]
        public string key;
        [Tooltip("Optional: sprite shown on the front")]
        public Sprite icon;
        [Tooltip("Optional: text shown on the front")]
        public string label;
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
    [Tooltip("Define all possible pairs here. The controller will pick the first N (or shuffle)")]
    [SerializeField] private List<PairEntry> allPairs = new();
    [Tooltip("How many pairs to play this round.")]
    [SerializeField] private int pairsToUse = 6;
    [Tooltip("Shuffle which pairs are used and their positions.")]
    [SerializeField] private bool shufflePairs = true;
    [Tooltip("Columns in the grid (must match GridLayoutGroup constraint).")]
    [SerializeField] private int gridColumns = 4;

    [Header("Events")]
    public UnityEvent OnCompleted;

    // runtime
    private List<MemoryCard> _cards = new();
    private MemoryCard _first;
    private MemoryCard _second;
    private int _matches;
    private int _moves;
    private bool _inputLocked;

    void Awake()
    {
        if (winBanner) winBanner.SetActive(false);
        BuildBoard();
        UpdateHUD();
    }

    private void BuildBoard()
    {
        // pick pairs
        var pool = new List<PairEntry>(allPairs.Where(p => !string.IsNullOrWhiteSpace(p.key)));
        if (shufflePairs)
            Shuffle(pool);

        pairsToUse = Mathf.Clamp(pairsToUse, 1, pool.Count);
        var chosen = pool.Take(pairsToUse).ToList();

        // build a deck of 2 cards per pair
        var deck = new List<(string key, Sprite icon, string label)>();
        foreach (var p in chosen)
        {
            deck.Add((p.key, p.icon, p.label));
            deck.Add((p.key, p.icon, p.label));
        }

        // shuffle deck positions
        Shuffle(deck);

        // clear grid
        foreach (Transform c in gridRoot) Destroy(c.gameObject);
        _cards.Clear();
        _matches = 0;
        _moves = 0;
        _first = _second = null;
        _inputLocked = false;

        // spawn cards
        foreach (var d in deck)
        {
            var card = Instantiate(cardPrefab, gridRoot);
            card.Init(d.key, d.icon, d.label, this);
            _cards.Add(card);
        }
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

            // evaluate
            if (_first.Key == _second.Key)
            {
                // match
                _first.SetMatched();
                _second.SetMatched();
                _matches++;
                _first = _second = null;

                if (_matches >= pairsToUse)
                {
                    // win
                    if (winBanner) winBanner.SetActive(true);
                    OnCompleted?.Invoke();
                }
            }
            else
            {
                // not a match -> flip back after delay
                StartCoroutine(FlipBackRoutine());
            }
        }
    }

    private System.Collections.IEnumerator FlipBackRoutine()
    {
        _inputLocked = true;
        yield return new WaitForSeconds(0.7f);
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

    // Public API
    public void ResetGame()
    {
        if (winBanner) winBanner.SetActive(false);
        BuildBoard();
        UpdateHUD();
    }
}
