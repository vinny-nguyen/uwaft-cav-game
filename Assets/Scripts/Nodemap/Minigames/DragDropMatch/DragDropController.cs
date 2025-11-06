using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragDropController : MonoBehaviour
{
    [Serializable]
    public class Pair
    {
        public string label;                 // e.g., "Tread"
        public string key;                   // e.g., "tread"
        public RectTransform targetRect;     // drop zone on the diagram
        public Vector2 snapOffset;           // small nudge after snap (optional)
    }

    [Header("Config")]
    [SerializeField] private Image diagram;            // optional (for clarity)
    [SerializeField] private RectTransform targetsRoot;
    [SerializeField] private Image targetTemplate;     // disabled template
    [SerializeField] private RectTransform itemsPanel; // where item chips spawn
    [SerializeField] private GameObject winBanner;     // set inactive initially

    [Header("Pairs")]
    [SerializeField] private List<Pair> pairs = new();

    [Header("Item Visuals")]
    [SerializeField] private GameObject itemChipPrefab; // a simple Button + TMP text

    [Header("Events")]
    public UnityEvent OnCompleted; // Hook into your ProgressionController

    private int _lockedCount = 0;
    private readonly Dictionary<string, DropZone> _zonesByKey = new();
    private Canvas _rootCanvas;

    void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
        if (winBanner != null) winBanner.SetActive(false);

        // Build DropZones from targets list
        foreach (var p in pairs)
        {
            if (p.targetRect == null || string.IsNullOrEmpty(p.key)) continue;

            // Clone a target from template (keeps it all prefab-contained)
            var zoneImg = Instantiate(targetTemplate, targetsRoot);
            zoneImg.gameObject.name = $"Target_{p.key}";
            zoneImg.rectTransform.position = p.targetRect.position;
            zoneImg.rectTransform.sizeDelta = p.targetRect.sizeDelta;
            zoneImg.gameObject.SetActive(true);

            var zone = zoneImg.gameObject.AddComponent<DropZone>();
            zone.Init(p.key, p.snapOffset);

            _zonesByKey[p.key] = zone;
        }

        // Spawn Draggable Items
        foreach (var p in pairs)
        {
            if (string.IsNullOrEmpty(p.label) || string.IsNullOrEmpty(p.key)) continue;

            var go = Instantiate(itemChipPrefab, itemsPanel);
            go.name = $"Item_{p.key}";

            var txt = go.GetComponentInChildren<TMP_Text>();
            if (txt) txt.text = p.label;

            var cg = go.GetComponent<CanvasGroup>();
            if (!cg) cg = go.AddComponent<CanvasGroup>();

            var rt = go.GetComponent<RectTransform>();

            var drag = go.AddComponent<DraggableItem>();
            drag.Init(p.key, this, _rootCanvas, cg, rt);
        }
    }

    // Called by DraggableItem when released
    public void TryPlace(DraggableItem item, PointerEventData eventData)
    {
        // Raycast UI to find a DropZone under pointer
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        Debug.Log($"TryPlace: {item.Key} - Found {results.Count} raycast results");

        foreach (var r in results)
        {
            // Check both the object itself and its parents for DropZone
            var zone = r.gameObject.GetComponent<DropZone>();
            if (zone == null)
            {
                zone = r.gameObject.GetComponentInParent<DropZone>();
            }

            if (zone != null)
            {
                Debug.Log($"Found zone: {zone.Key}, Item: {item.Key}, Locked: {zone.IsLocked}, Match: {zone.Key == item.Key}");
                
                if (zone.Key == item.Key && !zone.IsLocked)
                {
                    Debug.Log($"✓ Locking {item.Key} to zone!");
                    // Snap & lock
                    zone.LockToZone(item.RectTransform);
                    _lockedCount++;
                    item.Lock();
                    CheckComplete();
                    return;
                }
            }
        }

        Debug.Log($"✗ No valid zone found for {item.Key} - resetting to start");
        // No valid zone: return to original parent/anchored position
        item.ResetToStart();
    }

    private void CheckComplete()
    {
        if (_lockedCount >= _zonesByKey.Count && _zonesByKey.Count > 0)
        {
            if (winBanner != null) winBanner.SetActive(true);
            OnCompleted?.Invoke();
        }
    }
}
