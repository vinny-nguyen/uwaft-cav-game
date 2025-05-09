using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveCar: MonoBehaviour {
    [SerializeField] private Rigidbody2D _Front_TireRB;
    [SerializeField] private Rigidbody2D _Rear_TireRB;
    [SerializeField] private Rigidbody2D _CarRB;
    [SerializeField] private float _Speed = 150f;
    [SerializeField] private float _Rotation_Speed = 300f;
    [SerializeField] float _Flip_Angle_Threshold = 0f;
    [SerializeField] float _Required_Flip_Duration = 3.0f;

    private float _Flip_Timer = 0f;
    private float _Move_Input;
    public bool _Can_Control = true;

void DetectFlip() {
    // Calculate the dot product between the car's up direction and the world's up direction
    float dot = Vector2.Dot(transform.up, Vector2.up);
    Debug.Log($"Dot product: {dot}, Flip Timer: {_Flip_Timer}");

    // Check if the car is inverted beyond the threshold
    if (dot < Mathf.Cos(90f * Mathf.Deg2Rad)) { // 90 degrees threshold for being "flipped"
        _Flip_Timer += Time.deltaTime; // Increment the flip timer
        if (_Flip_Timer >= _Required_Flip_Duration) { // Check if the car has been flipped for the required duration
            if (_Can_Control) {
                Debug.Log("Car is flipped for too long. Triggering GameOver.");
                _Can_Control = false; // Disable control
                GameManager.instance.GameOver(); // Trigger GameOver
            }
        }
    } else {
        _Flip_Timer = 0f; // Reset the flip timer if the car is not flipped
    }
}

    private void Update() {

        if (!_Can_Control) return; // Disables input when false
        _Move_Input = Input.GetAxisRaw("Horizontal");
        DetectFlip();
    }

    private void FixedUpdate() {
        _Front_TireRB.AddTorque(-(_Move_Input) * _Speed * Time.fixedDeltaTime); // Torque
        _Rear_TireRB.AddTorque(-(_Move_Input) * _Speed * Time.fixedDeltaTime);
        _CarRB.AddTorque(-(_Move_Input) * _Rotation_Speed * Time.fixedDeltaTime);
    }
}