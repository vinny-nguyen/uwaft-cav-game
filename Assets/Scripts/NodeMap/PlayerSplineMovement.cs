using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using System.Collections.Generic;

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

        [Header("Wheel Setup")]
        [SerializeField] private Transform frontWheel;
        [SerializeField] private Transform rearWheel;
        [SerializeField] private float wheelSpinSpeed = 360f;

        [Header("Smoke VFX")]
        [SerializeField] private ParticleSystem smokeParticles;

        [Header("Node Setup")]
        [SerializeField] private List<GameObject> nodeMarkers = new List<GameObject>();
        [SerializeField] private List<Sprite> normalNodeSprites = new List<Sprite>();
        [SerializeField] private List<Sprite> activeNodeSprites = new List<Sprite>();
        [SerializeField] private List<Sprite> completeNodeSprites;

        [Header("Tutorial")]
        [SerializeField] private TutorialManager tutorialManager;
        [SerializeField] private int firstNodeIndex = 0; // The index of the first tutorial node
        #endregion

        #region Private Fields
        private List<SplineStop> stops = new List<SplineStop>();
        private bool isMoving = false;
        private bool isMovingForward = true;
        private ParticleSystem.MainModule smokeMain;
        private HashSet<int> completedNodes = new HashSet<int>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeStops();
            Debug.Log("Awake is running in PlayerSplineMovement.");
        }

        private void Start()
        {
            Debug.Log("Start is running in PlayerSplineMovement.");

            // Validate critical components
            if (spline == null)
            {
                Debug.LogError("Spline Container is missing!");
            }

            if (smokeParticles == null)
            {
                Debug.LogError("Smoke particle system is missing!");
            }
            else
            {
                Debug.Log($"Smoke particles found: {smokeParticles.gameObject.name}, Is active: {smokeParticles.gameObject.activeSelf}");
            }

            if (frontWheel == null || rearWheel == null)
            {
                Debug.LogError("One or more wheel transforms are missing!");
            }

            InitializeParticles();
            InitializePosition();

            if (stops.Count > 0)
            {
                Debug.Log($"Starting sequence to move to first node. Stop count: {stops.Count}");
                StartCoroutine(StartSequence());
            }
            else
            {
                Debug.LogError("No stops available to move to!");
            }
            Time.timeScale = 1;
        }

        void Update()
        {
            HandleKeyboardInput();
        }
        #endregion

        #region Initialization
        private void InitializeStops()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ForceGenerateStops();
                return;
            }
#endif
            GenerateStops();
        }

        private void InitializeParticles()
        {
            Debug.Log("Initializing smoke particles.");
            if (smokeParticles != null)
                smokeMain = smokeParticles.main;
        }

        private void InitializePosition()
        {
            Debug.Log("Initializing player position.");
            transform.position = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(0f));
        }
        #endregion

        #region Input Handling
        private void HandleKeyboardInput()
        {
            if (isMoving)
                return;

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                TryMoveToNode(NopeMapManager.Instance.CurrentNodeIndex + 1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                TryMoveToNode(NopeMapManager.Instance.CurrentNodeIndex - 1);
            }
        }
        #endregion

        #region Node Movement
        /// <summary>
        /// Initial sequence to move player to first node
        /// </summary>
        private IEnumerator StartSequence()
        {
            Debug.Log("Starting sequence to move player to first node.");
            // Move car from spline start (T=0) to first node (1/7)
            yield return MoveAlongSpline(0f, stops[0].splinePercent);

            // After arriving at first node
            SetNodeToActive(0);
            NopeMapManager.Instance.SetCurrentNode(1);
            if (tutorialManager != null && !tutorialManager.HasCompletedTutorial())
            {
                tutorialManager.TriggerNodeReachedTutorial();
            }
        }

        /// <summary>
        /// Attempts to move to a specific node if conditions allow
        /// </summary>
        public void TryMoveToNode(int targetNode)
        {
            targetNode = targetNode - 1; // Adjust for zero-based index

            if (targetNode < 0 || targetNode >= stops.Count || isMoving)
                return;

            int currentNode = NopeMapManager.Instance.CurrentNodeIndex - 1; // zero-based

            // Prevent forward movement if current node is not complete
            if (targetNode > currentNode && !IsNodeCompleted(currentNode))
            {
                Debug.Log($"Cannot move forward: Node {currentNode + 1} is not yet complete.");
                ShakeCurrentNode(currentNode);
                return;
            }

            isMovingForward = targetNode > currentNode;
            Debug.Log($"Moving to node {targetNode} (isMovingForward: {isMovingForward})");
            StartCoroutine(MoveToNode(targetNode));
        }

        /// <summary>
        /// Coroutine that handles the actual movement to a node
        /// </summary>
        private IEnumerator MoveToNode(int targetNode)
        {
            isMoving = true;

            if (NopeMapManager.Instance.CurrentNodeIndex != -1)
                SetNodeToNormal(NopeMapManager.Instance.CurrentNodeIndex);

            float startT = stops[NopeMapManager.Instance.CurrentNodeIndex - 1].splinePercent;
            float targetT = stops[targetNode].splinePercent;

            Vector3 startPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(startT));
            transform.position = startPos;

            yield return null;

            if (NopeMapManager.Instance != null)
            {
                NopeMapManager.Instance.SetCurrentNode(-1); // -1 = no active node
            }

            yield return MoveAlongSpline(startT, targetT);

            isMoving = false;

            NopeMapManager.Instance.SetCurrentNode(targetNode + 1); // +1 to match the node index in GameManager
            SetNodeToActive(NopeMapManager.Instance.CurrentNodeIndex - 1);
        }

        /// <summary>
        /// Moves the player along the spline from one percentage to another
        /// </summary>
        private IEnumerator MoveAlongSpline(float startT, float endT)
        {
            Debug.Log($"Moving along spline from {startT} to {endT}");
            StartSmokeEffect();

            if (spline == null)
            {
                Debug.LogError("Cannot move - spline is null!");
                yield break;
            }

            Vector3 startPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(startT));
            Vector3 endPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(endT));
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            Debug.Log($"Start position: {startPos}, End position: {endPos}, Distance: {distance}, Duration: {duration}, Speed: {moveSpeed}");

            // Force minimum duration
            if (duration < 0.1f)
            {
                Debug.LogWarning($"Duration too short: {duration}. Setting to minimum value.");
                duration = 0.1f;
            }

            while (elapsed < duration)
            {
                Debug.Log($"Animation progress: {elapsed}/{duration} - {elapsed / duration * 100}%");
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float easedProgress = EaseInOut(progress);
                float splineT = Mathf.Lerp(startT, endT, easedProgress);

                UpdatePlayerPosition(splineT);
                UpdatePlayerRotation(splineT);
                RotateWheels(isMovingForward ? 1f : -1f);

                yield return null;
            }

            // Ensure we end at exactly the target position
            UpdatePlayerPosition(endT);
            UpdatePlayerRotation(endT);

            Debug.Log("Movement along spline complete!");
            StopSmokeEffect();
        }
        #endregion

        #region Movement Helpers
        private void StartSmokeEffect()
        {
            Debug.Log("Starting smoke effect.");
            if (smokeParticles != null)
            {
                if (!smokeParticles.gameObject.activeSelf)
                {
                    smokeParticles.gameObject.SetActive(true);
                    Debug.Log("Smoke particles game object activated");
                }

                if (!smokeParticles.isPlaying)
                {
                    smokeParticles.Play();
                    Debug.Log("Smoke particles started playing");
                }
                else
                {
                    Debug.Log("Smoke particles were already playing");
                }
            }
            else
            {
                Debug.LogError("Cannot start smoke effect - particle system is null");
            }
        }

        private void StopSmokeEffect()
        {
            if (smokeParticles != null && smokeParticles.isPlaying)
                smokeParticles.Stop();
        }

        private void UpdatePlayerPosition(float splineT)
        {
            Vector3 worldPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(splineT));
            float bounce = Mathf.Sin(Time.time * 5f) * 0.05f;
            worldPos.y += bounce;
            transform.position = worldPos;
            Debug.Log($"Updated position to: {worldPos}");
        }

        private void UpdatePlayerRotation(float splineT)
        {
            Vector3 tangent = spline.transform.TransformDirection((Vector3)spline.EvaluateTangent(splineT)).normalized;
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void RotateWheels(float direction)
        {
            if (frontWheel != null)
                frontWheel.Rotate(Vector3.forward, -wheelSpinSpeed * Time.deltaTime * direction);

            if (rearWheel != null)
                rearWheel.Rotate(Vector3.forward, -wheelSpinSpeed * Time.deltaTime * direction);
        }

        private void ShakeCurrentNode(int nodeIndex)
        {
            if (nodeMarkers.Count <= nodeIndex)
                return;

            NodeHoverHandler handler = nodeMarkers[nodeIndex].GetComponent<NodeHoverHandler>();
            if (handler != null)
            {
                handler.StartShake();
            }
        }

        private float EaseInOut(float t)
        {
            return t * t * (3f - 2f * t);
        }
        #endregion

        #region Node State Management
        private void SetNodeToNormal(int nodeIndex)
        {
            nodeIndex = nodeIndex - 1; // Adjust for zero-based index

            if (completedNodes.Contains(nodeIndex))
                return; // Skip — leave as complete (green)

            if (nodeMarkers.Count > nodeIndex && normalNodeSprites.Count > nodeIndex)
            {
                GameObject marker = nodeMarkers[nodeIndex];
                if (marker != null)
                {
                    NodeVisualController visualController = marker.GetComponent<NodeVisualController>();
                    if (visualController != null)
                    {
                        visualController.TransitionToNormal(normalNodeSprites[nodeIndex]);
                    }
                    else
                    {
                        SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
                        if (sr != null && normalNodeSprites[nodeIndex] != null)
                        {
                            sr.sprite = normalNodeSprites[nodeIndex];
                        }
                    }
                }
            }
        }

        private void SetNodeToActive(int nodeIndex)
        {
            if (completedNodes.Contains(nodeIndex))
                return; // Skip — leave as complete (green)

            if (nodeMarkers.Count > nodeIndex && activeNodeSprites.Count > nodeIndex)
            {
                GameObject marker = nodeMarkers[nodeIndex];
                if (marker != null)
                {
                    NodeVisualController visualController = marker.GetComponent<NodeVisualController>();
                    NodeHoverHandler handler = marker.GetComponent<NodeHoverHandler>();

                    if (visualController != null)
                    {
                        visualController.TransitionToActive(activeNodeSprites[nodeIndex]);
                    }
                    else
                    {
                        SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
                        if (sr != null && activeNodeSprites[nodeIndex] != null)
                        {
                            sr.sprite = activeNodeSprites[nodeIndex];
                        }
                    }

                    if (handler != null)
                    {
                        handler.SetClickable(true);
                    }
                }
            }
        }

        /// <summary>
        /// Sets a node to the completed state
        /// </summary>
        public void SetNodeToComplete(int nodeIndex)
        {
            if (!completedNodes.Contains(nodeIndex))
            {
                completedNodes.Add(nodeIndex);
                NopeMapManager.Instance.CompleteNode(nodeIndex);
            }

            if (nodeMarkers.Count > nodeIndex && completeNodeSprites.Count > nodeIndex)
            {
                GameObject marker = nodeMarkers[nodeIndex];
                if (marker != null)
                {
                    NodeVisualController visualController = marker.GetComponent<NodeVisualController>();
                    NodeHoverHandler handler = marker.GetComponent<NodeHoverHandler>();

                    if (visualController != null)
                    {
                        visualController.TransitionToComplete(completeNodeSprites[nodeIndex]);
                    }
                    else
                    {
                        SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
                        if (sr != null && completeNodeSprites[nodeIndex] != null)
                        {
                            sr.sprite = completeNodeSprites[nodeIndex];
                        }
                    }

                    if (handler != null)
                    {
                        handler.SetClickable(true); // Keep completed nodes clickable for review
                    }
                }
            }

            // Update game manager and move to next node
            // NopeMapManager.Instance.SetCurrentNode(nodeIndex + 1);
            // TryMoveToNode(NopeMapManager.Instance.CurrentNodeIndex + 1);
        }
        #endregion

        #region Tutorial Management
        private void CheckForTutorialTrigger(int nodeIndex)
        {
            // Check if this is the first node and tutorial should be shown
            if (nodeIndex == firstNodeIndex && tutorialManager != null && !tutorialManager.HasCompletedTutorial())
            {
                tutorialManager.TriggerNodeReachedTutorial();
            }
        }
        #endregion

        #region Stops Management
        private void GenerateStops()
        {
            stops.Clear();
            int numberOfStops = 6;
            for (int i = 1; i <= numberOfStops; i++)
            {
                float percent = i / 7f;
                stops.Add(new SplineStop { splinePercent = percent });
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Forces stop generation in editor mode
        /// </summary>
        public void ForceGenerateStops()
        {
            GenerateStops();
        }
#endif

        /// <summary>
        /// Returns all spline stops for external tools
        /// </summary>
        public List<SplineStop> GetStops()
        {
            return stops;
        }

        /// <summary>
        /// Returns the spline container for external tools
        /// </summary>
        public SplineContainer GetSpline()
        {
            return spline;
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