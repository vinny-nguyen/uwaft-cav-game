using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UWAFT.UI.Hotspots
{
    public sealed class HotspotDot : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private string title;
        [TextArea]
        [SerializeField] private string body;
        [SerializeField] private Sprite icon;

        [Header("Placement")]
        [SerializeField] private Vector2 offset = new Vector2(18f, 18f); // preferred direction: top-right
        [SerializeField] private RectTransform rect; // this dot rect

        [Header("Visual")]
        [SerializeField] private bool pulseEnabled = true;
        [SerializeField] private float pulseScale = 1.12f;
        [SerializeField] private float pulseSpeed = 2f;

        private HotspotGroup _group;
        private Vector3 _baseScale;
        private bool _clicked;

        public string Title => title;
        public string Body => body;
        public Sprite Icon => icon;
        public Vector2 Offset => offset;
        public RectTransform Rect => rect != null ? rect : (rect = transform as RectTransform);

        void Awake()
        {
            _group = GetComponentInParent<HotspotGroup>(true);
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnClick);

            _baseScale = transform.localScale;
        }

        private void OnClick()
        {
            _group?.ToggleFrom(this);
            // mark clicked immediately so we stop pulsing and the group can track progress
            _clicked = true;
            _group?.NotifyClicked(this);
        }

        void Update()
        {
            if (!pulseEnabled || _clicked) return;
            // simple pulsing scale animation
            float s = 1f + (Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.5f + 0.5f) * (pulseScale - 1f);
            transform.localScale = _baseScale * s;
        }

        public void MarkClickedExternally()
        {
            _clicked = true;
            transform.localScale = _baseScale;
        }
    }
}
