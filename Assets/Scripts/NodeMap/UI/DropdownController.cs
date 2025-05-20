using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NodeMap.UI
{
    /// <summary>
    /// Manages the behavior of a UI dropdown element.
    /// </summary>
    public class DropdownController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button headerButton;
        [SerializeField] private RectTransform contentPanel; // The panel to show/hide
        [SerializeField] private RectTransform arrowIcon; // Optional: an arrow to rotate

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private float arrowRotationDuration = 0.2f; // Duration for arrow rotation animation

        private CanvasGroup contentCanvasGroup;
        private LayoutElement contentLayoutElement; // Added to manage preferredHeight
        private bool isOpen = false;
        private Coroutine animationCoroutine;
        private Coroutine arrowRotationCoroutine; // To manage arrow animation
        private Vector3 initialArrowRotationEuler; // Store initial Euler angles for consistent targeting

        private void Awake()
        {
            if (headerButton == null)
            {
                Debug.LogError("DropdownController: Header Button is not assigned.", this);
                enabled = false;
                return;
            }
            if (contentPanel == null)
            {
                Debug.LogError("DropdownController: Content Panel is not assigned.", this);
                enabled = false;
                return;
            }

            contentCanvasGroup = contentPanel.GetComponent<CanvasGroup>();
            if (contentCanvasGroup == null)
            {
                contentCanvasGroup = contentPanel.gameObject.AddComponent<CanvasGroup>();
            }

            contentLayoutElement = contentPanel.GetComponent<LayoutElement>();
            if (contentLayoutElement == null)
            {
                contentLayoutElement = contentPanel.gameObject.AddComponent<LayoutElement>();
            }

            if (arrowIcon != null)
            {
                initialArrowRotationEuler = arrowIcon.localEulerAngles;
            }

            // Initialize closed
            contentPanel.gameObject.SetActive(false);
            contentCanvasGroup.alpha = 0f;
            // contentPanel.localScale = new Vector3(1f, 0f, 1f); // No longer scaling, using preferredHeight
            if (contentLayoutElement != null)
            {
                contentLayoutElement.preferredHeight = 0f; // Start with zero preferred height
            }
            isOpen = false;
            UpdateArrowIcon(true); // Pass true for instant setup on Awake

            headerButton.onClick.AddListener(ToggleDropdown);
        }

        public void ToggleDropdown()
        {
            isOpen = !isOpen;

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            
            // AnimateRevealContent will handle SetActive(true) when opening
            // and DropdownController will handle SetActive(false) on close completion.

            animationCoroutine = StartCoroutine(UIAnimator.AnimateRevealContent(
                contentPanel, // Pass the RectTransform
                contentCanvasGroup,
                contentLayoutElement, // Pass the LayoutElement
                isOpen,
                animationDuration,
                () => { // onComplete callback
                    if (!isOpen)
                    {
                        contentPanel.gameObject.SetActive(false); // Deactivate after hiding animation
                    }
                    animationCoroutine = null;
                }
            ));
            
            UpdateArrowIcon(); // Default call will animate
        }

        private void UpdateArrowIcon(bool instant = false)
        {
            if (arrowIcon == null) return;

            if (arrowRotationCoroutine != null)
            {
                StopCoroutine(arrowRotationCoroutine);
                arrowRotationCoroutine = null;
            }

            Vector3 targetEuler = initialArrowRotationEuler + new Vector3(0, 0, isOpen ? -90f : 0f);
            Quaternion targetRotation = Quaternion.Euler(targetEuler);

            if (instant || arrowRotationDuration <= 0f)
            {
                arrowIcon.localRotation = targetRotation;
            }
            else
            {
                Quaternion startRotation = arrowIcon.localRotation;
                arrowRotationCoroutine = StartCoroutine(UIAnimator.AnimateOverTime(
                    arrowRotationDuration,
                    (smoothT) => // onUpdate
                    {
                        if (arrowIcon != null) // Double check in case object is destroyed during animation
                        {
                            arrowIcon.localRotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
                        }
                    },
                    () => // onComplete
                    {
                        if (arrowIcon != null)
                        {
                            arrowIcon.localRotation = targetRotation; // Ensure final rotation
                        }
                        arrowRotationCoroutine = null;
                    }
                ));
            }
        }

        private void OnDestroy()
        {
            if (headerButton != null)
            {
                headerButton.onClick.RemoveListener(ToggleDropdown);
            }
        }
    }
}
