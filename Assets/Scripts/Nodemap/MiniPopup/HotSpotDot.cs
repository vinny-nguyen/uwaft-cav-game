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

        private HotspotGroup _group;

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
        }

        private void OnClick()
        {
            _group?.ToggleFrom(this);
        }
    }
}
