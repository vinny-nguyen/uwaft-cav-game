using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryCard : MonoBehaviour
{
    [Header("Faces")]
    [SerializeField] private GameObject front;    // holds Icon + Label
    [SerializeField] private Image back;          // the button image (back face)
    [SerializeField] private Image iconImage;     // child under Front (optional)
    [SerializeField] private TMP_Text labelText;  // child under Front (optional)

    [Header("Flip")]
    [SerializeField] private float flipDuration = 0.2f; // quick flip

    public string Key { get; private set; }
    public bool IsMatched { get; private set; }
    public bool IsFaceUp { get; private set; }

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

    public void Init(string key, Sprite icon, string label, MemoryMatchController controller)
    {
        Key = key;
        _controller = controller;

        if (iconImage) {
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null);
        }

        if (labelText) {
            labelText.text = string.IsNullOrWhiteSpace(label) ? "" : label;
            labelText.gameObject.SetActive(!string.IsNullOrWhiteSpace(label));
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
        if (_cg) _cg.alpha = 0.7f; // slight fade to indicate matched
    }

    private System.Collections.IEnumerator FlipRoutine(bool showFront)
    {
        // simple scale-X flip
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
