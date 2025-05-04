using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroDrive : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _Front_TireRB;
    [SerializeField] private Rigidbody2D _Rear_TireRB;
    [SerializeField] private Rigidbody2D _CarRB;
    [SerializeField] private float _Speed = 40f;
    [SerializeField] private float _Rotation_Speed = 100f;
    private float _Move_Input;
    public bool _Can_Control = true;

    private void Update()
    {
        if (!_Can_Control) return; // Disables input when false
        _Move_Input = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        _Front_TireRB.AddTorque(-(_Move_Input) * _Speed * Time.fixedDeltaTime); // Torque
        _Rear_TireRB.AddTorque(-(_Move_Input) * _Speed * Time.fixedDeltaTime);
        _CarRB.AddTorque(-(_Move_Input) * _Rotation_Speed * Time.fixedDeltaTime);
    }
}