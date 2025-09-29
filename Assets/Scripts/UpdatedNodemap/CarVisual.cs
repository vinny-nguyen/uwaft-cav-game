using UnityEngine;

public class CarVisual : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] SpriteRenderer frameRenderer;
    [SerializeField] SpriteRenderer tireFrontRenderer;
    [SerializeField] SpriteRenderer tireRearRenderer;

    [Header("Default Art (optional)")]
    [SerializeField] Sprite defaultFrame;
    [SerializeField] Sprite defaultTire;

    void Awake()
    {
        // Optional: set defaults on load
        if (defaultFrame) frameRenderer.sprite = defaultFrame;
        if (defaultTire)
        {
            tireFrontRenderer.sprite = defaultTire;
            tireRearRenderer.sprite = defaultTire;
        }
    }

    // Upgrade API — pass null to keep current sprite
    public void ApplyUpgrade(Sprite newFrame, Sprite newTire)
    {
        if (newFrame) frameRenderer.sprite = newFrame;
        if (newTire)
        {
            tireFrontRenderer.sprite = newTire;
            tireRearRenderer.sprite = newTire;
        }
        // Optional tiny pop/fade here for polish
    }
}
