using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace UWAFT.UI.Hotspots
{
    public sealed class MiniPopover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rect;
        [SerializeField] private RectTransform card;
        [SerializeField] private RectTransform arrow;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image iconImage;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private float fadeDuration = 0.12f;
        [SerializeField] private Vector2 padding = new Vector2(8, 8); // keep inside container bounds

        [SerializeField] private LayoutElement cardLayout; // on Card
        [SerializeField] private float maxWidth = 280f;


        private HotspotGroup _group;
        private HotspotDot _source;
        private Canvas _canvas;
        private bool _hovered;
        private static readonly Vector3[] _corners = new Vector3[4];

        public HotspotDot Source => _source;

        void Reset()
        {
            rect = transform as RectTransform;
            group = GetComponent<CanvasGroup>();
        }

        public void Initialize(HotspotGroup group, HotspotDot source, Canvas canvas)
        {
            _group = group;
            _source = source;
            _canvas = canvas;

            // Ensure popover has consistent anchoring - anchor to center (0.5, 0.5)
            // This makes positioning calculations work with the container's coordinate system
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0f); // Keep pivot at bottom-left for predictable positioning

            titleText.textWrappingMode = TextWrappingModes.Normal;
            bodyText.textWrappingMode = TextWrappingModes.Normal;
            titleText.overflowMode = TMPro.TextOverflowModes.Overflow;
            bodyText.overflowMode = TMPro.TextOverflowModes.Overflow;

            titleText.text = source.Title;
            bodyText.text = source.Body;

            // Set max width constraint
            if (cardLayout != null)
                cardLayout.preferredWidth = maxWidth;

            canvasGroup.alpha = 0f;

            // Position after a frame to ensure layout is calculated
            StartCoroutine(PlaceAfterLayout(source));
        }

        private System.Collections.IEnumerator PlaceAfterLayout(HotspotDot source)
        {
            // Wait one frame for layout to be calculated
            yield return null;

            // Force layout rebuild now that content is set
            LayoutRebuilder.ForceRebuildLayoutImmediate(card);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            // Wait another frame to ensure size is properly calculated
            yield return null;

            // Manually set the popover size to match the card size
            // This ensures the popover rect has the correct size for positioning calculations
            if (rect.rect.size.magnitude == 0 && card.rect.size.magnitude > 0)
            {
                rect.sizeDelta = card.rect.size;
            }

            Place(source.Rect, source.Offset);
            LeanAlpha(1f, fadeDuration);
        }


        public bool IsPointerOverSelf(PointerEventData e)
        {
            if (rect == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(rect, e.position, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera);
        }

        public void OnPointerEnter(PointerEventData e) => _hovered = true;
        public void OnPointerExit(PointerEventData e) => _hovered = false;

        private void Place(RectTransform target, Vector2 preferredOffset)
        {
            var container = rect.parent as RectTransform;
            var isOverlay = _canvas.renderMode == RenderMode.ScreenSpaceOverlay;
            var cam = isOverlay ? null : _canvas.worldCamera;

            // Make sure our own size is correct before placing
            LayoutRebuilder.ForceRebuildLayoutImmediate(card);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            // Simplified approach: Use screen space positioning
            // 1) Get the DOT's screen position (top-right corner)
            target.GetWorldCorners(_corners);
            Vector3 worldTR = _corners[2]; // top-right corner
            Vector2 screenTR = RectTransformUtility.WorldToScreenPoint(cam, worldTR);

            // 2) Add offset in screen space
            Vector2 desiredScreenPos = screenTR + preferredOffset;

            // 3) Convert desired screen position to container local space
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                container,
                desiredScreenPos,
                cam,
                out localPos
            );

            // 4) Clamp to container bounds
            Rect c = container.rect;
            Vector2 size = rect.rect.size;

            float minX = c.xMin + padding.x;
            float maxX = c.xMax - padding.x - size.x;
            float minY = c.yMin + padding.y;
            float maxY = c.yMax - padding.y - size.y;

            Vector2 clamped = new Vector2(
                Mathf.Clamp(localPos.x, minX, maxX),
                Mathf.Clamp(localPos.y, minY, maxY)
            );

            rect.anchoredPosition = clamped;

            // Debug logging to help diagnose positioning issues
            Debug.Log($"[MiniPopover] Positioning Debug:" +
                     $"\n  Target: {target.name}" +
                     $"\n  Target WorldTR: {worldTR}" +
                     $"\n  Screen TR: {screenTR}" +
                     $"\n  Desired Screen: {desiredScreenPos}" +
                     $"\n  Local Pos: {localPos}" +
                     $"\n  Container Rect: {c}" +
                     $"\n  Popover Size: {size}" +
                     $"\n  Clamped Position: {clamped}" +
                     $"\n  Anchors: min={rect.anchorMin}, max={rect.anchorMax}");

            // 5) Position arrow (simplified)
            if (arrow != null)
            {
                // For now, just point to top-right
                arrow.anchorMin = arrow.anchorMax = new Vector2(0f, 0f);
                arrow.localRotation = Quaternion.Euler(0, 0, 45f);
            }
        }


        private void LeanAlpha(float to, float dur)
        {
            // tiny, dependency-free tween
            StopAllCoroutines();
            StartCoroutine(Fade(to, dur));
        }
        private System.Collections.IEnumerator Fade(float to, float dur)
        {
            float from = group.alpha;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, t / dur);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
