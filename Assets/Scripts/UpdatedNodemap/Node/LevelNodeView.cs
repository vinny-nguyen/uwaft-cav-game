
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public enum NodeState { Inactive, Active, Completed }

public class LevelNodeView : MonoBehaviour
{
    private Image icon;                 // the button image
    private Button button;
    private NodeStateAnimation _anim;

    private void Awake()
    {
        icon = GetComponent<Image>();
        button = GetComponent<Button>();
        _anim = GetComponent<NodeStateAnimation>();
    }

    public void PlayShake()
    {
        if (_anim != null)
            StartCoroutine(_anim.Shake());
    }

    // OPTIONAL: if you want a helper for state-change pop animation too:
    public void PlayStatePop(NodeState state)
    {
        if (_anim != null)
            StartCoroutine(_anim.AnimateStateChange(state));
    }

    public int Index { get; private set; }       // 1..6
    System.Action _onClick;

    private Sprite _currentSprite;
    private NodeState _currentState = NodeState.Inactive;

    public void BindIndex(int index) => Index = index;

    public void SetState(NodeState state, bool animate)
    {
        _currentState = state;
        var animator = GetComponent<NodeStateAnimation>();
        if (animator != null && animate)
        {
            animator.StopAllCoroutines();
            StartCoroutine(animator.AnimateStateChange(state));
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
