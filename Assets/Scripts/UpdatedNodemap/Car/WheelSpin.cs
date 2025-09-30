using UnityEngine;

/// <summary>
/// Spins the car's wheels when enabled.
/// </summary>
public class WheelSpinner : MonoBehaviour
{
    [SerializeField] private Transform tireFront;
    [SerializeField] private Transform tireRear;
    [SerializeField] private float spinSpeed = DefaultSpinSpeed;

    // --- Constants ---
    private const float DefaultSpinSpeed = 360f; // Degrees per second
    /// <summary>
    /// If true, wheels will spin in Update.
    /// </summary>
    public bool spinning;

    private void Update()
    {
        if (!spinning) return;
        float dt = Time.deltaTime * spinSpeed;
    tireFront.Rotate(0f, 0f, -dt);
    tireRear.Rotate(0f, 0f, -dt);
    }

    /// <summary>
    /// Stops the wheels from spinning.
    /// </summary>
    public void StopSpinning()
    {
        spinning = false;
    }
}
