using UnityEngine;
using Nodemap.Core;
using Nodemap.Car;
using Nodemap.UI;

namespace Nodemap.Controllers
{
    // Drop-in replacement for MapController with improved architecture while maintaining compatibility
    public class MapController : MonoBehaviour
    {
        [Header("Configuration")]
        private MapConfig config; // Auto-loaded via singleton
        
        [Header("Component References")]
        [SerializeField] private NodeManager nodeManager;
        [SerializeField] private CarController carController;
        [SerializeField] private PopupController popupController;

        // Core systems
        private MapState mapState;
        
        private void Awake()
        {
            // Ensure time is running normally
            Time.timeScale = 1f;
            
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

            // Show tutorial when car arrives at a node, but only if not already completed
            if (MapTutorialManager.Instance != null && PlayerPrefs.GetInt("HasSeenTutorial", 0) != 1)
            {
                MapTutorialManager.Instance.StartTutorial();
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
                // NOTE: Automatic movement to the next node on state change was removed.
                // The car will no longer auto-drive when a node is completed.
                // If you need programmatic movement, call `carController.MoveToNode(...)` explicitly.
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

        // Called by quiz/minigame systems when a node is completed
        public void CompleteNode(int nodeIndex)
        {
            var nodeId = new NodeId(nodeIndex);
            mapState?.TryCompleteNode(nodeId);
        }

        // Reset all progression
        public void ResetProgression()
        {
            mapState?.ResetProgression();
        }

        // Get current active node index (for backward compatibility)
        public int GetCurrentActiveNodeIndex()
        {
            return mapState?.ActiveNodeId.Value ?? 0;
        }

        // Check if node is unlocked (for backward compatibility)
        public bool IsUnlocked(int nodeIndex)
        {
            return mapState?.IsNodeUnlocked(new NodeId(nodeIndex)) ?? false;
        }

        // Check if node is completed (for backward compatibility)
        public bool IsCompleted(int nodeIndex)
        {
            return mapState?.IsNodeCompleted(new NodeId(nodeIndex)) ?? false;
        }

        // Get the current node where the car is located
        public int GetCurrentCarNodeIndex()
        {
            return mapState?.CurrentCarNodeId.Value ?? 0;
        }

        // Check if the car is currently at a completed node that has a driving scene
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

        // Get the driving scene name for the current car node
        public string GetCurrentNodeDrivingScene()
        {
            if (nodeManager == null || mapState == null) return null;
            
            NodeId currentCarNode = mapState.CurrentCarNodeId;
            NodeData nodeData = nodeManager.GetNodeData(currentCarNode);
            return nodeData?.drivingSceneName;
        }

        // Subscribe to state changes (for UI elements that need to react)
        public void SubscribeToStateChanges(System.Action callback)
        {
            if (mapState != null)
            {
                mapState.OnStateChanged += callback;
            }
        }

        // Unsubscribe from state changes
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

        // Applies all upgrades from completed nodes to the car on game load
        private void ApplyAccumulatedUpgrades()
        {
            if (nodeManager == null || mapState == null)
                return;

            var carVisual = GameServices.Instance?.CarVisual;
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