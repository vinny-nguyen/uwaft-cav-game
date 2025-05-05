using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] InputField usernameInput;
    [SerializeField] GameObject authPanel;
    [SerializeField] GameObject leaderboardPanel;

    private string generatedUsername;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        ShowAuthUI();
    }

    void ShowAuthUI()
    {
        authPanel.SetActive(true);
        leaderboardPanel.SetActive(false);
    }

    // Called from "Play Anonymously" button
    public async void OnAnonymousLogin()
    {
        generatedUsername = GenerateRandomUsername();
        await SignInWithUsername(generatedUsername);
    }

    // Called from "Submit Username" button
    public async void OnCustomUsernameSubmit()
    {
        if (!string.IsNullOrEmpty(usernameInput.text))
        {
            generatedUsername = usernameInput.text;
            await SignInWithUsername(generatedUsername);
        }
    }

    private async Task SignInWithUsername(string username)
    {
        try
        {
            // 1. Anonymous sign-in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            // 2. Store username locally
            PlayerPrefs.SetString("PlayerUsername", username);

            // 3. Transition to game
            authPanel.SetActive(false);
            leaderboardPanel.SetActive(true);

            Debug.Log($"Signed in as: {username}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Login failed: {e.Message}");
        }
    }

    private string GenerateRandomUsername()
    {
        string[] adjectives = { "Mighty", "Swift", "Golden", "Epic", "Fluffy" };
        string[] nouns = { "Wolf", "Dragon", "Phoenix", "Warrior", "Panda", "Platypus", "Lion", "Tiger", "Cat" };
        return $"{adjectives[UnityEngine.Random.Range(0, adjectives.Length)]}{nouns[UnityEngine.Random.Range(0, nouns.Length)]}{UnityEngine.Random.Range(100, 999)}";
    }
}
