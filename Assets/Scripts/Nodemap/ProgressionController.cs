using UnityEngine;

/// <summary>
/// Centralized controller for node progression, unlocks, completions, and persistence.
/// Attach this to a GameObject in your scene (e.g., "ProgressionController").
/// </summary>
public class ProgressionController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private MapConfig mapConfig;
    
    [Header("Node Progression")]
    [SerializeField] private int nodeCount = 6; // Fallback if mapConfig not set

    private bool[] unlocked;
    private bool[] completed;

    // Events
    public delegate void NodeStateChangedHandler(int nodeIndex, bool unlocked, bool completed);
    public event NodeStateChangedHandler OnNodeStateChanged;
    public delegate void ActiveNodeChangedHandler(int nodeIndex);
    public event ActiveNodeChangedHandler OnActiveNodeChanged;

    public int ActiveNodeIndex { get; private set; } = 0;

    private const string KeyUnlocked = "NodeUnlocked_";
    private const string KeyCompleted = "NodeCompleted_";
    private const string KeyActive = "ActiveNodeIndex";

    private void Awake()
    {
        // Initialize config if not assigned and get actual node count
        if (!mapConfig) mapConfig = MapConfig.Instance;
        nodeCount = mapConfig.nodeCount;
        
        unlocked = new bool[nodeCount];
        completed = new bool[nodeCount];
        Load();
    }

    public bool IsUnlocked(int index) 
    {
        return unlocked != null && index >= 0 && index < unlocked.Length && unlocked[index];
    }
    
    public bool IsUnlocked(NodeId nodeId) => IsUnlocked(nodeId.Value);
    
    public bool IsCompleted(int index) 
    {
        return completed != null && index >= 0 && index < completed.Length && completed[index];
    }
    
    public bool IsCompleted(NodeId nodeId) => IsCompleted(nodeId.Value);

    public void CompleteNode(int index)
    {
        if (index < 0 || index >= nodeCount) return;
        completed[index] = true;
        FireNodeStateChanged(index);
        UnlockNextNode(index);
        Save();
    }

    public void CompleteNode(NodeId nodeId) => CompleteNode(nodeId.Value);

    private void UnlockNextNode(int index)
    {
        int next = index + 1;
        if (next < nodeCount)
        {
            // Only unlock if all previous nodes are completed
            for (int i = 0; i <= index; i++)
            {
                if (!completed[i]) return;
            }
            unlocked[next] = true;
            ActiveNodeIndex = next;
            FireNodeStateChanged(next);
            FireActiveNodeChanged(next);
        }
    }

    public void ResetProgress()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            unlocked[i] = false;
            completed[i] = false;
            FireNodeStateChanged(i);
        }
        unlocked[0] = true;
        ActiveNodeIndex = 0;
        FireNodeStateChanged(0);
        FireActiveNodeChanged(0);
        Save();
    }

    private void FireNodeStateChanged(int index)
    {
        OnNodeStateChanged?.Invoke(index, unlocked[index], completed[index]);
    }

    private void FireActiveNodeChanged(int index)
    {
        OnActiveNodeChanged?.Invoke(index);
    }

    public void Save()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            PlayerPrefs.SetInt(KeyUnlocked + i, unlocked[i] ? 1 : 0);
            PlayerPrefs.SetInt(KeyCompleted + i, completed[i] ? 1 : 0);
        }
        PlayerPrefs.SetInt(KeyActive, ActiveNodeIndex);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            unlocked[i] = PlayerPrefs.GetInt(KeyUnlocked + i, i == 0 ? 1 : 0) == 1;
            completed[i] = PlayerPrefs.GetInt(KeyCompleted + i, 0) == 1;
        }
        ActiveNodeIndex = PlayerPrefs.GetInt(KeyActive, 0);
    }

    /// <summary>
    /// Returns the index of the first unlocked but not completed node, or 0 if all are completed.
    /// </summary>
    public int GetCurrentActiveNodeIndex()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            if (IsUnlocked(i) && !IsCompleted(i))
                return i;
        }
        return 0;
    }
}
