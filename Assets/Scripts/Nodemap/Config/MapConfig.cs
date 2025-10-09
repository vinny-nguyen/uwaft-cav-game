using UnityEngine;

/// <summary>
/// ScriptableObject that centralizes all tunables for the map system.
/// Includes speeds, durations, easing parameters, and asset folders.
/// </summary>
[CreateAssetMenu(fileName = "MapConfig", menuName = "Game/MapConfig")]
public class MapConfig : ScriptableObject
{
    [Header("Car Movement")]
    [Tooltip("Speed of car movement along the spline (units per second)")]
    public float moveSpeed = 2f;
    
    [Tooltip("Minimum duration for any car movement")]
    public float minMoveDuration = 0.1f;

    [Header("Node Animation - Pop")]
    [Tooltip("Scale multiplier for node pop animation")]
    public float popScaleUp = 1.05f;
    
    [Tooltip("Duration for node pop animation (in and out)")]
    public float popDuration = 0.2f;

    [Header("Node Animation - Shake")]
    [Tooltip("Duration of shake animation when node is locked")]
    public float shakeDuration = 0.2f;
    
    [Tooltip("Magnitude of shake animation")]
    public float shakeMagnitude = 5f;

    [Header("Spline Range")]
    [Tooltip("Normalized start position on spline where first node is placed")]
    [Range(0f, 1f)]
    public float tStart = 0.2f;
    
    [Tooltip("Normalized end position on spline where last node is placed")]
    [Range(0f, 1f)]
    public float tEnd = 0.8f;

    [Header("Game Configuration")]
    [Tooltip("Total number of nodes in the game")]
    [Range(1, 10)]
    public int nodeCount = 6;

    [Header("Asset Paths")]
    [Tooltip("Folder path for node sprite assets")]
    public string nodeSpriteFolder = "Sprites/Nodes";

    [Header("Popup Settings")]
    [Tooltip("Duration for popup fade in/out animations")]
    public float popupFadeDuration = 0.3f;
    
    [Tooltip("Duration for slide transition animations")]
    public float slideTransitionDuration = 0.2f;

    [Header("Car Animation")]
    [Tooltip("Bounce frequency for car animation")]
    public float carBounceFrequency = 5f;
    
    [Tooltip("Bounce amplitude for car animation")]
    public float carBounceAmplitude = 0.05f;
    
    [Tooltip("Duration for car rotation smoothing")]
    public float carRotationDuration = 0.3f;
    
    [Tooltip("Wheel spin speed (degrees per second)")]
    public float wheelSpinSpeed = 360f;

    /// <summary>
    /// Returns a singleton instance of MapConfig. 
    /// If no instance exists, creates one with default values.
    /// </summary>
    private static MapConfig _instance;
    public static MapConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<MapConfig>("Config/MapConfig");
                if (_instance == null)
                {
                    Debug.LogWarning("MapConfig asset not found in Resources/Config/. Using default values. Please create a MapConfig asset.");
                    _instance = CreateInstance<MapConfig>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Validates configuration values on load to ensure they're within reasonable ranges.
    /// </summary>
    private void OnValidate()
    {
        // Ensure positive values for durations and speeds
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        minMoveDuration = Mathf.Max(0.01f, minMoveDuration);
        popDuration = Mathf.Max(0.01f, popDuration);
        shakeDuration = Mathf.Max(0.01f, shakeDuration);
        popupFadeDuration = Mathf.Max(0.01f, popupFadeDuration);
        slideTransitionDuration = Mathf.Max(0.01f, slideTransitionDuration);
        carRotationDuration = Mathf.Max(0.01f, carRotationDuration);
        
        // Ensure positive values for animation parameters
        popScaleUp = Mathf.Max(1f, popScaleUp);
        shakeMagnitude = Mathf.Max(0f, shakeMagnitude);
        carBounceFrequency = Mathf.Max(0.1f, carBounceFrequency);
        carBounceAmplitude = Mathf.Max(0f, carBounceAmplitude);
        wheelSpinSpeed = Mathf.Max(1f, wheelSpinSpeed);
        
        // Ensure tStart comes before tEnd
        if (tStart >= tEnd)
        {
            tEnd = Mathf.Min(1f, tStart + 0.1f);
        }
    }
}