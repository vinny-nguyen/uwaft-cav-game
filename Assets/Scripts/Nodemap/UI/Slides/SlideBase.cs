using UnityEngine;

namespace Nodemap.UI
{
    public class SlideBase : MonoBehaviour
{
    [SerializeField] private string key;   // auto-filled from prefab name
    public string Key => key;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Only set automatically if empty or matches GameObject name
        if (string.IsNullOrEmpty(key) || key == gameObject.name)
        {
            key = gameObject.name;
        }
    }
#endif

    public virtual void OnEnter() {}
    public virtual void OnExit() {}
}
}
