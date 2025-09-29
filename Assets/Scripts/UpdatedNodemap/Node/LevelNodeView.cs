using UnityEngine;
using UnityEngine.UI;

public enum NodeState { Inactive, Active, Completed }

public class LevelNodeView : MonoBehaviour
{
    [SerializeField] Image icon;                 // the button image
    [SerializeField] Button button;
    [Header("Per-node sprites (assign for this index)")]
    [SerializeField] Sprite inactiveSprite;
    [SerializeField] Sprite activeSprite;
    [SerializeField] Sprite completedSprite;

    public int Index { get; private set; }       // 1..6
    System.Action _onClick;

    public void BindIndex(int index) => Index = index;

    public void SetState(NodeState state)
    {
        icon.sprite = state switch
        {
            NodeState.Inactive => inactiveSprite,
            NodeState.Active => activeSprite,
            NodeState.Completed => completedSprite,
            _ => inactiveSprite
        };
        button.interactable = state == NodeState.Active;

        Debug.Log("Set node " + Index + " to " + state);
    }

    public void SetOnClick(System.Action onClick)
    {
        _onClick = onClick;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke());
    }
}
