using UnityEngine;

public class SplineCurvedLine : MonoBehaviour
{
    public GameObject mainNodesParent; // Assign the MainNodes parent GameObject here
    public GameObject controlPointsParent; // Assign the ControlPoints parent GameObject here
    public LineRenderer lineRenderer;
    public int curveResolution = 20; // Number of points between nodes

    public bool showGizmos = false; // Toggle this in the Inspector to show/hide Gizmos

    private Transform[] mainNodes; // Array to store the main nodes
    private Transform[][] controlPoints; // Array of arrays to store control points for each segment

    void Start()
    {
        GenerateCurvedLine();

        // Disable the LineRenderer so the line doesn't show up in the game
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void OnDrawGizmos()
    {
        // Only draw Gizmos if the flag is enabled
        if (!showGizmos)
        {
            return;
        }

        // Initialize arrays if they are not already initialized
        if (mainNodes == null || controlPoints == null)
        {
            InitializeNodesAndControlPoints();
        }

        // Draw the curved path in the Scene view
        if (mainNodes != null && controlPoints != null)
        {
            Transform[] allNodes = CombineNodesAndControlPoints(mainNodes, controlPoints);
            if (allNodes != null && allNodes.Length >= 2)
            {
                for (int i = 0; i < allNodes.Length - 1; i++)
                {
                    Vector3 p0 = i == 0 ? allNodes[i].position : allNodes[i - 1].position;
                    Vector3 p1 = allNodes[i].position;
                    Vector3 p2 = allNodes[i + 1].position;
                    Vector3 p3 = i == allNodes.Length - 2 ? allNodes[i + 1].position : allNodes[i + 2].position;

                    Vector3 previousPoint = p1;
                    for (int j = 1; j <= curveResolution; j++)
                    {
                        float t = j / (float)curveResolution;
                        Vector3 point = CalculateCatmullRomPoint(p0, p1, p2, p3, t);
                        Gizmos.DrawLine(previousPoint, point);
                        previousPoint = point;
                    }
                }
            }
        }
    }

    void GenerateCurvedLine()
    {
        InitializeNodesAndControlPoints();

        if (mainNodes == null || mainNodes.Length < 2)
        {
            UnityEngine.Debug.LogError("At least 2 main nodes are required.");
            return;
        }

        // Combine main nodes and control points into a single array
        Transform[] allNodes = CombineNodesAndControlPoints(mainNodes, controlPoints);

        // Set the number of points in the LineRenderer
        lineRenderer.positionCount = (allNodes.Length - 1) * curveResolution;

        // Generate the curved line
        for (int i = 0; i < allNodes.Length - 1; i++)
        {
            // Get the four control points for the Catmull-Rom spline
            Vector3 p0 = i == 0 ? allNodes[i].position : allNodes[i - 1].position;
            Vector3 p1 = allNodes[i].position;
            Vector3 p2 = allNodes[i + 1].position;
            Vector3 p3 = i == allNodes.Length - 2 ? allNodes[i + 1].position : allNodes[i + 2].position;

            // Generate points along the curve
            for (int j = 0; j < curveResolution; j++)
            {
                float t = j / (float)curveResolution;
                Vector3 point = CalculateCatmullRomPoint(p0, p1, p2, p3, t);
                lineRenderer.SetPosition(i * curveResolution + j, point);
            }
        }
    }

    void InitializeNodesAndControlPoints()
    {
        // Automatically populate the mainNodes array with the child nodes of mainNodesParent
        if (mainNodesParent != null)
        {
            mainNodes = new Transform[mainNodesParent.transform.childCount];
            for (int i = 0; i < mainNodesParent.transform.childCount; i++)
            {
                mainNodes[i] = mainNodesParent.transform.GetChild(i);
            }
        }

        // Automatically populate the controlPoints array with the child nodes of controlPointsParent
        if (controlPointsParent != null)
        {
            // Each child of controlPointsParent is a segment (e.g., ControlPointsSegment1, ControlPointsSegment2, etc.)
            controlPoints = new Transform[controlPointsParent.transform.childCount][];
            for (int i = 0; i < controlPointsParent.transform.childCount; i++)
            {
                Transform segment = controlPointsParent.transform.GetChild(i);
                controlPoints[i] = new Transform[segment.childCount];
                for (int j = 0; j < segment.childCount; j++)
                {
                    controlPoints[i][j] = segment.GetChild(j);
                }
            }
        }
    }

    // Combine main nodes and control points into a single array
    Transform[] CombineNodesAndControlPoints(Transform[] mainNodes, Transform[][] controlPoints)
    {
        // Calculate total number of points
        int totalPoints = mainNodes.Length;
        if (controlPoints != null)
        {
            for (int i = 0; i < controlPoints.Length; i++)
            {
                if (controlPoints[i] != null)
                {
                    totalPoints += controlPoints[i].Length;
                }
            }
        }

        // Create the combined array
        Transform[] allNodes = new Transform[totalPoints];
        int index = 0;

        // Add main nodes and control points
        for (int i = 0; i < mainNodes.Length; i++)
        {
            allNodes[index++] = mainNodes[i];

            // Add control points for this segment (if any)
            if (controlPoints != null && i < controlPoints.Length && controlPoints[i] != null)
            {
                for (int j = 0; j < controlPoints[i].Length; j++)
                {
                    allNodes[index++] = controlPoints[i][j];
                }
            }
        }

        return allNodes;
    }

    // Calculate a point on a Catmull-Rom spline
    Vector3 CalculateCatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 point =
            0.5f * ((2f * p1) +
                    (-p0 + p2) * t +
                    (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                    (-p0 + 3f * p1 - 3f * p2 + p3) * t3);

        return point;
    }
}