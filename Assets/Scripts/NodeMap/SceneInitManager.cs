using UnityEngine;
using UnityEngine.SceneManagement;

namespace NodeMap
{
    /// <summary>
    /// Handles scene initialization after scene transitions
    /// </summary>
    public class SceneInitManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSplineMovement playerMovement;
        [SerializeField] private int startNodeIndex = 0;
        [SerializeField] private float startDelay = 0.5f;

        private void Start()
        {
            // Subscribe to scene loaded events
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Also initialize immediately for the first load
            Invoke("InitializeScene", startDelay);
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Called when a scene is loaded
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // If this is our scene, initialize it
            if (scene.name == gameObject.scene.name)
            {
                Invoke("InitializeScene", startDelay);
            }
        }

        /// <summary>
        /// Initializes the scene properly
        /// </summary>
        private void InitializeScene()
        {
            // Find player movement if not assigned
            if (playerMovement == null)
            {
                playerMovement = FindFirstObjectByType<PlayerSplineMovement>();

                if (playerMovement == null)
                {
                    Debug.LogError("Cannot find PlayerSplineMovement component in the scene!");
                    return;
                }
            }

            // Move to the start node
            playerMovement.MoveToNode(startNodeIndex);

            Debug.Log($"Scene initialized. Moving player to node {startNodeIndex}");
        }
    }
}