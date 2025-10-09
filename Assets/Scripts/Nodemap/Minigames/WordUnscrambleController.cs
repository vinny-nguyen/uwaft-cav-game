using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WordUnscrambleController : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        [Tooltip("Correct answer, e.g., TREAD")]
        public string answer;
        [Tooltip("Optional hint/clue, e.g., 'Part of the tire that touches the road'")]
        public string hint;
        [Tooltip("Override scrambled display (optional). Leave blank to auto-scramble.")]
        public string customScramble;
    }

    [Header("UI Refs")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text scrambledText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button checkButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject winBanner; // panel shown on completion

    [Header("Config")]
    [SerializeField] private List<Entry> entries = new();
    [Tooltip("Randomize the order of entries each time play starts.")]
    [SerializeField] private bool randomizeOrder = true;
    [Tooltip("Ignore case and punctuation when checking answers.")]
    [SerializeField] private bool forgivingCheck = true;
    [Tooltip("If true, shows answers in ALL CAPS for display & check.")]
    [SerializeField] private bool forceAllCaps = true;
    [Tooltip("Allow the same letters in the same order as answer if scrambling fails multiple times.")]
    [SerializeField] private int scrambleRetries = 15;

    [Header("Events")]
    public UnityEvent OnCompleted; // Hook to your progression / popup close

    private int _currentIndex = -1;
    private List<int> _order = new();
    private bool _roundSolved = false;

    void Awake()
    {
        // Wire buttons
        checkButton.onClick.AddListener(HandleCheck);
        if (skipButton) skipButton.onClick.AddListener(HandleSkip);
        nextButton.onClick.AddListener(HandleNext);

        if (winBanner) winBanner.SetActive(false);
        nextButton.interactable = false;
        SetFeedback("");

        PrepareOrder();
        LoadNextRound();
    }

    private void PrepareOrder()
    {
        _order = Enumerable.Range(0, entries.Count).ToList();
        if (randomizeOrder)
        {
            for (int i = 0; i < _order.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, _order.Count);
                (_order[i], _order[j]) = (_order[j], _order[i]);
            }
        }
    }

    private void LoadNextRound()
    {
        _currentIndex++;
        _roundSolved = false;
        nextButton.interactable = false;
        answerInput.text = "";
        SetFeedback("");

        if (_currentIndex >= _order.Count || entries.Count == 0)
        {
            // Done!
            if (winBanner) winBanner.SetActive(true);
            OnCompleted?.Invoke();
            return;
        }

        var e = entries[_order[_currentIndex]];

        // Display hint
        if (hintText) hintText.text = string.IsNullOrWhiteSpace(e.hint) ? "" : e.hint;

        // Display scrambled
        string display = string.IsNullOrWhiteSpace(e.customScramble)
            ? MakeScramble(e.answer)
            : e.customScramble;

        if (forceAllCaps)
        {
            display = display.ToUpperInvariant();
        }
        scrambledText.text = display;

        // Progress
        if (progressText)
            progressText.text = $"{_currentIndex + 1} / {entries.Count}";
    }

    private string MakeScramble(string answer)
    {
        string clean = NormalizeForDisplay(answer);
        char[] chars = clean.ToCharArray();

        // Shuffle until different (or give up after retries)
        for (int r = 0; r < scrambleRetries; r++)
        {
            Shuffle(chars);
            if (!new string(chars).Equals(clean, StringComparison.InvariantCulture))
                break;
        }
        return new string(chars);
    }

    private static void Shuffle(char[] a)
    {
        for (int i = 0; i < a.Length; i++)
        {
            int j = UnityEngine.Random.Range(i, a.Length);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }

    private void HandleCheck()
    {
        if (_roundSolved) return;

        var e = entries[_order[_currentIndex]];

        string user = answerInput.text ?? "";
        string ans  = e.answer ?? "";

        bool correct = AnswersMatch(user, ans);

        if (correct)
        {
            _roundSolved = true;
            SetFeedback("<color=#1BBB55>Correct!</color>");
            nextButton.interactable = true;
        }
        else
        {
            SetFeedback("<color=#D9534F>Try again</color>");
        }
    }

    private void HandleSkip()
    {
        // Optional skip: move this entry to the end if not solved
        if (_currentIndex < _order.Count)
        {
            int cur = _order[_currentIndex];
            _order.RemoveAt(_currentIndex);
            _order.Add(cur);
            _currentIndex--; // so LoadNextRound() advances to the next
        }
        LoadNextRound();
    }

    private void HandleNext()
    {
        if (!_roundSolved) return;
        LoadNextRound();
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText) feedbackText.text = msg;
    }

    private string NormalizeForDisplay(string s)
    {
        if (s == null) return "";
        var lettersOnly = new string(s.Where(char.IsLetterOrDigit).ToArray());
        return forceAllCaps ? lettersOnly.ToUpperInvariant() : lettersOnly;
    }

    private string NormalizeForCheck(string s)
    {
        if (s == null) return "";
        if (!forgivingCheck) return forceAllCaps ? s.ToUpperInvariant() : s;

        // Remove spaces, punctuation; compare case-insensitively
        var norm = new string(s.Where(char.IsLetterOrDigit).ToArray());
        return forceAllCaps ? norm.ToUpperInvariant() : norm.ToLowerInvariant();
    }

    private bool AnswersMatch(string user, string answer)
    {
        string u = NormalizeForCheck(user);
        string a = NormalizeForCheck(answer);
        if (forceAllCaps) a = a.ToUpperInvariant();
        return string.Equals(u, a, StringComparison.InvariantCulture);
    }

    // --- Public API (optional) ---
    public void ResetGame()
    {
        _currentIndex = -1;
        if (winBanner) winBanner.SetActive(false);
        PrepareOrder();
        LoadNextRound();
    }
}
