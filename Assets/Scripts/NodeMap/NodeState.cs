namespace NodeMap
{
    /// <summary>
    /// Represents the possible states of a node in the map
    /// </summary>
    public enum NodeState
    {
        Normal,   // Gray - not yet accessible
        Active,   // Yellow - currently active
        Complete  // Green - completed
    }
}