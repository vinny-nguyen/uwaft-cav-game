using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class FriendsUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField friendNameInput;
    [SerializeField] private LeaderboardController leaderboard;

    public void OnAddFriendClicked()
    {
        if (friendNameInput == null || leaderboard == null) return;

        string typed = friendNameInput.text.Trim();
        if (string.IsNullOrWhiteSpace(typed)) return;

        string fullName = ResolveToFullName(typed);

        if (fullName == null)
        {
            Debug.Log("[FriendsUI] No matching player found on leaderboard for: " + typed);
            return;
        }

        FriendsManager.Instance?.AddFriend(fullName);
        friendNameInput.text = string.Empty;

        _ = leaderboard.RefreshAsync();
    }

    private string ResolveToFullName(string typed)
    {
        var names = leaderboard.KnownNames;
        if (names == null || names.Count == 0) return null;

        var exact = names.FirstOrDefault(n =>
            n.Equals(typed, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        var prefix = names.FirstOrDefault(n =>
            n.StartsWith(typed, StringComparison.OrdinalIgnoreCase));
        return prefix;
    }
}