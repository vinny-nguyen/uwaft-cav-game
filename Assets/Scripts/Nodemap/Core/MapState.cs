using System;
using UnityEngine;
using Nodemap.UI;

namespace Nodemap.Core
{
    /// <summary>
    /// Centralized state management for the map system.
    /// Single source of truth for all node states, car position, and progression.
    /// </summary>
    public class MapState
    {
        // Core state data
        private NodeId _currentCarNodeId;
        private NodeId _activeNodeId;
        private readonly bool[] _unlockedNodes;
        private readonly bool[] _completedNodes;
        private readonly int _nodeCount;
        
        // Simple state change notification
        public event Action OnStateChanged;

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
            OnStateChanged?.Invoke();
            return true;
        }

        public bool TryCompleteNode(NodeId nodeId)
        {
            if (!IsValidNodeId(nodeId) || !IsNodeUnlocked(nodeId)) 
                return false;
            
            if (IsNodeCompleted(nodeId)) 
                return true; // Already completed
            
            _completedNodes[nodeId.Value] = true;
            OnStateChanged?.Invoke();
            
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
            
            OnStateChanged?.Invoke();
        }

        #endregion

        #region Private Helpers

        private bool IsValidNodeId(NodeId nodeId)
        {
            return nodeId.Value >= 0 && nodeId.Value < _nodeCount;
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
                _activeNodeId = nextNodeId;
                OnStateChanged?.Invoke();
            }
        }

        #endregion

        #region Persistence

        public void SaveToPlayerPrefs()
        {
            for (int i = 0; i < _nodeCount; i++)
            {
                PlayerPrefs.SetInt(PlayerPrefsKeys.NodeUnlocked(i), _unlockedNodes[i] ? 1 : 0);
                PlayerPrefs.SetInt(PlayerPrefsKeys.NodeCompleted(i), _completedNodes[i] ? 1 : 0);
            }
            PlayerPrefs.SetInt(PlayerPrefsKeys.CurrentCarNode, _currentCarNodeId.Value);
            // Note: ActiveNode is calculated from unlocked nodes, not saved
            PlayerPrefs.Save();
        }

        public void LoadFromPlayerPrefs()
        {
            for (int i = 0; i < _nodeCount; i++)
            {
                _unlockedNodes[i] = PlayerPrefs.GetInt(PlayerPrefsKeys.NodeUnlocked(i), i == 0 ? 1 : 0) == 1;
                _completedNodes[i] = PlayerPrefs.GetInt(PlayerPrefsKeys.NodeCompleted(i), 0) == 1;
            }
            
            int carNode = PlayerPrefs.GetInt(PlayerPrefsKeys.CurrentCarNode, 0);
            _currentCarNodeId = new NodeId(Mathf.Clamp(carNode, 0, _nodeCount - 1));
            
            // Find the last unlocked node to set as active
            int lastUnlockedIndex = 0;
            for (int i = _nodeCount - 1; i >= 0; i--)
            {
                if (_unlockedNodes[i])
                {
                    lastUnlockedIndex = i;
                    break;
                }
            }
            _activeNodeId = new NodeId(lastUnlockedIndex);
        }

        #endregion
    }
}