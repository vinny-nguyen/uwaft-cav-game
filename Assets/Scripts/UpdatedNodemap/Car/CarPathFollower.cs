using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Moves the car along a spline path with smooth movement and bounce effect.
/// </summary>
public class CarPathFollower : MonoBehaviour
{
    [SerializeField] private SplineContainer spline;     // World spline reference
    [SerializeField] private float moveSpeed = 2f;       // Units per second
    [SerializeField] private float minMoveDuration = 0.1f;
    [Header("Animation Settings")]
    [SerializeField] private float bounceFrequency = 5f;
    [SerializeField] private float bounceAmplitude = 0.05f;
    [SerializeField] private float smoothRotateDuration = 0.3f;
    private float t;

    /// <summary>
    /// Instantly snaps the car to a normalized position on the spline.
    /// </summary>
    public void SnapTo(float tNorm)
    {
        t = Mathf.Clamp01(tNorm);
        UpdatePlayerPosition(t);
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Smoothly moves the car to a target normalized position on the spline.
    /// </summary>
    public void MoveTo(float targetT)
    {
        StopAllCoroutines();
        StartCoroutine(MoveAlongSpline(t, Mathf.Clamp01(targetT)));
    }

    private System.Collections.IEnumerator MoveAlongSpline(float startT, float endT)
    {
        if (spline == null)
        {
            Debug.LogError("Cannot move - spline is null!");
            yield break;
        }

        Vector3 startPos = spline.EvaluatePosition(startT);
        Vector3 endPos = spline.EvaluatePosition(endT);
        float distance = Vector3.Distance(startPos, endPos);
        float duration = Mathf.Max(distance / moveSpeed, minMoveDuration);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = SmoothStep(progress);
            float splineT = Mathf.Lerp(startT, endT, easedProgress);

            // Update position with bounce
            Vector3 worldPos = spline.EvaluatePosition(splineT);
            worldPos.y += Mathf.Sin(Time.time * bounceFrequency) * bounceAmplitude; // Add bounce
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
        yield return StartCoroutine(SmoothRotateToStraight(0.3f));
        t = endT;

        // Stop wheel spinning when car reaches the node
        var wheelSpinner = GetComponent<WheelSpinner>();
        if (wheelSpinner != null)
        {
            wheelSpinner.StopSpinning();
        }
    }

    private System.Collections.IEnumerator SmoothRotateToStraight(float duration)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.identity;
        float elapsed = 0f;
        while (elapsed < smoothRotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / smoothRotateDuration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        transform.rotation = endRot;
    }

    private void UpdatePlayerPosition(float splineT)
    {
        Vector3 worldPos = spline.EvaluatePosition(splineT);
        worldPos.y += Mathf.Sin(Time.time * bounceFrequency) * bounceAmplitude;
        transform.position = worldPos;
    }

    // Smoothstep function for animation
    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}
