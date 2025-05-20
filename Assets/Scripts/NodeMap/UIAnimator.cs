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
        public static readonly float ShakeDuration = 0.3f;

        private static readonly float PopupScaleFactor = 0.8f;
        private static readonly float SlideScaleFactor = 0.9f;
        private static readonly float BounceScaleFactor = 0.9f;
        public static readonly float ShakeMagnitude = 10f;
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

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            Transform popupTransform = canvasGroup.transform;
            Vector3 startScale = Vector3.one * PopupScaleFactor;
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
                backgroundOverlay, 0f, BackgroundOverlayAlpha,
                PopupOpenDuration
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
            Vector3 startScale = Vector3.one; // Current scale
            Vector3 endScale = Vector3.one * PopupScaleFactor;
            float startAlpha = canvasGroup.alpha; // Current alpha
            float startBgAlpha = backgroundOverlay != null ? backgroundOverlay.color.a : 0f;

            yield return AnimateScaleAndFade(
                popupTransform, startScale, endScale,
                canvasGroup, startAlpha, 0f,
                backgroundOverlay, startBgAlpha, 0f,
                PopupCloseDuration
            );

            canvasGroup.blocksRaycasts = false;
            popupTransform.localScale = Vector3.one; // Reset scale
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
                slideTransform, startPos, targetLocalPos,
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

            Vector3 startLocalPos = slideTransform.localPosition;
            Vector3 endPos = startLocalPos + exitDirection * SlideJabDistance;

            Vector3 initialScale = slideTransform.localScale;
            Vector3 finalScale = Vector3.one * SlideScaleFactor;
            float startAlpha = canvasGroup.alpha;

            yield return AnimatePositionFadeScale(
                slideTransform, startLocalPos, endPos,
                canvasGroup, startAlpha, 0f,
                initialScale, finalScale,
                SlideTransitionDuration
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

            CanvasGroup fromGroup = GetOrAddCanvasGroup(fromPanel);
            CanvasGroup toGroup = GetOrAddCanvasGroup(toPanel);

            toPanel.SetActive(true);
            toGroup.alpha = 0f;

            float halfDuration = duration * 0.5f;
            Vector3 fromOriginalScale = fromPanel.transform.localScale;
            Vector3 fromEndScale = fromOriginalScale * SlideScaleFactor;

            yield return AnimateScaleAndFade(
                fromPanel.transform, fromOriginalScale, fromEndScale,
                fromGroup, 1f, 0f,
                null, 0f, 0f,
                halfDuration
            );

            fromPanel.SetActive(false);
            fromPanel.transform.localScale = fromOriginalScale;

            Vector3 toOriginalScale = Vector3.one; // Assuming toPanel should end at normal scale
            Vector3 toStartScale = toOriginalScale * SlideScaleFactor;
            toPanel.transform.localScale = toStartScale;

            yield return AnimateScaleAndFade(
                toPanel.transform, toStartScale, toOriginalScale,
                toGroup, 0f, 1f,
                null, 0f, 0f,
                halfDuration
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
        public static IEnumerator ShakeElement(Transform elementTransform, float duration, float magnitude, float frequency = 40f)
        {
            if (elementTransform == null) yield break;

            Vector3 originalPos = elementTransform.localPosition;
            float elapsed = 0f;

            // Shake with decreasing magnitude
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float diminishFactor = 1f - (elapsed / duration); // Use parameterized duration
                float offsetX = Mathf.Sin(elapsed * frequency) * magnitude * diminishFactor; // Use parameterized magnitude and frequency

                elementTransform.localPosition = originalPos + new Vector3(offsetX, 0f, 0f);
                yield return null;
            }

            // Reset to original position
            elementTransform.localPosition = originalPos;
        }

        /// <summary>
        /// Animates a sprite's scale for a pop effect and changes the sprite mid-animation.
        /// </summary>
        public static IEnumerator AnimateSpritePop(
            Transform markerTransform,
            SpriteRenderer spriteRenderer,
            Sprite targetSprite,
            float popScaleFactor,
            float upDuration,
            float downDuration)
        {
            if (markerTransform == null || spriteRenderer == null) yield break;

            Vector3 originalScale = markerTransform.localScale;
            Vector3 poppedScale = originalScale * popScaleFactor;

            // Pop up animation
            yield return AnimateScale(markerTransform, originalScale, poppedScale, upDuration);

            // Change sprite
            spriteRenderer.sprite = targetSprite;

            // Pop down animation
            yield return AnimateScale(markerTransform, poppedScale, originalScale, downDuration);
        }

        /// <summary>
        /// Animates the scale and opacity of car part SpriteRenderers for an upgrade effect.
        /// </summary>
        public static IEnumerator AnimateCarPartUpgrade(
            SpriteRenderer carBodyRenderer,
            SpriteRenderer frontWheelRenderer,
            SpriteRenderer rearWheelRenderer,
            System.Action onMidpointAction,
            float totalDuration,
            float targetScaleMultiplier,
            float targetAlpha)
        {
            if (carBodyRenderer == null || frontWheelRenderer == null || rearWheelRenderer == null) yield break;

            Vector3 initialBodyScale = carBodyRenderer.transform.localScale;
            Vector3 initialFrontWheelScale = frontWheelRenderer.transform.localScale;
            Vector3 initialRearWheelScale = rearWheelRenderer.transform.localScale;
            Color initialBodyColor = carBodyRenderer.color;
            Color initialWheelColor = frontWheelRenderer.color;

            float halfDuration = totalDuration / 2f;

            // Phase 1: Scale down and fade out
            yield return AnimateOverTime(halfDuration,
                smoothT =>
                {
                    float currentScale = Mathf.Lerp(1f, targetScaleMultiplier, smoothT);
                    float currentAlpha = Mathf.Lerp(initialBodyColor.a, targetAlpha, smoothT);
                    ApplyRendererProperties(carBodyRenderer, initialBodyScale * currentScale, initialBodyColor, currentAlpha);
                    ApplyRendererProperties(frontWheelRenderer, initialFrontWheelScale * currentScale, initialWheelColor, currentAlpha);
                    ApplyRendererProperties(rearWheelRenderer, initialRearWheelScale * currentScale, initialWheelColor, currentAlpha);
                },
                () => // onComplete
                {
                    float finalScale = targetScaleMultiplier;
                    ApplyRendererProperties(carBodyRenderer, initialBodyScale * finalScale, initialBodyColor, targetAlpha);
                    ApplyRendererProperties(frontWheelRenderer, initialFrontWheelScale * finalScale, initialWheelColor, targetAlpha);
                    ApplyRendererProperties(rearWheelRenderer, initialRearWheelScale * finalScale, initialWheelColor, targetAlpha);
                }
            );

            onMidpointAction?.Invoke();

            // After sprites change, their base color might be different.
            Color bodyColorAfterMidpoint = carBodyRenderer.color;
            Color wheelColorAfterMidpoint = frontWheelRenderer.color;

            // Phase 2: Scale up and fade in
            yield return AnimateOverTime(halfDuration,
                smoothT =>
                {
                    float currentScale = Mathf.Lerp(targetScaleMultiplier, 1f, smoothT);
                    float currentAlpha = Mathf.Lerp(targetAlpha, 1.0f, smoothT);
                    ApplyRendererProperties(carBodyRenderer, initialBodyScale * currentScale, bodyColorAfterMidpoint, currentAlpha);
                    ApplyRendererProperties(frontWheelRenderer, initialFrontWheelScale * currentScale, wheelColorAfterMidpoint, currentAlpha);
                    ApplyRendererProperties(rearWheelRenderer, initialRearWheelScale * currentScale, wheelColorAfterMidpoint, currentAlpha);
                },
                () => // onComplete
                {
                    ApplyRendererProperties(carBodyRenderer, initialBodyScale, bodyColorAfterMidpoint, 1.0f);
                    ApplyRendererProperties(frontWheelRenderer, initialFrontWheelScale, wheelColorAfterMidpoint, 1.0f);
                    ApplyRendererProperties(rearWheelRenderer, initialRearWheelScale, wheelColorAfterMidpoint, 1.0f);
                }
            );
        }

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
            // Optionally reset scale if needed when coroutine stops externally
            // if (elementTransform != null) elementTransform.localScale = baseScale;
        }

        /// <summary>
        /// Animates a content panel revealing or hiding by animating LayoutElement.preferredHeight and CanvasGroup.alpha.
        /// Requires the contentTransform to have a ContentSizeFitter for height calculation when showing.
        /// </summary>
        public static IEnumerator AnimateRevealContent(
            RectTransform contentTransform, // The RectTransform of the content panel
            CanvasGroup contentCanvasGroup,
            LayoutElement contentLayoutElement, // The LayoutElement to animate
            bool show,
            float duration,
            System.Action onComplete = null)
        {
            if (contentTransform == null || contentCanvasGroup == null || contentLayoutElement == null)
            {
                Debug.LogError("AnimateRevealContent: Missing RectTransform, CanvasGroup, or LayoutElement.");
                onComplete?.Invoke();
                yield break;
            }

            float startAlpha = show ? 0f : 1f;
            float endAlpha = show ? 1f : 0f;
            float startHeight;
            float endHeight;

            if (show)
            {
                contentTransform.gameObject.SetActive(true); // Activate to measure preferred height
                contentCanvasGroup.alpha = 0f;               // Start transparent
                contentLayoutElement.preferredHeight = 0f;   // Start collapsed

                // Wait a frame for ContentSizeFitter to calculate the actual height
                yield return null; 
                // Or yield return new WaitForEndOfFrame(); if more reliability is needed for layout calculation

                startHeight = 0f;
                endHeight = LayoutUtility.GetPreferredHeight(contentTransform);
                if (endHeight <= 0) {
                    // This can happen if ContentSizeFitter hasn't updated or content is truly zero height.
                    // Provide a small default or log a warning.
                    // Debug.LogWarning($"AnimateRevealContent: Calculated target height for {contentTransform.name} is {endHeight}. Ensure ContentSizeFitter is setup correctly and content has size.");
                    // endHeight = 1; // Fallback to a tiny height to avoid division by zero or no animation.
                                   // Or, if content is legitimately zero, this is fine.
                }
            }
            else // Hiding
            {
                startHeight = contentLayoutElement.preferredHeight; // Current actual height
                endHeight = 0f;
            }

            // Ensure duration is positive to prevent division by zero if heights are same
            if (Mathf.Approximately(startHeight, endHeight) && Mathf.Approximately(startAlpha, endAlpha))
            {
                 // If already at target state, just ensure final values and complete
                contentLayoutElement.preferredHeight = endHeight;
                contentCanvasGroup.alpha = endAlpha;
                LayoutRebuilder.MarkLayoutForRebuild(contentTransform);
                onComplete?.Invoke();
                yield break;
            }
            
            if (duration <= 0) duration = 0.01f; // Prevent instant animation issues with AnimateOverTime if duration is zero

            yield return AnimateOverTime(duration,
                smoothT =>
                {
                    contentLayoutElement.preferredHeight = Mathf.Lerp(startHeight, endHeight, smoothT);
                    contentCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);
                    if (contentTransform != null) // Guard against object destruction
                    {
                        LayoutRebuilder.MarkLayoutForRebuild(contentTransform);
                    }
                },
                () => // onComplete for AnimateOverTime
                {
                    if (contentLayoutElement != null) contentLayoutElement.preferredHeight = endHeight;
                    if (contentCanvasGroup != null) contentCanvasGroup.alpha = endAlpha;
                    if (contentTransform != null) LayoutRebuilder.MarkLayoutForRebuild(contentTransform);
                    onComplete?.Invoke();
                }
            );
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Generic coroutine to animate properties over a duration using SmoothStep.
        /// </summary>
        public static IEnumerator AnimateOverTime(float duration, System.Action<float> onUpdate, System.Action onComplete = null) // Changed from private to public
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                onUpdate(SmoothStep(t));
                yield return null;
            }
            onComplete?.Invoke();
        }

        /// <summary>
        /// Reusable animation for scale and fade effects
        /// </summary>
        private static IEnumerator AnimateScaleAndFade(
            Transform targetTransform, Vector3 startScale, Vector3 endScale,
            CanvasGroup canvasGroup, float startAlpha, float endAlpha,
            Image backgroundOverlay = null, float startBgAlpha = 0f, float endBgAlpha = 0f,
            float duration = 0.3f)
        {
            yield return AnimateOverTime(duration,
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

        /// <summary>
        /// Animates only the scale of an object
        /// </summary>
        public static IEnumerator AnimateScale(Transform targetTransform, Vector3 startScale, Vector3 endScale, float duration)
        {
            if (targetTransform == null) yield break;
            yield return AnimateOverTime(duration,
                smoothT => targetTransform.localScale = Vector3.Lerp(startScale, endScale, smoothT),
                () => targetTransform.localScale = endScale
            );
        }

        private static IEnumerator AnimatePositionFadeScale(
            Transform targetTransform, Vector3 startPos, Vector3 endPos,
            CanvasGroup canvasGroup, float startAlpha, float endAlpha,
            Vector3 startScale, Vector3 endScale,
            float duration = 0.3f)
        {
            if (targetTransform == null) yield break;
            if (canvasGroup == null) canvasGroup = GetOrAddCanvasGroup(targetTransform.gameObject);

            yield return AnimateOverTime(duration,
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

        /// <summary>
        /// Animates the scale and alpha of a canvas group and its associated transform.
        /// </summary>
        public static IEnumerator AnimateGroupScaleAndFade(CanvasGroup group, Transform transformToScale, Vector3 targetScale, float targetAlpha, float duration)
        {
            if (group == null || transformToScale == null) yield break;

            Vector3 startScale = transformToScale.localScale;
            float startAlpha = group.alpha;

            yield return AnimateOverTime(duration,
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
        /// Animates the alpha of a CanvasGroup.
        /// </summary>
        public static IEnumerator AnimateFade(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
        {
            if (canvasGroup == null) yield break;
            yield return AnimateOverTime(duration,
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
            CanvasGroup panelCanvasGroup = GetOrAddCanvasGroup(panelToShow);

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

        /// <summary>
        /// Helper to apply scale and opacity to a SpriteRenderer.
        /// </summary>
        private static void ApplyRendererProperties(SpriteRenderer renderer, Vector3 scale, Color baseColor, float alpha)
        {
            if (renderer == null) return;
            renderer.transform.localScale = scale;
            Color newColor = baseColor;
            newColor.a = alpha;
            renderer.color = newColor;
        }
        #endregion
    }
}