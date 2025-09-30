
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum NodeState { Inactive, Active, Completed }

public class LevelNodeView : MonoBehaviour
{
    [SerializeField] Image icon;                 // the button image
    [SerializeField] Button button;

    public int Index { get; private set; }       // 1..6
    System.Action _onClick;

    private Sprite _currentSprite;
    private NodeState _currentState = NodeState.Inactive;

    public void BindIndex(int index) => Index = index;

    public void SetState(NodeState state)
    {
        _currentState = state;
        // Play animation for state change
        var animator = GetComponent<NodeStateAnimation>();
        if (animator != null)
        {
            animator.PlayStateChange(state);
        }

        LoadAndSetSprite(state, Index);
        // button.interactable = state == NodeState.Active;
        Debug.Log($"Set node {Index} to {state}");
    }

    private void LoadAndSetSprite(NodeState state, int index)
    {
        string folder = state switch
        {
            NodeState.Inactive => "Inactive",
            NodeState.Active => "Active",
            NodeState.Completed => "Completed",
            _ => "Inactive"
        };
        string address = $"Nodes/{folder}/node_{index}";
        Addressables.LoadAssetAsync<Sprite>(address).Completed += (AsyncOperationHandle<Sprite> op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _currentSprite = op.Result;
                icon.sprite = _currentSprite;
            }
            else
            {
                Debug.LogWarning($"Failed to load sprite at address: {address}");
            }
        };
    }

    public void SetOnClick(System.Action onClick)
    {
        _onClick = onClick;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke());
    }
}
