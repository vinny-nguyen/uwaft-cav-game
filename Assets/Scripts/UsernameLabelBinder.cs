using TMPro;
using UnityEngine;

public class UsernameLabelBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameLabel;

    private void Awake()
    {
        if (usernameLabel == null)
            usernameLabel = GetComponent<TMP_Text>();

        if (usernameLabel == null)
        {
            Debug.LogWarning("[UsernameLabelBinder] No TMP_Text found.");
            return;
        }

        var fullName = PlayerProfile.Username;
        // Strip tag: remove everything starting with '#'
        string displayName = StripTag(fullName);
        Debug.Log($"[UsernameLabelBinder] Setting label to '{name}' on {gameObject.name}");
        usernameLabel.text = displayName;
    }

    private string StripTag(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw;
        int pos = raw.IndexOf('#');
        return pos >= 0 ? raw.Substring(0, pos) : raw;
    }
}