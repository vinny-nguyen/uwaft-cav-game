using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using System.Collections.Generic;

namespace NodeMap
{
    /// <summary>
    /// Handles movement sequences, tutorials, and scene transitions
    /// </summary>
    public class PlayerMovementSequencer : MonoBehaviour
    {
        [Header("Tutorial")]
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private int firstNodeIndex = 0;

        [Header("Scene Management")]
        [SerializeField] private NodeMap.UI.SceneTransitionManager sceneTransitionManager;

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
        /// Initial sequence to move player to first node
        /// </summary>
        public IEnumerator StartSequence()
        {
            // Wait for SceneTransitionManager to be ready and opening transition to complete
            Debug.Log("[PlayerMovementSequencer] StartSequence initiated. Waiting for opening transition...");
            while (NodeMap.UI.SceneTransitionManager.Instance == null || !NodeMap.UI.SceneTransitionManager.Instance.IsOpeningTransitionComplete)
            {
                if (NodeMap.UI.SceneTransitionManager.Instance == null)
                {
                    Debug.LogWarning("[PlayerMovementSequencer] Waiting for SceneTransitionManager instance...");
                }
                yield return null;
            }

            Debug.Log("[PlayerMovementSequencer] Opening transition complete. Waiting 2 seconds before moving car.");
            yield return new WaitForSeconds(2f);

            // Move car from spline start to first node
            Debug.Log("[PlayerMovementSequencer] Starting car movement to first node.");
            yield return movementController.MoveAlongSpline(spline, 0f, stops[firstNodeIndex].splinePercent, true);

            // Set up first node
            nodeStateManager.SetNodeToActive(firstNodeIndex);
            NodeMapManager.Instance.SetCurrentNode(firstNodeIndex);

            // Check for tutorial
            if (tutorialManager != null && !tutorialManager.HasCompletedTutorial())
            {
                if (sceneTransitionManager != null)
                {
                    Debug.Log("[PlayerMovementSequencer] Reached first node. Waiting 1 second before initiating zoom out and tutorial sequence.");
                    yield return new WaitForSeconds(1f);

                    Debug.Log("[PlayerMovementSequencer] Calling InitiateZoomOutAndTutorialSequence on SceneTransitionManager.");
                    sceneTransitionManager.InitiateZoomOutAndTutorialSequence();
                }
                else
                {
                    Debug.LogWarning("[PlayerMovementSequencer] SceneTransitionManager reference is null. Cannot start zoom out and tutorial sequence.");
                    tutorialManager.StartTutorial();
                }
            }
        }
    }
}
