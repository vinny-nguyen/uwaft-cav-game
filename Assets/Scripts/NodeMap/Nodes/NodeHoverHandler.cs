using UnityEngine;
using UnityEngine.EventSystems;
using NodeMap.Tutorial; 
using TMPro;

namespace NodeMap.Nodes
{
    /// <summary>
    /// Handles node hover and click interactions
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class NodeHoverHandler : MonoBehaviour
    {
        [Header("Hover Settings")]
        [SerializeField] private float hoverScaleMultiplier = 1.2f;
        [SerializeField] private float scaleSpeed = 5f;
        [SerializeField] private Texture2D pointerCursorTexture;
        [SerializeField] private Color inactiveHoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color normalColor = Color.white;

        [Header("Node Info")]
        public int nodeIndex;

        [Header("Feedback UI")]
        [SerializeField] private TextMeshProUGUI statusMessageText;

        [Header("Animation Settings")]
        [SerializeField] private float shakeIntensity = 0.15f;
        [SerializeField] private float shakeDuration = 0.15f;
        [SerializeField] private float shakeFrequency = 50f;
        [SerializeField] private float messageDisplayDuration = 1.5f;
        [SerializeField] private float messageFadeDuration = 0.25f;

        // Constants
        private const string NODE_UNAVAILABLE_MESSAGE = "This node is not available.";

        // Private fields
        private Vector3 originalScale;
        private bool isHovered = false;
        private SpriteRenderer spriteRenderer;
        private bool isClickable = false;
        private NodeVisualController visualController;
        private Coroutine activeMessageCoroutine;
        
        // Cached references for performance
        private NodeMapTutorialManager cachedTutorialManager;
        private bool hasCachedTutorialManager = false;

        private void Start()
        {
            // Cache initial values
            originalScale = transform.localScale;
            spriteRenderer = GetComponent<SpriteRenderer>();
            visualController = GetComponent<NodeVisualController>();
            spriteRenderer.color = normalColor;
            
            // Cache tutorial manager reference once
            CacheTutorialManager();
        }

        /// <summary>
        /// Cache the TutorialManager reference to avoid repeated FindFirstObjectByType calls
        /// </summary>
        private void CacheTutorialManager()
        {
            if (!hasCachedTutorialManager)
            {
                cachedTutorialManager = FindFirstObjectByType<NodeMapTutorialManager>();
                hasCachedTutorialManager = true;
            }
        }

        /// <summary>
        /// Check if tutorial is currently active
        /// </summary>
        private bool IsTutorialActive()
        {
            return cachedTutorialManager != null && cachedTutorialManager.IsTutorialActive();
        }

        private void Update()
        {
            // Update scale based on hover state
            Vector3 targetScale = isHovered && IsNodeInteractable() ?
                originalScale * hoverScaleMultiplier : originalScale;

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime * scaleSpeed);
        }

        private void OnMouseEnter()
        {
            // Ignore if UI is in the way
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            isHovered = true;

            // Update cursor
            if (pointerCursorTexture != null)
                Cursor.SetCursor(pointerCursorTexture, Vector2.zero, CursorMode.Auto);

            // Update appearance
            if (!IsNodeInteractable() && spriteRenderer != null)
                spriteRenderer.color = inactiveHoverColor;
        }

        private void OnMouseExit()
        {
            isHovered = false;

            // Reset cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            // Reset appearance
            if (spriteRenderer != null)
                spriteRenderer.color = normalColor;
        }

        private void OnMouseDown()
        {
            // Skip if tutorial is active (using cached reference)
            if (IsTutorialActive())
                return;

            // Skip if no manager
            if (NodeMapManager.Instance == null)
                return;

            // Check if node can be interacted with
            if (!IsNodeInteractable())
            {
                PlayNodeUnavailableAnimation();
                return;
            }

            // Open popup for this node
            if (PopupManager.Instance != null)
                PopupManager.Instance.OpenPopupForNode(nodeIndex);
        }

        /// <summary>
        /// Play animation and show message when node is not available
        /// </summary>
        private void PlayNodeUnavailableAnimation()
        {
            // Play shake animation
            StartCoroutine(UI.UIAnimator.ShakeElement(transform, shakeIntensity, shakeDuration, shakeFrequency));
            
            // Show message if text component is available
            if (statusMessageText != null)
            {
                // Stop previous message coroutine if still running
                if (activeMessageCoroutine != null)
                {
                    StopCoroutine(activeMessageCoroutine);
                }
                
                // Start new message display
                activeMessageCoroutine = StartCoroutine(
                    UI.UIAnimator.ShowTemporaryMessage(
                        statusMessageText, 
                        NODE_UNAVAILABLE_MESSAGE, 
                        messageDisplayDuration, 
                        messageFadeDuration));
            }
        }

        /// <summary>
        /// Sets whether this node can be clicked regardless of other conditions
        /// </summary>
        public void SetClickable(bool value)
        {
            isClickable = value;
        }

        /// <summary>
        /// Checks if this node is in a state that can be interacted with
        /// </summary>
        private bool IsNodeInteractable()
        {
            if (isClickable) return true;

            // Check if the node is active or complete
            if (visualController != null)
            {
                NodeState state = visualController.GetCurrentState();
                return state == NodeState.Active || state == NodeState.Complete;
            }

            // Fallback to old method if no visual controller
            if (NodeMapManager.Instance == null) return false;
            
            bool isCompleted = NodeMapManager.Instance.IsNodeCompleted(nodeIndex);
            bool isCurrent = NodeMapManager.Instance.CurrentNodeIndex == nodeIndex;
            return isCurrent || isCompleted;
        }

        /// <summary>
        /// Public method to trigger the shake animation from external scripts
        /// </summary>
        public void StartShake()
        {
            StartCoroutine(UI.UIAnimator.ShakeElement(transform, shakeIntensity, shakeDuration, shakeFrequency));
        }
        
        /// <summary>
        /// Clean up when object is destroyed
        /// </summary>
        private void OnDestroy()
        {
            // Stop any running coroutines to prevent memory leaks
            if (activeMessageCoroutine != null)
            {
                StopCoroutine(activeMessageCoroutine);
                activeMessageCoroutine = null;
            }
        }
    }
}