using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveCar: MonoBehaviour
{
    [SerializeField] private Rigidbody2D _Front_TireRB;
    [SerializeField] private Rigidbody2D _Rear_TireRB;
    [SerializeField] private Rigidbody2D _CarRB;
    [SerializeField] private float _speed = 150f;
    [SerializeField] private float _rotationSpeed = 300f;
    private float _moveInput;

    private void Update() {
        _moveInput = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate() {
        _Front_TireRB.AddTorque(-(_moveInput) * _speed * Time.fixedDeltaTime); // Torque
        _Rear_TireRB.AddTorque(-(_moveInput) * _speed * Time.fixedDeltaTime);
        _CarRB.AddTorque(-(_moveInput) * _rotationSpeed * Time.fixedDeltaTime);
    }
}
