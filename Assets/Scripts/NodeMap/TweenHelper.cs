using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace NodeMap
{
    /// <summary>
    /// Centralized helper for tweening/animations (scaling, moving, fading)
    /// </summary>
    public static class TweenHelper
    {
        public static IEnumerator ScaleTo(Transform target, Vector3 to, float duration)
        {
            Vector3 from = target.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            target.localScale = to;
        }

        public static IEnumerator MoveTo(Transform target, Vector3 to, float duration)
        {
            Vector3 from = target.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.localPosition = Vector3.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            target.localPosition = to;
        }

        public static IEnumerator FadeTo(CanvasGroup canvasGroup, float to, float duration)
        {
            float from = canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = to;
        }

        public static IEnumerator FadeImageTo(Image image, float to, float duration)
        {
            float from = image.color.a;
            float elapsed = 0f;
            Color color = image.color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(from, to, elapsed / duration);
                image.color = color;
                yield return null;
            }
            color.a = to;
            image.color = color;
        }
    }
}
