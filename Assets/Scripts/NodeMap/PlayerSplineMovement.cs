using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using System.Collections.Generic;

public class PlayerSplineMovement : MonoBehaviour
{
    [Header("Spline Setup")]
    [SerializeField] private SplineContainer spline;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Wheel Setup")]
    [SerializeField] private Transform frontWheel;
    [SerializeField] private Transform rearWheel;
    [SerializeField] private float wheelSpinSpeed = 360f;

    [Header("Smoke VFX")]
    [SerializeField] private ParticleSystem smokeParticles;

    [Header("Node Setup")]
    [SerializeField] private List<GameObject> nodeMarkers = new List<GameObject>();
    [SerializeField] private List<Sprite> normalNodeSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> activeNodeSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> completeNodeSprites;


    private List<SplineStop> stops = new List<SplineStop>();
    private int currentStopIndex = 0;
    private bool isMoving = false;
    private bool isMovingForward = true;
    private int currentActiveNode = -1;
    private ParticleSystem.MainModule smokeMain;

    private void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ForceGenerateStops();
            return;
        }
#endif
        GenerateStops();
    }

    void Start()
    {
        if (smokeParticles != null)
            smokeMain = smokeParticles.main;

        transform.position = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(0f));

        if (stops.Count > 0)
        {
            StartCoroutine(StartSequence());
        }
    }

    private IEnumerator StartSequence()
    {
        // Move car from spline start (T=0) to first node (1/7)
        yield return MoveAlongSpline(0f, stops[0].splinePercent);

        // After arriving at first node
        currentStopIndex = 0;
        SetNodeToActive(currentStopIndex);
        NodeMapGameManager.Instance.SetActiveNode(1);

    }


    void Update()
    {
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                TryMoveToNode(currentStopIndex + 1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                TryMoveToNode(currentStopIndex - 1);
            }
        }
    }

    void TryMoveToNode(int targetNode)
    {
        if (targetNode < 0 || targetNode >= stops.Count || isMoving)
            return;

        isMovingForward = targetNode > currentStopIndex;
        StartCoroutine(MoveToNode(targetNode));
    }

    IEnumerator MoveToNode(int targetNode)
    {
        isMoving = true;

        if (NodeMapGameManager.Instance != null)
        {
            NodeMapGameManager.Instance.SetActiveNode(-1); // -1 = no active node
        }

        if (currentActiveNode != -1)
            SetNodeToNormal(currentActiveNode);

        float startT = stops[currentStopIndex].splinePercent;
        float targetT = stops[targetNode].splinePercent;

        Vector3 startPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(startT));
        transform.position = startPos;

        yield return null;

        yield return MoveAlongSpline(startT, targetT);

        currentStopIndex = targetNode;
        isMoving = false;

        SetNodeToActive(currentStopIndex);

        NodeMapGameManager.Instance.SetActiveNode(currentStopIndex + 1);

    }

    IEnumerator MoveAlongSpline(float startT, float endT)
    {
        if (smokeParticles != null && !smokeParticles.isPlaying)
            smokeParticles.Play();

        Vector3 startPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(startT));
        Vector3 endPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(endT));
        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = EaseInOut(progress);
            float splineT = Mathf.Lerp(startT, endT, easedProgress);

            Vector3 worldPos = spline.transform.TransformPoint((Vector3)spline.EvaluatePosition(splineT));
            Vector3 tangent = spline.transform.TransformDirection((Vector3)spline.EvaluateTangent(splineT)).normalized;

            float bounce = Mathf.Sin(Time.time * 5f) * 0.05f;
            worldPos.y += bounce;

            transform.position = worldPos;

            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            float wheelDirection = isMovingForward ? 1f : -1f;
            RotateWheels(wheelDirection);

            yield return null;
        }

        if (smokeParticles != null && smokeParticles.isPlaying)
            smokeParticles.Stop();
    }

    public void SetNodeToComplete(int nodeIndex)
    {
        if (nodeMarkers.Count > nodeIndex && completeNodeSprites.Count > nodeIndex)
        {
            GameObject marker = nodeMarkers[nodeIndex];
            if (marker != null)
            {
                SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
                NodeHoverHandler handler = marker.GetComponent<NodeHoverHandler>();

                if (sr != null && completeNodeSprites[nodeIndex] != null)
                {
                    StartCoroutine(AnimateToComplete(sr, completeNodeSprites[nodeIndex]));
                }

                if (handler != null)
                {
                    handler.SetClickable(false); // ✅ Mark node as no longer clickable after completion
                }
            }
        }
    }


    private void RotateWheels(float direction)
    {
        if (frontWheel != null)
            frontWheel.Rotate(Vector3.forward, -wheelSpinSpeed * Time.deltaTime * direction);

        if (rearWheel != null)
            rearWheel.Rotate(Vector3.forward, -wheelSpinSpeed * Time.deltaTime * direction);
    }

    float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void SetNodeToNormal(int nodeIndex)
    {
        if (nodeMarkers.Count > nodeIndex && normalNodeSprites.Count > nodeIndex)
        {
            GameObject marker = nodeMarkers[nodeIndex];
            if (marker != null)
            {
                SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
                if (sr != null && normalNodeSprites[nodeIndex] != null)
                {
                    StartCoroutine(AnimateToNormal(sr, normalNodeSprites[nodeIndex]));
                }
            }
        }
    }


    private void SetNodeToActive(int nodeIndex)
    {
        if (nodeMarkers.Count > nodeIndex && activeNodeSprites.Count > nodeIndex)
        {
            GameObject marker = nodeMarkers[nodeIndex];
            if (marker != null)
            {
                SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
                NodeHoverHandler handler = marker.GetComponent<NodeHoverHandler>();

                if (sr != null && activeNodeSprites[nodeIndex] != null)
                {
                    StartCoroutine(AnimateToActive(sr, activeNodeSprites[nodeIndex]));
                    currentActiveNode = nodeIndex;
                }

                if (handler != null)
                {
                    handler.SetClickable(true); // 🔥 Make node clickable
                }
            }
        }
    }



    private IEnumerator AnimateToActive(SpriteRenderer sr, Sprite activeSprite)
    {
        float popUpDuration = 0.1f;   // Faster pop up
        float popDownDuration = 0.15f; // Slower settle back
        float t = 0f;

        Transform markerTransform = sr.transform;
        Vector3 originalScale = markerTransform.localScale;
        Vector3 poppedScale = originalScale * 1.1f;

        // Fast Pop Up
        while (t < popUpDuration)
        {
            t += Time.deltaTime;
            float scaleT = Mathf.Lerp(0f, 1f, t / popUpDuration);
            markerTransform.localScale = Vector3.Lerp(originalScale, poppedScale, scaleT);
            yield return null;
        }

        // Swap to active sprite
        sr.sprite = activeSprite;

        // Slower Settle Down
        t = 0f;
        while (t < popDownDuration)
        {
            t += Time.deltaTime;
            float scaleT = Mathf.Lerp(0f, 1f, t / popDownDuration);
            markerTransform.localScale = Vector3.Lerp(poppedScale, originalScale, scaleT);
            yield return null;
        }

        markerTransform.localScale = originalScale;
    }


    private IEnumerator AnimateToNormal(SpriteRenderer sr, Sprite normalSprite)
    {
        float popUpDuration = 0.1f;
        float popDownDuration = 0.15f;
        float t = 0f;

        Transform markerTransform = sr.transform;
        Vector3 originalScale = markerTransform.localScale;
        Vector3 poppedScale = originalScale * 1.1f;

        // Fast Pop Up
        while (t < popUpDuration)
        {
            t += Time.deltaTime;
            float scaleT = Mathf.Lerp(0f, 1f, t / popUpDuration);
            markerTransform.localScale = Vector3.Lerp(originalScale, poppedScale, scaleT);
            yield return null;
        }

        // Swap to normal sprite
        sr.sprite = normalSprite;

        // Slow Settle Down
        t = 0f;
        while (t < popDownDuration)
        {
            t += Time.deltaTime;
            float scaleT = Mathf.Lerp(0f, 1f, t / popDownDuration);
            markerTransform.localScale = Vector3.Lerp(poppedScale, originalScale, scaleT);
            yield return null;
        }

        markerTransform.localScale = originalScale;
    }

    private IEnumerator AnimateToComplete(SpriteRenderer sr, Sprite completeSprite)
    {
        float popUpDuration = 0.1f;
        float popDownDuration = 0.15f;
        float t = 0f;

        Transform markerTransform = sr.transform;
        Vector3 originalScale = markerTransform.localScale;
        Vector3 poppedScale = originalScale * 1.1f;

        // Fast pop up
        while (t < popUpDuration)
        {
            t += Time.deltaTime;
            float scaleT = Mathf.Lerp(0f, 1f, t / popUpDuration);
            markerTransform.localScale = Vector3.Lerp(originalScale, poppedScale, scaleT);
            yield return null;
        }

        // Swap to complete sprite
        sr.sprite = completeSprite;

        // Slow settle down
        t = 0f;
        while (t < popDownDuration)
        {
            t += Time.deltaTime;
            float scaleT = Mathf.Lerp(0f, 1f, t / popDownDuration);
            markerTransform.localScale = Vector3.Lerp(poppedScale, originalScale, scaleT);
            yield return null;
        }

        markerTransform.localScale = originalScale;
    }

    private void GenerateStops()
    {
        stops.Clear();
        int numberOfStops = 6;
        for (int i = 1; i <= numberOfStops; i++)
        {
            float percent = i / 7f;
            stops.Add(new SplineStop { splinePercent = percent });
        }
    }

#if UNITY_EDITOR
    public void ForceGenerateStops()
    {
        GenerateStops();
    }
#endif

    public List<SplineStop> GetStops()
    {
        return stops;
    }

    public SplineContainer GetSpline()
    {
        return spline;
    }
}
