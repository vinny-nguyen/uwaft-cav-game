using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Nodemap.Core;
using Nodemap.UI;

namespace Nodemap.Controllers
{
    /// <summary>
    /// Simplified NodeManager that works with existing NodeView.
    /// Uses NodeId consistently but keeps compatibility with current code.
    /// </summary>
    public class NodeManager : MonoBehaviour
    {
        [Header("Configuration")]
        private MapConfig config; // Auto-loaded via singleton
        
        [Header("Node Management")]
        [SerializeField] private List<NodeData> nodeData;
        [SerializeField] private NodeView nodePrefab;
        [SerializeField] private RectTransform nodesParent;
        
        [Header("Positioning")]
        [SerializeField] private SplineContainer spline;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private RectTransform canvasRect;

        // State
        private readonly List<NodeView> nodeViews = new();
        
        // Events
        public event Action<NodeId> OnNodeClicked;

        private void Awake()
        {
            if (config == null) config = MapConfig.Instance;
        }

        #region Public API

        public void Initialize()
        {
            CreateNodeViews();
            PositionNodes();
        }

        public void UpdateNodeVisual(NodeId nodeId, NodeState state, bool hasCarPresent)
        {
            var nodeView = GetNodeView(nodeId);
            if (nodeView == null) return;

            // Update visual state using existing API
            nodeView.SetState(state, true);
            
            // Simple car presence indicator using scale
            if (hasCarPresent)
            {
                nodeView.transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                nodeView.transform.localScale = Vector3.one;
            }
        }

        // Overload for backward compatibility with int
        public void UpdateNodeVisual(int nodeIndex, bool unlocked, bool completed)
        {
            var nodeId = new NodeId(nodeIndex);
            bool isCarHere = false; // You'd get this from MapState in the real implementation
            NodeState state = completed ? NodeState.Completed : 
                             unlocked ? NodeState.Active : NodeState.Inactive;
            UpdateNodeVisual(nodeId, state, isCarHere);
        }

        public NodeData GetNodeData(NodeId nodeId)
        {
            int index = nodeId.Value;
            if (index < 0 || index >= nodeData.Count)
            {
                Debug.LogWarning($"[NodeManagerSimple] Node data not found for index {index}");
                return null;
            }
            return nodeData[index];
        }

        // Overload for backward compatibility
        public NodeData GetNodeData(int nodeIndex)
        {
            return GetNodeData(new NodeId(nodeIndex));
        }

        public float GetSplineT(NodeId nodeId)
        {
            if (nodeViews.Count <= 1) 
                return config ? config.tStart : 0.2f;

            float tStart = config ? config.tStart : 0.2f;
            float tEnd = config ? config.tEnd : 0.8f;
            
            return Mathf.Lerp(tStart, tEnd, (float)nodeId.Value / (nodeViews.Count - 1));
        }

        // Overload for backward compatibility
        public float GetSplineT(int nodeIndex)
        {
            return GetSplineT(new NodeId(nodeIndex));
        }

        #endregion

        #region Node Creation & Positioning

        private void CreateNodeViews()
        {
            // Clean up existing nodes
            ClearNodes();

            // Create new node views
            for (int i = 0; i < nodeData.Count; i++)
            {
                var nodeId = new NodeId(i);
                var nodeView = CreateSingleNode(nodeId);
                nodeViews.Add(nodeView);
            }
        }

        private NodeView CreateSingleNode(NodeId nodeId)
        {
            var nodeObject = Instantiate(nodePrefab, nodesParent);
            var nodeView = nodeObject.GetComponent<NodeView>();
            
            // Initialize using existing API
            nodeView.BindIndex(nodeId.Value + 1); // NodeView expects 1-based index
            nodeView.SetOnClick(() => OnNodeClicked?.Invoke(nodeId));
            
            return nodeView;
        }

        private void PositionNodes()
        {
            if (spline == null || uiCamera == null || canvasRect == null) 
            {
                Debug.LogWarning("[NodeManager] Missing required components for positioning");
                return;
            }

            for (int i = 0; i < nodeViews.Count; i++)
            {
                var nodeId = new NodeId(i);
                PositionSingleNode(nodeViews[i], nodeId);
            }
        }

        private void PositionSingleNode(NodeView nodeView, NodeId nodeId)
        {
            var worldPos = spline.EvaluatePosition(GetSplineT(nodeId));
            var screenPos = uiCamera.WorldToScreenPoint(worldPos);
            
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, uiCamera, out Vector2 localPos))
            {
                var rectTransform = nodeView.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = localPos;
                }
            }
        }

        #endregion

        #region Helpers

        private NodeView GetNodeView(NodeId nodeId)
        {
            int index = nodeId.Value;
            return index >= 0 && index < nodeViews.Count ? nodeViews[index] : null;
        }

        private void ClearNodes()
        {
            // Clean up existing node objects
            foreach (var nodeView in nodeViews)
            {
                if (nodeView != null && nodeView.gameObject != null)
                {
                    DestroyImmediate(nodeView.gameObject);
                }
            }
            nodeViews.Clear();

            // Clean up any remaining children
            foreach (Transform child in nodesParent)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            OnNodeClicked = null;
            ClearNodes();
        }

        #endregion
    }
}