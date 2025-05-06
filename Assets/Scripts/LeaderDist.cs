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

    private void TrackDistance()
    {
        float distanceThisFrame = Vector2.Distance(_CarRB.position, _lastPosition);
        _totalDistance += distanceThisFrame;
        _lastPosition = _CarRB.position;

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
            // Submit the score
            await LeaderboardsService.Instance.AddPlayerScoreAsync(_leaderboardId, _totalDistance);
            Debug.Log($"Successfully submitted score: {_totalDistance}");

            // Verify by fetching the leaderboard
            var scores = await LeaderboardsService.Instance.GetScoresAsync(_leaderboardId);
            Debug.Log($"Leaderboard contains {scores.Results.Count} entries");

            // Check if our score appears in the results
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
        if (!_isInitialized) return;
        await SubmitDistanceToLeaderboard();
    }
}