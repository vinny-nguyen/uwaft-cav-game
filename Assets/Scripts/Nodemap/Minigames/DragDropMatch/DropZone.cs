using UnityEngine;

public class DropZone : MonoBehaviour
{
    public string Key { get; private set; }
    public bool IsLocked { get; private set; }

    private RectTransform _rt;
    private Vector2 _snapOffset;

    public void Init(string key, Vector2 snapOffset)
    {
        Key = key;
        _rt = GetComponent<RectTransform>();
        _snapOffset = snapOffset;

        // Make zone visually invisible at runtime (optional)
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img) img.color = new Color(0,0,0,0);
    }

    public void LockToZone(RectTransform item)
    {
        IsLocked = true;
        item.SetParent(_rt);
        item.anchoredPosition = _snapOffset;
    }
}
