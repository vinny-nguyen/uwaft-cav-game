using System.Collections.Generic;
using UnityEngine;

namespace NodeMap
{
    /// <summary>
    /// Core manager for node-based game progression
    /// </summary>
    /// 
    public class NopeMapManager : MonoBehaviour
    {
        public delegate void NodeCompletedHandler(int nodeIndex);

        // Add event
        public event NodeCompletedHandler OnNodeCompleted;

        public static NopeMapManager Instance { get; private set; }

        [Header("Node Progression")]
        // Changed from -1 to 0, representing no active node
        public int CurrentNodeIndex { get; private set; } = -1;

        // Track completion status
        private HashSet<int> completedNodes = new HashSet<int>();

        #region Unity Lifecycle

        private void Awake()
        {
            SetupSingleton();
        }

        #endregion

        #region Singleton Setup

        private void SetupSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Node Progression Management

        /// <summary>
        /// Advances to the next node in sequence
        /// </summary>
        public void AdvanceToNextNode()
        {
            CurrentNodeIndex++;
            // Debug.Log($"Advanced to Node {CurrentNodeIndex}");
        }

        /// <summary>
        /// Sets the currently active node
        /// </summary>
        public void SetCurrentNode(int nodeIndex)
        {
            CurrentNodeIndex = nodeIndex;
            // Debug.Log($"Set current node to {CurrentNodeIndex}");
        }

        /// <summary>
        /// Marks a node as completed
        /// </summary>
        public void CompleteNode(int nodeIndex)
        {
            if (!completedNodes.Contains(nodeIndex))
            {
                completedNodes.Add(nodeIndex);
                // Debug.Log($"Node {nodeIndex} marked as completed");
            }

            OnNodeCompleted?.Invoke(nodeIndex);
        }

        /// <summary>
        /// Checks if a node is completed
        /// </summary>
        public bool IsNodeCompleted(int nodeIndex)
        {
            return completedNodes.Contains(nodeIndex);
        }

        #endregion
    }
}