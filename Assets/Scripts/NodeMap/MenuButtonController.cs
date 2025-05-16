using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

namespace NodeMap
{
    /// <summary>
    /// Menu button controller with scale-on-hover effect and scene transition
    /// </summary>
    public class MenuButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Navigation")]
        [SerializeField] private string targetSceneName = "MainMenu";

        [Header("Hover Effect")]
        [SerializeField] private bool enableHoverScale = true;
        [SerializeField] private float hoverScaleMultiplier = 1.1f;
        [SerializeField] private float hoverTransitionTime = 0.1f;

        private Button button;
        private Vector3 originalScale;
        private Coroutine scaleCoroutine;

        private void Awake()
        {
            // Cache components and original scale
            button = GetComponent<Button>();
            originalScale = transform.localScale;

            // Set up button click handler
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enableHoverScale || !button.interactable) return;

            // Cancel existing animation if any
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);

            // Start new hover animation
            scaleCoroutine = StartCoroutine(AnimateScale(originalScale * hoverScaleMultiplier));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enableHoverScale) return;

            // Cancel existing animation if any
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);

            // Return to original scale
            scaleCoroutine = StartCoroutine(AnimateScale(originalScale));
        }

        private IEnumerator AnimateScale(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float time = 0;

            while (time < hoverTransitionTime)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / hoverTransitionTime);

                // Smooth step easing for more natural feel
                t = t * t * (3f - 2f * t);

                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
            scaleCoroutine = null;
        }

        private void OnButtonClick()
        {
            SaveAndLoadMainMenu();
        }

        private void SaveAndLoadMainMenu()
        {
            if (string.IsNullOrEmpty(targetSceneName))
                return;

            // Save progress before scene transition
            if (NopeMapManager.Instance != null)
            {
                NopeMapManager.Instance.SaveNodeProgress();
            }

            // Load main menu scene
            SceneManager.LoadScene(targetSceneName);
        }

        private void OnDisable()
        {
            // Cancel any ongoing animations
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }

            // Reset to original scale
            transform.localScale = originalScale;
        }
    }
}