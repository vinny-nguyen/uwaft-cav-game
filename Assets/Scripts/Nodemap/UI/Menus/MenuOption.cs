using UnityEngine;

namespace Nodemap.UI.Menus
{
    /// <summary>
    /// Defines a single menu option with a label and scene to load
    /// </summary>
    [System.Serializable]
    public class MenuOption
    {
        [Tooltip("Display text for the menu button")]
        public string label = "Menu Item";
        
        [Tooltip("Scene name to load when clicked")]
        public string sceneName = "";
    }
}
