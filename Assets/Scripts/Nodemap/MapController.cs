using UnityEngine;

/// <summary>
/// Simple map controller that coordinates NodeManager, CarController, and ProgressionController.
/// </summary>
public class MapController : MonoBehaviour
{
    [SerializeField] private NodeManager nodeManager;
    [SerializeField] private CarController carController;
    [SerializeField] private ProgressionController progressionController;
    [SerializeField] private PopupController popupController;
    [SerializeField] private MapConfig mapConfig;

    private int carCurrentlyAtNodeIndex; // Track where the car actually is

    private void Start()
    {
        // Initialize config if not assigned
        if (!mapConfig) mapConfig = MapConfig.Instance;
        
        // Initialize components
        nodeManager.Initialize();
        carController.Initialize(nodeManager);
        
        // Subscribe to events
        nodeManager.OnNodeClicked += HandleNodeClick;
        carController.OnArrivedAtNode += HandleCarArrival;
        carController.OnStartedMovingToNode += HandleMovementStart;
        progressionController.OnNodeStateChanged += HandleNodeStateChange;
        progressionController.OnActiveNodeChanged += HandleActiveNodeChange;

        // Set initial state
        carCurrentlyAtNodeIndex = progressionController.GetCurrentActiveNodeIndex(); // Car starts at the current active node
        
        int nodeCount = mapConfig.nodeCount;
        
        // Initialize all nodes as inactive except where car currently is
        for (int i = 0; i < nodeCount; i++)
        {
            bool isCarHere = (i == carCurrentlyAtNodeIndex);
            nodeManager.UpdateNodeVisual(i, isCarHere, progressionController.IsCompleted(i) && isCarHere);
        }
        
        // Move car to current position - this will update completed nodes as it passes them
        bool[] completed = new bool[nodeCount];
        for (int i = 0; i < nodeCount; i++) completed[i] = progressionController.IsCompleted(i);
        carController.MoveToNode(carCurrentlyAtNodeIndex, completed);
    }

    private void Update()
    {
        // Keyboard navigation - use progression controller as source of truth
        int activeNode = progressionController.GetCurrentActiveNodeIndex();
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMoveToNode(activeNode + 1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) TryMoveToNode(activeNode - 1);
    }

    private void HandleNodeClick(int nodeIndex)
    {
        if (progressionController == null || nodeManager == null) return;
        
        if (!progressionController.IsUnlocked(nodeIndex))
        {
            nodeManager.ShakeNode(nodeIndex);
            return;
        }

        var data = nodeManager.GetNodeData(nodeIndex);
        if (data != null && popupController != null)
            popupController.Open(data, progressionController.IsCompleted(nodeIndex));
    }

    private void HandleMovementStart(int targetNodeIndex)
    {
        if (mapConfig == null || progressionController == null || nodeManager == null) return;
        
        // Car is leaving its current position - make that node inactive immediately
        int nodeCount = mapConfig.nodeCount;
        if (carCurrentlyAtNodeIndex >= 0 && carCurrentlyAtNodeIndex < nodeCount)
        {
            bool wasCompleted = progressionController.IsCompleted(carCurrentlyAtNodeIndex);
            nodeManager.UpdateNodeVisual(carCurrentlyAtNodeIndex, false, wasCompleted);
        }
    }

    private void HandleCarArrival(int nodeIndex)
    {
        if (nodeManager == null || progressionController == null) return;
        
        // Car has arrived - set the arrived node to active
        nodeManager.UpdateNodeVisual(nodeIndex, true, progressionController.IsCompleted(nodeIndex));
        
        // Update tracking variable
        carCurrentlyAtNodeIndex = nodeIndex; // Car is now at this node
    }

    private void HandleNodeStateChange(int nodeIndex, bool unlocked, bool completed)
    {
        // Let the car movement handle visual updates during transit
        // Only immediate updates for unlocked state changes (not completion)
        if (!completed) // Only update if it's not about completion
            nodeManager.UpdateNodeVisual(nodeIndex, unlocked, completed);
    }

    private void HandleActiveNodeChange(int newActiveIndex)
    {
        // Don't pass completed nodes here - let normal movement handle it
        carController.MoveToNode(newActiveIndex);
    }

    private void TryMoveToNode(int nodeIndex)
    {
        int nodeCount = mapConfig.nodeCount;
        if (nodeIndex < 0 || nodeIndex >= nodeCount) return;
        if (!progressionController.IsUnlocked(nodeIndex))
        {
            nodeManager.ShakeNode(nodeIndex);
            return;
        }
        
        carController.MoveToNode(nodeIndex);
    }

    private void RefreshAllNodes()
    {
        int nodeCount = mapConfig.nodeCount;
        for (int i = 0; i < nodeCount; i++)
            nodeManager.UpdateNodeVisual(i, progressionController.IsUnlocked(i), progressionController.IsCompleted(i));
    }

    public void RepositionNodes() => nodeManager.Initialize();

    private void OnDestroy()
    {
        // Clean up all event subscriptions to prevent memory leaks
        if (nodeManager != null)
            nodeManager.OnNodeClicked -= HandleNodeClick;
            
        if (carController != null)
        {
            carController.OnArrivedAtNode -= HandleCarArrival;
            carController.OnStartedMovingToNode -= HandleMovementStart;
        }
        
        if (progressionController != null)
        {
            progressionController.OnNodeStateChanged -= HandleNodeStateChange;
            progressionController.OnActiveNodeChanged -= HandleActiveNodeChange;
        }
    }
}