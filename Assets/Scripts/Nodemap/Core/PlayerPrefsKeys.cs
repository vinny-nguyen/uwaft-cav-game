namespace Nodemap.Core
{
    /// <summary>
    /// Centralized PlayerPrefs key constants to prevent typos and ensure consistency.
    /// All map progression state is saved using these keys.
    /// </summary>
    public static class PlayerPrefsKeys
    {
        // Node state keys (use methods for node-specific keys)
        private const string NodeUnlockedPrefix = "NodeUnlocked_";
        private const string NodeCompletedPrefix = "NodeCompleted_";
        
        // Car position key
        public const string CurrentCarNode = "CurrentCarNode";
        
        /// <summary>
        /// Gets the PlayerPrefs key for a node's unlocked state.
        /// </summary>
        /// <param name="nodeIndex">The zero-based node index</param>
        /// <returns>The PlayerPrefs key string (e.g., "NodeUnlocked_0")</returns>
        public static string NodeUnlocked(int nodeIndex) => $"{NodeUnlockedPrefix}{nodeIndex}";
        
        /// <summary>
        /// Gets the PlayerPrefs key for a node's completed state.
        /// </summary>
        /// <param name="nodeIndex">The zero-based node index</param>
        /// <returns>The PlayerPrefs key string (e.g., "NodeCompleted_0")</returns>
        public static string NodeCompleted(int nodeIndex) => $"{NodeCompletedPrefix}{nodeIndex}";
    }
}
