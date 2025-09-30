using UnityEngine;
/// <summary>
/// Controls the map UI, node placement, and car movement based on progression.
/// </summary>
public class MapController : MonoBehaviour
{

    [SerializeField] private LevelNodeView[] nodes;       // 6 buttons in order
    [Header("World")]
    [SerializeField] private UnityEngine.Splines.SplineContainer spline;   // your world spline

    [Header("UI")]
    [SerializeField] private Camera uiCamera;          // the Canvas' camera (Main Camera)
    [SerializeField] private RectTransform canvasRect; // root Canvas RectTransform
    [SerializeField] private RectTransform[] nodeRects; // your 6 node buttons, in order

    [Header("Range on Spline (normalized t)")]
    [Range(0f, 1f)] public float tStart = 0.2f;
    [Range(0f, 1f)] public float tEnd = 0.8f;

    [SerializeField] private CarPathFollower car;         // CarRoot
    [SerializeField] private ProgressionController progressionController;  // Centralized progression



    // --- Constants ---
    private const float CarSnapStartT = 0f;
    private const float CarArrivalTolerance = 0.1f;

    private void Start()
    {
        if (!progressionController)
            progressionController = FindFirstObjectByType<ProgressionController>();

        // Subscribe to events
        progressionController.OnNodeStateChanged += HandleNodeStateChanged;
        progressionController.OnActiveNodeChanged += HandleActiveNodeChanged;

        // 1) Place nodes along spline
        PlaceNodes();

        // 2) Set node visuals & clickability (all start as Inactive, no animation)
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].BindIndex(i + 1);
            nodes[i].SetState(NodeState.Inactive, false); // no animation on setup
            int idx = i;
            nodes[i].SetOnClick(() => OnNodeClicked(idx));
        }

        // 3) Animate car passing completed nodes and update their state as car passes
        int activeIdx0 = progressionController.GetCurrentActiveNodeIndex();
        int totalNodes = nodes.Length;
        StartCoroutine(AnimateCarAndCompletedNodes(activeIdx0, totalNodes));
    }

    // Helper coroutine to move car using CarPathFollower's private MoveAlongSpline
    private System.Collections.IEnumerator MoveCarTo(float fromT, float toT, bool straightenAtEnd, bool eased)
    {
        var moveAlong = car.GetType().GetMethod("MoveAlongSpline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (moveAlong != null)
        {
            var paramCount = moveAlong.GetParameters().Length;
            object[] parameters;
            if (paramCount == 4)
                parameters = new object[] { fromT, toT, straightenAtEnd, eased };
            else if (paramCount == 3)
                parameters = new object[] { fromT, toT, straightenAtEnd };
            else
                parameters = new object[] { fromT, toT };
            var enumerator = (System.Collections.IEnumerator)moveAlong.Invoke(car, parameters);
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
        else
        {
            car.SnapTo(toT);
            yield return null;
        }
    }

    // Animate car passing completed nodes and update their state as car passes (original logic)
    private System.Collections.IEnumerator AnimateCarAndCompletedNodes(int activeIdx0, int totalNodes)
    {
        float[] nodeT = new float[totalNodes];
        for (int i = 0; i < totalNodes; i++)
            nodeT[i] = Mathf.Lerp(tStart, tEnd, i / (float)(totalNodes - 1));

        car.SnapTo(CarSnapStartT);
        float prevT = CarSnapStartT;
        for (int i = 0; i < activeIdx0; i++)
        {
            float t = nodeT[i];
            yield return StartCoroutine(MoveCarTo(prevT, t, false, false)); // Linear, no ease, no straighten
            nodes[i].SetState(NodeState.Completed, true);
            prevT = t;
        }
        float activeT = nodeT[activeIdx0];
        yield return StartCoroutine(MoveCarTo(prevT, activeT, true, true)); // Eased, straighten at end
        nodes[activeIdx0].SetState(NodeState.Active, true);
        nodes[activeIdx0].SetOnClick(() => OnNodeClicked(activeIdx0));
    }



    void OnRectTransformDimensionsChange() => PlaceNodes(); // re-place on resize

    public void PlaceNodes()
    {
        if (nodeRects == null || nodeRects.Length == 0 || spline == null || uiCamera == null || canvasRect == null)
            return;

        int n = nodeRects.Length;
        for (int i = 0; i < n; i++)
        {
            float a = (n == 1) ? 0f : (float)i / (n - 1);      // 0..1
            float t = Mathf.Lerp(tStart, tEnd, a);             // map into [tStart, tEnd]

            Vector3 worldPos = spline.EvaluatePosition(t);    // replace with your spline API if different
            Vector3 screenPos = uiCamera.WorldToScreenPoint(worldPos);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out Vector2 local))
                nodeRects[i].anchoredPosition = local;
        }
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
        nodes[nodeIdx].SetState(NodeState.Active, true); // animate
        nodes[nodeIdx].SetOnClick(() => OnNodeClicked(nodeIdx));
    }

    /// <summary>
    /// Called when a node is clicked. Handles completion and progression.
    /// </summary>
    /// <param name="nodeIdx">Zero-based index of the node.</param>
    private void OnNodeClicked(int nodeIdx)
    {
        var nodeAnim = nodes[nodeIdx].GetComponent<NodeStateAnimation>();
        if (!progressionController.IsUnlocked(nodeIdx))
        {
            Debug.Log($"Node {nodeIdx + 1} is locked. Shake animation.");
            if (nodeAnim != null)
                StartCoroutine(nodeAnim.Shake());
            return;
        }

        Debug.Log($"Node {nodeIdx + 1} clicked. Open popup/quiz.");
        // TODO: Open your LearningPopup here.

        // Mark node as completed and unlock next if applicable
        progressionController.CompleteNode(nodeIdx);
        // Node visuals will be updated via event handlers
    }

    // Event handler for node state changes
    private void HandleNodeStateChanged(int nodeIndex, bool unlocked, bool completed)
    {
        UpdateNodeVisual(nodeIndex);
    }

    // Event handler for active node changes
    private void HandleActiveNodeChanged(int nodeIndex)
    {
        // Optionally, move car or trigger other logic when active node changes
        // For now, just update visuals
        UpdateNodeVisual(nodeIndex);
    }

    // Helper to update node visuals based on progression state
    private void UpdateNodeVisual(int i)
    {
        NodeState state = NodeState.Inactive;
        if (progressionController.IsCompleted(i))
            state = NodeState.Completed;
        else if (progressionController.IsUnlocked(i))
            state = NodeState.Active;
        // Only update state without animation (animation only on car arrival)
        nodes[i].SetState(state, false);
    }

}
