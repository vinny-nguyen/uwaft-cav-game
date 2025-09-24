using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace NodeMap.UI
{
    /// <summary>
    /// Reusable component that adds scale effects to UI buttons on hover and click
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Hover Effect")]
        [SerializeField] private bool enableHoverEffect = true;
        [SerializeField] private float hoverScaleMultiplier = 1.05f;
        [SerializeField] private float hoverTransitionTime = 0.1f;

        [Header("Click Effect")]
        [SerializeField] private bool enableClickEffect = true;
        [SerializeField] private float clickScaleMultiplier = 0.9f;
        [SerializeField] private float clickTransitionTime = 0.05f;

        private Button button;
        private Vector3 originalScale;
        private Coroutine activeCoroutine;
        private bool isPointerDown = false;
        private bool isHovered = false;

        private void Awake()
        {
            button = GetComponent<Button>();
            originalScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enableHoverEffect || !button.interactable) return;

            isHovered = true;

            // Don't scale if button is being clicked
            if (isPointerDown && enableClickEffect) return;

            AnimateTo(originalScale * hoverScaleMultiplier, hoverTransitionTime);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enableHoverEffect) return;

            isHovered = false;

            // Only reset if not being clicked
            if (!isPointerDown)
            {
                AnimateTo(originalScale, hoverTransitionTime);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!enableClickEffect || !button.interactable) return;

            isPointerDown = true;

            AnimateTo(originalScale * clickScaleMultiplier, clickTransitionTime);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!enableClickEffect) return;

            isPointerDown = false;

            // Return to hover state if still hovered, otherwise to original
            if (isHovered && enableHoverEffect)
            {
                AnimateTo(originalScale * hoverScaleMultiplier, clickTransitionTime);
            }
            else
            {
                AnimateTo(originalScale, clickTransitionTime);
            }
        }

        private void AnimateTo(Vector3 targetScale, float duration)
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }
            activeCoroutine = StartCoroutine(NodeMap.TweenHelper.ScaleTo(transform, targetScale, duration));
        }

        private void OnDisable()
        {
            // Cancel animations and reset state
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }

            transform.localScale = originalScale;
            isPointerDown = false;
            isHovered = false;
        }
    }
}