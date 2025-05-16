using UnityEngine;

namespace NodeMap
{
    /// <summary>
    /// Core manager for node-based game progression
    /// </summary>
    public class NopeMapManager : MonoBehaviour
    {
        #region Events & Singleton
        
        public delegate void NodeCompletedHandler(int nodeIndex);
        public event NodeCompletedHandler OnNodeCompleted;
        public static NopeMapManager Instance { get; private set; }
        
        #endregion

        #region Properties & Fields
        
        [Header("Node Progression")]
        public int CurrentNodeIndex { get; private set; } = -1;
        public int HighestCompletedNodeIndex { get; private set; } = -1;

        [Header("Debug Tools")]
        [SerializeField] private bool enableResetOption = true;
        [SerializeField] private KeyCode resetKey = KeyCode.Delete;
        [SerializeField] private KeyCode resetModifierKey = KeyCode.LeftControl;
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadNodeProgress();
        }
        
        #endregion

        #region Node Progression Methods

        /// <summary>
        /// Sets the currently active node
        /// </summary>
        public void SetCurrentNode(int nodeIndex)
        {
            CurrentNodeIndex = nodeIndex;
        }

        /// <summary>
        /// Deactivates all nodes (-1 represents no active node)
        /// </summary>
        public void SetNodesToInactive()
        {
            CurrentNodeIndex = -1;
        }

        /// <summary>
        /// Marks a node as completed and triggers events if it's a new high score
        /// </summary>
        public void CompleteNode(int nodeIndex)
        {
            if (nodeIndex > HighestCompletedNodeIndex)
            {
                HighestCompletedNodeIndex = nodeIndex;
                OnNodeCompleted?.Invoke(nodeIndex);
            }
        }

        /// <summary>
        /// Checks if a node is completed
        /// </summary>
        public bool IsNodeCompleted(int nodeIndex)
        {
            return (nodeIndex <= HighestCompletedNodeIndex) && (nodeIndex != -1);
        }
        
        #endregion

        #region Save & Load

        /// <summary>
        /// Saves current progress to PlayerPrefs
        /// </summary>
        public void SaveNodeProgress()
        {
            PlayerPrefs.SetInt("CurrentNodeIndex", CurrentNodeIndex);
            PlayerPrefs.SetInt("HighestCompletedNodeIndex", HighestCompletedNodeIndex);
            PlayerPrefs.Save();
            
            Debug.Log($"Saved progress: Current={CurrentNodeIndex}, Highest={HighestCompletedNodeIndex}");
        }

        /// <summary>
        /// Loads saved progress from PlayerPrefs
        /// </summary>
        public void LoadNodeProgress()
        {
            if (PlayerPrefs.HasKey("CurrentNodeIndex"))
                CurrentNodeIndex = PlayerPrefs.GetInt("CurrentNodeIndex");
            
            if (PlayerPrefs.HasKey("HighestCompletedNodeIndex"))
                HighestCompletedNodeIndex = PlayerPrefs.GetInt("HighestCompletedNodeIndex");
            
            Debug.Log($"Loaded progress: Current={CurrentNodeIndex}, Highest={HighestCompletedNodeIndex}");
        }
        
        #endregion
    }
}