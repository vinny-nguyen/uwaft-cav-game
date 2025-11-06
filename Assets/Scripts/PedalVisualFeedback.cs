using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Unity.VisualScripting;
#if UNITY_VECTOR_GRAPHICS
using Unity.VectorGraphics;
#endif

public class PedalVisualFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float zoomScale = 1.2f;
    public float zoomSpeed = 10f;
    private Vector3 originalScale;
    private bool isPressed = false;

    void Start()
    {
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        Vector3 targetScale = isPressed ? originalScale * zoomScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * zoomSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void SetPressed(bool pressed)
    {
        isPressed = pressed;
    }
}