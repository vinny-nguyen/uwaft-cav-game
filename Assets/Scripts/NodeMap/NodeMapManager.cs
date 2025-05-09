using System.Collections.Generic;
using UnityEngine;

namespace NodeMap
{
    /// <summary>
    /// Core manager for node-based game progression
    /// </summary>
    /// 
    public class NopeMapManager : MonoBehaviour
    {
        public delegate void NodeCompletedHandler(int nodeIndex);

        // Add event
        public event NodeCompletedHandler OnNodeCompleted;

        public static NopeMapManager Instance { get; private set; }

        [Header("Node Progression")]
        // Changed from -1 to 0, representing no active node
        public int CurrentNodeIndex { get; private set; } = -1;

        [Header("Debug Tools")]
        [SerializeField] private bool enableResetOption = true;
        [SerializeField] private KeyCode resetKey = KeyCode.Delete;
        [SerializeField] private KeyCode resetModifierKey = KeyCode.LeftControl;


        // Track completion status
        private HashSet<int> completedNodes = new HashSet<int>();

        #region Unity Lifecycle

        private void Awake()
        {
            SetupSingleton();
        }

        private void Start()
        {
            // Load saved progress when starting the scene
            LoadNodeProgress();
        }

        private void Update()
        {
            // Check for reset key combo (Ctrl+Delete)
            if (enableResetOption &&
                Input.GetKey(resetModifierKey) &&
                Input.GetKeyDown(resetKey))
            {
                ResetAllPlayerPrefs();
            }
        }
        #endregion

        #region Singleton Setup

        private void SetupSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Node Progression Management

        /// <summary>
        /// Advances to the next node in sequence
        /// </summary>
        public void AdvanceToNextNode()
        {
            CurrentNodeIndex++;
            // Debug.Log($"Advanced to Node {CurrentNodeIndex}");
        }

        /// <summary>
        /// Sets the currently active node
        /// </summary>
        public void SetCurrentNode(int nodeIndex)
        {
            CurrentNodeIndex = nodeIndex;
            // Debug.Log($"Set current node to {CurrentNodeIndex}");
        }

        /// <summary>
        /// Marks a node as completed
        /// </summary>
        public void CompleteNode(int nodeIndex)
        {
            if (!completedNodes.Contains(nodeIndex))
            {
                completedNodes.Add(nodeIndex);
                // Debug.Log($"Node {nodeIndex} marked as completed");
            }

            OnNodeCompleted?.Invoke(nodeIndex);
        }

        /// <summary>
        /// Checks if a node is completed
        /// </summary>
        public bool IsNodeCompleted(int nodeIndex)
        {
            return completedNodes.Contains(nodeIndex);
        }

        #endregion

        public void LoadNodeProgress()
        {
            // Load current node index
            if (PlayerPrefs.HasKey("CurrentNodeIndex"))
            {
                CurrentNodeIndex = PlayerPrefs.GetInt("CurrentNodeIndex");
            }

            // Load completed nodes
            if (PlayerPrefs.HasKey("CompletedNodes"))
            {
                string completedNodesStr = PlayerPrefs.GetString("CompletedNodes");
                DeserializeCompletedNodes(completedNodesStr);
            }

            Debug.Log($"Loaded progress: Current Node = {CurrentNodeIndex}, " +
                      $"Completed Nodes Count = {completedNodes.Count}");
        }

        /// <summary>
        /// Parses a string of node indices back into the completedNodes HashSet
        /// </summary>
        private void DeserializeCompletedNodes(string completedNodesStr)
        {
            completedNodes.Clear();

            if (string.IsNullOrEmpty(completedNodesStr))
                return;

            string[] nodeIndices = completedNodesStr.Split(',');

            foreach (string nodeIndexStr in nodeIndices)
            {
                if (int.TryParse(nodeIndexStr, out int nodeIndex))
                {
                    completedNodes.Add(nodeIndex);
                }
            }
        }

        /// <summary>
        /// Clears saved progress (useful for restarting the game)
        /// </summary>
        public void ResetAllPlayerPrefs()
        {
            // Clear all game progress
            PlayerPrefs.DeleteKey("CurrentNodeIndex");
            PlayerPrefs.DeleteKey("CompletedNodes");
            PlayerPrefs.DeleteKey("CarUpgradeIndex");
            PlayerPrefs.DeleteKey("CompletedTutorial");

            // Add any other PlayerPrefs keys your game uses

            // Reset in-memory state
            CurrentNodeIndex = 0; // Or -1, depending on your initial state
            completedNodes.Clear();

            // Apply changes immediately
            PlayerPrefs.Save();

            Debug.Log("<color=yellow>⚠ DEBUG: All PlayerPrefs data has been reset!</color>");
        }

        // Existing ClearSavedProgress method can call this new method:
        public void ClearSavedProgress()
        {
            ResetAllPlayerPrefs();
            Debug.Log("Node progress cleared");
        }
    }
}