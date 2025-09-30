
using UnityEngine;

/// <summary>
/// Controls the map UI, node placement, and car movement based on progression.
/// </summary>
public class MapController : MonoBehaviour
{

    [SerializeField] private LevelNodeView[] nodes;       // 6 buttons in order
    [SerializeField] private NodePlacer nodePlacer;       // reference to NodePlacer
    [SerializeField] private CarPathFollower car;         // CarRoot
    [SerializeField] private ProgressionController prog;  // ProgressionController

    private Nodemap.NodeProgression nodeProgression;

    // --- Constants ---
    private const float CarSnapStartT = 0f;
    private const float CarArrivalTolerance = 0.1f;

    private void Start()
    {
        if (!prog)
            prog = FindFirstObjectByType<ProgressionController>();

        // Initialize node progression
        nodeProgression = new Nodemap.NodeProgression(nodes.Length);

        // 1) Place nodes along spline
        nodePlacer.PlaceNodes();

        // 2) Set node visuals & clickability
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].BindIndex(i + 1);
            NodeState state = NodeState.Inactive;
            if (nodeProgression.IsCompleted(i))
                state = NodeState.Completed;
            else if (nodeProgression.IsUnlocked(i))
                state = NodeState.Active;
            nodes[i].SetState(state);
            int idx = i;
            nodes[i].SetOnClick(() => OnNodeClicked(idx)); // All nodes clickable, correct index
        }

        // 3) Move car from off-screen to active node
        int activeIdx0 = GetCurrentActiveNodeIndex();
        int totalNodes = nodes.Length;
        float a = activeIdx0 / (float)(totalNodes - 1);
        float t = Mathf.Lerp(nodePlacer.tStart, nodePlacer.tEnd, a);


    car.SnapTo(CarSnapStartT);
    car.MoveTo(t);

        // When car reaches node, activate it and play animation
        StartCoroutine(ActivateNodeWhenCarArrives(activeIdx0, t));
    }

    private System.Collections.IEnumerator ActivateNodeWhenCarArrives(int nodeIdx, float targetT)
    {
        // Wait until car reaches targetT (with small tolerance)
        while (Mathf.Abs(car.transform.position.x - nodes[nodeIdx].transform.position.x) > CarArrivalTolerance ||
               Mathf.Abs(car.transform.position.y - nodes[nodeIdx].transform.position.y) > CarArrivalTolerance)
        {
            yield return null;
        }
        // Activate node and play animation
        nodes[nodeIdx].SetState(NodeState.Active);
        nodes[nodeIdx].SetOnClick(() => OnNodeClicked(nodeIdx));
    }

    /// <summary>
    /// Called when a node is clicked. Handles completion and progression.
    /// </summary>
    /// <param name="nodeIdx">Zero-based index of the node.</param>
    private void OnNodeClicked(int nodeIdx)
    {
        var nodeAnim = nodes[nodeIdx].GetComponent<NodeStateAnimation>();
        if (!nodeProgression.IsUnlocked(nodeIdx))
        {
            Debug.Log($"Node {nodeIdx + 1} is locked. Shake animation.");
            if (nodeAnim != null)
                nodeAnim.PlayLockedShake();
            return;
        }

        Debug.Log($"Node {nodeIdx + 1} clicked. Open popup/quiz.");
        // TODO: Open your LearningPopup here.

        // Mark node as completed and unlock next if applicable
        nodeProgression.CompleteNode(nodeIdx);

        // Update visuals and interactivity for all nodes
        for (int i = 0; i < nodes.Length; i++)
        {
            NodeState state = NodeState.Inactive;
            if (nodeProgression.IsCompleted(i))
                state = NodeState.Completed;
            else if (nodeProgression.IsUnlocked(i))
                state = NodeState.Active;
            nodes[i].SetState(state);
        }
    }

    /// <summary>
    /// Returns the index of the first unlocked but not completed node, or 0 if all are completed.
    /// </summary>
    private int GetCurrentActiveNodeIndex()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodeProgression.IsUnlocked(i) && !nodeProgression.IsCompleted(i))
                return i;
        }
        return 0;
    }
}
