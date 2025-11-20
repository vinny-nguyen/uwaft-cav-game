using TMPro;
using UnityEngine;

public class FriendsUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField friendNameInput;
    [SerializeField] private LeaderboardController leaderboard;

    public void OnAddFriendClicked()
    {
        if (friendNameInput == null) return;

        var name = friendNameInput.text;
        if (string.IsNullOrWhiteSpace(name)) return;

        FriendsManager.Instance?.AddFriend(name);
        friendNameInput.text = "";

        if (leaderboard != null)
            _ = leaderboard.RefreshAsync();
    }
}
