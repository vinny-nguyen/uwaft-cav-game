using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryCard : MonoBehaviour
{
    [Header("Faces")]
    [SerializeField] private GameObject front;    // holds Icon + Label
    [SerializeField] private Image back;          // the button image (back face)
    [SerializeField] private Image iconImage;     // child under Front (optional)
    [SerializeField] private TMP_Text labelText;  // child under Front (wrapped text)

    [Header("Flip")]
    [SerializeField] private float flipDuration = 0.2f; // quick flip

    public string Key { get; private set; }
    public bool IsMatched { get; private set; }
    public bool IsFaceUp { get; private set; }
    public bool IsTerm { get; private set; } // true = term card, false = definition card

    private Button _btn;
    private CanvasGroup _cg;
    private MemoryMatchController _controller;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _cg = GetComponent<CanvasGroup>();
        _btn.onClick.AddListener(HandleClick);
        ShowBackImmediate();
    }

    // New init for term/definition content
    public void InitWordDef(string key, string displayText, Sprite icon, bool isTerm, MemoryMatchController controller)
    {
        Key = key;
        _controller = controller;
        IsTerm = isTerm;

        if (iconImage)
        {
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null);
        }

        if (labelText)
        {
            labelText.textWrappingMode = TextWrappingModes.Normal;
            labelText.text = string.IsNullOrWhiteSpace(displayText) ? "" : displayText.Trim();
            labelText.gameObject.SetActive(!string.IsNullOrEmpty(labelText.text));
        }
    }

    private void HandleClick()
    {
        if (IsMatched) return;
        _controller.OnCardClicked(this);
    }

    public void FlipUp()
    {
        if (IsFaceUp) return;
        IsFaceUp = true;
        StopAllCoroutines();
        StartCoroutine(FlipRoutine(showFront: true));
    }

    public void FlipDown()
    {
        if (!IsFaceUp) return;
        IsFaceUp = false;
        StopAllCoroutines();
        StartCoroutine(FlipRoutine(showFront: false));
    }

    public void SetMatched()
    {
        IsMatched = true;
        _btn.interactable = false;
        if (_cg) _cg.alpha = 0.7f; // slight fade
    }

    private System.Collections.IEnumerator FlipRoutine(bool showFront)
    {
        var rt = (RectTransform)transform;
        float t = 0f;
        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / flipDuration);
            rt.localScale = new Vector3(k, 1f, 1f);
            yield return null;
        }

        if (showFront) ShowFrontImmediate(); else ShowBackImmediate();

        t = 0f;
        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / flipDuration);
            rt.localScale = new Vector3(k, 1f, 1f);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    private void ShowFrontImmediate()
    {
        if (front) front.SetActive(true);
        if (back) back.enabled = false;
    }

    private void ShowBackImmediate()
    {
        if (front) front.SetActive(false);
        if (back) back.enabled = true;
        IsFaceUp = false;
    }
}
