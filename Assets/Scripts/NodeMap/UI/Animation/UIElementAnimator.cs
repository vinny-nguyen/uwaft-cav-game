using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace NodeMap.UI
{
    /// <summary>
    /// Handles element-specific animations like bounce, shake, and breathing effects
    /// </summary>
    public static class UIElementAnimator
    {
        #region Element Animations
        /// <summary>
        /// Creates a bounce animation for a UI element
        /// </summary>
        public static IEnumerator BounceElement(Transform elementTransform)
        {
            if (elementTransform == null) yield break;

            Vector3 baseScale = Vector3.one;
            Vector3 dipScale = baseScale * AnimationCore.BounceScaleFactor;
            float halfDuration = AnimationCore.BounceDuration * 0.5f;

            yield return AnimateScale(
                elementTransform,
                baseScale,
                dipScale,
                halfDuration
            );

            yield return AnimateScale(
                elementTransform,
                dipScale,
                baseScale,
                halfDuration
            );
        }

        /// <summary>
        /// Creates a side-to-side shake animation
        /// </summary>
        public static IEnumerator ShakeElement(Transform elementTransform, float duration, float magnitude, float frequency = 40f)
        {
            if (elementTransform == null) yield break;

            Vector3 originalPos = elementTransform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float diminishFactor = 1f - (elapsed / duration);
                float offsetX = Mathf.Sin(elapsed * frequency) * magnitude * diminishFactor;

                elementTransform.localPosition = originalPos + new Vector3(offsetX, 0f, 0f);
                yield return null;
            }

            elementTransform.localPosition = originalPos;
        }

        /// <summary>
        /// Creates a continuous breathing animation effect
        /// </summary>
        public static IEnumerator BreatheElement(Transform elementTransform, float breatheDuration, float breatheMagnitude, Vector3 baseScale)
        {
            if (elementTransform == null) yield break;
            float timer = 0f;

            while (elementTransform != null && elementTransform.gameObject.activeInHierarchy)
            {
                timer += Time.deltaTime;
                float scaleFactor = 1f + Mathf.Sin(timer * Mathf.PI * 2f / breatheDuration) * breatheMagnitude;
                elementTransform.localScale = baseScale * scaleFactor;
                yield return null;
            }
        }

        /// <summary>
        /// Animates only the scale of an object
        /// </summary>
        public static IEnumerator AnimateScale(Transform targetTransform, Vector3 startScale, Vector3 endScale, float duration)
        {
            if (targetTransform == null) yield break;
            yield return AnimationCore.AnimateOverTime(duration,
                smoothT => targetTransform.localScale = Vector3.Lerp(startScale, endScale, smoothT),
                () => targetTransform.localScale = endScale
            );
        }

        /// <summary>
        /// Animates the scale and alpha of a canvas group and its associated transform.
        /// </summary>
        public static IEnumerator AnimateGroupScaleAndFade(CanvasGroup group, Transform transformToScale, Vector3 targetScale, float targetAlpha, float duration)
        {
            if (group == null || transformToScale == null) yield break;

            Vector3 startScale = transformToScale.localScale;
            float startAlpha = group.alpha;

            yield return AnimationCore.AnimateOverTime(duration,
                smoothT =>
                {
                    transformToScale.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
                    group.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothT);
                },
                () => // onComplete
                {
                    transformToScale.localScale = targetScale;
                    group.alpha = targetAlpha;
                }
            );
        }

        /// <summary>
        /// Shows a temporary message using a TextMeshProUGUI element, with fade-in, hold, and fade-out.
        /// </summary>
        public static IEnumerator ShowTemporaryMessage(TextMeshProUGUI textElement, string message, float holdDuration, float fadeDuration)
        {
            if (textElement == null)
            {
                Debug.LogWarning("ShowTemporaryMessage: textElement is null.");
                yield break;
            }

            // Store original state
            Color originalColor = textElement.color;
            string originalText = textElement.text;
            bool wasGameObjectActive = textElement.gameObject.activeSelf;

            // Prepare for animation
            textElement.gameObject.SetActive(true);
            textElement.text = message;

            float targetAlpha = 1.0f;

            // Set initial state for fade in (fully transparent)
            Color currentColor = textElement.color;
            currentColor.a = 0f;
            textElement.color = currentColor;

            // Fade In animation
            yield return AnimationCore.AnimateOverTime(fadeDuration,
                smoothT => {
                    if (textElement == null) return;
                    Color c = textElement.color;
                    c.a = Mathf.Lerp(0f, targetAlpha, smoothT);
                    textElement.color = c;
                },
                () => {
                    if (textElement == null) return;
                    Color c = textElement.color;
                    c.a = targetAlpha;
                    textElement.color = c;
                }
            );

            // Hold
            if (textElement != null)
            {
                yield return new WaitForSeconds(holdDuration);
            }

            // Fade Out animation
            float fadeOutSourceAlpha = (textElement != null) ? textElement.color.a : targetAlpha;

            yield return AnimationCore.AnimateOverTime(fadeDuration,
                smoothT => {
                    if (textElement == null) return;
                    Color c = textElement.color;
                    c.a = Mathf.Lerp(fadeOutSourceAlpha, 0f, smoothT);
                    textElement.color = c;
                },
                () => {
                    if (textElement == null) return;
                    Color c = textElement.color;
                    c.a = 0f;
                    textElement.color = c;
                }
            );

            // Cleanup: Restore original state
            if (textElement != null)
            {
                textElement.text = originalText;
                textElement.color = originalColor;
                textElement.gameObject.SetActive(wasGameObjectActive);
            }
        }
        #endregion
    }
}
