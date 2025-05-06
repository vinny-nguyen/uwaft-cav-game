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
    [SerializeField] float _Required_Flip_Duration = 2.0f;

    private float _Flip_Timer = 0f;
    private float _Move_Input;
    public bool _Can_Control = true;

    void DetectFlip() {
        float dot = Vector2.Dot(transform.up, Vector2.up); // Dot product between up-direction & world up

        // Checking if the car is inverted beyond threshold:
        if (dot < Mathf.Cos(180f * Mathf.Deg2Rad)) { // dot < -1: Dot product < - 1 meaning full inversion
            _Flip_Timer += Time.deltaTime;
            if (_Flip_Timer >= _Required_Flip_Duration) {
                if (_Can_Control) {
                    _Can_Control = false;
                    GameManager.instance.GameOver();
                }
            }
        } else {
            _Flip_Timer = 0f;
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