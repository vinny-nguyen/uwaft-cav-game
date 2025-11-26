using UnityEngine;

// ScriptableObject that centralizes all tunables for the map system
[CreateAssetMenu(fileName = "MapConfig", menuName = "Game/MapConfig")]
public class MapConfig : ScriptableObject
{
    [Header("Car Movement")]
    [Tooltip("Speed of car movement along the spline (units per second)")]
    public float moveSpeed = 2f;
    
    [Tooltip("Minimum duration for any car movement")]
    public float minMoveDuration = 0.1f;

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
    [Tooltip("Wheel spin speed (degrees per second)")]
    public float wheelSpinSpeed = 360f;

    // Returns a singleton instance of MapConfig (creates one with defaults if not found)
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

    // Validates configuration values on load to ensure they're within reasonable ranges
    private void OnValidate()
    {
        // Ensure positive values for durations and speeds
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        minMoveDuration = Mathf.Max(0.01f, minMoveDuration);
        popupFadeDuration = Mathf.Max(0.01f, popupFadeDuration);
        slideTransitionDuration = Mathf.Max(0.01f, slideTransitionDuration);
        wheelSpinSpeed = Mathf.Max(1f, wheelSpinSpeed);
        
        // Ensure tStart comes before tEnd
        if (tStart >= tEnd)
        {
            tEnd = Mathf.Min(1f, tStart + 0.1f);
        }
    }
}