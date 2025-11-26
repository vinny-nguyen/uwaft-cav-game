using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

namespace UWAFT.UI.Hotspots
{
    public sealed class HotspotGroup : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform container;        // usually this GameObject’s RectTransform (slide content root)
        [SerializeField] private RectTransform miniPopoverPrefab; // assign MiniPopover.prefab
        [SerializeField] private Canvas canvas;                   // the canvas this UI lives on
        [Header("UX")]
        [SerializeField] private TMP_Text remainingText;
        [SerializeField] private bool requireAllClicked = true;
        [SerializeField] private UnityEvent onAllClicked;

        private MiniPopover _open;
        private HotspotDot[] _allDots;
        private HashSet<HotspotDot> _clicked = new HashSet<HotspotDot>();

        void Awake()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            // gather all hotspot dots under this group
            _allDots = GetComponentsInChildren<HotspotDot>(true);
            UpdateRemainingText();
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

        // Called by HotspotDot when user clicks a dot
        public void NotifyClicked(HotspotDot dot)
        {
            if (dot == null) return;
            if (!_clicked.Contains(dot))
            {
                _clicked.Add(dot);
                // keep the dot interactable so user can re-open the popover if desired
                dot.MarkClickedExternally();
                UpdateRemainingText();

                if (_allDots != null && _clicked.Count == _allDots.Length)
                {
                    onAllClicked?.Invoke();
                }
            }
        }

        private void UpdateRemainingText()
        {
            if (remainingText == null) return;
            int remaining = _allDots == null ? 0 : Mathf.Max(0, _allDots.Length - _clicked.Count);
            remainingText.text = remaining > 0 ? $"Hotspots remaining: {remaining}" : "All hotspots viewed";
        }

        // Expose whether this group requires all hotspots clicked to progress
        public bool RequireAllClicked => requireAllClicked;

        // Expose a read-only UnityEvent for external subscription
        public UnityEvent OnAllClicked => onAllClicked;

        // Expose whether all hotspots have been clicked
        public bool IsComplete => (_allDots != null) && (_clicked.Count >= _allDots.Length);

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
