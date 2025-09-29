
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

    private void Start()
    {
        if (!prog)
            prog = FindFirstObjectByType<ProgressionController>();

        // 1) Place nodes along spline
        nodePlacer.PlaceNodes();

        // 2) Set node visuals & clickability
        int activeIdx0 = prog.ActiveNodeIndex - 1;
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].BindIndex(i + 1);
            var state = i < activeIdx0 ? NodeState.Completed
                      : NodeState.Inactive;
            nodes[i].SetState(state);
            nodes[i].SetOnClick(null); // Only clickable when active
        }

        // 3) Move car from off-screen to active node
        int totalNodes = nodes.Length; // should be 6
        float a = activeIdx0 / (float)(totalNodes - 1);
        float t = Mathf.Lerp(nodePlacer.tStart, nodePlacer.tEnd, a);

        car.SnapTo(0f); // or wherever your off-screen t is; adjust if needed
        car.MoveTo(t);

        // When car reaches node, activate it and play animation
        StartCoroutine(ActivateNodeWhenCarArrives(activeIdx0, t));
    }

    private System.Collections.IEnumerator ActivateNodeWhenCarArrives(int nodeIdx, float targetT)
    {
        // Wait until car reaches targetT (with small tolerance)
        while (Mathf.Abs(car.transform.position.x - nodes[nodeIdx].transform.position.x) > 0.1f ||
               Mathf.Abs(car.transform.position.y - nodes[nodeIdx].transform.position.y) > 0.1f)
        {
            yield return null;
        }
        // Activate node and play animation
        nodes[nodeIdx].SetState(NodeState.Active);
        nodes[nodeIdx].SetOnClick(() => OnActiveNodeClicked(nodeIdx));
    }

    /// <summary>
    /// Called when the active node is clicked. Opens the learning popup/quiz.
    /// </summary>
    /// <param name="activeIdx0">Zero-based index of the active node.</param>
    private void OnActiveNodeClicked(int activeIdx0)
    {
        Debug.Log($"Node {activeIdx0 + 1} clicked (active). Open popup/quiz.");
        // TODO: Open your LearningPopup here.
    }
}
