using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NodeMap.UI
{
    /// <summary>
    /// Handles layout and content reveal animations
    /// </summary>
    public static class LayoutAnimator
    {
        #region Layout Animations
        /// <summary>
        /// Animates a content panel revealing or hiding by animating LayoutElement.preferredHeight and CanvasGroup.alpha.
        /// Requires the contentTransform to have a ContentSizeFitter for height calculation when showing.
        /// </summary>
        public static IEnumerator AnimateRevealContent(
            RectTransform contentTransform,
            CanvasGroup contentCanvasGroup,
            LayoutElement contentLayoutElement,
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
                contentTransform.gameObject.SetActive(true);
                contentCanvasGroup.alpha = 0f;
                contentLayoutElement.preferredHeight = 0f;

                // Wait a frame for ContentSizeFitter to calculate the actual height
                yield return null;

                startHeight = 0f;
                endHeight = LayoutUtility.GetPreferredHeight(contentTransform);
            }
            else // Hiding
            {
                startHeight = contentLayoutElement.preferredHeight;
                endHeight = 0f;
            }

            // Ensure duration is positive to prevent division by zero if heights are same
            if (Mathf.Approximately(startHeight, endHeight) && Mathf.Approximately(startAlpha, endAlpha))
            {
                contentLayoutElement.preferredHeight = endHeight;
                contentCanvasGroup.alpha = endAlpha;
                LayoutRebuilder.MarkLayoutForRebuild(contentTransform);
                onComplete?.Invoke();
                yield break;
            }

            if (duration <= 0) duration = 0.01f;

            yield return AnimationCore.AnimateOverTime(duration,
                smoothT =>
                {
                    contentLayoutElement.preferredHeight = Mathf.Lerp(startHeight, endHeight, smoothT);
                    contentCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);
                    if (contentTransform != null)
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
    }
}
