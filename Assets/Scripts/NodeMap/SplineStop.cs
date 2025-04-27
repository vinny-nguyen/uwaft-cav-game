using UnityEngine;

[System.Serializable]
public class SplineStop
{
    [Range(0f, 1f)] public float splinePercent;
    public Sprite nodeSprite;
    public Vector3 offset = Vector3.zero;
}
