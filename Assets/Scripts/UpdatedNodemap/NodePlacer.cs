using UnityEngine;
using UnityEngine.Splines;

public class NodePlacer : MonoBehaviour
{
    [Header("World")]
    [SerializeField] SplineContainer spline;   // your world spline

    [Header("UI")]
    [SerializeField] Camera uiCamera;          // the Canvas' camera (Main Camera)
    [SerializeField] RectTransform canvasRect; // root Canvas RectTransform
    [SerializeField] RectTransform[] nodeRects; // your 6 node buttons, in order

    [Header("Range on Spline (normalized t)")]
    [Range(0f, 1f)] public float tStart = 0.2f;
    [Range(0f, 1f)] public float tEnd = 0.8f;

    void Start() => PlaceNodes();
    void OnRectTransformDimensionsChange() => PlaceNodes(); // re-place on resize

    public void PlaceNodes()
    {
        if (nodeRects == null || nodeRects.Length == 0 || spline == null || uiCamera == null || canvasRect == null)
            return;

        int n = nodeRects.Length;
        for (int i = 0; i < n; i++)
        {
            float a = (n == 1) ? 0f : (float)i / (n - 1);      // 0..1
            float t = Mathf.Lerp(tStart, tEnd, a);             // map into [tStart, tEnd]

            Vector3 worldPos = spline.EvaluatePosition(t);    // replace with your spline API if different
            Vector3 screenPos = uiCamera.WorldToScreenPoint(worldPos);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out Vector2 local))
                nodeRects[i].anchoredPosition = local;
        }
    }
}
