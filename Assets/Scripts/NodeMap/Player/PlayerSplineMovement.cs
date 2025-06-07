using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using TMPro;
using NodeMap.Nodes;
using NodeMap.Movement;

namespace NodeMap
{
    /// <summary>
    /// Main controller that coordinates player movement between nodes
    /// </summary>
    public class PlayerSplineMovement : MonoBehaviour
    {
        [Header("Spline Setup")]
        [SerializeField] private SplineContainer spline;

        [Header("Feedback UI")]
        [SerializeField] private TextMeshProUGUI statusMessageText;

        // Component references
        private SplineMovementController movementController;
        private NodeStateManager nodeStateManager;
        private PlayerProgressManager progressManager;
        private SplineStopGenerator stopGenerator;
        private PlayerMovementSequencer sequencer;

        private bool isMoving = false;
        private Coroutine activeMessageCoroutine;

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
            stopGenerator.GenerateStops();
        }

        private void Start()
        {
            // Only proceed if components are valid
            if (!ValidateComponents())
            {
                Debug.LogError("[PlayerSplineMovement] Cannot start - missing required components!");
                return;
            }

            InitializePosition();

            if (stopGenerator.GetStops().Count > 0)
            {
                if (progressManager.HasSavedProgress())
                {
                    StartCoroutine(progressManager.LoadSavedProgress());
                }
                else
                {
                    StartCoroutine(sequencer.StartSequence());
                }
            }
            else
            {
                Debug.LogError("No stops available to move to!");
            }
        }

        private void Update()
        {
            HandleKeyboardInput();
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            movementController = GetComponent<SplineMovementController>();
            nodeStateManager = GetComponent<NodeStateManager>();
            progressManager = GetComponent<PlayerProgressManager>();
            stopGenerator = GetComponent<SplineStopGenerator>();
            sequencer = GetComponent<PlayerMovementSequencer>();

            // Validate all components are present
            if (!ValidateComponents())
            {
                Debug.LogError($"[PlayerSplineMovement] Missing required components on {gameObject.name}. Please add all required components.");
                return;
            }

            // Initialize components with dependencies only if all components exist
            progressManager.Initialize(movementController, nodeStateManager, stopGenerator.GetStops(), spline);
            sequencer.Initialize(movementController, nodeStateManager, stopGenerator.GetStops(), spline);
        }

        private bool ValidateComponents()
        {
            bool isValid = true;

            if (movementController == null)
            {
                Debug.LogError($"[PlayerSplineMovement] SplineMovementController component missing on {gameObject.name}");
                isValid = false;
            }

            if (nodeStateManager == null)
            {
                Debug.LogError($"[PlayerSplineMovement] NodeStateManager component missing on {gameObject.name}");
                isValid = false;
            }

            if (progressManager == null)
            {
                Debug.LogError($"[PlayerSplineMovement] PlayerProgressManager component missing on {gameObject.name}");
                isValid = false;
            }

            if (stopGenerator == null)
            {
                Debug.LogError($"[PlayerSplineMovement] SplineStopGenerator component missing on {gameObject.name}");
                isValid = false;
            }

            if (sequencer == null)
            {
                Debug.LogError($"[PlayerSplineMovement] PlayerMovementSequencer component missing on {gameObject.name}");
                isValid = false;
            }

            if (spline == null)
            {
                Debug.LogError($"[PlayerSplineMovement] SplineContainer not assigned in inspector on {gameObject.name}");
                isValid = false;
            }

            return isValid;
        }

        private void InitializePosition()
        {
            transform.position = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(0f));
        }
        #endregion

        #region Input Handling
        private void HandleKeyboardInput()
        {
            if (isMoving || (PopupManager.Instance != null && PopupManager.Instance.IsPopupActive()))
                return;

            if (Input.GetKeyDown(KeyCode.RightArrow))
                TryMoveToNode(NodeMapManager.Instance.CurrentNodeIndex + 1);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                TryMoveToNode(NodeMapManager.Instance.CurrentNodeIndex - 1);
        }
        #endregion

        #region Node Movement
        /// <summary>
        /// Attempts to move to a specific node if conditions allow
        /// </summary>
        public void TryMoveToNode(int targetNode)
        {
            if (isMoving)
                return;

            int currentNode = NodeMapManager.Instance.CurrentNodeIndex;

            if (!progressManager.CanMoveToNode(targetNode, currentNode))
            {
                // Handle invalid movement attempt
                if (targetNode > currentNode && !nodeStateManager.IsNodeCompleted(currentNode))
                {
                    nodeStateManager.ShakeNode(currentNode);
                    ShowStatusMessage("Complete the current node first.");
                }
                return;
            }

            bool isMovingForward = targetNode > currentNode;
            StartCoroutine(MoveToNode(targetNode, isMovingForward));
        }

        /// <summary>
        /// Coroutine that handles the actual movement to a node
        /// </summary>
        private IEnumerator MoveToNode(int targetNode, bool isMovingForward)
        {
            isMoving = true;

            // Reset current node if valid
            if (NodeMapManager.Instance.CurrentNodeIndex != -1)
                nodeStateManager.SetNodeToNormal(NodeMapManager.Instance.CurrentNodeIndex);

            // Get starting position
            float startT = 0f;
            if (NodeMapManager.Instance.CurrentNodeIndex >= 0 &&
                NodeMapManager.Instance.CurrentNodeIndex < stopGenerator.GetStops().Count)
            {
                startT = stopGenerator.GetStops()[NodeMapManager.Instance.CurrentNodeIndex].splinePercent;
            }

            // Mark no active node during movement
            NodeMapManager.Instance.SetCurrentNode(-1);

            // Do the movement
            yield return movementController.MoveAlongSpline(spline, startT,
                stopGenerator.GetStops()[targetNode].splinePercent, isMovingForward);

            // Update node state
            isMoving = false;
            NodeMapManager.Instance.SetCurrentNode(targetNode);
            nodeStateManager.SetNodeToActive(targetNode);
        }
        #endregion

        #region Public Methods
        public void SetNodeToComplete(int nodeIndex)
        {
            nodeStateManager.SetNodeToComplete(nodeIndex);
        }

        public bool IsNodeCompleted(int nodeIndex) => nodeStateManager.IsNodeCompleted(nodeIndex);
        #endregion

        #region Utility
        private void ShowStatusMessage(string message)
        {
            if (statusMessageText != null)
            {
                if (activeMessageCoroutine != null)
                {
                    StopCoroutine(activeMessageCoroutine);
                }
                activeMessageCoroutine = StartCoroutine(UI.UIAnimator.ShowTemporaryMessage(statusMessageText, message, 1.5f, 0.25f));
            }
        }
        #endregion

        #region Editor Tools
#if UNITY_EDITOR
        public void ForceGenerateStops() => stopGenerator.ForceGenerateStops();
#endif
        public SplineContainer GetSpline() => spline;
        #endregion
    }
}