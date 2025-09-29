using UnityEngine;

public class ProgressionController : MonoBehaviour {
    const string KeyActive = "ActiveNodeIndex";
    public int ActiveNodeIndex { get; private set; } = 1; // 1..6

    void Awake() => Load();
    public void Load() => ActiveNodeIndex = PlayerPrefs.GetInt(KeyActive, 1);
    public void Save() { PlayerPrefs.SetInt(KeyActive, ActiveNodeIndex); PlayerPrefs.Save(); }

    public void CompleteCurrentNode() {
        ActiveNodeIndex = Mathf.Clamp(ActiveNodeIndex + 1, 1, 6);
        Save();
    }
}
