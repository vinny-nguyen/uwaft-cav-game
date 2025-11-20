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

    private void Awake()
    {
        mapController = FindFirstObjectByType<MapControllerSimple>();
        
        if (mapController == null)
        {
            Debug.LogError("[QuizCompletionHandler] MapControllerSimple not found in scene!");
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
        
        // Complete the node (this unlocks the next node and moves the car)
        mapController.CompleteNode(currentNodeIndex);
        
        // Optionally close popup after a delay to let user see completion message
        Invoke(nameof(ClosePopupDelayed), 2.5f);
    }

    private void ClosePopupDelayed()
    {
        var popup = FindFirstObjectByType<PopupController>();
        if (popup != null)
        {
            popup.Hide();
        }
    }
}
