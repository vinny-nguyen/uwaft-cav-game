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
        }

        void Start()
        {
            InitializeParticles();
            InitializePosition();

            if (stops.Count > 0)
            {
                StartCoroutine(StartSequence());
            }
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
            if (smokeParticles != null)
                smokeMain = smokeParticles.main;
        }

        private void InitializePosition()
        {
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
            // Move car from spline start (T=0) to first node (1/7)
            yield return MoveAlongSpline(0f, stops[0].splinePercent);

            // After arriving at first node
            SetNodeToActive(0);
            NopeMapManager.Instance.SetCurrentNode(1);
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
            StartSmokeEffect();

            Vector3 startPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(startT));
            Vector3 endPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(endT));
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float easedProgress = EaseInOut(progress);
                float splineT = Mathf.Lerp(startT, endT, easedProgress);

                UpdatePlayerPosition(splineT);
                UpdatePlayerRotation(splineT);
                RotateWheels(isMovingForward ? 1f : -1f);

                yield return null;
            }

            StopSmokeEffect();
        }
        #endregion

        #region Movement Helpers
        private void StartSmokeEffect()
        {
            if (smokeParticles != null && !smokeParticles.isPlaying)
                smokeParticles.Play();
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
            NopeMapManager.Instance.SetCurrentNode(nodeIndex + 1);
            TryMoveToNode(NopeMapManager.Instance.CurrentNodeIndex + 1);
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
