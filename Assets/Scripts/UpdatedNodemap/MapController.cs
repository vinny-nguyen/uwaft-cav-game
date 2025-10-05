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

    private void Start()
    {
        // Initialize components
        nodeManager.Initialize();
        carController.Initialize(nodeManager);
        
        // Subscribe to events
        nodeManager.OnNodeClicked += HandleNodeClick;
        carController.OnArrivedAtNode += HandleCarArrival;
        progressionController.OnNodeStateChanged += HandleNodeStateChange;
        progressionController.OnActiveNodeChanged += HandleActiveNodeChange;

        // Set initial state
        currentNodeIndex = progressionController.GetCurrentActiveNodeIndex();
        RefreshAllNodes();
        
        // Move car to current position with completed node updates
        bool[] completed = new bool[6]; // Assuming 6 nodes
        for (int i = 0; i < 6; i++) completed[i] = progressionController.IsCompleted(i);
        carController.MoveToCurrentActive(currentNodeIndex, completed);
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

    private void HandleCarArrival(int nodeIndex)
    {
        // Update the arrived node to active state if it should be
        RefreshAllNodes();
    }

    private void HandleNodeStateChange(int nodeIndex, bool unlocked, bool completed)
    {
        nodeManager.UpdateNodeVisual(nodeIndex, unlocked, completed);
    }

    private void HandleActiveNodeChange(int newActiveIndex)
    {
        currentNodeIndex = newActiveIndex;
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