using UnityEngine;
using System.Collections;
using NodeMap.UI;

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
                
                StartCoroutine(UIAnimator.AnimateSpritePop(transform, spriteRenderer, stateSprite, popScale, upTime, downTime));
            }
        }

        /// <summary>
        /// Get the current state of this node
        /// </summary>
        public NodeState GetCurrentState()
        {
            return currentState;
        }
    }
}