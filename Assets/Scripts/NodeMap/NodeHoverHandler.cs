using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace NodeMap
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

        // Private fields
        private Vector3 originalScale;
        private bool isHovered = false;
        private SpriteRenderer spriteRenderer;
        private bool isClickable = false;

        private void Start()
        {
            // Cache initial values
            originalScale = transform.localScale;
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = normalColor;
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
            // Skip if tutorial is active
            TutorialManager tutorialManager = FindFirstObjectByType<TutorialManager>();
            if (tutorialManager != null && tutorialManager.IsTutorialActive())
                return;

            // Skip if no manager
            if (NopeMapManager.Instance == null)
                return;

            // Check if node can be interacted with
            if (!IsNodeInteractable())
            {
                StartCoroutine(ShakeNode());
                return;
            }

            // Open popup for this node
            if (PopupManager.Instance != null)
                PopupManager.Instance.OpenPopupForNode(nodeIndex);
        }

        /// <summary>
        /// Sets whether this node can be clicked regardless of other conditions
        /// </summary>
        public void SetClickable(bool value)
        {
            isClickable = value;
        }

        /// <summary>
        /// Checks if this node can be interacted with
        /// </summary>
        private bool IsNodeInteractable()
        {
            if (isClickable) return true;
            
            PlayerSplineMovement mover = FindFirstObjectByType<PlayerSplineMovement>();
            bool isCompleted = mover != null && mover.IsNodeCompleted(nodeIndex);
            bool isCurrent = NopeMapManager.Instance != null && 
                             NopeMapManager.Instance.CurrentNodeIndex == nodeIndex;
                             
            return isCurrent || isCompleted;
        }
        
        /// <summary>
        /// Public method to trigger the shake animation from external scripts
        /// </summary>
        public void StartShake()
        {
            StartCoroutine(ShakeNode());
        }

        /// <summary>
        /// Shake animation to show invalid interaction
        /// </summary>
        private IEnumerator ShakeNode()
        {
            float shakeDuration = 0.15f;
            float shakeMagnitude = 0.15f;
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float diminish = 1f - (elapsed / shakeDuration);
                float offsetX = Mathf.Sin(elapsed * 50f) * shakeMagnitude * diminish;
                
                transform.localPosition = originalPosition + new Vector3(offsetX, 0, 0);
                yield return null;
            }

            transform.localPosition = originalPosition;
        }
    }
}