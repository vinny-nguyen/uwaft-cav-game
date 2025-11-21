namespace Nodemap.Core
{
    // Centralized PlayerPrefs key constants for map progression state
    public static class PlayerPrefsKeys
    {
        // Node state keys (use methods for node-specific keys)
        private const string NodeUnlockedPrefix = "NodeUnlocked_";
        private const string NodeCompletedPrefix = "NodeCompleted_";
        
        // Car position key
        public const string CurrentCarNode = "CurrentCarNode";
        
        // Gets the PlayerPrefs key for a node's unlocked state
        public static string NodeUnlocked(int nodeIndex) => $"{NodeUnlockedPrefix}{nodeIndex}";
        
        // Gets the PlayerPrefs key for a node's completed state
        public static string NodeCompleted(int nodeIndex) => $"{NodeCompletedPrefix}{nodeIndex}";
    }
}
