using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using System.Collections.Generic;

namespace NodeMap
{
    /// <summary>
    /// Manages player progress including save/load functionality
    /// </summary>
    public class PlayerProgressManager : MonoBehaviour
    {
        private SplineMovementController movementController;
        private NodeStateManager nodeStateManager;
        private List<SplineStop> stops;
        private SplineContainer spline;

        public void Initialize(SplineMovementController controller, NodeStateManager stateManager, 
            List<SplineStop> splineStops, SplineContainer splineContainer)
        {
            movementController = controller;
            nodeStateManager = stateManager;
            stops = splineStops;
            spline = splineContainer;
        }

        /// <summary>
        /// Loads saved progress and positions the player at the saved node
        /// </summary>
        public IEnumerator LoadSavedProgress()
        {
            NopeMapManager manager = NopeMapManager.Instance;
            if (manager == null)
            {
                Debug.LogError("NopeMapManager not found when trying to load saved progress!");
                yield break;
            }

            // Wait a frame to ensure NodeMapManager has loaded its data
            yield return null;

            int currentNodeIndex = manager.CurrentNodeIndex;
            if (currentNodeIndex < 0 || currentNodeIndex >= stops.Count)
            {
                Debug.LogWarning($"Saved node index {currentNodeIndex} is invalid. Starting from beginning.");
                yield break;
            }

            // Position at saved node
            float nodePosition = stops[currentNodeIndex].splinePercent;
            transform.position = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(nodePosition));

            // Set rotation
            movementController.UpdatePlayerRotation(spline, nodePosition);

            // Update node visuals
            nodeStateManager.UpdateAllNodeVisuals();
            nodeStateManager.SetNodeToActive(currentNodeIndex);
        }

        public bool HasSavedProgress()
        {
            return PlayerPrefs.HasKey("CurrentNodeIndex");
        }

        public bool IsValidNodeIndex(int nodeIndex)
        {
            return nodeIndex >= 0 && nodeIndex < stops.Count;
        }

        public bool CanMoveToNode(int targetNode, int currentNode)
        {
            // Can't move to invalid nodes or same node
            if (targetNode < 0 || targetNode >= stops.Count || targetNode == currentNode)
                return false;

            // Prevent forward movement if current node is not complete
            if (targetNode > currentNode && !nodeStateManager.IsNodeCompleted(currentNode))
                return false;

            return true;
        }
    }
}
