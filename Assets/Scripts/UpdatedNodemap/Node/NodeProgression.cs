using System.Collections.Generic;

namespace Nodemap
{
    /// <summary>
    /// Tracks the unlock and completion state of each node in the nodemap.
    /// </summary>
    public class NodeProgression
    {
        private readonly int _nodeCount;
        private readonly bool[] _unlocked;
        private readonly bool[] _completed;

        public NodeProgression(int nodeCount)
        {
            _nodeCount = nodeCount;
            _unlocked = new bool[nodeCount];
            _completed = new bool[nodeCount];
            _unlocked[0] = true; // Node 1 is always unlocked
        }

        public bool IsUnlocked(int index) => _unlocked[index];
        public bool IsCompleted(int index) => _completed[index];

        public void CompleteNode(int index)
        {
            if (index < 0 || index >= _nodeCount) return;
            _completed[index] = true;
            UnlockNextNode(index);
        }

        private void UnlockNextNode(int index)
        {
            int next = index + 1;
            if (next < _nodeCount)
            {
                // Only unlock if all previous nodes are completed
                for (int i = 0; i <= index; i++)
                {
                    if (!_completed[i]) return;
                }
                _unlocked[next] = true;
            }
        }

        public void ResetProgress()
        {
            for (int i = 0; i < _nodeCount; i++)
            {
                _unlocked[i] = false;
                _completed[i] = false;
            }
            _unlocked[0] = true;
        }
    }
}
