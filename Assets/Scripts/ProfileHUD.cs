using TMPro;
using UnityEngine;
using Unity.Services.Authentication;

public class ProfileHUD : MonoBehaviour
{
    public static ProfileHUD Instance;

    [SerializeField] private TMP_Text usernameLabel;

    private void Awake()
    {
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
        var name = PlayerPrefs.GetString("GeneratedUsername", "");

        if (string.IsNullOrWhiteSpace(name) &&
            AuthenticationService.Instance != null &&
            AuthenticationService.Instance.IsSignedIn)
        {
            name = AuthenticationService.Instance.PlayerName;
        }

        if (string.IsNullOrWhiteSpace(name))
            name = "Player";

        SetName(name);
    }
}
