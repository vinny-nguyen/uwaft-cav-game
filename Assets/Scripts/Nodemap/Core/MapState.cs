using System;
using UnityEngine;

namespace Nodemap.Core
{
    /// <summary>
    /// Centralized state management for the map system.
    /// Single source of truth for all node states, car position, and progression.
    /// Implements proper event-driven updates with clear data flow.
    /// </summary>
    public class MapState : IDisposable
    {
        // Core state data
        private NodeId _currentCarNodeId;
        private NodeId _activeNodeId;
        private readonly bool[] _unlockedNodes;
        private readonly bool[] _completedNodes;
        private readonly int _nodeCount;
        
        // State change events - unidirectional flow
        public event Action<NodeId> OnCarNodeChanged;
        public event Action<NodeId> OnActiveNodeChanged;
        public event Action<NodeId, bool> OnNodeUnlockedChanged;
        public event Action<NodeId, bool> OnNodeCompletedChanged;
        public event Action OnStateReset;

        public MapState(int nodeCount)
        {
            _nodeCount = nodeCount;
            _unlockedNodes = new bool[nodeCount];
            _completedNodes = new bool[nodeCount];
            
            // Initialize with first node unlocked and active
            _currentCarNodeId = new NodeId(0);
            _activeNodeId = new NodeId(0);
            _unlockedNodes[0] = true;
        }

        #region Public State Accessors
        
        public NodeId CurrentCarNodeId => _currentCarNodeId;
        public NodeId ActiveNodeId => _activeNodeId;
        public int NodeCount => _nodeCount;

        public bool IsNodeUnlocked(NodeId nodeId)
        {
            if (!IsValidNodeId(nodeId)) return false;
            return _unlockedNodes[nodeId.Value];
        }

        public bool IsNodeCompleted(NodeId nodeId)
        {
            if (!IsValidNodeId(nodeId)) return false;
            return _completedNodes[nodeId.Value];
        }

        public NodeState GetNodeState(NodeId nodeId)
        {
            if (!IsValidNodeId(nodeId)) return NodeState.Inactive;
            
            if (IsNodeCompleted(nodeId)) return NodeState.Completed;
            if (IsNodeUnlocked(nodeId)) return NodeState.Active;
            return NodeState.Inactive;
        }

        #endregion

        #region State Mutations (Command-like methods)

        public bool TryMoveCarTo(NodeId nodeId)
        {
            if (!IsValidNodeId(nodeId) || !IsNodeUnlocked(nodeId)) 
                return false;
            
            if (_currentCarNodeId.Equals(nodeId)) 
                return true; // Already there
            
            _currentCarNodeId = nodeId;
            OnCarNodeChanged?.Invoke(nodeId);
            return true;
        }

        public bool TryCompleteNode(NodeId nodeId)
        {
            if (!IsValidNodeId(nodeId) || !IsNodeUnlocked(nodeId)) 
                return false;
            
            if (IsNodeCompleted(nodeId)) 
                return true; // Already completed
            
            _completedNodes[nodeId.Value] = true;
            OnNodeCompletedChanged?.Invoke(nodeId, true);
            
            // Auto-unlock next node if all previous are completed
            TryUnlockNextNode(nodeId);
            
            return true;
        }

        public void ResetProgression()
        {
            // Reset all state
            for (int i = 0; i < _nodeCount; i++)
            {
                _unlockedNodes[i] = (i == 0);
                _completedNodes[i] = false;
            }
            
            _currentCarNodeId = new NodeId(0);
            _activeNodeId = new NodeId(0);
            
            OnStateReset?.Invoke();
        }

        #endregion

        #region Private Helpers

        private bool IsValidNodeId(NodeId nodeId)
        {
            return nodeId.IsValid(_nodeCount);
        }

        private void TryUnlockNextNode(NodeId completedNodeId)
        {
            var nextNodeId = new NodeId(completedNodeId.Value + 1);
            
            if (!IsValidNodeId(nextNodeId)) 
                return; // No next node
            
            // Check if all previous nodes are completed
            for (int i = 0; i <= completedNodeId.Value; i++)
            {
                if (!_completedNodes[i]) 
                    return; // Still have incomplete previous nodes
            }
            
            // Unlock and make active
            if (!_unlockedNodes[nextNodeId.Value])
            {
                _unlockedNodes[nextNodeId.Value] = true;
                OnNodeUnlockedChanged?.Invoke(nextNodeId, true);
                
                _activeNodeId = nextNodeId;
                OnActiveNodeChanged?.Invoke(nextNodeId);
            }
        }

        #endregion

        #region Persistence

        public void SaveToPlayerPrefs()
        {
            for (int i = 0; i < _nodeCount; i++)
            {
                PlayerPrefs.SetInt($"NodeUnlocked_{i}", _unlockedNodes[i] ? 1 : 0);
                PlayerPrefs.SetInt($"NodeCompleted_{i}", _completedNodes[i] ? 1 : 0);
            }
            PlayerPrefs.SetInt("CurrentCarNode", _currentCarNodeId.Value);
            PlayerPrefs.SetInt("ActiveNode", _activeNodeId.Value);
            PlayerPrefs.Save();
        }

        public void LoadFromPlayerPrefs()
        {
            for (int i = 0; i < _nodeCount; i++)
            {
                _unlockedNodes[i] = PlayerPrefs.GetInt($"NodeUnlocked_{i}", i == 0 ? 1 : 0) == 1;
                _completedNodes[i] = PlayerPrefs.GetInt($"NodeCompleted_{i}", 0) == 1;
            }
            
            int carNode = PlayerPrefs.GetInt("CurrentCarNode", 0);
            int activeNode = PlayerPrefs.GetInt("ActiveNode", 0);
            
            _currentCarNodeId = new NodeId(Mathf.Clamp(carNode, 0, _nodeCount - 1));
            _activeNodeId = new NodeId(Mathf.Clamp(activeNode, 0, _nodeCount - 1));
        }

        #endregion

        public void Dispose()
        {
            // Clear all event subscriptions to prevent memory leaks
            OnCarNodeChanged = null;
            OnActiveNodeChanged = null;
            OnNodeUnlockedChanged = null;
            OnNodeCompletedChanged = null;
            OnStateReset = null;
        }
    }
}