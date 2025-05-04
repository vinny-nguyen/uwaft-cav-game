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
        #region Animation Parameters
        [Header("Animation Settings")]
        [SerializeField] private float popUpDuration = 0.1f;
        [SerializeField] private float popDownDuration = 0.15f;
        [SerializeField] private float popScale = 1.1f;
        #endregion

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Transitions the node to normal state with animation
        /// </summary>
        public void TransitionToNormal(Sprite normalSprite)
        {
            if (spriteRenderer != null && normalSprite != null)
            {
                StartCoroutine(AnimateTransition(normalSprite));
            }
        }

        /// <summary>
        /// Transitions the node to active state with animation
        /// </summary>
        public void TransitionToActive(Sprite activeSprite)
        {
            if (spriteRenderer != null && activeSprite != null)
            {
                StartCoroutine(AnimateTransition(activeSprite));
            }
        }

        /// <summary>
        /// Transitions the node to completed state with animation
        /// </summary>
        public void TransitionToComplete(Sprite completeSprite)
        {
            if (spriteRenderer != null && completeSprite != null)
            {
                StartCoroutine(AnimateTransition(completeSprite, 0.15f, 0.2f));
            }
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