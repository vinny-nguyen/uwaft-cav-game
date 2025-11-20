using UnityEngine;

/// <summary>
/// Handles car sprite visuals and upgrades.
/// </summary>
public class CarVisual : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer frameRenderer;
    [SerializeField] private SpriteRenderer tireFrontRenderer;
    [SerializeField] private SpriteRenderer tireRearRenderer;

    [Header("Default Art (optional)")]
    [SerializeField] private Sprite defaultFrame;
    [SerializeField] private Sprite defaultTire;

    private void Awake()
    {
        // Set defaults on load
        if (defaultFrame) frameRenderer.sprite = defaultFrame;
        if (defaultTire)
        {
            tireFrontRenderer.sprite = defaultTire;
            tireRearRenderer.sprite = defaultTire;
        }
    }

    /// <summary>
    /// Applies upgrade sprites to the car. Pass null to keep current sprite.
    /// </summary>
    public void ApplyUpgrade(Sprite newFrame, Sprite newTire)
    {
        if (newFrame) frameRenderer.sprite = newFrame;
        if (newTire)
        {
            tireFrontRenderer.sprite = newTire;
            tireRearRenderer.sprite = newTire;
        }
        // Optional: add pop/fade effect for polish
    }

    /// <summary>
    /// Get the current frame sprite for before/after comparison.
    /// </summary>
    public Sprite GetCurrentFrameSprite()
    {
        return frameRenderer != null ? frameRenderer.sprite : null;
    }

    /// <summary>
    /// Get the current tire sprite for before/after comparison.
    /// </summary>
    public Sprite GetCurrentTireSprite()
    {
        return tireFrontRenderer != null ? tireFrontRenderer.sprite : null;
    }
}
