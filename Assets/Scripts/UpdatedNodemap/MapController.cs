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

    private int currentNodeIndex;
    private int carCurrentlyAtNodeIndex; // Track where the car actually is

    private void Start()
    {
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
        currentNodeIndex = progressionController.GetCurrentActiveNodeIndex();
        carCurrentlyAtNodeIndex = currentNodeIndex; // Car starts at the current active node
        
        // Initialize all nodes as inactive except where car currently is
        for (int i = 0; i < 6; i++)
        {
            bool isCarHere = (i == carCurrentlyAtNodeIndex);
            nodeManager.UpdateNodeVisual(i, isCarHere, progressionController.IsCompleted(i) && isCarHere);
        }
        
        // Move car to current position - this will update completed nodes as it passes them
        bool[] completed = new bool[6]; // Assuming 6 nodes
        for (int i = 0; i < 6; i++) completed[i] = progressionController.IsCompleted(i);
        carController.MoveToNode(currentNodeIndex, completed);
    }

    private void Update()
    {
        // Keyboard navigation
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMoveToNode(currentNodeIndex + 1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) TryMoveToNode(currentNodeIndex - 1);
    }

    private void HandleNodeClick(int nodeIndex)
    {
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
        // Car is leaving its current position - make that node inactive immediately
        if (carCurrentlyAtNodeIndex >= 0 && carCurrentlyAtNodeIndex < 6)
        {
            bool wasCompleted = progressionController.IsCompleted(carCurrentlyAtNodeIndex);
            nodeManager.UpdateNodeVisual(carCurrentlyAtNodeIndex, false, wasCompleted);
        }
    }

    private void HandleCarArrival(int nodeIndex)
    {
        // Car has arrived - set the arrived node to active
        nodeManager.UpdateNodeVisual(nodeIndex, true, progressionController.IsCompleted(nodeIndex));
        
        // Update tracking variables
        currentNodeIndex = nodeIndex;
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
        currentNodeIndex = newActiveIndex;
        // Don't pass completed nodes here - let normal movement handle it
        carController.MoveToNode(newActiveIndex);
    }

    private void TryMoveToNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= 6) return;
        if (!progressionController.IsUnlocked(nodeIndex))
        {
            nodeManager.ShakeNode(nodeIndex);
            return;
        }
        
        currentNodeIndex = nodeIndex;
        carController.MoveToNode(nodeIndex);
    }

    private void RefreshAllNodes()
    {
        for (int i = 0; i < 6; i++)
            nodeManager.UpdateNodeVisual(i, progressionController.IsUnlocked(i), progressionController.IsCompleted(i));
    }

    public void RepositionNodes() => nodeManager.Initialize();
}