using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[ExecuteInEditMode]
public class NodePlacerTool : MonoBehaviour
{
    [Header("References")]
    public PlayerSplineMovement playerSplineMovement;
    public List<GameObject> nodeMarkers = new List<GameObject>();

    public void RepositionNodeMarkers()
    {
        if (playerSplineMovement == null)
        {
            Debug.LogError("PlayerSplineMovement not assigned!");
            return;
        }

#if UNITY_EDITOR
        playerSplineMovement.ForceGenerateStops();
#endif

        List<SplineStop> stops = playerSplineMovement.GetStops();
        if (stops == null || stops.Count == 0)
        {
            Debug.LogError("No stops found! Make sure stops are generated first.");
            return;
        }

        if (nodeMarkers.Count != stops.Count)
        {
            Debug.LogError("Node marker count does not match stops count!");
            return;
        }

        SplineContainer spline = playerSplineMovement.GetSpline();

        for (int i = 0; i < nodeMarkers.Count; i++)
        {
            if (nodeMarkers[i] != null)
            {
                Vector3 newPosition = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(stops[i].splinePercent));
                nodeMarkers[i].transform.position = newPosition;
            }
        }

        Debug.Log("Node markers repositioned successfully!");
    }
}
