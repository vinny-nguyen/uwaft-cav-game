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

        // Make zone visually invisible at runtime (set to transparent)
        // Change the alpha (4th value) to make visible: 0 = invisible, 0.3 = semi-transparent
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img)
        {
            img.color = new Color(1, 0, 0, 0); // Red but fully transparent (invisible)
            // CRITICAL: Enable raycast target so the zone can be detected
            img.raycastTarget = true;
        }
    }

    public void LockToZone(RectTransform item)
    {
        IsLocked = true;
        item.SetParent(_rt);
        // Center the item in the drop zone with optional offset
        item.anchoredPosition = Vector2.zero + _snapOffset;
        // Reset anchors to center for proper centering
        item.anchorMin = new Vector2(0.5f, 0.5f);
        item.anchorMax = new Vector2(0.5f, 0.5f);
        item.pivot = new Vector2(0.5f, 0.5f);
    }
}
