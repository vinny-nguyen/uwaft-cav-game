using UnityEngine;
using Nodemap.Core;
using Nodemap.Commands;
using Nodemap.Car;

namespace Nodemap
{
    /// <summary>
    /// Drop-in replacement for MapController that uses improved architecture
    /// but maintains compatibility with existing prefab setup.
    /// </summary>
    public class MapControllerSimple : MonoBehaviour
    {
        [Header("Configuration")]
        private MapConfig config; // Auto-loaded via singleton
        
        [Header("Component References")]
        [SerializeField] private NodeManagerSimple nodeManager;
        [SerializeField] private CarMovementController carController;
        [SerializeField] private PopupController popupController;

        // Core systems
        private MapState mapState;
        
        private void Awake()
        {
            if (config == null) config = MapConfig.Instance;
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
            int nodeCount = config ? config.nodeCount : 6;
            mapState = new MapState(nodeCount);
            mapState.LoadFromPlayerPrefs();
        }

        private void InitializeComponents()
        {
            // Initialize components
            nodeManager?.Initialize();
            
            // Start car at spawn position and drive to active node
            if (carController != null && nodeManager != null)
            {
                // Car stays at spawn position (beginning of path) initially
                // Then we'll drive it to the active node after initial state is set
            }
        }

        private void SubscribeToEvents()
        {
            // State change notification
            if (mapState != null)
            {
                mapState.OnStateChanged += OnMapStateChanged;
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
            
            // Apply accumulated upgrades to the car based on completed nodes
            ApplyAccumulatedUpgrades();
            
            // Drive car from spawn position to the active node
            if (carController != null && nodeManager != null && mapState != null)
            {
                carController.MoveToNode(mapState.ActiveNodeId, nodeManager);
            }
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
            // Locked nodes do nothing when clicked
        }

        private void HandleCarArrived(NodeId nodeId)
        {
            // Update the car's position in state when it arrives
            // We temporarily unsubscribe from state changes to avoid triggering auto-movement
            if (mapState != null)
            {
                mapState.OnStateChanged -= OnMapStateChanged;
                mapState.TryMoveCarTo(nodeId);
                mapState.SaveToPlayerPrefs();
                mapState.OnStateChanged += OnMapStateChanged;
                
                // Manually refresh visuals without triggering movement
                RefreshAllVisuals();
            }
        }

        private void OnMapStateChanged()
        {
            // Refresh visuals when state changes
            RefreshAllVisuals();
            
            // Auto-move car only when active node changes (e.g., node completion unlocks new node)
            // This is indicated by the active node being different from where the car currently is
            if (carController != null && nodeManager != null && mapState != null)
            {
                NodeId targetNode = mapState.ActiveNodeId;
                NodeId currentCarNode = mapState.CurrentCarNodeId;
                
                // Only auto-move if a new node was unlocked and became active
                if (!currentCarNode.Equals(targetNode) && mapState.IsNodeCompleted(currentCarNode))
                {
                    carController.MoveToNode(targetNode, nodeManager);
                }
            }
        }

        #endregion

        #region Input Handling

        private void HandleKeyboardInput()
        {
            if (mapState == null || carController == null || nodeManager == null) return;

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int nextIndex = mapState.CurrentCarNodeId.Value + 1;
                if (nextIndex < mapState.NodeCount)
                {
                    var nextNode = new NodeId(nextIndex);
                    if (mapState.IsNodeUnlocked(nextNode))
                    {
                        // Directly move the car (it will update state when it arrives)
                        carController.MoveToNode(nextNode, nodeManager);
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int prevIndex = mapState.CurrentCarNodeId.Value - 1;
                if (prevIndex >= 0)
                {
                    var prevNode = new NodeId(prevIndex);
                    if (mapState.IsNodeUnlocked(prevNode))
                    {
                        // Directly move the car (it will update state when it arrives)
                        carController.MoveToNode(prevNode, nodeManager);
                    }
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

        /// <summary>
        /// Get the current node where the car is located
        /// </summary>
        public int GetCurrentCarNodeIndex()
        {
            return mapState?.CurrentCarNodeId.Value ?? 0;
        }

        /// <summary>
        /// Check if the car is currently at a completed node that has a driving scene
        /// </summary>
        public bool IsCarAtCompletedNodeWithDriving()
        {
            if (mapState == null || nodeManager == null) return false;
            
            // Don't show button if car is currently moving
            if (carController != null && carController.IsMoving)
                return false;
            
            NodeId currentCarNode = mapState.CurrentCarNodeId;
            
            // Check if current node is completed
            if (!mapState.IsNodeCompleted(currentCarNode))
                return false;
            
            // Check if node has driving scene assigned
            NodeData nodeData = nodeManager.GetNodeData(currentCarNode);
            return nodeData != null && !string.IsNullOrEmpty(nodeData.drivingSceneName);
        }

        /// <summary>
        /// Get the driving scene name for the current car node
        /// </summary>
        public string GetCurrentNodeDrivingScene()
        {
            if (nodeManager == null || mapState == null) return null;
            
            NodeId currentCarNode = mapState.CurrentCarNodeId;
            NodeData nodeData = nodeManager.GetNodeData(currentCarNode);
            return nodeData?.drivingSceneName;
        }

        /// <summary>
        /// Subscribe to state changes (for UI elements that need to react)
        /// </summary>
        public void SubscribeToStateChanges(System.Action callback)
        {
            if (mapState != null)
            {
                mapState.OnStateChanged += callback;
            }
        }

        /// <summary>
        /// Unsubscribe from state changes
        /// </summary>
        public void UnsubscribeFromStateChanges(System.Action callback)
        {
            if (mapState != null)
            {
                mapState.OnStateChanged -= callback;
            }
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

        /// <summary>
        /// Applies all upgrades from completed nodes to the car.
        /// Called on game load to restore the car's visual state.
        /// </summary>
        private void ApplyAccumulatedUpgrades()
        {
            if (nodeManager == null || mapState == null)
                return;

            var carVisual = FindFirstObjectByType<CarVisual>();
            if (carVisual == null)
            {
                Debug.LogWarning("[MapControllerSimple] CarVisual not found - cannot apply upgrades");
                return;
            }

            // Apply upgrades from all completed nodes in order
            for (int i = 0; i < mapState.NodeCount; i++)
            {
                var nodeId = new NodeId(i);
                if (mapState.IsNodeCompleted(nodeId))
                {
                    var nodeData = nodeManager.GetNodeData(nodeId);
                    if (nodeData != null && (nodeData.upgradeFrame != null || nodeData.upgradeTire != null))
                    {
                        Debug.Log($"[MapControllerSimple] Applying upgrade from completed node {i}");
                        carVisual.ApplyUpgrade(nodeData.upgradeFrame, nodeData.upgradeTire);
                    }
                }
            }
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (mapState != null)
            {
                mapState.OnStateChanged -= OnMapStateChanged;
            }

            if (nodeManager != null)
                nodeManager.OnNodeClicked -= HandleNodeClicked;

            if (carController != null)
                carController.OnArrivedAtNode -= HandleCarArrived;
        }

        #endregion
    }
}