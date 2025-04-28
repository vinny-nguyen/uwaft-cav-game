using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class NodeHoverHandler : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverScaleMultiplier = 1.2f;
    [SerializeField] private float scaleSpeed = 5f;
    [SerializeField] private Texture2D pointerCursorTexture;
    [SerializeField] private Color inactiveHoverColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Slightly darker
    [SerializeField] private Color normalColor = Color.white;

    private Vector3 originalScale;
    private bool isHovered = false;
    private bool isClickable = false;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return; // Ignore if over UI

        isHovered = true;

        if (pointerCursorTexture != null)
        {
            Cursor.SetCursor(pointerCursorTexture, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        if (!isClickable && spriteRenderer != null)
        {
            spriteRenderer.color = inactiveHoverColor; // 🔥 Darken if not clickable
        }
    }

    private void OnMouseExit()
    {
        isHovered = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Reset cursor

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor; // 🔥 Restore normal color
        }
    }

    private void OnMouseDown()
    {
        if (!isClickable)
        {
            // 🔥 Play shake animation if inactive
            StartCoroutine(ShakeNode());
            return;
        }

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.OpenPopup();
        }
        else
        {
            Debug.LogWarning("PopupManager instance not found!");
        }
    }


    private void Update()
    {
        if (!isHovered)
        {
            // If not hovered, return smoothly to original scale
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleSpeed);
            return;
        }

        if (isClickable)
        {
            // 🔥 Only scale if clickable
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * hoverScaleMultiplier, Time.deltaTime * scaleSpeed);
        }
        else
        {
            // 🔥 If not clickable, don't scale even if hovered
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleSpeed);
        }
    }

    private IEnumerator ShakeNode()
    {
        float shakeDuration = 0.15f; // 🔥 Much shorter
        float shakeMagnitude = 0.15f;   // 🔥 Much smaller side movement
        float time = 0f;

        Vector3 originalPosition = transform.localPosition;

        while (time < shakeDuration)
        {
            time += Time.deltaTime;
            float offsetX = Mathf.Sin(time * 50f) * shakeMagnitude * (1f - time / shakeDuration);
            transform.localPosition = originalPosition + new Vector3(offsetX, 0f, 0f);

            yield return null;
        }

        transform.localPosition = originalPosition;
    }



    public void SetClickable(bool value)
    {
        isClickable = value;
    }
}
