using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace UWAFT.UI.Hotspots
{
    public sealed class HotspotGroup : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform container;        // usually this GameObject’s RectTransform (slide content root)
        [SerializeField] private RectTransform miniPopoverPrefab; // assign MiniPopover.prefab
        [SerializeField] private Canvas canvas;                   // the canvas this UI lives on

        private MiniPopover _open;

        void Awake()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
        }

        void Reset()
        {
            container = transform as RectTransform;
            canvas = GetComponentInParent<Canvas>();
        }

        public void ToggleFrom(HotspotDot dot)
        {
            if (_open != null && _open.Source == dot)
            {
                Close();
                return;
            }
            Open(dot);
        }

        public void Open(HotspotDot dot)
        {
            Close();
            var inst = Instantiate(miniPopoverPrefab, container);
            _open = inst.GetComponent<MiniPopover>();
            _open.Initialize(this, dot, canvas);
        }

        public void Close()
        {
            if (_open == null) return;
            Destroy(_open.gameObject);
            _open = null;
        }

        // Close when clicking outside (only if this group is the click target area)
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_open == null) return;
            if (_open.IsPointerOverSelf(eventData)) return; // click on popover; ignore
            // If click isn't on any hotspot or the popover, close
            if (eventData.pointerPress == null || eventData.pointerPress.GetComponent<HotspotDot>() == null)
                Close();
        }
    }
}
