using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Simple node management: spawn, position, visuals, and clicks.
/// </summary>
public class NodeManager : MonoBehaviour
{
    [SerializeField] private List<NodeData> nodeData;
    [SerializeField] private LevelNodeView nodePrefab;
    [SerializeField] private RectTransform nodesParent;
    [SerializeField] private SplineContainer spline;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private MapConfig config;

    public event Action<int> OnNodeClicked;

    private List<LevelNodeView> nodes = new();

    public void Initialize()
    {
        SpawnNodes();
        PositionNodes();
    }

    public void UpdateNodeVisual(int index, bool unlocked, bool completed)
    {
        if (index < 0 || index >= nodes.Count) return;
        
        NodeState state = completed ? NodeState.Completed : 
                         unlocked ? NodeState.Active : NodeState.Inactive;
        nodes[index].SetState(state, true);
    }

    public void ShakeNode(int index)
    {
        if (index >= 0 && index < nodes.Count)
            nodes[index].PlayShake();
    }

    public NodeData GetNodeData(int index) => 
        index >= 0 && index < nodeData.Count ? nodeData[index] : null;

    public float GetSplineT(int index)
    {
        if (nodes.Count <= 1) return config.tStart;
        return Mathf.Lerp(config.tStart, config.tEnd, (float)index / (nodes.Count - 1));
    }

    private void SpawnNodes()
    {
        foreach (Transform child in nodesParent) Destroy(child.gameObject);
        nodes.Clear();

        for (int i = 0; i < nodeData.Count; i++)
        {
            var node = Instantiate(nodePrefab, nodesParent);
            node.BindIndex(i + 1);
            int idx = i;
            node.SetOnClick(() => OnNodeClicked?.Invoke(idx));
            nodes.Add(node);
        }
    }

    private void PositionNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var worldPos = spline.EvaluatePosition(GetSplineT(i));
            var screenPos = uiCamera.WorldToScreenPoint(worldPos);
            
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out Vector2 local))
                nodes[i].GetComponent<RectTransform>().anchoredPosition = local;
        }
    }
}