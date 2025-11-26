using UnityEngine;

// Handles car sprite visuals and upgrades
public class CarVisual : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer frameRenderer;
    [SerializeField] private SpriteRenderer tireFrontRenderer;
    [SerializeField] private SpriteRenderer tireRearRenderer;

    [Header("Default Art (optional)")]
    [SerializeField] private SpriteRenderer defaultFrame;
    [SerializeField] private SpriteRenderer defaultTire;

    private void Awake()
    {
        // Set defaults on load
        if (defaultFrame) frameRenderer.sprite = defaultFrame.sprite;
        if (defaultTire)
        {
            tireFrontRenderer.sprite = defaultTire.sprite;
            tireRearRenderer.sprite = defaultTire.sprite;
        }
    }

    // Applies upgrade sprites to the car (pass null to keep current sprite)
    public void ApplyUpgrade(SpriteRenderer newFrame, SpriteRenderer newTire)
    {
        if (newFrame && newFrame.sprite) frameRenderer.sprite = newFrame.sprite;
        if (newTire && newTire.sprite)
        {
            tireFrontRenderer.sprite = newTire.sprite;
            tireRearRenderer.sprite = newTire.sprite;
        }
    }

    // Get the current frame sprite for before/after comparison
    public Sprite GetCurrentFrameSprite()
    {
        return frameRenderer != null ? frameRenderer.sprite : null;
    }

    // Get the current tire sprite for before/after comparison
    public Sprite GetCurrentTireSprite()
    {
        return tireFrontRenderer != null ? tireFrontRenderer.sprite : null;
    }
}
