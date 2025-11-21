using UnityEngine;
using Nodemap;

/// <summary>
/// Centralized service locator for the nodemap system.
/// Provides fast, single-point access to commonly-needed services
/// instead of repeatedly searching the scene with FindFirstObjectByType.
/// 
/// Usage: GameServices.Instance.MapController.CompleteNode(0);
/// </summary>
public class GameServices : MonoBehaviour
{
    private static GameServices _instance;
    
    /// <summary>
    /// Singleton instance accessible from anywhere.
    /// </summary>
    public static GameServices Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameServices>();
                if (_instance == null)
                {
                    Debug.LogError("[GameServices] No GameServices instance found in scene! Please add a GameObject with GameServices component.");
                }
            }
            return _instance;
        }
    }

    // Core nodemap services
    public MapControllerSimple MapController { get; private set; }
    public NodeManagerSimple NodeManager { get; private set; }
    public PopupController PopupController { get; private set; }
    public QuizCompletionHandler QuizCompletionHandler { get; private set; }
    
    // Visual services
    public CarVisual CarVisual { get; private set; }
    
    // Score services
    public ScoreManager ScoreManager { get; private set; }
    public TotalScoreUploader ScoreUploader { get; private set; }

    private void Awake()
    {
        // Enforce singleton pattern
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("[GameServices] Duplicate GameServices instance found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // Register all services once at startup
        RegisterServices();
        
        // Validate that critical services were found
        ValidateServices();
    }

    private void RegisterServices()
    {
        Debug.Log("[GameServices] Registering services...");
        
        // Core nodemap systems
        MapController = FindFirstObjectByType<MapControllerSimple>();
        NodeManager = FindFirstObjectByType<NodeManagerSimple>();
        PopupController = FindFirstObjectByType<PopupController>();
        QuizCompletionHandler = FindFirstObjectByType<QuizCompletionHandler>();
        
        // Visual systems
        CarVisual = FindFirstObjectByType<CarVisual>();
        
        // Score systems
        ScoreManager = ScoreManager.Instance; // Already a singleton
        ScoreUploader = FindFirstObjectByType<TotalScoreUploader>();
        
        Debug.Log("[GameServices] Service registration complete.");
    }

    private void ValidateServices()
    {
        // Critical services - log errors if missing
        if (MapController == null)
            Debug.LogError("[GameServices] MapControllerSimple not found in scene!");
        
        if (NodeManager == null)
            Debug.LogError("[GameServices] NodeManagerSimple not found in scene!");
        
        if (PopupController == null)
            Debug.LogError("[GameServices] PopupController not found in scene!");
        
        // Optional services - log warnings if missing
        if (QuizCompletionHandler == null)
            Debug.LogWarning("[GameServices] QuizCompletionHandler not found - quiz completion may not work.");
        
        if (CarVisual == null)
            Debug.LogWarning("[GameServices] CarVisual not found - car upgrades may not work.");
        
        if (ScoreManager == null)
            Debug.LogWarning("[GameServices] ScoreManager not found - scoring may not work.");
        
        if (ScoreUploader == null)
            Debug.LogWarning("[GameServices] TotalScoreUploader not found - score upload may not work.");
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
