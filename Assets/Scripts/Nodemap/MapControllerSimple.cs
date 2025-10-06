using UnityEngine;
using Nodemap.Core;
using Nodemap.Commands;
using Nodemap.Car;

namespace Nodemap
{
    /// <summary>
    /// Drop-in replacement for MapController that uses improved architecture
    /// but maintains compatibility with existing prefab setup.
    /// Much cleaner than original but works with current Unity setup.
    /// </summary>
    public class MapControllerSimple : ConfigurableComponent, System.IDisposable
    {
        [Header("Component References")]
        [SerializeField] private NodeManagerSimple nodeManager;
        [SerializeField] private CarMovementController carController;
        [SerializeField] private PopupController popupController;

        // Core systems
        private MapState mapState;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeState();
        }

        private void Start()
        {
            InitializeComponents();
            SubscribeToEvents();
            SetInitialState();
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        #region Initialization

        private void InitializeState()
        {
            int nodeCount = GetConfig(c => c.nodeCount, 6);
            mapState = new MapState(nodeCount);
            mapState.LoadFromPlayerPrefs();
        }

        private void InitializeComponents()
        {
            // Initialize components
            nodeManager?.Initialize();
            
            // Position car at loaded state
            if (carController != null && nodeManager != null)
            {
                carController.SnapToNode(mapState.CurrentCarNodeId, nodeManager);
            }
        }

        private void SubscribeToEvents()
        {
            // State events
            if (mapState != null)
            {
                mapState.OnCarNodeChanged += HandleCarNodeChanged;
                mapState.OnNodeCompletedChanged += HandleNodeCompletionChanged;
                mapState.OnNodeUnlockedChanged += HandleNodeUnlockedChanged;
            }

            // Component events
            if (nodeManager != null)
                nodeManager.OnNodeClicked += HandleNodeClicked;

            if (carController != null)
                carController.OnArrivedAtNode += HandleCarArrived;
        }

        private void SetInitialState()
        {
            RefreshAllVisuals();
        }

        #endregion

        #region Event Handlers

        private void HandleNodeClicked(NodeId nodeId)
        {
            // Try to open popup if unlocked
            if (mapState.IsNodeUnlocked(nodeId))
            {
                var nodeData = nodeManager.GetNodeData(nodeId);
                if (nodeData != null && popupController != null)
                {
                    bool isCompleted = mapState.IsNodeCompleted(nodeId);
                    popupController.Open(nodeData, isCompleted);
                }
            }
            else
            {
                // Shake locked node
                nodeManager.ShakeNode(nodeId);
            }
        }

        private void HandleCarNodeChanged(NodeId nodeId)
        {
            if (carController != null && nodeManager != null)
            {
                carController.MoveToNode(nodeId, nodeManager);
            }
            RefreshNodeVisual(nodeId);
        }

        private void HandleCarArrived(NodeId nodeId)
        {
            RefreshNodeVisual(nodeId);
            mapState?.SaveToPlayerPrefs();
        }

        private void HandleNodeCompletionChanged(NodeId nodeId, bool completed)
        {
            RefreshNodeVisual(nodeId);
            mapState?.SaveToPlayerPrefs();
        }

        private void HandleNodeUnlockedChanged(NodeId nodeId, bool unlocked)
        {
            RefreshNodeVisual(nodeId);
            mapState?.SaveToPlayerPrefs();
        }

        #endregion

        #region Input Handling

        private void HandleKeyboardInput()
        {
            if (mapState == null) return;

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                var nextNode = mapState.CurrentCarNodeId.GetNext(mapState.NodeCount);
                if (nextNode.HasValue && mapState.IsNodeUnlocked(nextNode.Value))
                {
                    mapState.TryMoveCarTo(nextNode.Value);
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                var prevNode = mapState.CurrentCarNodeId.GetPrevious();
                if (prevNode.HasValue && mapState.IsNodeUnlocked(prevNode.Value))
                {
                    mapState.TryMoveCarTo(prevNode.Value);
                }
            }
        }

        #endregion

        #region Public API (for external systems)

        /// <summary>
        /// Called by quiz/minigame systems when a node is completed
        /// </summary>
        public void CompleteNode(int nodeIndex)
        {
            var nodeId = new NodeId(nodeIndex);
            mapState?.TryCompleteNode(nodeId);
        }

        /// <summary>
        /// Reset all progression
        /// </summary>
        public void ResetProgression()
        {
            mapState?.ResetProgression();
        }

        /// <summary>
        /// Get current active node index (for backward compatibility)
        /// </summary>
        public int GetCurrentActiveNodeIndex()
        {
            return mapState?.ActiveNodeId.Value ?? 0;
        }

        /// <summary>
        /// Check if node is unlocked (for backward compatibility)
        /// </summary>
        public bool IsUnlocked(int nodeIndex)
        {
            return mapState?.IsNodeUnlocked(new NodeId(nodeIndex)) ?? false;
        }

        /// <summary>
        /// Check if node is completed (for backward compatibility)
        /// </summary>
        public bool IsCompleted(int nodeIndex)
        {
            return mapState?.IsNodeCompleted(new NodeId(nodeIndex)) ?? false;
        }

        #endregion

        #region Visual Updates

        private void RefreshAllVisuals()
        {
            if (mapState == null || nodeManager == null) return;

            for (int i = 0; i < mapState.NodeCount; i++)
            {
                var nodeId = new NodeId(i);
                RefreshNodeVisual(nodeId);
            }
        }

        private void RefreshNodeVisual(NodeId nodeId)
        {
            if (nodeManager == null || mapState == null) return;

            bool isCarHere = mapState.CurrentCarNodeId.Equals(nodeId);
            bool isCompleted = mapState.IsNodeCompleted(nodeId);
            bool isUnlocked = mapState.IsNodeUnlocked(nodeId);
            
            NodeState state = isCompleted ? NodeState.Completed :
                             (isUnlocked ? NodeState.Active : NodeState.Inactive);
            
            nodeManager.UpdateNodeVisual(nodeId, state, isCarHere);
        }

        #endregion

        #region Lifecycle

        public void Dispose()
        {
            // Unsubscribe from events
            if (mapState != null)
            {
                mapState.OnCarNodeChanged -= HandleCarNodeChanged;
                mapState.OnNodeCompletedChanged -= HandleNodeCompletionChanged;
                mapState.OnNodeUnlockedChanged -= HandleNodeUnlockedChanged;
                mapState.Dispose();
            }

            if (nodeManager != null)
                nodeManager.OnNodeClicked -= HandleNodeClicked;

            if (carController != null)
                carController.OnArrivedAtNode -= HandleCarArrived;
        }

        private void OnDestroy()
        {
            Dispose();
        }

        #endregion
    }
}