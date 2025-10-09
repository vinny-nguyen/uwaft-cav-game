using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string Key { get; private set; }
    public RectTransform RectTransform { get; private set; }

    private DragDropController _controller;
    private Canvas _canvas;
    private CanvasGroup _cg;
    private Transform _startParent;
    private Vector2 _startAnchoredPos;
    private bool _locked;

    public void Init(string key, DragDropController controller, Canvas rootCanvas, CanvasGroup cg, RectTransform rt)
    {
        Key = key;
        _controller = controller;
        _canvas = rootCanvas;
        _cg = cg;
        RectTransform = rt;
        _startParent = rt.parent;
        _startAnchoredPos = rt.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_locked) return;
        _cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_locked) return;
        RectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_locked) return;
        _cg.blocksRaycasts = true;
        _controller.TryPlace(this, eventData);
    }

    public void ResetToStart()
    {
        RectTransform.SetParent(_startParent);
        RectTransform.anchoredPosition = _startAnchoredPos;
    }

    public void Lock()
    {
        _locked = true;
        _cg.interactable = false;
        _cg.blocksRaycasts = false;
    }
}
