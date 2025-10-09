using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image avatar;        // optional circle
    [SerializeField] private TMP_Text nameText;   // player name
    [SerializeField] private TMP_Text scoreText;  // text inside ScorePill
    [SerializeField] private TMP_Text rankText;   // optional: "#4"
    static string TrimName(string s, int max = 16)
    {
        if (string.IsNullOrWhiteSpace(s)) return "Anonymous";
        s = s.Trim();
        return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
    }
    public void Bind(int rank, string playerName, int score, Sprite avatarSprite = null)
    {
        if (rankText) rankText.text = $"#{rank}";
        if (nameText) nameText.text = TrimName(playerName, 16);
        if (scoreText) scoreText.text = score.ToString("N0");
        if (avatar && avatarSprite) avatar.sprite = avatarSprite;
    }
}
