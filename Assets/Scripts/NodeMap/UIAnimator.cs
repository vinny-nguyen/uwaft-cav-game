using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NodeMap.UI
{
    /// <summary>
    /// Provides reusable UI animation coroutines for the node map system
    /// </summary>
    public static class UIAnimator
    {
        #region Animation Constants
        private static readonly float PopupOpenDuration = 0.4f;
        private static readonly float PopupCloseDuration = 0.3f;
        private static readonly float SlideTransitionDuration = 0.2f;
        private static readonly float BounceDuration = 0.2f;
        private static readonly float ShakeDuration = 0.3f;

        private static readonly float PopupScaleFactor = 0.8f;
        private static readonly float SlideScaleFactor = 0.9f;
        private static readonly float BounceScaleFactor = 0.8f;
        private static readonly float ShakeMagnitude = 10f;
        private static readonly float BackgroundOverlayAlpha = 0.6f;
        #endregion

        #region Popup Animations
        /// <summary>
        /// Animates a popup panel opening with scale and fade
        /// </summary>
        public static IEnumerator AnimatePopupOpen(CanvasGroup canvasGroup, Image backgroundOverlay = null)
        {
            if (canvasGroup == null) yield break;

            // Enable interaction
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            // Initial state
            Transform popupTransform = canvasGroup.transform;
            Vector3 startScale = Vector3.one * PopupScaleFactor;
            Vector3 endScale = Vector3.one;

            popupTransform.localScale = startScale;
            canvasGroup.alpha = 0f;

            yield return AnimateScaleAndFade(
                popupTransform,      // transform
                startScale,          // startScale
                endScale,            // endScale
                canvasGroup,         // canvasGroup
                0f,                  // startAlpha
                1f,                  // endAlpha
                backgroundOverlay,   // backgroundOverlay
                0f,                  // startBackgroundAlpha
                BackgroundOverlayAlpha,  // endBackgroundAlpha
                PopupOpenDuration    // duration
            );
        }

        /// <summary>
        /// Animates a popup panel closing with scale and fade
        /// </summary>
        public static IEnumerator AnimatePopupClose(CanvasGroup canvasGroup, Image backgroundOverlay = null)
        {
            if (canvasGroup == null) yield break;

            // Disable interaction immediately
            canvasGroup.interactable = false;

            Transform popupTransform = canvasGroup.transform;
            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.one * PopupScaleFactor;

            yield return AnimateScaleAndFade(
                popupTransform,      // transform
                startScale,          // startScale
                endScale,            // endScale
                canvasGroup,         // canvasGroup
                1f,                  // startAlpha
                0f,                  // endAlpha
                backgroundOverlay,   // backgroundOverlay
                BackgroundOverlayAlpha,  // startBackgroundAlpha
                0f,                  // endBackgroundAlpha
                PopupCloseDuration   // duration
            );

            // Disable raycast blocking after animation
            canvasGroup.blocksRaycasts = false;

            // Reset scale to avoid affecting future animations
            popupTransform.localScale = Vector3.one;
        }
        #endregion

        #region Slide Animations
        /// <summary>
        /// Animates a slide appearing with scale and fade in
        /// </summary>
        public static IEnumerator AnimateSlideIn(Transform slideTransform)
        {
            if (slideTransform == null) yield break;

            // Get or add canvas group
            CanvasGroup slideCg = GetOrAddCanvasGroup(slideTransform.gameObject);

            // Initial state
            Vector3 startScale = Vector3.one * SlideScaleFactor;
            Vector3 endScale = Vector3.one;

            slideTransform.localScale = startScale;
            slideCg.alpha = 0f;

            yield return AnimateScaleAndFade(
                slideTransform,          // transform
                startScale,              // startScale
                endScale,                // endScale
                slideCg,                 // canvasGroup
                0f,                      // startAlpha
                1f,                      // endAlpha
                null,                    // no background
                0f, 0f,                  // unused background values
                SlideTransitionDuration  // duration
            );
        }

        /// <summary>
        /// Animates a slide disappearing with scale and fade out
        /// </summary>
        public static IEnumerator AnimateSlideOut(GameObject slideObject)
        {
            if (slideObject == null) yield break;

            Transform slideTransform = slideObject.transform;
            CanvasGroup slideCg = GetOrAddCanvasGroup(slideObject);

            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.one * SlideScaleFactor;

            yield return AnimateScaleAndFade(
                slideTransform,          // transform
                startScale,              // startScale
                endScale,                // endScale
                slideCg,                 // canvasGroup
                1f,                      // startAlpha
                0f,                      // endAlpha
                null,                    // no background
                0f, 0f,                  // unused background values
                SlideTransitionDuration  // duration
            );

            // Deactivate slide after animation
            slideObject.SetActive(false);
        }

        /// <summary>
        /// Animates a transition between two panels
        /// </summary>
        public static IEnumerator TransitionBetweenPanels(GameObject fromPanel, GameObject toPanel, float duration = 0.4f)
        {
            if (fromPanel == null || toPanel == null) yield break;

            // Get or add canvas groups
            CanvasGroup fromGroup = GetOrAddCanvasGroup(fromPanel);
            CanvasGroup toGroup = GetOrAddCanvasGroup(toPanel);

            // Prepare panels
            toPanel.SetActive(true);
            toGroup.alpha = 0f;

            // Calculate durations and scales
            float halfDuration = duration * 0.5f;
            Vector3 fromOriginalScale = fromPanel.transform.localScale;
            Vector3 fromEndScale = fromOriginalScale * SlideScaleFactor;

            // Animate first panel out
            yield return AnimateScaleAndFade(
                fromPanel.transform,     // transform
                fromOriginalScale,       // startScale
                fromEndScale,            // endScale
                fromGroup,               // canvasGroup
                1f,                      // startAlpha
                0f,                      // endAlpha
                null,                    // no background
                0f, 0f,                  // unused background values
                halfDuration             // duration
            );

            // Clean up first panel
            fromPanel.SetActive(false);
            fromPanel.transform.localScale = fromOriginalScale;

            // Setup second panel
            Vector3 toOriginalScale = Vector3.one;
            Vector3 toStartScale = toOriginalScale * SlideScaleFactor;
            toPanel.transform.localScale = toStartScale;

            // Animate second panel in
            yield return AnimateScaleAndFade(
                toPanel.transform,       // transform
                toStartScale,            // startScale
                toOriginalScale,         // endScale
                toGroup,                 // canvasGroup
                0f,                      // startAlpha
                1f,                      // endAlpha
                null,                    // no background
                0f, 0f,                  // unused background values
                halfDuration             // duration
            );
        }
        #endregion

        #region Element Animations
        /// <summary>
        /// Creates a bounce animation for a UI element
        /// </summary>
        public static IEnumerator BounceElement(Transform elementTransform)
        {
            if (elementTransform == null) yield break;

            Vector3 originalScale = elementTransform.localScale;
            Vector3 smallScale = originalScale * BounceScaleFactor;
            float halfDuration = BounceDuration * 0.5f;

            // Scale down (squeeze)
            yield return AnimateScale(
                elementTransform,    // transform
                originalScale,       // startScale
                smallScale,          // endScale
                halfDuration         // duration
            );

            // Scale up (expand)
            yield return AnimateScale(
                elementTransform,    // transform
                smallScale,          // startScale
                originalScale,       // endScale
                halfDuration         // duration
            );
        }

        /// <summary>
        /// Creates a side-to-side shake animation
        /// </summary>
        public static IEnumerator ShakeElement(Transform elementTransform)
        {
            if (elementTransform == null) yield break;

            Vector3 originalPos = elementTransform.localPosition;
            float elapsed = 0f;

            // Shake with decreasing magnitude
            while (elapsed < ShakeDuration)
            {
                elapsed += Time.deltaTime;
                float diminishFactor = 1f - (elapsed / ShakeDuration);
                float offsetX = Mathf.Sin(elapsed * 40f) * ShakeMagnitude * diminishFactor;

                elementTransform.localPosition = originalPos + new Vector3(offsetX, 0f, 0f);
                yield return null;
            }

            // Reset to original position
            elementTransform.localPosition = originalPos;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Reusable animation for scale and fade effects
        /// </summary>
        private static IEnumerator AnimateScaleAndFade(
            Transform transform, Vector3 startScale, Vector3 endScale,
            CanvasGroup canvasGroup, float startAlpha, float endAlpha,
            Image backgroundOverlay = null, float startBgAlpha = 0f, float endBgAlpha = 0f,
            float duration = 0.3f)
        {
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float smoothT = SmoothStep(t);

                // Update scale
                transform.localScale = Vector3.Lerp(startScale, endScale, smoothT);

                // Update alpha
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);

                // Update background if provided
                if (backgroundOverlay != null)
                {
                    Color bgColor = backgroundOverlay.color;
                    bgColor.a = Mathf.Lerp(startBgAlpha, endBgAlpha, smoothT);
                    backgroundOverlay.color = bgColor;
                }

                yield return null;
            }

            // Ensure final values are set exactly
            transform.localScale = endScale;
            canvasGroup.alpha = endAlpha;

            if (backgroundOverlay != null)
            {
                Color bgColor = backgroundOverlay.color;
                bgColor.a = endBgAlpha;
                backgroundOverlay.color = bgColor;
            }
        }

        /// <summary>
        /// Animates only the scale of an object
        /// </summary>
        private static IEnumerator AnimateScale(Transform transform, Vector3 startScale, Vector3 endScale, float duration)
        {
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                transform.localScale = Vector3.Lerp(startScale, endScale, SmoothStep(t));
                yield return null;
            }

            transform.localScale = endScale;
        }

        /// <summary>
        /// Gets or adds a canvas group to the game object
        /// </summary>
        private static CanvasGroup GetOrAddCanvasGroup(GameObject gameObject)
        {
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            return canvasGroup;
        }

        /// <summary>
        /// Smoothstep interpolation for more natural animations
        /// </summary>
        private static float SmoothStep(float t)
        {
            return t * t * (3f - 2f * t);
        }
        #endregion
    }
}