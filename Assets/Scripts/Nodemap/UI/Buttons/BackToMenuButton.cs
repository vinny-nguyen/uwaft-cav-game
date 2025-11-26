using UnityEngine;
using UnityEngine.UI;
using Nodemap.UI.Menus;

namespace Nodemap.UI
{
    /// <summary>
    /// Hamburger menu button that opens a popup menu with multiple navigation options
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class HamburgerMenuButton : MonoBehaviour
    {
        [SerializeField] private HamburgerMenuController menuController;
        
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClicked);
            
            // Try to find menu controller if not assigned
            if (menuController == null)
            {
                menuController = FindFirstObjectByType<HamburgerMenuController>();
                
                if (menuController == null)
                {
                    Debug.LogError("[HamburgerMenuButton] No HamburgerMenuController found in scene! Please assign one.");
                }
            }
        }
        
        private void OnButtonClicked()
        {
            if (menuController != null)
            {
                menuController.ToggleMenu();
            }
        }
    }
}
