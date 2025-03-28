using UnityEngine;

public class WheelLogic : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeedMultiplier = 100f;
    private float _currentRotationSpeed;

    void Update()
    {
        // Rotate continuously based on current speed
        transform.Rotate(Vector3.forward, _currentRotationSpeed * Time.deltaTime);
    }

    public void SetRotationSpeed(float movementSpeed)
    {
        // Convert movement speed to wheel rotation speed
        _currentRotationSpeed = -movementSpeed * rotationSpeedMultiplier;
    }
}