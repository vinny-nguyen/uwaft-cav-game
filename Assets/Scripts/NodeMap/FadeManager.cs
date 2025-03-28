using UnityEngine;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public CanvasGroup popupCanvasGroup; // Assign the CanvasGroup of the PopupCanvas
    public float fadeDuration = 0.5f; // Duration of the fade animation

    // Coroutine to fade the popup in or out
    public IEnumerator Fade(float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Fade the entire popup canvas
            popupCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        // Ensure the final alpha value is set
        popupCanvasGroup.alpha = endAlpha;

        // Invoke the completion callback (if provided)
        onComplete?.Invoke();
    }
}