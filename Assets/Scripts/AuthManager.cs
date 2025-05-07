using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] Button signUpButton;
    [SerializeField] Button nextButton;
    [SerializeField] TextMeshProUGUI usernameDisplay;
    [SerializeField] TextMeshProUGUI playerIdDisplay; // New field for Player ID

    void Start()
    {
        signUpButton.onClick.AddListener(OnSignUpClicked);
        nextButton.onClick.AddListener(OnNextClicked);
        nextButton.gameObject.SetActive(false);
        playerIdDisplay.gameObject.SetActive(false); // Hide initially
        usernameDisplay.text = "";

    }

    public async void OnSignUpClicked()
    {
        signUpButton.interactable = false;
        usernameDisplay.text = "Generating your identity...";

        string username = await InitializeAndLogin();

        // Update both displays
        usernameDisplay.text = $"Welcome:\n{username}";
        AuthenticationService.Instance.UpdatePlayerNameAsync(username); // updating the leaderboard ID to username
        playerIdDisplay.text = $"ID:\n{AuthenticationService.Instance.PlayerId}";
        playerIdDisplay.gameObject.SetActive(true); // Show Player ID

        signUpButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(true);
    }

    async Task<string> InitializeAndLogin()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        return GetOrGenerateUsername();
    }

    string GetOrGenerateUsername()
    {
        string username = PlayerPrefs.GetString("GeneratedUsername");
        if (string.IsNullOrEmpty(username))
        {
            username = GenerateRandomUsername();
            //AuthenticationService.Instance.UpdatePlayerNameAsync(username); // updating the leaderboard ID to username
            PlayerPrefs.SetString("GeneratedUsername", username);
        }
        return username;
    }

    void OnNextClicked() => SceneManager.LoadScene("MainMenu");

    string GenerateRandomUsername()
    {
        string[] prefixes = { "Cosmic", "Neon", "Quantum", "Steel", "Phantom" };
        string[] suffixes = { "Fox", "Raptor", "Wizard", "Pioneer", "Samurai" };
        return $"{prefixes[Random.Range(0, prefixes.Length)]}" +
               $"{suffixes[Random.Range(0, suffixes.Length)]}" +
               $"{Random.Range(100, 999)}";
    }
}