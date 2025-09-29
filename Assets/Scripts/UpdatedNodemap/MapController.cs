using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] LevelNodeView[] nodes;       // 6 buttons in order
    [SerializeField] NodePlacer nodePlacer;       // reference to NodePlacer
    [SerializeField] CarPathFollower car;         // CarRoot
    [SerializeField] ProgressionController prog;  // ProgressionController

    void Start()
    {
        if (!prog) prog = FindFirstObjectByType<ProgressionController>();

        // 1) Place nodes along spline
        nodePlacer.PlaceNodes();

        // 2) Set node visuals & clickability
        int activeIdx0 = prog.ActiveNodeIndex - 1;
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].BindIndex(i + 1);
            var state = i < activeIdx0 ? NodeState.Completed
                      : i == activeIdx0 ? NodeState.Active
                      : NodeState.Inactive;
            nodes[i].SetState(state);
            if (state == NodeState.Active)
            {
                nodes[i].SetOnClick(() => OnActiveNodeClicked(activeIdx0));
            }
            else
            {
                nodes[i].SetOnClick(null);
            }
        }

        // 3) Move car from off-screen to active node
        int totalNodes = nodes.Length; // should be 6
        float a = activeIdx0 / (float)(totalNodes - 1);
        float t = Mathf.Lerp(nodePlacer.tStart, nodePlacer.tEnd, a);

        car.SnapTo(0f); // or wherever your off-screen t is; adjust if needed
        car.MoveTo(t);
    }

    void OnActiveNodeClicked(int activeIdx0)
    {
        // Open your LearningPopup here.
        Debug.Log($"Node {activeIdx0 + 1} clicked (active). Open popup/quiz.");
    }
}
