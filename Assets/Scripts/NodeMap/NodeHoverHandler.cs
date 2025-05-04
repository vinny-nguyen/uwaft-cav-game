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

    [Header("Node Info")]
    public int nodeIndex;

    private Vector3 originalScale;
    private bool isHovered = false;
    private SpriteRenderer spriteRenderer;
    private bool isClickable = false;


    private void Start()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    private void Update()
    {
        bool shouldScale = NodeMapGameManager.Instance.CurrentNodeIndex == nodeIndex || isClickable;

        if (!isHovered)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleSpeed);
            return;
        }

        if (shouldScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * hoverScaleMultiplier, Time.deltaTime * scaleSpeed);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleSpeed);
        }
    }


    private void OnMouseEnter()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        isHovered = true;

        if (pointerCursorTexture != null)
        {
            Cursor.SetCursor(pointerCursorTexture, Vector2.zero, CursorMode.Auto);
        }

        PlayerSplineMovement mover = FindFirstObjectByType<PlayerSplineMovement>();
        bool isCompleted = mover != null && mover.IsNodeCompleted(nodeIndex);
        bool isClickable = NodeMapGameManager.Instance.CurrentNodeIndex == nodeIndex || isCompleted;

        if (!isClickable && spriteRenderer != null)
        {
            spriteRenderer.color = inactiveHoverColor; // Darken if not clickable
        }
    }

    private void OnMouseExit()
    {
        isHovered = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    private void OnMouseDown()
    {
        if (NodeMapGameManager.Instance == null)
        {
            Debug.LogWarning("NodeMapGameManager instance missing!");
            return;
        }

        PlayerSplineMovement mover = FindFirstObjectByType<PlayerSplineMovement>();
        bool isCompleted = mover != null && mover.IsNodeCompleted(nodeIndex - 1); // ✅ Adjust for zero-based index

        if (NodeMapGameManager.Instance.CurrentNodeIndex != nodeIndex && !isCompleted)
        {
            StartCoroutine(ShakeNode());
            return;
        }

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.OpenPopupForNode(nodeIndex);
        }
    }


    private IEnumerator ShakeNode()
    {
        float shakeDuration = 0.15f;
        float shakeMagnitude = 0.15f;
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

    public void StartShake()
    {
        StartCoroutine(ShakeNode());
    }

}
