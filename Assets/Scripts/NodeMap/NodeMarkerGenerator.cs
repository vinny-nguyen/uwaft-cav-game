using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class NodeMarkerGenerator : MonoBehaviour
{
    [Header("References")]
    public PlayerSplineMovement playerSplineMovement;
    public GameObject nodeMarkerPrefab;
    public List<Sprite> nodeSprites = new List<Sprite>();

    [HideInInspector] public List<GameObject> spawnedMarkers = new List<GameObject>();

    [ContextMenu("Generate Node Markers In Scene")]
    public void GenerateNodeMarkersInScene()
    {
        if (playerSplineMovement == null || nodeMarkerPrefab == null)
        {
            Debug.LogError("PlayerSplineMovement or Prefab not assigned!");
            return;
        }

        List<SplineStop> stops = playerSplineMovement.GetStops();
        if (stops == null || stops.Count == 0)
        {
            Debug.LogError("No stops found! Make sure PlayerSplineMovement generated stops first.");
            return;
        }

        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        spawnedMarkers.Clear();

        SplineContainer spline = playerSplineMovement.GetSpline(); // 🆕 safe access

        for (int i = 0; i < stops.Count; i++)
        {
            Vector3 position = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(stops[i].splinePercent));
            GameObject marker = (GameObject)PrefabUtility.InstantiatePrefab(nodeMarkerPrefab, this.transform);
            marker.transform.position = position;

            SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (i < nodeSprites.Count && nodeSprites[i] != null)
                {
                    sr.sprite = nodeSprites[i];
                }
                sr.sortingLayerName = "Foreground";
                sr.sortingOrder = 1;
            }

            spawnedMarkers.Add(marker);
        }

        Debug.Log("Node Markers generated successfully!");
    }
}
