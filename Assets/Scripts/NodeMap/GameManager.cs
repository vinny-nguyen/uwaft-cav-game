using UnityEngine;

public class NodeMapGameManager : MonoBehaviour
{
    public static NodeMapGameManager Instance { get; private set; }

    [Header("Node Progression")]
    public int CurrentNodeIndex { get; private set; } = 1; // starts at Node 1

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentNodeIndex = -1; // start with no active node

    }

    // Call this after quiz completion to unlock next node
    public void AdvanceToNextNode()
    {
        CurrentNodeIndex++;
        Debug.Log($"Advanced to Node {CurrentNodeIndex}");
    }

    // Optional: manually set the active node (e.g., from a save file)
    public void SetCurrentNode(int nodeIndex)
    {
        CurrentNodeIndex = nodeIndex;
        Debug.Log($"Set current node to {CurrentNodeIndex}");
    }
}
