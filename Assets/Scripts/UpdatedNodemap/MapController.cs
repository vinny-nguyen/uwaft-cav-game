using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class MapController : MonoBehaviour
{
    [Header("Data & UI Prefab")]
    [SerializeField] private List<NodeData> nodeDataList; // One per node, in order
    [SerializeField] private LevelNodeView nodePrefab;    // Prefabs/UI/NodeButton.prefab
    [SerializeField] private RectTransform nodesParent;   // Parent under Canvas to hold spawned nodes

    [Header("Spline & Car (World)")]
    [SerializeField] private SplineContainer spline;
    [SerializeField] private CarPathFollower car;

    [Header("Canvas Mapping")]
    [SerializeField] private Camera uiCamera;                 // Canvas camera
    [SerializeField] private RectTransform canvasRect;        // Root Canvas RectTransform

    [Header("Popup & Progression")]
    [SerializeField] private PopupController popupController;
    [SerializeField] private ProgressionController progressionController;

    [Header("Configuration")]
    [SerializeField] private MapConfig mapConfig;
    
    [Header("Spline Range (normalized) - Overridden by MapConfig")]
    [Range(0f, 1f)] public float tStart = 0.2f;   // where first node lives (fallback if no config)
    [Range(0f, 1f)] public float tEnd = 0.8f;   // where last node lives (fallback if no config)

    [Header("Car Spawn (normalized)")]
    [Tooltip("Where the car spawns initially along the spline (0..1). Set this outside [tStart, tEnd] so it starts off-screen.")]
    [Range(0f, 1f)] public float carSpawnT = 0.0f;

    private readonly List<LevelNodeView> nodes = new();
    private readonly List<RectTransform> nodeRects = new();

    private int currentNodeIndex = 0;

    // Track arrival gating so active node doesn't light up prematurely
    private int pendingActiveIndex = -1;
    private bool isMovingToTarget = false;

    // -------------------- Configuration Helpers --------------------
    
    private float GetTStart() => mapConfig ? mapConfig.tStart : tStart;
    private float GetTEnd() => mapConfig ? mapConfig.tEnd : tEnd;

    // -------------------- Lifecycle --------------------

    private void Start()
    {
        // Initialize config if not assigned
        if (!mapConfig) mapConfig = MapConfig.Instance;
        
        // Guards
        if (!progressionController) progressionController = FindFirstObjectByType<ProgressionController>();
        if (!progressionController)
        {
            Debug.LogError("MapController: ProgressionController is not assigned.");
            enabled = false;
            return;
        }
        if (!spline) { Debug.LogError("MapController: SplineContainer not assigned."); enabled = false; return; }
        if (!car) { Debug.LogError("MapController: CarPathFollower not assigned."); enabled = false; return; }
        if (!nodePrefab || !nodesParent) { Debug.LogError("MapController: nodePrefab or nodesParent missing."); enabled = false; return; }
        if (!uiCamera || !canvasRect) { Debug.LogError("MapController: uiCamera or canvasRect missing."); enabled = false; return; }

        if (popupController) popupController.Hide();

        // Subscribe to progression
        progressionController.OnNodeStateChanged += HandleNodeStateChanged;
        progressionController.OnActiveNodeChanged += HandleActiveNodeChanged;

        // Spawn nodes from data and place them on the canvas
        SpawnNodesFromData();
        PlaceNodes();

        // Spawn car off-screen (or wherever carSpawnT is)
        car.SnapTo(Mathf.Clamp01(carSpawnT));

        // Set up movement tracking BEFORE visual updates to prevent premature activation
        currentNodeIndex = Mathf.Clamp(progressionController.GetCurrentActiveNodeIndex(), 0, Mathf.Max(0, nodes.Count - 1));
        pendingActiveIndex = currentNodeIndex;
        isMovingToTarget = true;

        // Paint states (Completed/Active/Inactive) without animation - now with proper gating
        for (int i = 0; i < nodes.Count; i++)
            UpdateNodeVisual(i, animate: false);

        // Move through completed nodes (if any) then to the current active node, and only
        // then light it up as Active (activate-on-arrival).
        StartCoroutine(AnimateCarThroughCompletedToActive(currentNodeIndex));
    }

    private void OnDestroy()
    {
        if (progressionController != null)
        {
            progressionController.OnNodeStateChanged -= HandleNodeStateChanged;
            progressionController.OnActiveNodeChanged -= HandleActiveNodeChanged;
        }
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    // -------------------- Spawning & Placement --------------------

    private void SpawnNodesFromData()
    {
        // Clear existing children
        for (int i = nodesParent.childCount - 1; i >= 0; i--)
            Destroy(nodesParent.GetChild(i).gameObject);

        nodes.Clear();
        nodeRects.Clear();

        int count = nodeDataList != null ? nodeDataList.Count : 0;
        if (count == 0)
        {
            Debug.LogWarning("MapController: nodeDataList is empty; nothing to spawn.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var view = Instantiate(nodePrefab, nodesParent);
            view.BindIndex(i + 1);                    // numbering, if you show 1..N
            view.SetState(NodeState.Inactive, false); // no pop on initial paint
            int idx = i;
            view.SetOnClick(() => OnNodeClicked(idx));

            nodes.Add(view);
            nodeRects.Add(view.GetComponent<RectTransform>());
        }
    }

    private void PlaceNodes()
    {
        int n = nodeRects.Count;
        if (n == 0) return;

        for (int i = 0; i < n; i++)
        {
            float a = (n == 1) ? 0f : i / (float)(n - 1); // 0..1 across nodes
            float t = Mathf.Lerp(GetTStart(), GetTEnd(), a);

            Vector3 worldPos = spline.EvaluatePosition(t);
            Vector3 screenPos = uiCamera.WorldToScreenPoint(worldPos);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out Vector2 local))
                nodeRects[i].anchoredPosition = local;
        }
    }

    private float GetSplineTForNode(int index)
    {
        if (nodes.Count <= 1) return GetTStart();
        index = Mathf.Clamp(index, 0, nodes.Count - 1);
        return Mathf.Lerp(GetTStart(), GetTEnd(), index / (float)(nodes.Count - 1));
    }

    // -------------------- Progression Events --------------------

    private void HandleNodeStateChanged(int index, bool unlocked, bool completed)
    {
        if (index < 0 || index >= nodes.Count) return;
        UpdateNodeVisual(index, animate: false);
    }

    private void HandleActiveNodeChanged(int nodeIndex)
    {
        int prev = currentNodeIndex;
        currentNodeIndex = Mathf.Clamp(nodeIndex, 0, Mathf.Max(0, nodes.Count - 1));

        // Defer lighting up the new active node; only show it Active on arrival
        pendingActiveIndex = currentNodeIndex;
        isMovingToTarget = true;

        // Refresh all node visuals to ensure proper gating is applied
        for (int i = 0; i < nodes.Count; i++)
            UpdateNodeVisual(i, animate: false);

        MoveCarAndSetNodeState(prev, currentNodeIndex);
    }

    // -------------------- Click & Popup --------------------

    private void OnNodeClicked(int nodeIdx)
    {
        // Robust guards to avoid NullReference
        if (nodeIdx < 0 || nodeIdx >= nodes.Count)
        {
            Debug.LogWarning($"MapController.OnNodeClicked: index {nodeIdx} out of range.");
            return;
        }
        if (progressionController == null)
        {
            Debug.LogError("MapController.OnNodeClicked: ProgressionController is null.");
            return;
        }
        if (popupController == null)
        {
            Debug.LogError("MapController.OnNodeClicked: PopupController is null.");
            return;
        }

        // Locked feedback
        if (!progressionController.IsUnlocked(nodeIdx))
        {
            nodes[nodeIdx].PlayShake();
            return;
        }

        // Open learning popup from NodeData
        NodeData data = (nodeIdx < nodeDataList.Count) ? nodeDataList[nodeIdx] : null;
        if (data == null)
        {
            Debug.LogWarning($"MapController: No NodeData assigned for node {nodeIdx}. (OK while testing Tires only)");
            return;
        }

        bool isCompleted = progressionController.IsCompleted(nodeIdx);
        popupController.Open(data, isCompleted);
    }

    // -------------------- Car Movement --------------------

    private void MoveCarAndSetNodeState(int fromIndex, int toIndex)
    {
        float fromT = (fromIndex < 0) ? Mathf.Clamp01(carSpawnT) : GetSplineTForNode(fromIndex);
        float toT = GetSplineTForNode(toIndex);
        StartCoroutine(MoveCarAndActivateNodeCoroutine(fromT, toT, toIndex));
    }

    private IEnumerator MoveCarAndActivateNodeCoroutine(float fromT, float toT, int nodeIdx)
    {
        // Start the car movement and update nodes as the car passes through them
        yield return StartCoroutine(MoveCarWithPassThroughUpdates(fromT, toT));

        // Only light up the final target node as Active (if not completed)
        isMovingToTarget = false;

        if (!progressionController.IsCompleted(nodeIdx))
        {
            // Gate: if this node is the pending one, activate it
            if (pendingActiveIndex == nodeIdx)
            {
                nodes[nodeIdx].SetState(NodeState.Active, true);
            }
        }

        // Refresh visuals for all (so "Active before arrival" is corrected if anything drifted)
        for (int i = 0; i < nodes.Count; i++)
            UpdateNodeVisual(i, animate: false);
    }

    private IEnumerator MoveCarWithPassThroughUpdates(float fromT, float toT)
    {
        if (spline == null)
        {
            Debug.LogError("Cannot move - spline is null!");
            yield break;
        }

        // Set up movement parameters similar to CarPathFollower
        Vector3 startPos = spline.EvaluatePosition(fromT);
        Vector3 endPos = spline.EvaluatePosition(toT);
        float distance = Vector3.Distance(startPos, endPos);
        float duration = Mathf.Max(distance / car.MoveSpeed, car.MinMoveDuration);

        // Track which nodes we've already updated
        bool[] nodesPassed = new bool[nodes.Count];

        // Start the car's movement coroutine
        var carMoveCoroutine = StartCoroutine(car.MoveAlong(fromT, toT, eased: true));

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            
            // Calculate current spline position using same easing as car
            float easedProgress = progress * progress * (3f - 2f * progress); // SmoothStep
            float currentT = Mathf.Lerp(fromT, toT, easedProgress);

            // Check each node to see if the car has passed it
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!nodesPassed[i] && progressionController.IsCompleted(i))
                {
                    float nodeT = GetSplineTForNode(i);
                    
                    // Check if car has passed this completed node
                    bool hasPassed = (fromT < toT && currentT >= nodeT) || (fromT > toT && currentT <= nodeT);
                    
                    if (hasPassed)
                    {
                        nodesPassed[i] = true;
                        // Update the node visual to completed state with animation
                        nodes[i].SetState(NodeState.Completed, true);
                    }
                }
            }

            yield return null;
        }

        // Wait for the car movement to finish
        yield return carMoveCoroutine;
    }

    private IEnumerator AnimateCarThroughCompletedToActive(int activeIndex)
    {
        if (nodes.Count == 0) yield break;

        // Start off-screen (or wherever carSpawnT is)
        car.SnapTo(Mathf.Clamp01(carSpawnT));

        // Set up for continuous movement
        pendingActiveIndex = activeIndex;
        isMovingToTarget = true;

        // Calculate starting position - either current spawn position or tStart
        float startT = Mathf.Clamp01(carSpawnT);
        if (startT > GetTStart())
        {
            startT = GetTStart(); // If we spawn after tStart, move to tStart first
        }

        // Move continuously from start position directly to the active node
        // This will pass through any completed nodes without stopping and update them visually
        float targetT = GetSplineTForNode(activeIndex);
        
        yield return StartCoroutine(MoveCarAndActivateNodeCoroutine(startT, targetT, activeIndex));
    }

    // -------------------- Keyboard Nav --------------------

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
        if (targetIndex < 0 || targetIndex >= nodes.Count) return;

        if (!progressionController.IsUnlocked(targetIndex))
        {
            nodes[targetIndex].PlayShake();
            return;
        }

        // Keep current from changing to Active too early
        pendingActiveIndex = targetIndex;
        isMovingToTarget = true;

        // Refresh all node visuals to ensure proper gating is applied
        for (int i = 0; i < nodes.Count; i++)
            UpdateNodeVisual(i, animate: false);

        int prev = currentNodeIndex;
        currentNodeIndex = targetIndex;
        MoveCarAndSetNodeState(prev, currentNodeIndex);
    }

    // -------------------- Visual Helper --------------------

    private void UpdateNodeVisual(int i, bool animate)
    {
        // “Activate only on arrival” rule:
        // If we're moving toward this index, keep it visually Inactive until arrival.
        bool gateActive = (isMovingToTarget && i == pendingActiveIndex);

        NodeState state;
        if (progressionController.IsCompleted(i))
            state = NodeState.Completed;
        else if (progressionController.IsUnlocked(i) && !gateActive)
            state = NodeState.Active;
        else
            state = NodeState.Inactive;

        nodes[i].SetState(state, animate);
    }

    // -------------------- External --------------------

    public void RepositionNodes() => PlaceNodes();
}
