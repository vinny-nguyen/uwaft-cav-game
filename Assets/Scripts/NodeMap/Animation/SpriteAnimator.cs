using UnityEngine;
using System.Collections;

namespace NodeMap.UI
{
    /// <summary>
    /// Handles sprite and renderer-specific animations
    /// </summary>
    public static class SpriteAnimator
    {
        #region Sprite Animations
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
            yield return UIElementAnimator.AnimateScale(markerTransform, originalScale, poppedScale, upDuration);

            // Change sprite
            spriteRenderer.sprite = targetSprite;

            // Pop down animation
            yield return UIElementAnimator.AnimateScale(markerTransform, poppedScale, originalScale, downDuration);
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
            yield return AnimationCore.AnimateOverTime(halfDuration,
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
            yield return AnimationCore.AnimateOverTime(halfDuration,
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
