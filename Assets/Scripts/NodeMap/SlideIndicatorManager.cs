using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NodeMap.UI
{
    /// <summary>
    /// Manages slide indicators (dots) showing current position in a slide sequence
    /// </summary>
    public class SlideIndicatorManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Indicator Settings")]
        [SerializeField] private GameObject slideDotPrefab;
        [SerializeField] private Transform slideIndicatorsParent;
        [SerializeField] private Sprite activeDotSprite;
        [SerializeField] private Sprite inactiveDotSprite;
        [SerializeField] private CanvasGroup slideIndicatorsCanvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float transitionDuration = 0.2f;
        [SerializeField] private float scaleAmount = 0.9f;
        [SerializeField] private float breatheDuration = 1f;
        [SerializeField] private float breatheMagnitude = 0.2f;
        #endregion

        #region Private Fields
        private List<GameObject> spawnedDots = new List<GameObject>();
        private Coroutine activeDotBreathing;
        private int lastActiveIndex = -1;
        private Coroutine animationCoroutine;
        #endregion

        #region Public Methods
        /// <summary>
        /// Generates indicator dots for the specified number of slides
        /// </summary>
        public void GenerateIndicators(int count)
        {
            ClearIndicators();

            for (int i = 0; i < count; i++)
            {
                GameObject dot = Instantiate(slideDotPrefab, slideIndicatorsParent);
                spawnedDots.Add(dot);
            }

            UpdateActiveIndicator(0, false);
        }

        /// <summary>
        /// Updates the active indicator
        /// </summary>
        public void UpdateActiveIndicator(int activeIndex, bool animate = true)
        {
            if (spawnedDots.Count == 0) return;

            StopAnimation(ref animationCoroutine);

            if (animate && activeIndex != lastActiveIndex)
            {
                animationCoroutine = StartCoroutine(AnimateIndicatorTransition(activeIndex));
            }
            else
            {
                UpdateIndicatorsImmediate(activeIndex);
            }

            lastActiveIndex = activeIndex;
        }

        /// <summary>
        /// Clears all indicator dots
        /// </summary>
        public void ClearIndicators()
        {
            StopAnimation(ref activeDotBreathing);
            StopAnimation(ref animationCoroutine);

            foreach (var dot in spawnedDots)
            {
                if (dot != null)
                    Destroy(dot);
            }

            spawnedDots.Clear();
            lastActiveIndex = -1;
        }

        /// <summary>
        /// Shows or hides the indicators with animation
        /// </summary>
        public void SetVisibility(bool visible, bool animate = true)
        {
            EnsureCanvasGroup();
            slideIndicatorsParent.gameObject.SetActive(true);

            StopAnimation(ref animationCoroutine);

            if (animate)
            {
                animationCoroutine = StartCoroutine(AnimateVisibility(visible));
            }
            else
            {
                // Immediate change
                slideIndicatorsCanvasGroup.alpha = visible ? 1f : 0f;
                if (!visible) slideIndicatorsParent.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Animation Methods
        /// <summary>
        /// Animates the transition between indicator states
        /// </summary>
        private IEnumerator AnimateIndicatorTransition(int newActiveIndex)
        {
            CanvasGroup parentGroup = EnsureCanvasGroup();

            // Scale down and fade out slightly
            yield return ScaleAndFadeGroup(parentGroup,
                slideIndicatorsParent.localScale,
                slideIndicatorsParent.localScale * scaleAmount,
                1f, 0.7f,
                transitionDuration / 2f);

            // Update the indicators while scaled down
            UpdateIndicatorsImmediate(newActiveIndex);

            // Scale back up and fade in
            yield return ScaleAndFadeGroup(parentGroup,
                slideIndicatorsParent.localScale,
                slideIndicatorsParent.localScale / scaleAmount,
                0.7f, 1f,
                transitionDuration / 2f);
        }

        /// <summary>
        /// Animates the visibility change
        /// </summary>
        private IEnumerator AnimateVisibility(bool visible)
        {
            float targetAlpha = visible ? 1f : 0f;
            float startAlpha = slideIndicatorsCanvasGroup.alpha;
            Vector3 startScale = slideIndicatorsParent.localScale;
            Vector3 targetScale = visible ? Vector3.one : Vector3.one * scaleAmount;

            yield return ScaleAndFadeGroup(slideIndicatorsCanvasGroup,
                startScale, targetScale,
                startAlpha, targetAlpha,
                transitionDuration);

            // Only deactivate after animation if hiding
            if (!visible) slideIndicatorsParent.gameObject.SetActive(false);
        }

        /// <summary>
        /// Animates the scale and alpha of a canvas group
        /// </summary>
        private IEnumerator ScaleAndFadeGroup(CanvasGroup group, Vector3 startScale, Vector3 targetScale,
                                              float startAlpha, float targetAlpha, float duration)
        {
            float time = 0f;
            Transform parentTransform = group.transform;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);

                parentTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                yield return null;
            }

            // Ensure we hit the target values exactly
            parentTransform.localScale = targetScale;
            group.alpha = targetAlpha;
        }

        /// <summary>
        /// Animates a dot "breathing" (pulsing scale)
        /// </summary>
        private IEnumerator BreatheDot(Transform dotTransform)
        {
            float timer = 0f;

            while (dotTransform != null && dotTransform.gameObject.activeInHierarchy)
            {
                timer += Time.deltaTime;
                float scale = 1f + Mathf.Sin(timer * Mathf.PI * 2f / breatheDuration) * breatheMagnitude;
                dotTransform.localScale = Vector3.one * scale;
                yield return null;
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Updates indicators immediately without animation
        /// </summary>
        private void UpdateIndicatorsImmediate(int activeIndex)
        {
            for (int i = 0; i < spawnedDots.Count; i++)
            {
                GameObject dotContainer = spawnedDots[i];
                if (dotContainer == null) continue;

                Transform dotVisual = dotContainer.transform.Find("DotVisual");
                if (dotVisual == null) continue;

                Image img = dotVisual.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = (i == activeIndex) ? activeDotSprite : inactiveDotSprite;
                }

                // Reset scale on all dots
                dotVisual.localScale = Vector3.one;
            }

            StartBreathingAnimation(activeIndex);
        }

        /// <summary>
        /// Starts breathing animation on the active dot
        /// </summary>
        private void StartBreathingAnimation(int activeIndex)
        {
            StopAnimation(ref activeDotBreathing);

            if (activeIndex >= 0 && activeIndex < spawnedDots.Count && spawnedDots[activeIndex] != null)
            {
                Transform dotVisual = spawnedDots[activeIndex].transform.Find("DotVisual");
                if (dotVisual != null)
                {
                    activeDotBreathing = StartCoroutine(BreatheDot(dotVisual));
                }
            }
        }

        /// <summary>
        /// Ensures the canvas group exists
        /// </summary>
        private CanvasGroup EnsureCanvasGroup()
        {
            if (slideIndicatorsCanvasGroup == null)
            {
                slideIndicatorsCanvasGroup = slideIndicatorsParent.GetComponent<CanvasGroup>();
                if (slideIndicatorsCanvasGroup == null)
                {
                    slideIndicatorsCanvasGroup = slideIndicatorsParent.gameObject.AddComponent<CanvasGroup>();
                }
            }
            return slideIndicatorsCanvasGroup;
        }

        /// <summary>
        /// Safely stops a coroutine and nulls its reference
        /// </summary>
        private void StopAnimation(ref Coroutine animation)
        {
            if (animation != null)
            {
                StopCoroutine(animation);
                animation = null;
            }
        }
        #endregion
    }
}