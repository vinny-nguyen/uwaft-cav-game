using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

/// <summary>
/// Moves the car along a spline path with smooth movement, bounce, and wheel spin animation.
/// </summary>
public class CarPathFollower : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private MapConfig mapConfig;
    
    [SerializeField] private SplineContainer spline;     // World spline reference
    
    [Header("Movement Settings - Overridden by MapConfig")]
    [SerializeField] private float moveSpeed = 2f;       // Units per second
    [SerializeField] private float minMoveDuration = 0.1f;
    
    [Header("Animation Settings - Overridden by MapConfig")]
    [SerializeField] private float bounceFrequency = 5f;
    [SerializeField] private float bounceAmplitude = 0.05f;
    [SerializeField] private float smoothRotateDuration = 0.3f;

    [Header("Wheel Spin Animation - Overridden by MapConfig")]
    [SerializeField] private Transform tireFront;
    [SerializeField] private Transform tireRear;
    [SerializeField] private float spinSpeed = 360f;
    private bool spinning = false;
    private int spinDirection = 1;
    public float NormalizedT => t;
    private float t;

    // Public accessors for movement parameters
    public float MoveSpeed => GetMoveSpeed();
    public float MinMoveDuration => GetMinMoveDuration();
    
    // Configuration Helpers
    private float GetMoveSpeed() => mapConfig ? mapConfig.moveSpeed : moveSpeed;
    private float GetMinMoveDuration() => mapConfig ? mapConfig.minMoveDuration : minMoveDuration;
    private float GetBounceFrequency() => mapConfig ? mapConfig.carBounceFrequency : bounceFrequency;
    private float GetBounceAmplitude() => mapConfig ? mapConfig.carBounceAmplitude : bounceAmplitude;
    private float GetSpinSpeed() => mapConfig ? mapConfig.wheelSpinSpeed : spinSpeed;
    
    private void Awake()
    {
        // Initialize config if not assigned
        if (!mapConfig) mapConfig = MapConfig.Instance;
    }

    /// <summary>
    /// Instantly snaps the car to a normalized position on the spline.
    /// </summary>
    public void SnapTo(float tNorm)
    {
        t = Mathf.Clamp01(tNorm);
        var pos = spline.EvaluatePosition(t);
        transform.position = pos;
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Smoothly moves the car to a target normalized position on the spline.
    /// </summary>
    public void MoveTo(float targetT)
    {
        StopAllCoroutines();
        // Set spin direction based on movement
        spinDirection = (targetT >= t) ? 1 : -1;
        StartCoroutine(MoveAlong(t, Mathf.Clamp01(targetT)));
    }

    public void StartSpinning()
    {
        spinning = true;
    }

    public void StopSpinning()
    {
        spinning = false;
    }

    public IEnumerator MoveAlong(float startT, float endT, bool eased = true)
    {
        if (spline == null)
        {
            Debug.LogError("Cannot move - spline is null!");
            yield break;
        }

        // Set spin direction based on movement
        spinDirection = (endT >= startT) ? 1 : -1;
        StartSpinning();

        Vector3 startPos = spline.EvaluatePosition(startT);
        Vector3 endPos = spline.EvaluatePosition(endT);
        float distance = Vector3.Distance(startPos, endPos);
        float duration = Mathf.Max(distance / GetMoveSpeed(), GetMinMoveDuration());

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float interp = eased ? SmoothStep(progress) : progress;
            float splineT = Mathf.Lerp(startT, endT, interp);

            // Update position with bounce
            Vector3 worldPos = spline.EvaluatePosition(splineT);
            worldPos.y += Mathf.Sin(Time.time * GetBounceFrequency()) * GetBounceAmplitude(); // Add bounce
            transform.position = worldPos;

            // Update rotation to follow spline tangent
            var tangent3 = spline.EvaluateTangent(splineT);
            Vector3 tangent = new Vector3(tangent3.x, tangent3.y, tangent3.z).normalized;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);

            t = splineT;
            yield return null;
        }

        // Ensure final position and rotation are precise
        UpdatePlayerPosition(endT);
        t = endT;
        StopSpinning();
    }

    private void UpdatePlayerPosition(float splineT)
    {
        Vector3 worldPos = spline.EvaluatePosition(splineT);
        worldPos.y += Mathf.Sin(Time.time * GetBounceFrequency()) * GetBounceAmplitude();
        transform.position = worldPos;
    }

    // Smoothstep function for animation
    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    // --- Wheel Spin Animation ---
    private void Update()
    {
        if (!spinning) return;
        float dt = Time.deltaTime * GetSpinSpeed() * spinDirection;
        if (tireFront != null) tireFront.Rotate(0f, 0f, -dt);
        if (tireRear != null) tireRear.Rotate(0f, 0f, -dt);
    }
}
