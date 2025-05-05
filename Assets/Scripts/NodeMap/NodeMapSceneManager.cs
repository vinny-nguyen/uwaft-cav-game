using UnityEngine;
using UnityEngine.SceneManagement;

namespace NodeMap
{
    public class NodeMapSceneManager : MonoBehaviour
    {
        public static NodeMapSceneManager Instance { get; private set; }

        [SerializeField] private PlayerSplineMovement playerMovement;

        private bool isComingFromAnotherScene = false;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            // Check if we're loading from another scene
            isComingFromAnotherScene = Time.timeSinceLevelLoad < 0.5f;
        }

        private void Start()
        {
            if (isComingFromAnotherScene)
            {
                StartCoroutine(InitializeAfterSceneLoad());
            }
        }

        private System.Collections.IEnumerator InitializeAfterSceneLoad()
        {
            // Wait for all scene objects to initialize
            yield return new WaitForSeconds(0.1f);

            // Initialize NodeMapManager first
            if (NodeMapManager.Instance != null)
            {
                NodeMapManager.Instance.SetCurrentNode(-1);
            }

            // Then initialize player movement
            if (playerMovement != null)
            {
                playerMovement.ResetPosition();
                yield return null;
                playerMovement.StartInitialSequence();
            }
        }

        // Call this method from your button in the previous scene
        public static void PrepareForSceneTransition()
        {
            // Set any needed PlayerPrefs or static variables here
            PlayerPrefs.SetInt("ComingFromPreviousScene", 1);
        }
    }
}