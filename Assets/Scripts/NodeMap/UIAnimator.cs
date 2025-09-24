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
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            Transform popupTransform = canvasGroup.transform;
            Vector3 startScale = Vector3.one * PopupScaleFactor;
            Vector3 endScale = Vector3.one;
            popupTransform.localScale = startScale;
            canvasGroup.alpha = 0f;
            // Run scale and fade in parallel
            yield return NodeMap.TweenHelper.ScaleTo(popupTransform, endScale, PopupOpenDuration);
            yield return NodeMap.TweenHelper.FadeTo(canvasGroup, 1f, PopupOpenDuration);
            if (backgroundOverlay != null)
                yield return NodeMap.TweenHelper.FadeImageTo(backgroundOverlay, BackgroundOverlayAlpha, PopupOpenDuration);
        }

        /// <summary>
        /// Animates a popup panel closing with scale and fade
        /// </summary>
        public static IEnumerator AnimatePopupClose(CanvasGroup canvasGroup, Image backgroundOverlay = null)
        {
            if (canvasGroup == null) yield break;
            canvasGroup.interactable = false;
            Transform popupTransform = canvasGroup.transform;
            Vector3 endScale = Vector3.one * PopupScaleFactor;
            yield return NodeMap.TweenHelper.ScaleTo(popupTransform, endScale, PopupCloseDuration);
            yield return NodeMap.TweenHelper.FadeTo(canvasGroup, 0f, PopupCloseDuration);
            if (backgroundOverlay != null)
                yield return NodeMap.TweenHelper.FadeImageTo(backgroundOverlay, 0f, PopupCloseDuration);
            canvasGroup.blocksRaycasts = false;
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
            CanvasGroup slideCg = GetOrAddCanvasGroup(slideTransform.gameObject);
            Vector3 startScale = Vector3.one * SlideScaleFactor;
            Vector3 endScale = Vector3.one;
            slideTransform.localScale = startScale;
            slideCg.alpha = 0f;
            yield return NodeMap.TweenHelper.ScaleTo(slideTransform, endScale, SlideTransitionDuration);
            yield return NodeMap.TweenHelper.FadeTo(slideCg, 1f, SlideTransitionDuration);
        }

        /// <summary>
        /// Animates a slide disappearing with scale and fade out
        /// </summary>
        public static IEnumerator AnimateSlideOut(GameObject slideObject)
        {
            if (slideObject == null) yield break;
            Transform slideTransform = slideObject.transform;
            CanvasGroup slideCg = GetOrAddCanvasGroup(slideObject);
            Vector3 endScale = Vector3.one * SlideScaleFactor;
            yield return NodeMap.TweenHelper.ScaleTo(slideTransform, endScale, SlideTransitionDuration);
            yield return NodeMap.TweenHelper.FadeTo(slideCg, 0f, SlideTransitionDuration);
            slideObject.SetActive(false);
        }

        /// <summary>
        /// Animates a transition between two panels
        /// </summary>
        public static IEnumerator TransitionBetweenPanels(GameObject fromPanel, GameObject toPanel, float duration = 0.4f)
        {
            if (fromPanel == null || toPanel == null) yield break;
            CanvasGroup fromGroup = GetOrAddCanvasGroup(fromPanel);
            CanvasGroup toGroup = GetOrAddCanvasGroup(toPanel);
            toPanel.SetActive(true);
            toGroup.alpha = 0f;
            float halfDuration = duration * 0.5f;
            Vector3 fromEndScale = fromPanel.transform.localScale * SlideScaleFactor;
            yield return NodeMap.TweenHelper.ScaleTo(fromPanel.transform, fromEndScale, halfDuration);
            yield return NodeMap.TweenHelper.FadeTo(fromGroup, 0f, halfDuration);
            fromPanel.SetActive(false);
            fromPanel.transform.localScale = Vector3.one;
            Vector3 toStartScale = Vector3.one * SlideScaleFactor;
            toPanel.transform.localScale = toStartScale;
            yield return NodeMap.TweenHelper.ScaleTo(toPanel.transform, Vector3.one, halfDuration);
            yield return NodeMap.TweenHelper.FadeTo(toGroup, 1f, halfDuration);
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
            yield return NodeMap.TweenHelper.ScaleTo(elementTransform, smallScale, halfDuration);
            yield return NodeMap.TweenHelper.ScaleTo(elementTransform, originalScale, halfDuration);
        }

        /// <summary>
        /// Creates a side-to-side shake animation
        /// </summary>
        public static IEnumerator ShakeElement(Transform elementTransform)
        {
            if (elementTransform == null) yield break;
            Vector3 originalPos = elementTransform.localPosition;
            Vector3 shakeOffset = originalPos + new Vector3(ShakeMagnitude * 0.1f, 0f, 0f);
            yield return NodeMap.TweenHelper.MoveTo(elementTransform, shakeOffset, ShakeDuration * 0.5f);
            yield return NodeMap.TweenHelper.MoveTo(elementTransform, originalPos, ShakeDuration * 0.5f);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Reusable animation for scale and fade effects
        /// </summary>
        // AnimateScaleAndFade and AnimateScale replaced by TweenHelper usage in animation routines above.

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