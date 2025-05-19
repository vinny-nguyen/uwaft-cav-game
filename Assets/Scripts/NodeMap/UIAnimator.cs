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
        private static readonly float SlideTransitionDuration = 0.3f;  // Increased from 0.4f to make easing more noticeable
        private static readonly float BounceDuration = 0.15f;
        private static readonly float ShakeDuration = 0.3f;

        private static readonly float PopupScaleFactor = 0.8f;
        private static readonly float SlideScaleFactor = 0.9f;
        private static readonly float BounceScaleFactor = 0.9f;
        private static readonly float ShakeMagnitude = 10f;
        private static readonly float BackgroundOverlayAlpha = 0.6f;
        private static readonly float SlideJabDistance = 1300f;  // Adjusted from 2000f to balance visibility and speed
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
        public static IEnumerator AnimateSlideInFromSide(Transform slideTransform, Vector3 entryDirection)
        {
            if (slideTransform == null) yield break;

            slideTransform.gameObject.SetActive(true);
            CanvasGroup canvasGroup = GetOrAddCanvasGroup(slideTransform.gameObject);

            Vector3 targetLocalPos = Vector3.zero;
            Vector3 startPosOffset = (entryDirection == Vector3.left) ? Vector3.right : Vector3.left;
            Vector3 startPos = targetLocalPos + startPosOffset * SlideJabDistance;

            Vector3 initialScale = Vector3.one * SlideScaleFactor;
            Vector3 finalScale = Vector3.one;

            slideTransform.localPosition = startPos;
            slideTransform.localScale = initialScale;
            canvasGroup.alpha = 0f;

            yield return AnimatePositionFadeScale(
                slideTransform,
                startPos, targetLocalPos,
                canvasGroup, 0f, 1f,
                initialScale, finalScale,
                SlideTransitionDuration
            );
        }

        /// <summary>
        /// Animates a slide disappearing by sliding out and scaling.
        /// </summary>
        public static IEnumerator AnimateSlideOutToSide(Transform slideTransform, Vector3 exitDirection)
        {
            if (slideTransform == null) yield break;

            CanvasGroup canvasGroup = GetOrAddCanvasGroup(slideTransform.gameObject);
            if (canvasGroup != null) canvasGroup.alpha = 1f; // Ensure fully visible during movement

            Vector3 startLocalPos = slideTransform.localPosition;
            Vector3 endPos = startLocalPos + exitDirection * SlideJabDistance;

            Vector3 initialScale = slideTransform.localScale; // Or Vector3.one if always starting from normal scale
            Vector3 finalScale = Vector3.one * SlideScaleFactor;

            yield return AnimatePositionScale(
                slideTransform,
                startLocalPos, endPos,
                initialScale, finalScale,
                SlideTransitionDuration
            );

            slideTransform.gameObject.SetActive(false);
            slideTransform.localPosition = Vector3.zero;
            slideTransform.localScale = Vector3.one; // Reset scale
            if (canvasGroup != null) canvasGroup.alpha = 0f;
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

            Vector3 baseScale = Vector3.one; // Define the base and final scale as Vector3.one
            Vector3 dipScale = baseScale * BounceScaleFactor; // The scale to "dip" to during the bounce
            float halfDuration = BounceDuration * 0.5f;

            yield return AnimateScale(
                elementTransform,    // transform
                baseScale,           // startScale
                dipScale,            // endScale
                halfDuration         // duration
            );

            // Scale up (expand)
            // This will animate from dipScale back to baseScale (Vector3.one).
            yield return AnimateScale(
                elementTransform,    // transform
                dipScale,            // startScale
                baseScale,           // endScale
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
        /// Reusable animation for position and fade effects.
        /// </summary>
        private static IEnumerator AnimatePositionAndFade(
            Transform transform, Vector3 startPos, Vector3 endPos,
            CanvasGroup canvasGroup, float startAlpha, float endAlpha,
            float duration = 0.3f)
        {
            float time = 0f;
            // Ensure canvasGroup is not null for safety, though GetOrAddCanvasGroup should handle it.
            if (canvasGroup == null) canvasGroup = GetOrAddCanvasGroup(transform.gameObject);


            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float smoothT = SmoothStep(t); // This line applies the easing

                // Update position
                transform.localPosition = Vector3.Lerp(startPos, endPos, smoothT);

                // Update alpha
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);

                yield return null;
            }

            // Ensure final values are set exactly
            transform.localPosition = endPos;
            canvasGroup.alpha = endAlpha;
        }

        private static IEnumerator AnimatePositionOnly(Transform transform, Vector3 startPos, Vector3 endPos, float duration)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float smoothT = SmoothStep(t);
                transform.localPosition = Vector3.Lerp(startPos, endPos, smoothT);
                yield return null;
            }
            transform.localPosition = endPos; // Ensure final position is set
        }

        private static IEnumerator AnimatePositionFadeScale(
            Transform transform, Vector3 startPos, Vector3 endPos,
            CanvasGroup canvasGroup, float startAlpha, float endAlpha,
            Vector3 startScale, Vector3 endScale,
            float duration = 0.3f)
        {
            float time = 0f;
            if (canvasGroup == null) canvasGroup = GetOrAddCanvasGroup(transform.gameObject);

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float smoothT = SmoothStep(t);

                transform.localPosition = Vector3.Lerp(startPos, endPos, smoothT);
                transform.localScale = Vector3.Lerp(startScale, endScale, smoothT);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);

                yield return null;
            }

            transform.localPosition = endPos;
            transform.localScale = endScale;
            canvasGroup.alpha = endAlpha;
        }

        /// <summary>
        /// Animates the position and scale of an object.
        /// </summary>
        private static IEnumerator AnimatePositionScale(Transform transform, Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale, float duration)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float smoothT = SmoothStep(t);
                transform.localPosition = Vector3.Lerp(startPos, endPos, smoothT);
                transform.localScale = Vector3.Lerp(startScale, endScale, smoothT);
                yield return null;
            }
            transform.localPosition = endPos;
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
            // Quintic SmoothStep: 6t^5 - 15t^4 + 10t^3
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }
        #endregion
    }
}