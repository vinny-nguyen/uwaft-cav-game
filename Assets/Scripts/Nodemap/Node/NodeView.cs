
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public enum NodeState { Inactive, Active, Completed }

public class LevelNodeView : MonoBehaviour
{
    private Image icon;
    private Button button;

    private void Awake()
    {
        icon = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    public int Index { get; private set; }
    System.Action _onClick;

    private Sprite _currentSprite;

    public void BindIndex(int index) => Index = index;

    public void SetState(NodeState state, bool animate)
    {
        LoadAndSetSprite(state, Index);
        Debug.Log($"Set node {Index} to {state}");
    }

    private AsyncOperationHandle<Sprite>? currentLoadHandle;

    private void LoadAndSetSprite(NodeState state, int index)
    {
        // Cancel previous load if still pending
        if (currentLoadHandle.HasValue && currentLoadHandle.Value.IsValid())
        {
            Addressables.Release(currentLoadHandle.Value);
        }

        string folder = state switch
        {
            NodeState.Inactive => "Inactive",
            NodeState.Active => "Active", 
            NodeState.Completed => "Completed",
            _ => "Inactive"
        };
        string address = $"Nodes/{folder}/node_{index}";
        
        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        currentLoadHandle = handle;
        
        handle.Completed += (AsyncOperationHandle<Sprite> op) =>
        {
            // Check if this component still exists and this is still the current load
            if (this == null || !currentLoadHandle.HasValue || !currentLoadHandle.Value.Equals(op)) 
            {
                // Clean up and return early if component destroyed or superseded
                if (op.IsValid()) Addressables.Release(op);
                return;
            }

            if (op.Status == AsyncOperationStatus.Succeeded && op.Result != null)
            {
                _currentSprite = op.Result;
                if (icon != null) icon.sprite = _currentSprite;
            }
            else
            {
                Debug.LogWarning($"[LevelNodeView] Failed to load sprite at address: {address}. Using fallback.");
                // Use a simple colored sprite as fallback
                CreateFallbackSprite(state);
            }
        };
    }

    private void CreateFallbackSprite(NodeState state)
    {
        // Create a simple colored texture as fallback
        Color fallbackColor = state switch
        {
            NodeState.Active => Color.yellow,
            NodeState.Completed => Color.green,
            _ => Color.gray
        };
        
        // Create a simple 64x64 texture
        Texture2D fallbackTexture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = fallbackColor;
        fallbackTexture.SetPixels(pixels);
        fallbackTexture.Apply();
        
        _currentSprite = Sprite.Create(fallbackTexture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
        if (icon != null) icon.sprite = _currentSprite;
    }

    private void OnDestroy()
    {
        // Clean up addressables handle
        if (currentLoadHandle.HasValue && currentLoadHandle.Value.IsValid())
        {
            Addressables.Release(currentLoadHandle.Value);
        }
    }

    public void SetOnClick(System.Action onClick)
    {
        _onClick = onClick;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke());
    }
}
