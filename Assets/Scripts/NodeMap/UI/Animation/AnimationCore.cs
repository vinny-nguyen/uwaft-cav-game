using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NodeMap.UI
{
    /// <summary>
    /// Core animation utilities and constants shared across all animation systems
    /// </summary>
    public static class AnimationCore
    {
        #region Animation Constants
        public static readonly float PopupOpenDuration = 0.4f;
        public static readonly float PopupCloseDuration = 0.3f;
        public static readonly float SlideTransitionDuration = 0.3f;
        public static readonly float BounceDuration = 0.15f;
        public static readonly float ShakeDuration = 0.3f;

        public static readonly float PopupScaleFactor = 0.8f;
        public static readonly float SlideScaleFactor = 0.9f;
        public static readonly float BounceScaleFactor = 0.9f;
        public static readonly float ShakeMagnitude = 10f;
        public static readonly float BackgroundOverlayAlpha = 0.6f;
        public static readonly float SlideJabDistance = 1300f;
        #endregion

        #region Core Animation Methods
        /// <summary>
        /// Generic coroutine to animate properties over a duration using SmoothStep.
        /// </summary>
        public static IEnumerator AnimateOverTime(float duration, System.Action<float> onUpdate, System.Action onComplete = null)
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
        /// Smoothstep interpolation for more natural animations
        /// </summary>
        public static float SmoothStep(float t)
        {
            // Quintic SmoothStep: 6t^5 - 15t^4 + 10t^3
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        /// <summary>
        /// Gets or adds a canvas group to the game object
        /// </summary>
        public static CanvasGroup GetOrAddCanvasGroup(GameObject gameObject)
        {
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            return canvasGroup;
        }
        #endregion
    }
}
