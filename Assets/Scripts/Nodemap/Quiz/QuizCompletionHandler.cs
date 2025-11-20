using UnityEngine;
using Nodemap;

/// <summary>
/// Handles quiz completion events and connects them to the map controller
/// to complete nodes and unlock progression.
/// Attach this to the MapController GameObject.
/// </summary>
public class QuizCompletionHandler : MonoBehaviour
{
    private MapControllerSimple mapController;
    private NodeManagerSimple nodeManager;

    private void Awake()
    {
        mapController = FindFirstObjectByType<MapControllerSimple>();
        nodeManager = FindFirstObjectByType<NodeManagerSimple>();
        
        if (mapController == null)
        {
            Debug.LogError("[QuizCompletionHandler] MapControllerSimple not found in scene!");
        }

        if (nodeManager == null)
        {
            Debug.LogError("[QuizCompletionHandler] NodeManagerSimple not found in scene!");
        }
    }

    /// <summary>
    /// Called when a quiz is completed successfully.
    /// This method should be wired to QuizController.OnQuizCompleted event.
    /// </summary>
    public void OnQuizCompleted()
    {
        if (mapController == null)
        {
            Debug.LogError("[QuizCompletionHandler] Cannot complete node - MapController is null!");
            return;
        }

        // Get the current active node index
        int currentNodeIndex = mapController.GetCurrentActiveNodeIndex();
        
        Debug.Log($"[QuizCompletionHandler] Quiz completed for node {currentNodeIndex}");

        // Apply car upgrade visuals
        ApplyCarUpgrade(currentNodeIndex);
        
        // Complete the node (this unlocks the next node and moves the car)
        mapController.CompleteNode(currentNodeIndex);
    }

    private void ApplyCarUpgrade(int nodeIndex)
    {
        if (nodeManager == null)
        {
            Debug.LogWarning("[QuizCompletionHandler] Cannot apply upgrade - NodeManager is null!");
            return;
        }

        // Get node data
        var nodeData = nodeManager.GetNodeData(new NodeId(nodeIndex));
        if (nodeData == null)
        {
            Debug.LogWarning($"[QuizCompletionHandler] No node data found for index {nodeIndex}");
            return;
        }

        // Find car visual component
        var carVisual = FindFirstObjectByType<CarVisual>();
        if (carVisual == null)
        {
            Debug.LogWarning("[QuizCompletionHandler] CarVisual not found in scene!");
            return;
        }

        // Apply upgrade if available
        if (nodeData.upgradeFrame != null || nodeData.upgradeTire != null)
        {
            Debug.Log($"[QuizCompletionHandler] Applying upgrade: Frame={nodeData.upgradeFrame != null}, Tire={nodeData.upgradeTire != null}");
            carVisual.ApplyUpgrade(nodeData.upgradeFrame, nodeData.upgradeTire);
        }
    }
}
