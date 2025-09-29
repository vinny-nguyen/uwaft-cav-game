using UnityEngine;

/// <summary>
/// Controls progression through nodes and persists the active node index.
/// </summary>
public class ProgressionController : MonoBehaviour
{
    private const string KeyActive = "ActiveNodeIndex";
    private const int MinNodeIndex = 1;
    private const int MaxNodeIndex = 6;

    /// <summary>
    /// The currently active node index (range: 1..6).
    /// </summary>
    public int ActiveNodeIndex { get; private set; } = MinNodeIndex;

    private void Awake()
    {
        Load();
    }

    /// <summary>
    /// Loads the active node index from PlayerPrefs.
    /// </summary>
    public void Load()
    {
        ActiveNodeIndex = PlayerPrefs.GetInt(KeyActive, MinNodeIndex);
    }

    /// <summary>
    /// Saves the active node index to PlayerPrefs.
    /// </summary>
    public void Save()
    {
        PlayerPrefs.SetInt(KeyActive, ActiveNodeIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Completes the current node and advances to the next, clamped to valid range.
    /// </summary>
    public void CompleteCurrentNode()
    {
        ActiveNodeIndex = Mathf.Clamp(ActiveNodeIndex + 1, MinNodeIndex, MaxNodeIndex);
        Save();
    }
}
