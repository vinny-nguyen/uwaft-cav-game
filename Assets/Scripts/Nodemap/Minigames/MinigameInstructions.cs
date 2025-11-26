using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Attach this to your minigame prefab root or instructions panel.
/// Shows instructions and disables minigame logic until the user clicks Start.
/// </summary>
public class MinigameInstructions : MonoBehaviour
{
    [Header("Instructions UI")]
    public GameObject instructionsPanel; // The panel to show/hide
    public TextMeshProUGUI instructionsText; // The text field for instructions
    public Button startButton; // The button to start the minigame



    [Header("Minigame Root (optional)")]
    public GameObject minigameRoot; // The root GameObject for minigame logic (disable until start)

    void Awake()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
        // Do not set instructionsText.text here; set it manually in Unity
        if (minigameRoot != null)
            minigameRoot.SetActive(false);
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }
    }

    void OnStartClicked()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
        if (minigameRoot != null)
            minigameRoot.SetActive(true);
        // Optionally: enable other scripts/components here
    }
}
