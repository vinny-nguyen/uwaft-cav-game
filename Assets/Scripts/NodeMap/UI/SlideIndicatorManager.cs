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
        [SerializeField] private float scaleAmount = 0.1f;
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

            // Initialize with the first dot active, without animation, and set lastActiveIndex
            UpdateIndicatorsImmediate(0);
            this.lastActiveIndex = 0;
        }

        /// <summary>
        /// Updates the active indicator
        /// </summary>
        public void UpdateActiveIndicator(int activeIndex, bool animate = true)
        {
            if (spawnedDots.Count == 0) return;

            // If it's the same index and no transition animation is running,
            // just ensure breathing is on the correct dot.
            if (activeIndex == lastActiveIndex && animationCoroutine == null)
            {
                StartBreathingAnimation(activeIndex); // Ensures breathing is active
                return;
            }

            // Only proceed if the index is actually changing
            if (activeIndex != lastActiveIndex)
            {
                StopAnimation(ref animationCoroutine); // Stop any ongoing transition

                if (animate)
                {
                    // Pass the current lastActiveIndex to the animation coroutine
                    animationCoroutine = StartCoroutine(AnimateIndicatorTransition(activeIndex, this.lastActiveIndex));
                }
                else
                {
                    UpdateIndicatorsImmediate(activeIndex);
                }
                this.lastActiveIndex = activeIndex; // Update lastActiveIndex after initiating change
            }
            // If activeIndex == lastActiveIndex but an animation is running, let it complete.
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
        private IEnumerator AnimateIndicatorTransition(int newActiveIndex, int oldActiveIndex)
        {
            float halfDuration = transitionDuration * 0.5f;
            Vector3 fullScale = Vector3.one;
            Vector3 shrunkScale = Vector3.one * scaleAmount; // Use scaleAmount for consistency, not 0.8f directly
            Transform parentTransform = slideIndicatorsParent;

            if (oldActiveIndex >= 0 && oldActiveIndex < spawnedDots.Count && spawnedDots[oldActiveIndex] != null)
            {
                StopAnimation(ref activeDotBreathing);
                Transform oldDotVisual = spawnedDots[oldActiveIndex].transform.Find("DotVisual");
                if (oldDotVisual != null) oldDotVisual.localScale = Vector3.one;
            }

            // Animate parent from its current scale to shrunkScale using UIAnimator.AnimateScale
            yield return UIAnimator.AnimateScale(parentTransform, parentTransform.localScale, shrunkScale, halfDuration);

            // Update sprites to new state (while scaled down)
            for (int i = 0; i < spawnedDots.Count; i++)
            {
                GameObject dotContainer = spawnedDots[i];
                if (dotContainer == null) continue;
                Transform dotVisual = dotContainer.transform.Find("DotVisual");
                if (dotVisual == null) continue;
                Image img = dotVisual.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = (i == newActiveIndex) ? activeDotSprite : inactiveDotSprite;
                }
                dotVisual.localScale = Vector3.one; 
            }

            // Animate parent from shrunkScale back to fullScale using UIAnimator.AnimateScale
            yield return UIAnimator.AnimateScale(parentTransform, shrunkScale, fullScale, halfDuration);

            StartBreathingAnimation(newActiveIndex);
            // lastActiveIndex will be updated by the caller (UpdateActiveIndicator)
        }

        /// <summary>
        /// Animates the transition between indicator states
        /// </summary>
        private IEnumerator AnimateIndicatorTransition(int newActiveIndex)
        {
            // Simply update the indicators immediately without any scaling or fading
            UpdateIndicatorsImmediate(newActiveIndex);

            // Return completed coroutine
            yield break;
        }

        /// <summary>
        /// Animates the visibility change
        /// </summary>
        private IEnumerator AnimateVisibility(bool visible)
        {
            float targetAlpha = visible ? 1f : 0f;
            Vector3 targetScale = visible ? Vector3.one : Vector3.one * scaleAmount;

            // Use the new UIAnimator.AnimateGroupScaleAndFade
            yield return UIAnimator.AnimateGroupScaleAndFade(
                slideIndicatorsCanvasGroup,
                slideIndicatorsParent, // Pass the transform to scale
                targetScale,
                targetAlpha,
                transitionDuration
            );

            if (!visible) slideIndicatorsParent.gameObject.SetActive(false);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Updates indicators immediately without animation
        /// </summary>
        private void UpdateIndicatorsImmediate(int newActiveIndex)
        {
            // Stop current breathing on the old active dot (this.lastActiveIndex)
            // and reset its scale before changing sprites.
            if (this.lastActiveIndex >= 0 && this.lastActiveIndex < spawnedDots.Count && spawnedDots[this.lastActiveIndex] != null)
            {
                StopAnimation(ref activeDotBreathing);
                Transform oldDotVisual = spawnedDots[this.lastActiveIndex].transform.Find("DotVisual");
                if (oldDotVisual != null) oldDotVisual.localScale = Vector3.one;
            }

            for (int i = 0; i < spawnedDots.Count; i++)
            {
                GameObject dotContainer = spawnedDots[i];
                if (dotContainer == null) continue;

                Transform dotVisual = dotContainer.transform.Find("DotVisual");
                if (dotVisual == null) continue;

                Image img = dotVisual.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = (i == newActiveIndex) ? activeDotSprite : inactiveDotSprite;
                }
                dotVisual.localScale = Vector3.one; // Reset scale on all dots
            }

            StartBreathingAnimation(newActiveIndex);
            // Note: this.lastActiveIndex is updated by the calling UpdateActiveIndicator method
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
                    // Call the new UIAnimator.BreatheElement
                    activeDotBreathing = StartCoroutine(UIAnimator.BreatheElement(dotVisual, breatheDuration, breatheMagnitude, Vector3.one));
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