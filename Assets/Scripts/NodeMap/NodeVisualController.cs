using UnityEngine;
using System.Collections;

namespace NodeMap
{
    /// <summary>
    /// Handles the visual state transitions for nodes
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class NodeVisualController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float popUpDuration = 0.1f;
        [SerializeField] private float popDownDuration = 0.15f;
        [SerializeField] private float popScale = 1.1f;

        private SpriteRenderer spriteRenderer;
        private NodeState currentState = NodeState.Normal;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Transitions the node to a new state with animation
        /// </summary>
        public void TransitionToState(NodeState newState, Sprite stateSprite)
        {
            if (spriteRenderer != null && stateSprite != null)
            {
                currentState = newState;
                
                // Use longer animation for completion state
                float upTime = newState == NodeState.Complete ? 0.15f : popUpDuration;
                float downTime = newState == NodeState.Complete ? 0.2f : popDownDuration;
                
                StartCoroutine(AnimateTransition(stateSprite, upTime, downTime));
            }
        }

        /// <summary>
        /// Get the current state of this node
        /// </summary>
        public NodeState GetCurrentState()
        {
            return currentState;
        }

        private IEnumerator AnimateTransition(Sprite targetSprite, float upDuration = -1, float downDuration = -1)
        {
            // Use default durations if not specified
            if (upDuration < 0) upDuration = popUpDuration;
            if (downDuration < 0) downDuration = popDownDuration;
            
            float t = 0f;
            Transform markerTransform = transform;
            Vector3 originalScale = markerTransform.localScale;
            Vector3 poppedScale = originalScale * popScale;

            // Pop up animation
            while (t < upDuration)
            {
                t += Time.deltaTime;
                float scaleT = Mathf.Lerp(0f, 1f, t / upDuration);
                markerTransform.localScale = Vector3.Lerp(originalScale, poppedScale, scaleT);
                yield return null;
            }

            // Change sprite
            spriteRenderer.sprite = targetSprite;

            // Pop down animation
            t = 0f;
            while (t < downDuration)
            {
                t += Time.deltaTime;
                float scaleT = Mathf.Lerp(0f, 1f, t / downDuration);
                markerTransform.localScale = Vector3.Lerp(poppedScale, originalScale, scaleT);
                yield return null;
            }

            markerTransform.localScale = originalScale;
        }
    }
}