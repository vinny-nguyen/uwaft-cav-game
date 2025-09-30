using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// Controls the map UI, node placement, and car movement based on progression.
/// </summary>

[System.Serializable]
public class NodePopupData
{
    public string header;
    public GameObject slidesParent; // Assign a GameObject with all slides as children
}

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
    [Header("Popup")]
    [SerializeField] private PopupController popupController; // Assign in inspector
    [Tooltip("Popup data for each node (header and slides)")]
    [SerializeField] private List<NodePopupData> nodePopups; // Assign in inspector, one per node

    // --- Constants ---
    private const float CarSnapStartT = 0f;
    private const float CarArrivalTolerance = 0.1f;

    // --- Keyboard Navigation ---
    private int currentNodeIndex = 0; // zero-based, node 1 is index 0
    private const int minNode = 0;    // index 0 (node 1)
    private const int maxNode = 5;    // index 5 (node 6)

    private void Start()
    {
        if (!progressionController)
            progressionController = FindFirstObjectByType<ProgressionController>();

        // Hide popup at game start
        if (popupController != null)
            popupController.Hide();

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

        // Set currentNodeIndex to the active node at start
        currentNodeIndex = Mathf.Clamp(progressionController.GetCurrentActiveNodeIndex(), minNode, maxNode);
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TryMoveToNode(currentNodeIndex + 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TryMoveToNode(currentNodeIndex - 1);
        }
    }

    private void TryMoveToNode(int targetIndex)
    {
        if (targetIndex < minNode || targetIndex > maxNode)
            return;

        if (!progressionController.IsUnlocked(targetIndex))
        {
            // Shake current node if trying to go to a locked node
            var nodeAnim = nodes[currentNodeIndex].GetComponent<NodeStateAnimation>();
            if (nodeAnim != null)
                StartCoroutine(nodeAnim.Shake());
            return;
        }

        // Set previous node inactive if not completed
        if (!progressionController.IsCompleted(currentNodeIndex))
            nodes[currentNodeIndex].SetState(NodeState.Inactive, true);

        int prevIndex = currentNodeIndex;
        currentNodeIndex = targetIndex;

        MoveCarAndSetNodeState(prevIndex, currentNodeIndex);
    }


    // Helper to get normalized spline t for a node index
    private float GetSplineTForNode(int index)
    {
        return Mathf.Lerp(tStart, tEnd, index / (float)(nodes.Length - 1));
    }

    // Helper to move car and set node state when car arrives
    private void MoveCarAndSetNodeState(int fromIndex, int toIndex)
    {
        float fromT = GetSplineTForNode(fromIndex);
        float toT = GetSplineTForNode(toIndex);
        StartCoroutine(MoveCarAndActivateNodeCoroutine(fromT, toT, toIndex));
    }

    private System.Collections.IEnumerator MoveCarAndActivateNodeCoroutine(float fromT, float toT, int nodeIdx)
    {
        yield return StartCoroutine(MoveCarTo(fromT, toT, true));
        if (!progressionController.IsCompleted(nodeIdx))
            nodes[nodeIdx].SetState(NodeState.Active, true);
    }

    // Helper coroutine to move car using CarPathFollower's private MoveAlongSpline
    private System.Collections.IEnumerator MoveCarTo(float fromT, float toT, bool eased)
    {
        var moveAlong = car.GetType().GetMethod("MoveAlongSpline", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (moveAlong != null)
        {
            var paramCount = moveAlong.GetParameters().Length;
            object[] parameters;
            if (paramCount == 4)
                parameters = new object[] { fromT, toT, false, eased }; // always false for straighten
            else if (paramCount == 3)
                parameters = new object[] { fromT, toT, eased }; // drop straighten param
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
        car.SnapTo(CarSnapStartT);
        float prevT = CarSnapStartT;
        for (int i = 0; i < activeIdx0; i++)
        {
            float t = GetSplineTForNode(i);
            yield return StartCoroutine(MoveCarTo(prevT, t, false)); // Linear, no ease
            nodes[i].SetState(NodeState.Completed, true);
            prevT = t;
        }
        float activeT = GetSplineTForNode(activeIdx0);
        yield return StartCoroutine(MoveCarTo(prevT, activeT, true)); // Eased, no straighten
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

        // Show popup for this node
        if (popupController != null && nodePopups != null && nodeIdx < nodePopups.Count)
        {
            var popupData = nodePopups[nodeIdx];
            var slides = new List<GameObject>();
            if (popupData.slidesParent != null)
            {
                foreach (Transform child in popupData.slidesParent.transform)
                {
                    slides.Add(child.gameObject);
                }
            }
            bool isCompleted = progressionController.IsCompleted(nodeIdx);
            popupController.SetBackground(isCompleted);
            popupController.SetHeaderAndSlides(popupData.header, slides);
        }
        else
        {
            Debug.LogWarning("PopupController or nodePopups not set, or nodeIdx out of range!");
        }
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
