using System;

namespace NodeMap
{
    /// <summary>
    /// Central event system for node state changes
    /// </summary>
    public static class NodeEvents
    {
        public static event Action<int> OnCurrentNodeChanged;
        public static event Action<int> OnNodeCompleted;

        public static void RaiseCurrentNodeChanged(int nodeIndex)
        {
            OnCurrentNodeChanged?.Invoke(nodeIndex);
        }

        public static void RaiseNodeCompleted(int nodeIndex)
        {
            OnNodeCompleted?.Invoke(nodeIndex);
        }
    }
}
