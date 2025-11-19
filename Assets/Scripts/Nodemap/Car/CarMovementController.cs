using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using Nodemap.Core;
using Nodemap;

namespace Nodemap.Car
{
    /// <summary>
    /// Unified car movement system. Combines responsibilities of CarController and CarPathFollower
    /// into a single, focused class. Handles both movement and visual updates.
    /// </summary>
    public class CarMovementController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MapConfig config;
        
        [Header("References")]
        [SerializeField] private SplineContainer spline;
        [SerializeField] private Transform tireFront;
        [SerializeField] private Transform tireRear;
        
        [Header("Spline Positioning")]
        [SerializeField] private float spawnNormalizedT = 0f;

        // Current state
        private float currentNormalizedT;
        private NodeId currentNodeId;
        private bool isMoving;
        private bool isSpinning;
        private int wheelSpinDirection = 1;

        // Events - simple and focused
        public event Action<NodeId> OnArrivedAtNode;

        private void Awake()
        {
            if (config == null) config = MapConfig.Instance;
            currentNormalizedT = spawnNormalizedT;
            currentNodeId = new NodeId(0);
        }

        private void Start()
        {
            SnapToPosition(currentNormalizedT);
        }

        private void Update()
        {
            UpdateWheelSpin();
        }

        #region Public API

        /// <summary>
        /// Moves car to specified node. Returns false if movement is not possible.
        /// </summary>
        public bool MoveToNode(NodeId nodeId, NodeManagerSimple nodeManager)
        {
            if (isMoving || nodeManager == null)
                return false;

            float targetT = nodeManager.GetSplineT(nodeId);
            StartCoroutine(MovementCoroutine(nodeId, targetT));
            return true;
        }

        /// <summary>
        /// Instantly snaps car to specified node.
        /// </summary>
        public void SnapToNode(NodeId nodeId, NodeManagerSimple nodeManager)
        {
            if (nodeManager == null) return;
            
            float targetT = nodeManager.GetSplineT(nodeId);
            SnapToPosition(targetT);
            currentNodeId = nodeId;
        }

        /// <summary>
        /// Gets current node position of the car.
        /// </summary>
        public NodeId GetCurrentNode() => currentNodeId;

        /// <summary>
        /// Gets current normalized position on spline.
        /// </summary>
        public float GetNormalizedPosition() => currentNormalizedT;

        #endregion

        #region Movement Implementation

        private IEnumerator MovementCoroutine(NodeId targetNodeId, float targetT)
        {
            isMoving = true;

            float startT = currentNormalizedT;
            wheelSpinDirection = targetT >= startT ? 1 : -1;
            StartWheelSpin();

            // Calculate movement parameters
            Vector3 startPos = spline.EvaluatePosition(startT);
            Vector3 endPos = spline.EvaluatePosition(targetT);
            float distance = Vector3.Distance(startPos, endPos);
            
            float moveSpeed = config ? config.moveSpeed : 2f;
            float minDuration = config ? config.minMoveDuration : 0.1f;
            float duration = Mathf.Max(distance / moveSpeed, minDuration);

            // Animate movement
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float easedProgress = EaseInOutCubic(progress);
                
                currentNormalizedT = Mathf.Lerp(startT, targetT, easedProgress);
                UpdatePosition();
                
                yield return null;
            }

            // Ensure final position is precise
            currentNormalizedT = targetT;
            UpdatePosition();
            
            StopWheelSpin();
            currentNodeId = targetNodeId;
            isMoving = false;
            
            OnArrivedAtNode?.Invoke(targetNodeId);
        }

        private void SnapToPosition(float normalizedT)
        {
            currentNormalizedT = Mathf.Clamp01(normalizedT);
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (spline == null) return;

            // Get base position and add bounce
            Vector3 worldPos = spline.EvaluatePosition(currentNormalizedT);
            
            if (isMoving)
            {
                float bounceFreq = config ? config.carBounceFrequency : 5f;
                float bounceAmp = config ? config.carBounceAmplitude : 0.05f;
                worldPos.y += Mathf.Sin(Time.time * bounceFreq) * bounceAmp;
            }
            
            transform.position = worldPos;

            // Update rotation to follow spline
            Vector3 tangent = spline.EvaluateTangent(currentNormalizedT);
            if (tangent.magnitude > 0.001f)
            {
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        #endregion

        #region Wheel Animation

        private void StartWheelSpin()
        {
            isSpinning = true;
        }

        private void StopWheelSpin()
        {
            isSpinning = false;
        }

        private void UpdateWheelSpin()
        {
            if (!isSpinning) return;

            float spinSpeed = config ? config.wheelSpinSpeed : 360f;
            float deltaRotation = Time.deltaTime * spinSpeed * wheelSpinDirection;
            
            if (tireFront != null)
                tireFront.Rotate(0f, 0f, -deltaRotation);
                
            if (tireRear != null)
                tireRear.Rotate(0f, 0f, -deltaRotation);
        }

        #endregion

        #region Utilities

        private float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            StopAllCoroutines();
            OnArrivedAtNode = null;
        }

        #endregion
    }
}