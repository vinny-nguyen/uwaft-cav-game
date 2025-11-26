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
        [Tooltip("Array of hints (up to 3), shown progressively when hint button is clicked")]
        public string[] hints = new string[3];
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
    [SerializeField] private Button hintButton;
    [SerializeField] private ParticleSystem confettiFX;

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

    [Header("Scoring")]
    [SerializeField] private string levelId = "Mini3";
    [SerializeField] private string miniGameId = "WordUnscramble";
    [SerializeField] private int pointsPerWord = 10;

    private int _solvedCount = 0;   // runtime counter

    private int _currentIndex = -1;
    private List<int> _order = new();
    private bool _roundSolved = false;
    private int _currentHintIndex = 0; // Tracks which hint to show next (0-2)

    void Awake()
    {
        // Wire buttons
        checkButton.onClick.AddListener(HandleCheck);
        if (skipButton) skipButton.onClick.AddListener(HandleSkip);
        if (hintButton) hintButton.onClick.AddListener(HandleHint);

        if (confettiFX != null) confettiFX.gameObject.SetActive(false);
        SetFeedback("");

        _solvedCount = 0;

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
        _currentHintIndex = 0; // Reset hint index for new round
        answerInput.text = "";
        SetFeedback("");

        if (_currentIndex >= _order.Count || entries.Count == 0)
        {
            // Done!
            if (confettiFX != null)
            {
                confettiFX.gameObject.SetActive(true);
                confettiFX.Play();
            }
            OnCompleted?.Invoke();
            return;
        }

        var e = entries[_order[_currentIndex]];

        // Display first hint and update hint button state
        UpdateHintDisplay();

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
        string ans = e.answer ?? "";

        bool correct = AnswersMatch(user, ans);

        if (correct)
        {
            _roundSolved = true;
            _solvedCount++;
            SetFeedback("<color=#1BBB55>Correct!</color>");
            if (hintButton) hintButton.interactable = false; // Disable hint button after solving
            // Update score immediately then advance after a short pause so user sees feedback
            UpdateScoreAndUpload();
            StartCoroutine(AdvanceAfterCorrect());
        }
        else
        {
            SetFeedback("<color=#D9534F>Wrong — try a hint</color>");
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



    private void HandleHint()
    {
        if (_roundSolved) return; // No hints after solving
        if (_currentIndex < 0 || _currentIndex >= _order.Count) return;

        var e = entries[_order[_currentIndex]];

        // Show next hint if available
        if (_currentHintIndex < e.hints.Length && _currentHintIndex < 3)
        {
            _currentHintIndex++;
            UpdateHintDisplay();
        }
    }

    private void UpdateHintDisplay()
    {
        if (_currentIndex < 0 || _currentIndex >= _order.Count)
        {
            if (hintText) hintText.text = "";
            if (hintButton) hintButton.interactable = false;
            return;
        }

        var e = entries[_order[_currentIndex]];

        // Show only the current hint (not accumulated)
        string hintDisplay = "";
        if (_currentHintIndex > 0 && _currentHintIndex <= e.hints.Length && _currentHintIndex <= 3)
        {
            int hintIdx = _currentHintIndex - 1; // Convert to 0-based index
            if (!string.IsNullOrWhiteSpace(e.hints[hintIdx]))
            {
                int totalHints = Math.Min(3, e.hints.Length);
                hintDisplay = $"Hint {_currentHintIndex}/{totalHints}: {e.hints[hintIdx]}";
            }
        }

        if (hintText)
        {
            // If no hint has been shown yet, show a gentle prompt encouraging the player
            if (string.IsNullOrWhiteSpace(hintDisplay) && _currentHintIndex == 0)
            {
                hintText.text = "Stuck? Use a hint from some help";
            }
            else
            {
                hintText.text = hintDisplay;
            }
        }

        // Disable hint button if all 3 hints shown or no more hints available
        if (hintButton)
        {
            bool hasMoreHints = _currentHintIndex < 3 &&
                                _currentHintIndex < e.hints.Length &&
                                !string.IsNullOrWhiteSpace(e.hints[_currentHintIndex]);
            hintButton.interactable = hasMoreHints && !_roundSolved;
        }
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
        _solvedCount = 0;
        if (confettiFX != null) confettiFX.gameObject.SetActive(false);
        PrepareOrder();
        LoadNextRound();
    }

    private int CalculateScore()
    {
        // simple scoring: X points per correctly solved word
        return _solvedCount * Mathf.Max(1, pointsPerWord);
    }

    private void UpdateScoreAndUpload()
    {
        int score = CalculateScore();
        MinigameScoreHelper.ReportAndUpload(levelId, miniGameId, score);
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (checkButton != null)
            checkButton.onClick.RemoveListener(HandleCheck);
        if (skipButton != null)
            skipButton.onClick.RemoveListener(HandleSkip);
        // nextButton removed from UI; no cleanup needed
        if (hintButton != null)
            hintButton.onClick.RemoveListener(HandleHint);

        // Clean up event listeners
        OnCompleted?.RemoveAllListeners();
    }

    private System.Collections.IEnumerator AdvanceAfterCorrect()
    {
        yield return new WaitForSeconds(2f);
        LoadNextRound();
    }
}
