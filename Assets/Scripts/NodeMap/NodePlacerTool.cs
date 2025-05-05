using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

namespace NodeMap.EditorTools
{
    /// <summary>
    /// Tool for positioning node markers along a spline path
    /// Used in the Unity Editor
    /// </summary>
    [ExecuteInEditMode]
    public class NodePlacerTool : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the spline movement component")]
        public PlayerSplineMovement playerSplineMovement;
        
        [Tooltip("List of node marker GameObjects to position")]
        public List<GameObject> nodeMarkers = new List<GameObject>();

        /// <summary>
        /// Repositions all node markers along the spline based on stop percentages
        /// </summary>
        public void RepositionNodeMarkers()
        {
            if (!ValidateReferences())
                return;
                
            List<SplineStop> stops = GetStops();
            if (!ValidateStops(stops))
                return;
                
            PositionMarkersAlongSpline(stops);
        }
        
        private bool ValidateReferences()
        {
            if (playerSplineMovement == null)
            {
                Debug.LogError("PlayerSplineMovement not assigned!");
                return false;
            }
            return true;
        }
        
        private List<SplineStop> GetStops()
        {
            #if UNITY_EDITOR
            playerSplineMovement.ForceGenerateStops();
            #endif
            
            return playerSplineMovement.GetStops();
        }
        
        private bool ValidateStops(List<SplineStop> stops)
        {
            if (stops == null || stops.Count == 0)
            {
                Debug.LogError("No stops found! Make sure stops are generated first.");
                return false;
            }
            
            if (nodeMarkers.Count != stops.Count)
            {
                Debug.LogError($"Node marker count ({nodeMarkers.Count}) does not match stops count ({stops.Count})!");
                return false;
            }
            
            return true;
        }
        
        private void PositionMarkersAlongSpline(List<SplineStop> stops)
        {
            SplineContainer spline = playerSplineMovement.GetSpline();
            
            for (int i = 0; i < nodeMarkers.Count; i++)
            {
                GameObject marker = nodeMarkers[i];
                if (marker != null)
                {
                    Vector3 splinePosition = spline.transform.TransformPoint(
                        (Vector3)spline.EvaluatePosition(stops[i].splinePercent));
                    marker.transform.position = splinePosition;
                }
            }
            
            Debug.Log("Node markers repositioned successfully!");
        }
    }
}
