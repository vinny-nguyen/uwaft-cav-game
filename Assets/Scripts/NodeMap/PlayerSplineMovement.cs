using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Added for TextMeshProUGUI

namespace NodeMap
{
    /// <summary>
    /// Handles player movement along a spline path between nodes
    /// </summary>
    public class PlayerSplineMovement : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Spline Setup")]
        [SerializeField] private SplineContainer spline;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float minMoveDuration = 0.1f;

        [Header("Wheel Setup")]
        [SerializeField] private Transform frontWheel;
        [SerializeField] private Transform rearWheel;
        [SerializeField] private float wheelSpinSpeed = 360f;

        [Header("Smoke VFX")]
        [SerializeField] private ParticleSystem smokeParticles;

        [Header("Node Setup")]
        [SerializeField] private List<GameObject> nodeMarkers = new();
        [SerializeField] private List<Sprite> normalNodeSprites = new();
        [SerializeField] private List<Sprite> activeNodeSprites = new();
        [SerializeField] private List<Sprite> completeNodeSprites;

        [Header("Tutorial")]
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private int firstNodeIndex = 0;

        [Header("Feedback UI")] // Added Header for clarity
        [SerializeField] private TextMeshProUGUI statusMessageText; // Reference for status messages

        [Header("Scene Management")] // Added for SceneTransitionManager reference
        [SerializeField] private NodeMap.UI.SceneTransitionManager sceneTransitionManager;
        #endregion

        #region Private Fields
        private readonly List<SplineStop> stops = new();
        private bool isMoving = false;
        private bool isMovingForward = true;
        private Coroutine activeMessageCoroutine; // To manage the message display coroutine
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            GenerateStops();
        }

        private void Start()
        {
            InitializePosition();

            if (stops.Count > 0)
            {
                if (PlayerPrefs.HasKey("CurrentNodeIndex"))
                {
                    StartCoroutine(LoadSavedProgress());
                }
                else
                {
                    StartCoroutine(StartSequence());
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
        private void InitializePosition()
        {
            // Sets the car's initial visual position to the very start (0%) of the spline.
            transform.position = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(0f));
        }

        private void GenerateStops()
        {
            stops.Clear();
            int numberOfStops = 6;
            for (int i = 1; i <= numberOfStops; i++)
            {
                float percent = i / 7f;
                SplineStop stop = new SplineStop
                {
                    splinePercent = percent,
                    offset = Vector3.zero
                };

                // If we have sprites available, assign them
                if (i - 1 < normalNodeSprites.Count)
                    stop.nodeSprite = normalNodeSprites[i - 1];

                stops.Add(stop);
            }
        }
        #endregion

        #region Input Handling
        private void HandleKeyboardInput()
        {
            if (isMoving || (PopupManager.Instance != null && PopupManager.Instance.IsPopupActive()))
                return;

            if (Input.GetKeyDown(KeyCode.RightArrow))
                TryMoveToNode(NopeMapManager.Instance.CurrentNodeIndex + 1);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                TryMoveToNode(NopeMapManager.Instance.CurrentNodeIndex - 1);
        }
        #endregion

        #region Node Movement
        /// <summary>
        /// Initial sequence to move player to first node
        /// </summary>
        private IEnumerator StartSequence()
        {
            // Wait for SceneTransitionManager to be ready and opening transition to complete
            Debug.Log("[PlayerSplineMovement] StartSequence initiated. Waiting for opening transition...");
            while (NodeMap.UI.SceneTransitionManager.Instance == null || !NodeMap.UI.SceneTransitionManager.Instance.IsOpeningTransitionComplete)
            {
                if (NodeMap.UI.SceneTransitionManager.Instance == null)
                {
                    // This case should ideally not happen if script execution order is fine
                    // or if PlayerSplineMovement depends on SceneTransitionManager.
                    // Debug.LogWarning("[PlayerSplineMovement] Waiting for SceneTransitionManager instance...");
                }
                yield return null; // Wait a frame
            }

            Debug.Log("[PlayerSplineMovement] Opening transition complete. Waiting 2 seconds before moving car.");
            yield return new WaitForSeconds(2f);

            // Move car from spline start to first node
            Debug.Log("[PlayerSplineMovement] Starting car movement to first node.");
            // The car's first animated movement starts from 0% along the spline to the 'firstNodeIndex'.
            yield return MoveAlongSpline(0f, stops[firstNodeIndex].splinePercent); // Use firstNodeIndex

            // Set up first node
            SetNodeToActive(firstNodeIndex); // Use firstNodeIndex
            NopeMapManager.Instance.SetCurrentNode(firstNodeIndex); // Use firstNodeIndex

            // Check for tutorial
            // Debug.Log($"[PlayerSplineMovement] Reached first node. TutorialManager: {tutorialManager != null}, HasCompletedTutorial: {(tutorialManager != null ? tutorialManager.HasCompletedTutorial().ToString() : "N/A")}"); // Keep for debugging if needed
            if (tutorialManager != null && !tutorialManager.HasCompletedTutorial())
            {
                // tutorialManager.TriggerNodeReachedTutorial(); // Old direct call
                if (sceneTransitionManager != null)
                {
                    Debug.Log("[PlayerSplineMovement] Reached first node. Waiting 1 second before initiating zoom out and tutorial sequence.");
                    yield return new WaitForSeconds(1f); // Wait 1 second before zoom out

                    // Debug.Log("[PlayerSplineMovement] Calling StartZoomOutMapTransition on SceneTransitionManager."); // Keep for debugging if needed
                    // sceneTransitionManager.StartZoomOutMapTransition(); // Old direct call
                    Debug.Log("[PlayerSplineMovement] Calling InitiateZoomOutAndTutorialSequence on SceneTransitionManager.");
                    sceneTransitionManager.InitiateZoomOutAndTutorialSequence();
                }
                else
                {
                    Debug.LogWarning("[PlayerSplineMovement] SceneTransitionManager reference is null. Cannot start zoom out and tutorial sequence.");
                    // Fallback: If SceneTransitionManager is missing, but tutorial should run, start it directly.
                    tutorialManager.StartTutorial();
                }
            }
        }

        /// <summary>
        /// Attempts to move to a specific node if conditions allow
        /// </summary>
        public void TryMoveToNode(int targetNode)
        {
            if (targetNode < 0 || targetNode >= stops.Count || isMoving)
                return;

            int currentNode = NopeMapManager.Instance.CurrentNodeIndex;

            // Prevent forward movement if current node is not complete
            if (targetNode > currentNode && !IsNodeCompleted(currentNode))
            {
                ShakeCurrentNode(currentNode);
                if (statusMessageText != null)
                {
                    if (activeMessageCoroutine != null)
                    {
                        StopCoroutine(activeMessageCoroutine);
                    }
                    activeMessageCoroutine = StartCoroutine(UI.UIAnimator.ShowTemporaryMessage(statusMessageText, "Complete the current node first.", 1.5f, 0.25f));
                }
                return;
            }

            isMovingForward = targetNode > currentNode;
            StartCoroutine(MoveToNode(targetNode));
        }

        /// <summary>
        /// Coroutine that handles the actual movement to a node
        /// </summary>
        private IEnumerator MoveToNode(int targetNode)
        {
            isMoving = true;

            // Reset current node if valid
            if (NopeMapManager.Instance.CurrentNodeIndex != -1)
                SetNodeToNormal(NopeMapManager.Instance.CurrentNodeIndex);

            // Get starting position
            float startT = 0f;
            if (NopeMapManager.Instance.CurrentNodeIndex >= 0 &&
                NopeMapManager.Instance.CurrentNodeIndex < stops.Count)
            {
                startT = stops[NopeMapManager.Instance.CurrentNodeIndex].splinePercent;
            }

            // Mark no active node during movement
            NopeMapManager.Instance.SetCurrentNode(-1);

            // Do the movement
            yield return MoveAlongSpline(startT, stops[targetNode].splinePercent);

            // Update node state
            isMoving = false;
            NopeMapManager.Instance.SetCurrentNode(targetNode);
            SetNodeToActive(targetNode);
        }

        /// <summary>
        /// Moves the player along the spline from one percentage to another
        /// </summary>
        private IEnumerator MoveAlongSpline(float startT, float endT)
        {
            // Start effects
            if (smokeParticles != null)
            {
                smokeParticles.gameObject.SetActive(true);
                smokeParticles.Play();
            }

            if (spline == null)
            {
                Debug.LogError("Cannot move - spline is null!");
                yield break;
            }

            // Calculate move duration
            Vector3 startPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(startT));
            Vector3 endPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(endT));
            float distance = Vector3.Distance(startPos, endPos);
            float duration = Mathf.Max(distance / moveSpeed, minMoveDuration);

            // Animation loop
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float easedProgress = progress * progress * (3f - 2f * progress); // Smoothstep
                float splineT = Mathf.Lerp(startT, endT, easedProgress);

                // Update position with bounce
                Vector3 worldPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(splineT));
                worldPos.y += Mathf.Sin(Time.time * 5f) * 0.05f; // Add bounce
                transform.position = worldPos;

                // Update rotation to follow spline tangent
                Vector3 tangent = spline.transform.TransformDirection((Vector3)spline.EvaluateTangent(splineT)).normalized;
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);

                // Rotate wheels
                float direction = isMovingForward ? 1f : -1f;
                if (frontWheel != null)
                    frontWheel.Rotate(Vector3.forward, -wheelSpinSpeed * Time.deltaTime * direction);
                if (rearWheel != null)
                    rearWheel.Rotate(Vector3.forward, -wheelSpinSpeed * Time.deltaTime * direction);

                yield return null;
            }

            // Ensure final position and rotation are precise
            UpdatePlayerPosition(endT);
            UpdatePlayerRotation(endT);

            // Stop effects
            if (smokeParticles != null && smokeParticles.isPlaying)
                smokeParticles.Stop();
        }

        private void UpdatePlayerPosition(float splineT)
        {
            Vector3 worldPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(splineT));
            worldPos.y += Mathf.Sin(Time.time * 5f) * 0.05f;
            transform.position = worldPos;
        }

        private void UpdatePlayerRotation(float splineT)
        {
            Vector3 tangent = spline.transform.TransformDirection((Vector3)spline.EvaluateTangent(splineT)).normalized;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);
        }

        private void ShakeCurrentNode(int nodeIndex)
        {
            if (nodeIndex >= 0 && nodeIndex < nodeMarkers.Count)
            {
                NodeHoverHandler handler = nodeMarkers[nodeIndex].GetComponent<NodeHoverHandler>();
                if (handler != null)
                    handler.StartShake();
            }
        }
        #endregion

        #region Node State Management
        /// <summary>
        /// Sets a node to a specific state and updates its visual appearance
        /// </summary>
        private void SetNodeState(int nodeIndex, NodeState state)
        {
            // Skip if completed node is being changed to non-complete state
            if (NopeMapManager.Instance.IsNodeCompleted(nodeIndex) && state != NodeState.Complete)
                return;

            // Ensure node index is valid
            if (nodeIndex < 0 || nodeIndex >= nodeMarkers.Count)
                return;

            GameObject marker = nodeMarkers[nodeIndex];
            if (marker == null)
                return;

            // Get the sprite for this state
            Sprite stateSprite = null;
            switch (state)
            {
                case NodeState.Normal:
                    stateSprite = nodeIndex < normalNodeSprites.Count ? normalNodeSprites[nodeIndex] : null;
                    break;
                case NodeState.Active:
                    stateSprite = nodeIndex < activeNodeSprites.Count ? activeNodeSprites[nodeIndex] : null;
                    break;
                case NodeState.Complete:
                    stateSprite = nodeIndex < completeNodeSprites.Count ? completeNodeSprites[nodeIndex] : null;
                    break;
            }

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

        // Convenience methods
        private void SetNodeToNormal(int nodeIndex) => SetNodeState(nodeIndex, NodeState.Normal);
        private void SetNodeToActive(int nodeIndex) => SetNodeState(nodeIndex, NodeState.Active);

        public void SetNodeToComplete(int nodeIndex)
        {
            if (!NopeMapManager.Instance.IsNodeCompleted(nodeIndex))
                NopeMapManager.Instance.CompleteNode(nodeIndex);

            SetNodeState(nodeIndex, NodeState.Complete);
        }

        /// <summary>
        /// Updates all node visuals based on their state
        /// </summary>
        private void UpdateAllNodeVisuals()
        {
            int currentNode = NopeMapManager.Instance.CurrentNodeIndex;

            for (int i = 0; i < nodeMarkers.Count; i++)
            {
                if (NopeMapManager.Instance.IsNodeCompleted(i))
                    SetNodeState(i, NodeState.Complete);
                else if (i == currentNode)
                    SetNodeState(i, NodeState.Active);
                else
                    SetNodeState(i, NodeState.Normal);
            }
        }
        #endregion

        #region Progress Management
        public bool IsNodeCompleted(int nodeIndex) => NopeMapManager.Instance.IsNodeCompleted(nodeIndex);

        /// <summary>
        /// Loads saved progress and positions the player at the saved node
        /// </summary>
        private IEnumerator LoadSavedProgress()
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
                StartCoroutine(StartSequence());
                yield break;
            }

            // Position at saved node
            float nodePosition = stops[currentNodeIndex].splinePercent;
            transform.position = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(nodePosition));

            // Set rotation
            Vector3 tangent = spline.transform.TransformDirection((Vector3)spline.EvaluateTangent(nodePosition)).normalized;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);

            // Update node visuals
            UpdateAllNodeVisuals();
            SetNodeToActive(currentNodeIndex);
        }
        #endregion

        #region Editor Tools
#if UNITY_EDITOR
        public void ForceGenerateStops() => GenerateStops();
#endif
        public List<SplineStop> GetStops() => stops;
        public SplineContainer GetSpline() => spline;
        #endregion
    }
}