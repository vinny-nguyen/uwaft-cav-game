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
    
    // Track active animations to prevent overlapping
    private bool _isShaking;
    private Coroutine _popCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        
        // Initialize config if not assigned
        if (!mapConfig) mapConfig = MapConfig.Instance;
    }
    
    // Configuration Helpers - Cleaner pattern with single method
    private T GetConfigValue<T>(System.Func<MapConfig, T> configGetter, T fallback)
    {
        return mapConfig ? configGetter(mapConfig) : fallback;
    }
    
    private float GetScaleUp() => GetConfigValue(c => c.popScaleUp, scaleUp);
    private float GetDuration() => GetConfigValue(c => c.popDuration, duration);
    private float GetShakeDuration() => GetConfigValue(c => c.shakeDuration, shakeDuration);
    private float GetShakeMagnitude() => GetConfigValue(c => c.shakeMagnitude, shakeMagnitude);

    /// <summary>
    /// Play shake animation (when node is locked).
    /// Safe to call multiple times - won't stack.
    /// </summary>
    public void PlayShake()
    {
        if (!_isShaking)
            StartCoroutine(ShakeRoutine());
    }

    /// <summary>
    /// Play pop animation (when node state changes).
    /// Cancels previous pop if still running.
    /// </summary>
    public void PlayPop()
    {
        if (_popCoroutine != null)
            StopCoroutine(_popCoroutine);
        
        _popCoroutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        _isShaking = true;

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

        _isShaking = false;
    }

    private IEnumerator PopRoutine()
    {
        float elapsed = 0f;
        float configScaleUp = GetScaleUp();
        float configDuration = GetDuration();
        Vector3 targetScale = originalScale * configScaleUp;

        // Scale up
        while (elapsed < configDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / configDuration);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Scale down
        elapsed = 0f;
        while (elapsed < configDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / configDuration);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        _popCoroutine = null;
    }
}
