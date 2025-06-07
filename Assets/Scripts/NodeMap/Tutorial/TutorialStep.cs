using UnityEngine;

namespace NodeMap.Tutorial
{
    /// <summary>
    /// Represents a single step in the tutorial sequence
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        public string Message;
        public Transform Target;
        public bool IsUIElement;
        public Vector2 ArrowOffset = Vector2.zero;
        public float ArrowRotation = 0f;

        public TutorialStep(string message, Transform target, bool isUIElement = false)
            : this(message, target, isUIElement, Vector2.zero, 0f) { }

        public TutorialStep(string message, Transform target, bool isUIElement, Vector2 arrowOffset)
            : this(message, target, isUIElement, arrowOffset, 0f) { }

        public TutorialStep(string message, Transform target, bool isUIElement, Vector2 arrowOffset, float arrowRotation)
        {
            Message = message;
            Target = target;
            IsUIElement = isUIElement;
            ArrowOffset = arrowOffset;
            ArrowRotation = arrowRotation;
        }
    }
}
