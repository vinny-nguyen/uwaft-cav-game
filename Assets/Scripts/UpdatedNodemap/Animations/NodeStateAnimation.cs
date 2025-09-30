using UnityEngine;
using System.Collections;

/// <summary>
/// Simple scale-up animation for node state transitions.
/// Attach to node GameObject.
/// </summary>
public class NodeStateAnimation : MonoBehaviour
{
    [SerializeField] private float scaleUp = 1.2f;
    [SerializeField] private float duration = 0.25f;
    [Header("Random Shake Settings")]
    [SerializeField] private float randomMin = -1f;
    [SerializeField] private float randomMax = 1f;
    private Vector3 originalScale;
    [Header("Shake Animation")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 10f;
    private bool isShaking = false;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Plays a shake animation for locked nodes.
    /// </summary>
    public void PlayLockedShake()
    {
        if (!isShaking)
            StartCoroutine(ShakeNode());
    }

    private IEnumerator ShakeNode()
    {
        isShaking = true;
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(randomMin, randomMax) * shakeMagnitude;
            float y = Random.Range(randomMin, randomMax) * shakeMagnitude;
            transform.localPosition = startPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = startPos;
        isShaking = false;
    }

    /// <summary>
    /// Plays the scale-up animation for any state change.
    /// </summary>
    public void PlayStateChange(NodeState state)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScale(state));
    }

    private IEnumerator AnimateScale(NodeState state)
    {
        float elapsed = 0f;
        Vector3 targetScale = originalScale * scaleUp;
        Color targetColor = Color.white;
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            switch (state)
            {
                case NodeState.Active:
                    targetColor = Color.yellow;
                    break;
                case NodeState.Completed:
                    targetColor = Color.green;
                    break;
                case NodeState.Inactive:
                    targetColor = Color.gray;
                    break;
            }
        }

        Color originalColor = img != null ? img.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            if (img != null) img.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }
        // Return to original scale and color
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            if (img != null) img.color = Color.Lerp(targetColor, originalColor, t);
            yield return null;
        }
        transform.localScale = originalScale;
        if (img != null) img.color = originalColor;
    }
}
