using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NodeMap.UI
{
    /// <summary>
    /// Handles common UI animations for the popup system
    /// </summary>
    public class UIAnimator : MonoBehaviour
    {
        /// <summary>
        /// Animates a popup panel opening
        /// </summary>
        public static IEnumerator AnimatePopupOpen(CanvasGroup canvasGroup, Image backgroundOverlay = null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            float duration = 0.4f;
            float time = 0f;

            Transform popupTransform = canvasGroup.transform;
            Vector3 originalScale = Vector3.one;
            Vector3 startScale = Vector3.one * 0.8f;

            popupTransform.localScale = startScale;
            canvasGroup.alpha = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                popupTransform.localScale = Vector3.Lerp(startScale, originalScale, t);

                if (backgroundOverlay != null)
                    backgroundOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.6f, t));

                yield return null;
            }

            canvasGroup.alpha = 1f;
            popupTransform.localScale = originalScale;
        }

        /// <summary>
        /// Animates a popup panel closing
        /// </summary>
        public static IEnumerator AnimatePopupClose(CanvasGroup canvasGroup, Image backgroundOverlay = null)
        {
            canvasGroup.interactable = false;

            float duration = 0.3f;
            float time = 0f;

            Transform popupTransform = canvasGroup.transform;
            Vector3 originalScale = Vector3.one;
            Vector3 targetScale = Vector3.one * 0.8f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                popupTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);

                if (backgroundOverlay != null)
                    backgroundOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.6f, 0f, t));

                yield return null;
            }

            canvasGroup.alpha = 0f;
            popupTransform.localScale = originalScale;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Animates a slide appearing
        /// </summary>
        public static IEnumerator AnimateSlideIn(Transform slideTransform)
        {
            CanvasGroup slideCg = slideTransform.GetComponent<CanvasGroup>();
            if (slideCg == null)
            {
                slideCg = slideTransform.gameObject.AddComponent<CanvasGroup>();
            }

            float fadeDuration = 0.2f;
            float pulseScale = 0.9f;
            Vector3 smallScale = Vector3.one * pulseScale;
            Vector3 originalScale = Vector3.one;

            slideTransform.localScale = smallScale;
            slideCg.alpha = 0f;

            float time = 0f;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = time / fadeDuration;

                slideTransform.localScale = Vector3.Lerp(smallScale, originalScale, t);
                slideCg.alpha = t;

                yield return null;
            }

            slideTransform.localScale = originalScale;
            slideCg.alpha = 1f;
        }

        /// <summary>
        /// Animates a slide disappearing
        /// </summary>
        public static IEnumerator AnimateSlideOut(GameObject slide)
        {
            Transform slideTransform = slide.transform;
            CanvasGroup slideCg = slide.GetComponent<CanvasGroup>();
            if (slideCg == null)
            {
                slideCg = slide.AddComponent<CanvasGroup>();
            }

            float fadeDuration = 0.2f;
            float pulseScale = 0.9f;
            Vector3 originalScale = Vector3.one;
            Vector3 smallScale = originalScale * pulseScale;

            float time = 0f;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = time / fadeDuration;

                slideTransform.localScale = Vector3.Lerp(originalScale, smallScale, t);
                slideCg.alpha = 1f - t;

                yield return null;
            }

            slideCg.alpha = 0f;
            slideTransform.localScale = smallScale;
            slide.SetActive(false);
        }

        /// <summary>
        /// Animates a transition between UI panels
        /// </summary>
        /// <summary>
        /// Animates a transition between UI panels using the same effect as slides
        /// </summary>
        public static IEnumerator TransitionBetweenPanels(GameObject fromPanel, GameObject toPanel, float duration = 0.4f)
        {
            CanvasGroup fromGroup = fromPanel.GetComponent<CanvasGroup>();
            if (fromGroup == null) fromGroup = fromPanel.AddComponent<CanvasGroup>();

            CanvasGroup toGroup = toPanel.GetComponent<CanvasGroup>();
            if (toGroup == null) toGroup = toPanel.AddComponent<CanvasGroup>();

            // Make sure the destination panel is active but invisible
            toPanel.SetActive(true);
            toGroup.alpha = 0f;

            // First, animate the current panel out (same as AnimateSlideOut)
            float fadeDuration = duration * 0.5f;
            float pulseScale = 0.9f;
            Vector3 fromOriginalScale = fromPanel.transform.localScale;
            Vector3 fromSmallScale = fromOriginalScale * pulseScale;

            float time = 0f;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = time / fadeDuration;

                fromPanel.transform.localScale = Vector3.Lerp(fromOriginalScale, fromSmallScale, t);
                fromGroup.alpha = Mathf.Lerp(1f, 0f, t);

                yield return null;
            }

            // Ensure the panel is fully hidden and reset scale
            fromGroup.alpha = 0f;
            fromPanel.transform.localScale = fromOriginalScale;
            fromPanel.SetActive(false);

            // Now animate the new panel in (same as AnimateSlideIn)
            Vector3 toOriginalScale = Vector3.one;
            Vector3 toSmallScale = toOriginalScale * pulseScale;

            toPanel.transform.localScale = toSmallScale;
            time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = time / fadeDuration;

                toPanel.transform.localScale = Vector3.Lerp(toSmallScale, toOriginalScale, t);
                toGroup.alpha = Mathf.Lerp(0f, 1f, t);

                yield return null;
            }

            // Ensure the panel is fully visible and has the correct scale
            toGroup.alpha = 1f;
            toPanel.transform.localScale = toOriginalScale;
        }

        /// <summary>
        /// Bounces a UI element
        /// </summary>
        public static IEnumerator BounceElement(Transform elementTransform)
        {
            float bounceDuration = 0.2f;
            float bounceScale = 0.8f;
            float time = 0f;

            Vector3 originalScale = Vector3.one;
            Vector3 smallScale = Vector3.one * bounceScale;

            // Squeeze
            while (time < bounceDuration / 2f)
            {
                time += Time.deltaTime;
                float t = time / (bounceDuration / 2f);
                elementTransform.localScale = Vector3.Lerp(originalScale, smallScale, t);
                yield return null;
            }

            // Return to normal
            time = 0f;
            while (time < bounceDuration / 2f)
            {
                time += Time.deltaTime;
                float t = time / (bounceDuration / 2f);
                elementTransform.localScale = Vector3.Lerp(smallScale, originalScale, t);
                yield return null;
            }

            elementTransform.localScale = originalScale;
        }

        /// <summary>
        /// Shakes a UI element
        /// </summary>
        public static IEnumerator ShakeElement(Transform elementTransform)
        {
            float duration = 0.3f;
            float magnitude = 10f;
            float elapsed = 0f;
            Vector3 originalPos = elementTransform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Mathf.Sin(elapsed * 40f) * magnitude * (1f - elapsed / duration);
                elementTransform.localPosition = originalPos + new Vector3(x, 0f, 0f);
                yield return null;
            }

            elementTransform.localPosition = originalPos;
        }
    }
}