using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

namespace NodeMap
{
    /// <summary>
    /// Handles the actual movement along splines including visual effects
    /// </summary>
    public class SplineMovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float minMoveDuration = 0.1f;

        [Header("Wheel Setup")]
        [SerializeField] private Transform frontWheel;
        [SerializeField] private Transform rearWheel;
        [SerializeField] private float wheelSpinSpeed = 360f;

        [Header("Smoke VFX")]
        [SerializeField] private ParticleSystem smokeParticles;

        private bool isMovingForward = true;

        /// <summary>
        /// Moves the player along the spline from one percentage to another
        /// </summary>
        public IEnumerator MoveAlongSpline(SplineContainer spline, float startT, float endT, bool movingForward)
        {
            isMovingForward = movingForward;

            // Start effects
            if (smokeParticles != null)
            {
                smokeParticles.gameObject.SetActive(true);
                smokeParticles.Play();
            }

            if (spline == null)
            {
                Debug.LogError("Cannot move - spline is null!");
                yield break;
            }

            // Calculate move duration
            Vector3 startPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(startT));
            Vector3 endPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(endT));
            float distance = Vector3.Distance(startPos, endPos);
            float duration = Mathf.Max(distance / moveSpeed, minMoveDuration);

            // Animation loop
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float easedProgress = progress * progress * (3f - 2f * progress); // Smoothstep
                float splineT = Mathf.Lerp(startT, endT, easedProgress);

                // Update position and rotation
                UpdatePlayerPosition(spline, splineT);
                UpdatePlayerRotation(spline, splineT);

                // Rotate wheels
                RotateWheels();

                yield return null;
            }

            // Ensure final position and rotation are precise
            UpdatePlayerPosition(spline, endT);
            UpdatePlayerRotation(spline, endT);

            // Stop effects
            StopMovementEffects();
        }

        public void UpdatePlayerPosition(SplineContainer spline, float splineT)
        {
            Vector3 worldPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(splineT));
            worldPos.y += Mathf.Sin(Time.time * 5f) * 0.05f; // Add bounce
            transform.position = worldPos;
        }

        public void UpdatePlayerRotation(SplineContainer spline, float splineT)
        {
            Vector3 tangent = spline.transform.TransformDirection((Vector3)spline.EvaluateTangent(splineT)).normalized;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);
        }

        private void RotateWheels()
        {
            float direction = isMovingForward ? 1f : -1f;
            if (frontWheel != null)
                frontWheel.Rotate(Vector3.forward, -wheelSpinSpeed * Time.deltaTime * direction);
            if (rearWheel != null)
                rearWheel.Rotate(Vector3.forward, -wheelSpinSpeed * Time.deltaTime * direction);
        }

        private void StopMovementEffects()
        {
            if (smokeParticles != null && smokeParticles.isPlaying)
                smokeParticles.Stop();
        }
    }
}
