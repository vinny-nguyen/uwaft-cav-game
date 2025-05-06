using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayDistanceText : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _Distance_Text;
    [SerializeField] private Transform _Player_Transform;

    private Vector2 _Start_Position;

    private void Start() {
        _Start_Position = _Player_Transform.position;
    }

    private void Update() {
        Vector2 distance = (Vector2)_Player_Transform.position - _Start_Position;
        distance.y = 0f;

        if (distance.x < 0) {
            distance.x = 0;
        }

        _Distance_Text.text = distance.x.ToString("F0") + "m";
    }
}