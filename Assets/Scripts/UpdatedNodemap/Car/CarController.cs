using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Simple car control wrapper around CarPathFollower.
/// </summary>
public class CarController : MonoBehaviour
{
    [SerializeField] private CarPathFollower car;
    [SerializeField] private float spawnT = 0f;

    public event Action<int> OnArrivedAtNode;

    private NodeManager nodeManager;
    private int targetNodeIndex = -1;

    public void Initialize(NodeManager nodes)
    {
        nodeManager = nodes;
        car.SnapTo(spawnT);
    }

    public void MoveToNode(int nodeIndex)
    {
        if (nodeManager == null) return;
        
        targetNodeIndex = nodeIndex;
        float targetT = nodeManager.GetSplineT(nodeIndex);
        StartCoroutine(MoveAndNotify(targetT));
    }

    public void SnapToNode(int nodeIndex)
    {
        if (nodeManager == null) return;
        car.SnapTo(nodeManager.GetSplineT(nodeIndex));
    }

    public void MoveToCurrentActive(int activeNode, bool[] completed)
    {
        if (nodeManager == null) return;
        
        targetNodeIndex = activeNode;
        StartCoroutine(MoveWithCompletedUpdates(activeNode, completed));
    }

    private IEnumerator MoveAndNotify(float targetT)
    {
        yield return StartCoroutine(car.MoveAlong(car.NormalizedT, targetT, true));
        OnArrivedAtNode?.Invoke(targetNodeIndex);
    }

    private IEnumerator MoveWithCompletedUpdates(int activeNode, bool[] completed)
    {
        float startT = car.NormalizedT;
        float targetT = nodeManager.GetSplineT(activeNode);
        
        // Start car movement
        var moveCoroutine = StartCoroutine(car.MoveAlong(startT, targetT, true));
        
        // Update completed nodes as car passes them
        bool[] updated = new bool[completed.Length];
        while (car.NormalizedT != targetT)
        {
            for (int i = 0; i < completed.Length; i++)
            {
                if (completed[i] && !updated[i] && car.NormalizedT >= nodeManager.GetSplineT(i))
                {
                    nodeManager.UpdateNodeVisual(i, true, true);
                    updated[i] = true;
                }
            }
            yield return null;
        }
        
        yield return moveCoroutine;
        OnArrivedAtNode?.Invoke(targetNodeIndex);
    }
}