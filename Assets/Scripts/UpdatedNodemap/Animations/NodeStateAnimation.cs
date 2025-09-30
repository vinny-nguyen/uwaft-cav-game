using UnityEngine;
using System.Collections;

/// <summary>
/// Simple scale-up animation for node state transitions.
/// Attach to node GameObject.
/// </summary>
public class NodeStateAnimation : MonoBehaviour
{
    [SerializeField] private float scaleUp = 1.05f; // less scale up
    [SerializeField] private float duration = 0.2f;
    [Header("Random Shake Settings")]
    [SerializeField] private float randomMin = -0.4f;
    [SerializeField] private float randomMax = 0.4f;
    private Vector3 originalScale;
    [Header("Shake Animation")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 5f;
    private bool isShaking = false;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Triggers a minimal shake animation. Call as StartCoroutine(anim.Shake()).
    /// </summary>
    public IEnumerator Shake()
    {
        if (isShaking) yield break;
        isShaking = true;
        float elapsed = 0f;
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 startPos = rect ? (Vector3)rect.anchoredPosition : transform.localPosition;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(randomMin, randomMax) * shakeMagnitude;
            float y = Random.Range(randomMin, randomMax) * shakeMagnitude;
            Vector3 offset = new Vector3(x, y, 0f);
            if (rect)
                rect.anchoredPosition = (Vector2)startPos + (Vector2)offset;
            else
                transform.localPosition = startPos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (rect)
            rect.anchoredPosition = (Vector2)startPos;
        else
            transform.localPosition = startPos;
        isShaking = false;
    }

    /// <summary>
    /// Animates the scale and color for any state change. Call as StartCoroutine(anim.AnimateStateChange(state)).
    /// </summary>
    public IEnumerator AnimateStateChange(NodeState state)
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
