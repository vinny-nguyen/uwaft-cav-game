using UnityEngine;
using System.Collections.Generic;

namespace NodeMap.Nodes
{

    /// <summary>
    /// Manages node visual states and completion tracking
    /// </summary>
    public class NodeStateManager : MonoBehaviour
    {
        [Header("Node Setup")]
        [SerializeField] private List<GameObject> nodeMarkers = new();
        [SerializeField] private List<Sprite> normalNodeSprites = new();
        [SerializeField] private List<Sprite> activeNodeSprites = new();
        [SerializeField] private List<Sprite> completeNodeSprites;

        /// <summary>
        /// Sets a node to a specific state and updates its visual appearance
        /// </summary>
        public void SetNodeState(int nodeIndex, NodeState state)
        {
            // Skip if completed node is being changed to non-complete state
            if (NodeMapManager.Instance.IsNodeCompleted(nodeIndex) && state != NodeState.Complete)
                return;

            // Ensure node index is valid
            if (nodeIndex < 0 || nodeIndex >= nodeMarkers.Count)
                return;

            GameObject marker = nodeMarkers[nodeIndex];
            if (marker == null)
                return;

            // Get the sprite for this state
            Sprite stateSprite = GetSpriteForState(nodeIndex, state);

            // Apply state to visuals
            NodeVisualController visualController = marker.GetComponent<NodeVisualController>();
            if (visualController != null && stateSprite != null)
            {
                visualController.TransitionToState(state, stateSprite);
            }
            else
            {
                // Fallback if no visual controller
                SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
                if (sr != null && stateSprite != null)
                    sr.sprite = stateSprite;
            }

            // Make active and completed nodes clickable
            if (state == NodeState.Active || state == NodeState.Complete)
            {
                NodeHoverHandler handler = marker.GetComponent<NodeHoverHandler>();
                if (handler != null)
                    handler.SetClickable(true);
            }
        }

        private Sprite GetSpriteForState(int nodeIndex, NodeState state)
        {
            switch (state)
            {
                case NodeState.Normal:
                    return nodeIndex < normalNodeSprites.Count ? normalNodeSprites[nodeIndex] : null;
                case NodeState.Active:
                    return nodeIndex < activeNodeSprites.Count ? activeNodeSprites[nodeIndex] : null;
                case NodeState.Complete:
                    return nodeIndex < completeNodeSprites.Count ? completeNodeSprites[nodeIndex] : null;
                default:
                    return null;
            }
        }

        // Convenience methods
        public void SetNodeToNormal(int nodeIndex) => SetNodeState(nodeIndex, NodeState.Normal);
        public void SetNodeToActive(int nodeIndex) => SetNodeState(nodeIndex, NodeState.Active);

        public void SetNodeToComplete(int nodeIndex)
        {
            if (!NodeMapManager.Instance.IsNodeCompleted(nodeIndex))
                NodeMapManager.Instance.CompleteNode(nodeIndex);

            SetNodeState(nodeIndex, NodeState.Complete);
        }

        /// <summary>
        /// Updates all node visuals based on their state
        /// </summary>
        public void UpdateAllNodeVisuals()
        {
            int currentNode = NodeMapManager.Instance.CurrentNodeIndex;

            for (int i = 0; i < nodeMarkers.Count; i++)
            {
                if (NodeMapManager.Instance.IsNodeCompleted(i))
                    SetNodeState(i, NodeState.Complete);
                else if (i == currentNode)
                    SetNodeState(i, NodeState.Active);
                else
                    SetNodeState(i, NodeState.Normal);
            }
        }

        public void ShakeNode(int nodeIndex)
        {
            if (nodeIndex >= 0 && nodeIndex < nodeMarkers.Count)
            {
                NodeHoverHandler handler = nodeMarkers[nodeIndex].GetComponent<NodeHoverHandler>();
                if (handler != null)
                    handler.StartShake();
            }
        }

        public bool IsNodeCompleted(int nodeIndex) => NodeMapManager.Instance.IsNodeCompleted(nodeIndex);
    }
}
