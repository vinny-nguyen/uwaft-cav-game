using UnityEngine;

public class NodeMapGameManager : MonoBehaviour
{
    public static NodeMapGameManager Instance { get; private set; }

    [Header("Node Progression")]
    public int CurrentActiveNodeIndex { get; private set; } = 1; // starts at Node 1

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentActiveNodeIndex = -1; // start with no active node

    }

    // Call this after quiz completion to unlock next node
    public void AdvanceToNextNode()
    {
        CurrentActiveNodeIndex++;
        Debug.Log($"Advanced to Node {CurrentActiveNodeIndex}");
    }

    // Optional: manually set the active node (e.g., from a save file)
    public void SetActiveNode(int nodeIndex)
    {
        CurrentActiveNodeIndex = nodeIndex;
        Debug.Log($"Set active node to {CurrentActiveNodeIndex}");
    }
}
