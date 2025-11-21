using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Nodemap.Controllers;

namespace Nodemap.UI
{
    // Button that launches the driving experience for completed nodes with driving scenes
    [RequireComponent(typeof(Button))]
    public class PlayDrivingButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Controllers.MapController mapController;
        
        [Header("UI")]
        [SerializeField] private GameObject buttonObject;
        
        private Button button;
        private bool lastVisibilityState;
        
        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnPlayDrivingClicked);
            
            if (mapController == null)
                mapController = GameServices.Instance?.MapController;
                
            if (buttonObject == null)
                buttonObject = gameObject;
        }
        
        private void Start()
        {
            // Force hide button initially until we check state
            if (buttonObject != null)
                buttonObject.SetActive(false);
            
            if (mapController != null)
            {
                mapController.SubscribeToStateChanges(UpdateButtonVisibility);
            }
            
            UpdateButtonVisibility();
        }
        
        private void Update()
        {
            // Check every frame but only update if state changed
            UpdateButtonVisibility();
        }
        
        private void OnDestroy()
        {
            if (mapController != null)
            {
                mapController.UnsubscribeFromStateChanges(UpdateButtonVisibility);
            }
        }
        
        private void UpdateButtonVisibility()
        {
            bool shouldShow = ShouldShowButton();
            
            // Always update on first call, then only update if visibility changed
            if (shouldShow != lastVisibilityState || Time.frameCount < 2)
            {
                if (buttonObject != null)
                {
                    buttonObject.SetActive(shouldShow);
                }
                
                if (button != null)
                {
                    button.interactable = shouldShow;
                }
                
                lastVisibilityState = shouldShow;
            }
        }
        
        private bool ShouldShowButton()
        {
            if (mapController == null)
                return false;
            
            return mapController.IsCarAtCompletedNodeWithDriving();
        }
        
        private void OnPlayDrivingClicked()
        {
            if (mapController == null)
            {
                Debug.LogError("[PlayDrivingButton] MapController is null!");
                return;
            }
            
            string drivingSceneName = mapController.GetCurrentNodeDrivingScene();
            
            if (string.IsNullOrEmpty(drivingSceneName))
            {
                Debug.LogWarning("[PlayDrivingButton] No driving scene assigned to current node!");
                return;
            }
            
            SceneManager.LoadScene(drivingSceneName);
        }
    }
}
