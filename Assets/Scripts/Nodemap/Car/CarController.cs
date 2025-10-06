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
    public event Action<int> OnStartedMovingToNode;

    private NodeManager nodeManager;
    private int targetNodeIndex = -1;

    public void Initialize(NodeManager nodes)
    {
        nodeManager = nodes;
        car.SnapTo(spawnT);
    }

    /// <summary>
    /// Moves car to target node with optional pass-through visual updates for completed nodes.
    /// </summary>
    public void MoveToNode(int nodeIndex, bool[] completedNodes = null)
    {
        if (nodeManager == null) return;
        
        targetNodeIndex = nodeIndex;
        OnStartedMovingToNode?.Invoke(nodeIndex);
        StartCoroutine(MoveToNodeCoroutine(nodeIndex, completedNodes));
    }

    public void SnapToNode(int nodeIndex)
    {
        if (nodeManager == null) return;
        car.SnapTo(nodeManager.GetSplineT(nodeIndex));
    }

    private IEnumerator MoveToNodeCoroutine(int targetNodeIndex, bool[] completedNodes)
    {
        float startT = car.NormalizedT;
        float targetT = nodeManager.GetSplineT(targetNodeIndex);
        
        // Start car movement
        var moveCoroutine = StartCoroutine(car.MoveAlong(startT, targetT, true));
        
        // If we have completed nodes info, do pass-through visual updates
        if (completedNodes != null)
        {
            bool[] visualUpdated = new bool[completedNodes.Length];
            
            // Monitor movement and update completed nodes as car passes them
            while (car.NormalizedT != targetT)
            {
                for (int i = 0; i < completedNodes.Length; i++)
                {
                    if (completedNodes[i] && !visualUpdated[i])
                    {
                        float nodeT = nodeManager.GetSplineT(i);
                        
                        // Check if car has passed this completed node
                        bool hasPassed = (startT < targetT && car.NormalizedT >= nodeT) || 
                                        (startT > targetT && car.NormalizedT <= nodeT);
                        
                        if (hasPassed)
                        {
                            nodeManager.UpdateNodeVisual(i, true, true);
                            visualUpdated[i] = true;
                        }
                    }
                }
                yield return null;
            }
        }
        
        // Wait for movement to complete
        yield return moveCoroutine;
        
        // Notify arrival
        OnArrivedAtNode?.Invoke(targetNodeIndex);
    }
}