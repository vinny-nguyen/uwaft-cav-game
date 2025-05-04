using UnityEngine;

namespace NodeMap
{
    /// <summary>
    /// Represents a stopping point along a spline path
    /// </summary>
    [System.Serializable]
    public class SplineStop
    {
        [Tooltip("Position along the spline (0-1)")]
        [Range(0f, 1f)] 
        public float splinePercent;
        
        [Tooltip("Sprite to display at this node")]
        public Sprite nodeSprite;
        
        [Tooltip("Position offset from the spline path")]
        public Vector3 offset = Vector3.zero;
    }
}
