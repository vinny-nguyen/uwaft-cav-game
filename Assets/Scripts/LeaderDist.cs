using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class LeaderDist : MonoBehaviour
{
    [Header("Car Controls")]
    [SerializeField] private Rigidbody2D _Front_TireRB;
    [SerializeField] private Rigidbody2D _Rear_TireRB;
    [SerializeField] private Rigidbody2D _CarRB;
    [SerializeField] private float _Speed = 50f;
    [SerializeField] private float _Rotation_Speed = 100f;
    private float _Move_Input;
    public bool _Can_Control = true;

    [Header("Distance Tracking")]
    private Vector2 _lastPosition;
    [SerializeField] private float _totalDistance;
    [SerializeField] private float _distanceUpdateInterval = 5f;
    private float _nextUpdateThreshold;

    [Header("Leaderboard")]
    [SerializeField] private string _leaderboardId = "UWAFT_CAV_Game";
    private bool _isInitialized = false;

    private async void Start()
    {
        _lastPosition = _CarRB.position;

        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _isInitialized = true;
            Debug.Log("Leaderboard services initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
            Debug.Log($"Is Signed In: {AuthenticationService.Instance.IsSignedIn}");
            Debug.LogError($"Initialization failed: {e.Message}");
        }
    }

    private void Update()
    {
        if (!_Can_Control || !_isInitialized) return;

        _Move_Input = Input.GetAxisRaw("Horizontal");
        TrackDistance();
    }

    private void FixedUpdate()
    {
        _Front_TireRB.AddTorque(-_Move_Input * _Speed * Time.fixedDeltaTime);
        _Rear_TireRB.AddTorque(-_Move_Input * _Speed * Time.fixedDeltaTime);
        _CarRB.AddTorque(-_Move_Input * _Rotation_Speed * Time.fixedDeltaTime);
    }

    //private void TrackDistance()
    //{
    //    float distanceThisFrame = Vector2.Distance(_CarRB.position, _lastPosition);
    //    _totalDistance += distanceThisFrame;
    //    _lastPosition = _CarRB.position;

    //    if (_totalDistance >= _nextUpdateThreshold)
    //    {
    //        _ = SubmitDistanceToLeaderboard();
    //        _nextUpdateThreshold = _totalDistance + _distanceUpdateInterval;
    //    }
    //}

    private void TrackDistance()
    {
        Vector2 currentPosition = _CarRB.position;
        Vector2 movementVector = currentPosition - _lastPosition;

        // Use the car's right vector as the forward direction (adjust if needed)
        Vector2 forwardDirection = _CarRB.transform.right.normalized;

        // Project movement onto forward direction
        float signedDistance = Vector2.Dot(movementVector, forwardDirection);

        _totalDistance += signedDistance;
        _totalDistance = Mathf.Max(0f, _totalDistance); // Prevent negative total distance
        _lastPosition = currentPosition;

        if (_totalDistance >= _nextUpdateThreshold)
        {
            _ = SubmitDistanceToLeaderboard();
            _nextUpdateThreshold = _totalDistance + _distanceUpdateInterval;
        }
    }

    private async Task SubmitDistanceToLeaderboard()
    {
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(_leaderboardId, _totalDistance);
            Debug.Log($"Successfully submitted score: {_totalDistance}");

            // NEW: update local best distance
            int roundedDistance = Mathf.RoundToInt(_totalDistance);
            if (roundedDistance > PlayerProfile.BestDistance)
            {
                PlayerProfile.BestDistance = roundedDistance;
                PlayerProfile.Save();
                Debug.Log($"[LeaderDist] New best distance stored: {roundedDistance}");
            }

            // (Optional) Verify by fetching the leaderboard...
            var scores = await LeaderboardsService.Instance.GetScoresAsync(_leaderboardId);
            Debug.Log($"Leaderboard contains {scores.Results.Count} entries");

            bool scoreFound = false;
            foreach (var entry in scores.Results)
            {
                if (entry.PlayerId == AuthenticationService.Instance.PlayerId)
                {
                    Debug.Log($"Your score: {entry.Score} (Rank: {entry.Rank})");
                    scoreFound = true;
                    break;
                }
            }

            if (!scoreFound)
            {
                Debug.LogWarning("Your score doesn't appear in the leaderboard yet. It may take a moment to update.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Leaderboard operation failed: {e.Message}");
        }
    }


    public async void SubmitFinalDistance()
    {
        Debug.Log("[LeaderDist] SubmitFinalDistance() called.");
        Debug.Log($"[LeaderDist] BestDistance BEFORE upload: {PlayerProfile.BestDistance}");

        if (!_isInitialized) return;

        await SubmitDistanceToLeaderboard();

        // Also push new overall score through GameServices
        var uploader = GameServices.Instance?.ScoreUploader;
        Debug.Log($"[LeaderDist] GameServices.ScoreUploader found? {uploader != null}");
        if (uploader != null)
        {
            await uploader.UploadScoreAsync();
            Debug.Log("[LeaderDist] Overall score uploaded after distance run.");
        }
        else
        {
            Debug.LogWarning("[LeaderDist] GameServices.ScoreUploader is null. Overall score not uploaded.");
        }
    }

}