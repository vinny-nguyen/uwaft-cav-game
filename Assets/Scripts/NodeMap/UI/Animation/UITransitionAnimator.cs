using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NodeMap.UI
{
    /// <summary>
    /// Handles UI transition animations like popups, slides, and fades
    /// </summary>
    public static class UITransitionAnimator
    {
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
            Vector3 startScale = Vector3.one * AnimationCore.PopupScaleFactor;
            Vector3 endScale = Vector3.one;

            popupTransform.localScale = startScale;
            canvasGroup.alpha = 0f;
            if (backgroundOverlay != null)
            {
                Color bgColor = backgroundOverlay.color;
                bgColor.a = 0f;
                backgroundOverlay.color = bgColor;
            }

            yield return AnimateScaleAndFade(
                popupTransform, startScale, endScale,
                canvasGroup, 0f, 1f,
                backgroundOverlay, 0f, AnimationCore.BackgroundOverlayAlpha,
                AnimationCore.PopupOpenDuration
            );
        }

        /// <summary>
        /// Animates a popup panel closing with scale and fade
        /// </summary>
        public static IEnumerator AnimatePopupClose(CanvasGroup canvasGroup, Image backgroundOverlay = null)
        {
            if (canvasGroup == null) yield break;

            canvasGroup.interactable = false;

            Transform popupTransform = canvasGroup.transform;
            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.one * AnimationCore.PopupScaleFactor;
            float startAlpha = canvasGroup.alpha;
            float startBgAlpha = backgroundOverlay != null ? backgroundOverlay.color.a : 0f;

            yield return AnimateScaleAndFade(
                popupTransform, startScale, endScale,
                canvasGroup, startAlpha, 0f,
                backgroundOverlay, startBgAlpha, 0f,
                AnimationCore.PopupCloseDuration
            );

            canvasGroup.blocksRaycasts = false;
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
            CanvasGroup canvasGroup = AnimationCore.GetOrAddCanvasGroup(slideTransform.gameObject);

            Vector3 targetLocalPos = Vector3.zero;
            Vector3 startPosOffset = (entryDirection == Vector3.left) ? Vector3.right : Vector3.left;
            Vector3 startPos = targetLocalPos + startPosOffset * AnimationCore.SlideJabDistance;

            Vector3 initialScale = Vector3.one * AnimationCore.SlideScaleFactor;
            Vector3 finalScale = Vector3.one;

            slideTransform.localPosition = startPos;
            slideTransform.localScale = initialScale;
            canvasGroup.alpha = 0f;

            yield return AnimatePositionFadeScale(
                slideTransform, startPos, targetLocalPos,
                canvasGroup, 0f, 1f,
                initialScale, finalScale,
                AnimationCore.SlideTransitionDuration
            );
        }

        /// <summary>
        /// Animates a slide disappearing by sliding out and scaling.
        /// </summary>
        public static IEnumerator AnimateSlideOutToSide(Transform slideTransform, Vector3 exitDirection)
        {
            if (slideTransform == null) yield break;

            CanvasGroup canvasGroup = AnimationCore.GetOrAddCanvasGroup(slideTransform.gameObject);

            Vector3 startLocalPos = slideTransform.localPosition;
            Vector3 endPos = startLocalPos + exitDirection * AnimationCore.SlideJabDistance;

            Vector3 initialScale = slideTransform.localScale;
            Vector3 finalScale = Vector3.one * AnimationCore.SlideScaleFactor;
            float startAlpha = canvasGroup.alpha;

            yield return AnimatePositionFadeScale(
                slideTransform, startLocalPos, endPos,
                canvasGroup, startAlpha, 0f,
                initialScale, finalScale,
                AnimationCore.SlideTransitionDuration
            );

            slideTransform.gameObject.SetActive(false);
            slideTransform.localPosition = Vector3.zero;
            slideTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// Animates a transition between two panels
        /// </summary>
        public static IEnumerator TransitionBetweenPanels(GameObject fromPanel, GameObject toPanel, float duration = 0.4f)
        {
            if (fromPanel == null || toPanel == null) yield break;

            CanvasGroup fromGroup = AnimationCore.GetOrAddCanvasGroup(fromPanel);
            CanvasGroup toGroup = AnimationCore.GetOrAddCanvasGroup(toPanel);

            toPanel.SetActive(true);
            toGroup.alpha = 0f;

            float halfDuration = duration * 0.5f;
            Vector3 fromOriginalScale = fromPanel.transform.localScale;
            Vector3 fromEndScale = fromOriginalScale * AnimationCore.SlideScaleFactor;

            yield return AnimateScaleAndFade(
                fromPanel.transform, fromOriginalScale, fromEndScale,
                fromGroup, 1f, 0f,
                null, 0f, 0f,
                halfDuration
            );

            fromPanel.SetActive(false);
            fromPanel.transform.localScale = fromOriginalScale;

            Vector3 toOriginalScale = Vector3.one;
            Vector3 toStartScale = toOriginalScale * AnimationCore.SlideScaleFactor;
            toPanel.transform.localScale = toStartScale;

            yield return AnimateScaleAndFade(
                toPanel.transform, toStartScale, toOriginalScale,
                toGroup, 0f, 1f,
                null, 0f, 0f,
                halfDuration
            );
        }
        #endregion

        #region Fade Animations
        /// <summary>
        /// Animates the alpha of a CanvasGroup.
        /// </summary>
        public static IEnumerator AnimateFade(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
        {
            if (canvasGroup == null) yield break;
            yield return AnimationCore.AnimateOverTime(duration,
                smoothT => canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT),
                () => canvasGroup.alpha = endAlpha
            );
        }

        /// <summary>
        /// Shows a panel temporarily with fade-in and fade-out, optionally hiding another panel.
        /// </summary>
        public static IEnumerator ShowTemporaryPanel(
            GameObject panelToShow,
            float displayTime,
            float fadeInDuration,
            float fadeOutDuration,
            System.Action onComplete = null,
            GameObject panelToHideInitially = null)
        {
            if (panelToShow == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            if (panelToHideInitially != null)
            {
                panelToHideInitially.SetActive(false);
            }

            panelToShow.SetActive(true);
            CanvasGroup panelCanvasGroup = AnimationCore.GetOrAddCanvasGroup(panelToShow);

            // Fade In
            panelCanvasGroup.alpha = 0f;
            yield return AnimateFade(panelCanvasGroup, 0f, 1f, fadeInDuration);

            // Hold
            float holdTime = displayTime - fadeInDuration - fadeOutDuration;
            if (holdTime > 0)
            {
                yield return new WaitForSeconds(holdTime);
            }

            // Fade Out
            yield return AnimateFade(panelCanvasGroup, 1f, 0f, fadeOutDuration);

            panelToShow.SetActive(false);
            onComplete?.Invoke();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Reusable animation for scale and fade effects
        /// </summary>
        private static IEnumerator AnimateScaleAndFade(
            Transform targetTransform, Vector3 startScale, Vector3 endScale,
            CanvasGroup canvasGroup, float startAlpha, float endAlpha,
            Image backgroundOverlay = null, float startBgAlpha = 0f, float endBgAlpha = 0f,
            float duration = 0.3f)
        {
            yield return AnimationCore.AnimateOverTime(duration,
                smoothT =>
                {
                    if (targetTransform != null)
                        targetTransform.localScale = Vector3.Lerp(startScale, endScale, smoothT);
                    if (canvasGroup != null)
                        canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);
                    if (backgroundOverlay != null)
                    {
                        Color bgColor = backgroundOverlay.color;
                        bgColor.a = Mathf.Lerp(startBgAlpha, endBgAlpha, smoothT);
                        backgroundOverlay.color = bgColor;
                    }
                },
                () => // onComplete
                {
                    if (targetTransform != null)
                        targetTransform.localScale = endScale;
                    if (canvasGroup != null)
                        canvasGroup.alpha = endAlpha;
                    if (backgroundOverlay != null)
                    {
                        Color bgColor = backgroundOverlay.color;
                        bgColor.a = endBgAlpha;
                        backgroundOverlay.color = bgColor;
                    }
                }
            );
        }

        private static IEnumerator AnimatePositionFadeScale(
            Transform targetTransform, Vector3 startPos, Vector3 endPos,
            CanvasGroup canvasGroup, float startAlpha, float endAlpha,
            Vector3 startScale, Vector3 endScale,
            float duration = 0.3f)
        {
            if (targetTransform == null) yield break;
            if (canvasGroup == null) canvasGroup = AnimationCore.GetOrAddCanvasGroup(targetTransform.gameObject);

            yield return AnimationCore.AnimateOverTime(duration,
                smoothT =>
                {
                    targetTransform.localPosition = Vector3.Lerp(startPos, endPos, smoothT);
                    targetTransform.localScale = Vector3.Lerp(startScale, endScale, smoothT);
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);
                },
                () => // onComplete
                {
                    targetTransform.localPosition = endPos;
                    targetTransform.localScale = endScale;
                    canvasGroup.alpha = endAlpha;
                }
            );
        }
        #endregion
    }
}
