using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]

public class EnvironmentGenerator : MonoBehaviour
{
    public SpriteShapeController _Sprite_Shape_Controller;

    [Range(3f, 100f)] public int _Level_Length = 50; // Range for Level Length
    [Range(1f, 50f)] public float _X_Multiplier = 2f; // Stretches the Width (x-direction) of the map/level
    [Range(1f, 50f)] public float _Y_Multiplier = 2f; // Stretches the Height (y-direction) of the map/level
    [Range(0f, 1f)] public float _Curve_Smoothness = 0.5f; // Smoothness of the curves and edges on the terrain
    public float _Noise_Step = 0.5f; // Constantly randomizes map generation
    public float _Bottom = 10f; // How thick the ground is

    private Vector3 _Last_Position; // Vector3 constructor for Vector3(float x, float y, float z)
    public void OnValidate() {

        _Sprite_Shape_Controller.spline.Clear();

        for (int i = 0; i < _Level_Length; i++) {
            // Uses Perlin Noise for some randomness
            _Last_Position = transform.position + new Vector3(i * _X_Multiplier, Mathf.PerlinNoise(0, i * _Noise_Step) * _Y_Multiplier);
            _Sprite_Shape_Controller.spline.InsertPointAt(i, _Last_Position);

            if (i != 0 && i != _Level_Length - 1) { // if the point isn't the Top Left or Top Right point then continue

                _Sprite_Shape_Controller.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                _Sprite_Shape_Controller.spline.SetLeftTangent(i, Vector3.left * _X_Multiplier * _Curve_Smoothness);
                _Sprite_Shape_Controller.spline.SetRightTangent(i, Vector3.right * _X_Multiplier * _Curve_Smoothness);
            }
        }

        _Sprite_Shape_Controller.spline.InsertPointAt(_Level_Length, new Vector3(_Last_Position.x, transform.position.y - _Bottom));

        _Sprite_Shape_Controller.spline.InsertPointAt(_Level_Length + 1, new Vector3(transform.position.x, transform.position.y - _Bottom));
    }
}