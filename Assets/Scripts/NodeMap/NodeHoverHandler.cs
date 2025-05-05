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
        #region Inspector Fields
        [Header("Hover Settings")]
        [SerializeField] private float hoverScaleMultiplier = 1.2f;
        [SerializeField] private float scaleSpeed = 5f;
        [SerializeField] private Texture2D pointerCursorTexture;
        [SerializeField] private Color inactiveHoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color normalColor = Color.white;

        [Header("Node Info")]
        public int nodeIndex;
        #endregion

        #region Private Fields
        private Vector3 originalScale;
        private bool isHovered = false;
        private SpriteRenderer spriteRenderer;
        private bool isClickable = false;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateScale();
        }
        #endregion

        #region Mouse Interaction
        private void OnMouseEnter()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            isHovered = true;
            UpdateCursor(true);
            UpdateNodeAppearance();
        }

        private void OnMouseExit()
        {
            isHovered = false;
            UpdateCursor(false);
            ResetNodeAppearance();
        }

        private void OnMouseDown()
        {
            HandleNodeClick();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets whether this node can be clicked
        /// </summary>
        public void SetClickable(bool value)
        {
            isClickable = value;
        }

        /// <summary>
        /// Starts the shake animation to indicate invalid interaction
        /// </summary>
        public void StartShake()
        {
            StartCoroutine(ShakeNode());
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            originalScale = transform.localScale;
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer != null)
                spriteRenderer.color = normalColor;
        }

        private void UpdateScale()
        {
            bool shouldScale = NopeMapManager.Instance.CurrentNodeIndex == nodeIndex || isClickable;
            Vector3 targetScale = originalScale;

            if (isHovered && shouldScale)
                targetScale *= hoverScaleMultiplier;

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }

        private void UpdateCursor(bool showCustomCursor)
        {
            if (showCustomCursor && pointerCursorTexture != null)
                Cursor.SetCursor(pointerCursorTexture, Vector2.zero, CursorMode.Auto);
            else
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void UpdateNodeAppearance()
        {
            if (spriteRenderer == null) return;

            bool canInteract = IsNodeInteractable();
            if (!canInteract)
                spriteRenderer.color = inactiveHoverColor;
        }

        private void ResetNodeAppearance()
        {
            if (spriteRenderer != null)
                spriteRenderer.color = normalColor;
        }

        private bool IsNodeInteractable()
        {
            PlayerSplineMovement mover = FindFirstObjectByType<PlayerSplineMovement>();
            bool isCompleted = mover != null && mover.IsNodeCompleted(nodeIndex - 1);
            return NopeMapManager.Instance.CurrentNodeIndex == nodeIndex || isCompleted || isClickable;
        }

        private void HandleNodeClick()
        {
            if (NopeMapManager.Instance == null)
            {
                Debug.LogWarning("NopeMapManager instance missing!");
                return;
            }

            bool canInteract = IsNodeInteractable();

            if (!canInteract)
            {
                StartCoroutine(ShakeNode());
                return;
            }

            if (PopupManager.Instance != null)
                PopupManager.Instance.OpenPopupForNode(nodeIndex);
        }
        #endregion

        #region Animations
        private IEnumerator ShakeNode()
        {
            float shakeDuration = 0.15f;
            float shakeMagnitude = 0.15f;
            float time = 0f;
            Vector3 originalPosition = transform.localPosition;

            while (time < shakeDuration)
            {
                time += Time.deltaTime;
                float offsetX = Mathf.Sin(time * 50f) * shakeMagnitude * (1f - time / shakeDuration);
                transform.localPosition = originalPosition + new Vector3(offsetX, 0f, 0f);
                yield return null;
            }

            transform.localPosition = originalPosition;
        }
        #endregion
    }
}
