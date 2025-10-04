using UnityEngine;
using System.Collections;

public class NodeStateAnimation : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private MapConfig mapConfig;
    
    [Header("Pop Animation - Overridden by MapConfig")]
    [SerializeField] private float scaleUp = 1.05f;
    [SerializeField] private float duration = 0.2f;

    [Header("Random Shake Settings")]
    [SerializeField] private float randomMin = -0.4f;
    [SerializeField] private float randomMax = 0.4f;

    private Vector3 originalScale;

    [Header("Shake Animation - Overridden by MapConfig")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 5f;
    private bool isShaking;

    private void Awake()
    {
        originalScale = transform.localScale;
        
        // Initialize config if not assigned
        if (!mapConfig) mapConfig = MapConfig.Instance;
    }
    
    // Configuration Helpers
    private float GetScaleUp() => mapConfig ? mapConfig.popScaleUp : scaleUp;
    private float GetDuration() => mapConfig ? mapConfig.popDuration : duration;
    private float GetShakeDuration() => mapConfig ? mapConfig.shakeDuration : shakeDuration;
    private float GetShakeMagnitude() => mapConfig ? mapConfig.shakeMagnitude : shakeMagnitude;

    public IEnumerator Shake()
    {
        if (isShaking) yield break;
        isShaking = true;

        float elapsed = 0f;
        var rect = GetComponent<RectTransform>();
        Vector3 startPos = rect ? (Vector3)rect.anchoredPosition : transform.localPosition;
        float configShakeDuration = GetShakeDuration();
        float configShakeMagnitude = GetShakeMagnitude();

        while (elapsed < configShakeDuration)
        {
            float x = Random.Range(randomMin, randomMax) * configShakeMagnitude;
            float y = Random.Range(randomMin, randomMax) * configShakeMagnitude;
            Vector3 offset = new Vector3(x, y, 0f);

            if (rect) rect.anchoredPosition = (Vector2)startPos + (Vector2)offset;
            else      transform.localPosition = startPos + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rect) rect.anchoredPosition = (Vector2)startPos;
        else      transform.localPosition = startPos;

        isShaking = false;
    }

    public IEnumerator AnimateStateChange(NodeState _)
    {
        // scale pop only; no color tint
        float elapsed = 0f;
        float configScaleUp = GetScaleUp();
        float configDuration = GetDuration();
        Vector3 targetScale = originalScale * configScaleUp;

        while (elapsed < configDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / configDuration);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < configDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / configDuration);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
