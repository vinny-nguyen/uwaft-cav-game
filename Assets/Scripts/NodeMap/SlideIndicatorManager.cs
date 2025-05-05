using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NodeMap.UI
{
    /// <summary>
    /// Manages slide indicators (dots) showing current position in a slide sequence
    /// </summary>
    public class SlideIndicatorManager : MonoBehaviour
    {
        [Header("Indicator Settings")]
        [SerializeField] private GameObject slideDotPrefab;
        [SerializeField] private Transform slideIndicatorsParent;
        [SerializeField] private Sprite activeDotSprite;
        [SerializeField] private Sprite inactiveDotSprite;
        [SerializeField] private CanvasGroup slideIndicatorsCanvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float transitionDuration = 0.2f;
        [SerializeField] private float scaleAmount = 0.9f;
        
        private List<GameObject> spawnedDots = new List<GameObject>();
        private Coroutine activeDotBreathing;
        private int lastActiveIndex = -1;
        private Coroutine animationCoroutine;
        
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
            
            // Stop any ongoing animations
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            if (animate)
            {
                // Animate the indicator change with the same style as slide transitions
                animationCoroutine = StartCoroutine(AnimateIndicatorTransition(activeIndex));
            }
            else
            {
                // Immediately update without animation
                UpdateIndicatorsImmediate(activeIndex);
            }
            
            lastActiveIndex = activeIndex;
        }

        /// <summary>
        /// Animates the transition between indicator states
        /// </summary>
        private IEnumerator AnimateIndicatorTransition(int newActiveIndex)
        {
            // Animate out
            CanvasGroup parentGroup = slideIndicatorsParent.GetComponent<CanvasGroup>();
            if (parentGroup == null)
            {
                parentGroup = slideIndicatorsParent.gameObject.AddComponent<CanvasGroup>();
            }

            // Scale down and fade out slightly
            float time = 0;
            Vector3 originalScale = slideIndicatorsParent.localScale;
            Vector3 smallScale = originalScale * scaleAmount;
            
            while (time < transitionDuration / 2f)
            {
                time += Time.deltaTime;
                float t = time / (transitionDuration / 2f);
                slideIndicatorsParent.localScale = Vector3.Lerp(originalScale, smallScale, t);
                parentGroup.alpha = Mathf.Lerp(1f, 0.7f, t);
                yield return null;
            }
            
            // Update the indicators while scaled down
            UpdateIndicatorsImmediate(newActiveIndex);
            
            // Scale back up and fade in
            time = 0;
            while (time < transitionDuration / 2f)
            {
                time += Time.deltaTime;
                float t = time / (transitionDuration / 2f);
                slideIndicatorsParent.localScale = Vector3.Lerp(smallScale, originalScale, t);
                parentGroup.alpha = Mathf.Lerp(0.7f, 1f, t);
                yield return null;
            }
            
            slideIndicatorsParent.localScale = originalScale;
            parentGroup.alpha = 1f;
            
            // Start breathing on the active dot
            StartBreathingAnimation(newActiveIndex);
        }

        /// <summary>
        /// Updates indicators immediately without animation
        /// </summary>
        private void UpdateIndicatorsImmediate(int activeIndex)
        {
            for (int i = 0; i < spawnedDots.Count; i++)
            {
                GameObject dotContainer = spawnedDots[i];
                Transform dotVisual = dotContainer.transform.Find("DotVisual");
                if (dotVisual == null)
                {
                    Debug.LogWarning("DotVisual child not found!");
                    continue;
                }
                
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
            if (activeIndex >= 0 && activeIndex < spawnedDots.Count)
            {
                if (activeDotBreathing != null)
                {
                    StopCoroutine(activeDotBreathing);
                    activeDotBreathing = null;
                }
                
                Transform dotVisual = spawnedDots[activeIndex].transform.Find("DotVisual");
                if (dotVisual != null)
                {
                    activeDotBreathing = StartCoroutine(BreatheDot(dotVisual));
                }
            }
        }
        
        /// <summary>
        /// Clears all indicator dots
        /// </summary>
        public void ClearIndicators()
        {
            // Stop any animations
            if (activeDotBreathing != null)
            {
                StopCoroutine(activeDotBreathing);
                activeDotBreathing = null;
            }
            
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            
            foreach (var dot in spawnedDots)
            {
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
            if (slideIndicatorsCanvasGroup == null)
            {
                slideIndicatorsCanvasGroup = slideIndicatorsParent.gameObject.AddComponent<CanvasGroup>();
            }
            
            slideIndicatorsParent.gameObject.SetActive(true); // Always activate to allow animation
            
            // Cancel any ongoing animations
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            
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
        
        /// <summary>
        /// Animates the visibility change
        /// </summary>
        private IEnumerator AnimateVisibility(bool visible)
        {
            float targetAlpha = visible ? 1f : 0f;
            float startAlpha = slideIndicatorsCanvasGroup.alpha;
            float time = 0f;
            
            // Also animate scale
            Vector3 startScale = slideIndicatorsParent.localScale;
            Vector3 targetScale = visible ? Vector3.one : Vector3.one * scaleAmount;
            
            while (time < transitionDuration)
            {
                time += Time.deltaTime;
                float t = time / transitionDuration;
                
                slideIndicatorsCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                slideIndicatorsParent.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                yield return null;
            }
            
            slideIndicatorsCanvasGroup.alpha = targetAlpha;
            slideIndicatorsParent.localScale = targetScale;
            
            // Only deactivate after animation if hiding
            if (!visible) slideIndicatorsParent.gameObject.SetActive(false);
        }
        
        private IEnumerator BreatheDot(Transform dotTransform)
        {
            float breatheDuration = 1f;
            float breatheMagnitude = 0.2f;
            float timer = 0f;
            
            while (dotTransform != null && spawnedDots.Contains(dotTransform.parent.gameObject))
            {
                timer += Time.deltaTime;
                float scale = 1f + Mathf.Sin(timer * Mathf.PI * 2f / breatheDuration) * breatheMagnitude;
                dotTransform.localScale = Vector3.one * scale;
                yield return null;
            }
        }
    }
}