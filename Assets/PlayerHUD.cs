using TMPro;
using UnityEngine;
using Unity.Services.Authentication;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance;

    [SerializeField] private TMP_Text usernameLabel;

    private void Awake()
    {
        // Singleton + keep across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RefreshNameFromStorage();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (usernameLabel) usernameLabel.text = name;
    }

    public void RefreshNameFromStorage()
    {
        // 1) Try cached username from PlayerPrefs
        var name = PlayerPrefs.GetString("GeneratedUsername", "");

        // 2) Fallback to Unity Authentication player name if available
        if (string.IsNullOrWhiteSpace(name))
        {
            if (AuthenticationService.Instance != null &&
                AuthenticationService.Instance.IsSignedIn)
            {
                name = AuthenticationService.Instance.PlayerName;
            }
        }

        // 3) Fallback default
        if (string.IsNullOrWhiteSpace(name))
            name = "Player";

        SetName(name);
    }
}
