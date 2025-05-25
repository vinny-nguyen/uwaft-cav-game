using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace NodeMap.UI
{
    /// <summary>
    /// Main UIAnimator facade that provides access to all animation systems.
    /// This class maintains backward compatibility while delegating to specialized animators.
    /// </summary>
    public static class UIAnimator
    {
        #region Animation Constants (Exposed for backward compatibility)
        public static readonly float ShakeDuration = AnimationCore.ShakeDuration;
        public static readonly float ShakeMagnitude = AnimationCore.ShakeMagnitude;
        #endregion

        #region Popup Animations
        public static IEnumerator AnimatePopupOpen(CanvasGroup canvasGroup, Image backgroundOverlay = null)
            => UITransitionAnimator.AnimatePopupOpen(canvasGroup, backgroundOverlay);

        public static IEnumerator AnimatePopupClose(CanvasGroup canvasGroup, Image backgroundOverlay = null)
            => UITransitionAnimator.AnimatePopupClose(canvasGroup, backgroundOverlay);
        #endregion

        #region Slide Animations
        public static IEnumerator AnimateSlideInFromSide(Transform slideTransform, Vector3 entryDirection)
            => UITransitionAnimator.AnimateSlideInFromSide(slideTransform, entryDirection);

        public static IEnumerator AnimateSlideOutToSide(Transform slideTransform, Vector3 exitDirection)
            => UITransitionAnimator.AnimateSlideOutToSide(slideTransform, exitDirection);

        public static IEnumerator TransitionBetweenPanels(GameObject fromPanel, GameObject toPanel, float duration = 0.4f)
            => UITransitionAnimator.TransitionBetweenPanels(fromPanel, toPanel, duration);
        #endregion

        #region Element Animations
        public static IEnumerator BounceElement(Transform elementTransform)
            => UIElementAnimator.BounceElement(elementTransform);

        public static IEnumerator ShakeElement(Transform elementTransform, float duration, float magnitude, float frequency = 40f)
            => UIElementAnimator.ShakeElement(elementTransform, duration, magnitude, frequency);

        public static IEnumerator BreatheElement(Transform elementTransform, float breatheDuration, float breatheMagnitude, Vector3 baseScale)
            => UIElementAnimator.BreatheElement(elementTransform, breatheDuration, breatheMagnitude, baseScale);

        public static IEnumerator AnimateScale(Transform targetTransform, Vector3 startScale, Vector3 endScale, float duration)
            => UIElementAnimator.AnimateScale(targetTransform, startScale, endScale, duration);

        public static IEnumerator AnimateGroupScaleAndFade(CanvasGroup group, Transform transformToScale, Vector3 targetScale, float targetAlpha, float duration)
            => UIElementAnimator.AnimateGroupScaleAndFade(group, transformToScale, targetScale, targetAlpha, duration);

        public static IEnumerator ShowTemporaryMessage(TextMeshProUGUI textElement, string message, float holdDuration, float fadeDuration)
            => UIElementAnimator.ShowTemporaryMessage(textElement, message, holdDuration, fadeDuration);
        #endregion

        #region Sprite Animations
        public static IEnumerator AnimateSpritePop(Transform markerTransform, SpriteRenderer spriteRenderer, Sprite targetSprite, float popScaleFactor, float upDuration, float downDuration)
            => SpriteAnimator.AnimateSpritePop(markerTransform, spriteRenderer, targetSprite, popScaleFactor, upDuration, downDuration);

        public static IEnumerator AnimateCarPartUpgrade(SpriteRenderer carBodyRenderer, SpriteRenderer frontWheelRenderer, SpriteRenderer rearWheelRenderer, System.Action onMidpointAction, float totalDuration, float targetScaleMultiplier, float targetAlpha)
            => SpriteAnimator.AnimateCarPartUpgrade(carBodyRenderer, frontWheelRenderer, rearWheelRenderer, onMidpointAction, totalDuration, targetScaleMultiplier, targetAlpha);
        #endregion

        #region Layout Animations
        public static IEnumerator AnimateRevealContent(RectTransform contentTransform, CanvasGroup contentCanvasGroup, LayoutElement contentLayoutElement, bool show, float duration, System.Action onComplete = null)
            => LayoutAnimator.AnimateRevealContent(contentTransform, contentCanvasGroup, contentLayoutElement, show, duration, onComplete);
        #endregion

        #region Fade Animations
        public static IEnumerator AnimateFade(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
            => UITransitionAnimator.AnimateFade(canvasGroup, startAlpha, endAlpha, duration);

        public static IEnumerator ShowTemporaryPanel(GameObject panelToShow, float displayTime, float fadeInDuration, float fadeOutDuration, System.Action onComplete = null, GameObject panelToHideInitially = null)
            => UITransitionAnimator.ShowTemporaryPanel(panelToShow, displayTime, fadeInDuration, fadeOutDuration, onComplete, panelToHideInitially);
        #endregion

        #region Core Animation Methods (Exposed for backward compatibility)
        public static IEnumerator AnimateOverTime(float duration, System.Action<float> onUpdate, System.Action onComplete = null)
            => AnimationCore.AnimateOverTime(duration, onUpdate, onComplete);
        #endregion
    }
}