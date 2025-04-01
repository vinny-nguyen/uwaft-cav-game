using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Path Settings")]
    public LineRenderer path;
    public Transform mainNodesParent;
    public float speed = 3f;

    [Header("UI References")]
    public PopupManager popupManager;
    public SlideManager slideManager;
    public Button showPopupButton;
    public Button driveButton;

    [Header("Car Components")]
    public WheelLogic frontWheel;
    public WheelLogic rearWheel;
    public float wheelSpinSpeed = 200f;

    private int currentNode = 0;
    private bool isMoving = false;
    private bool isMovingForward = true;
    private Transform[] mainNodes;
    private int[] mainNodeIndices;

    void Start()
    {
        InitializeNodes();
        showPopupButton.onClick.AddListener(ShowCurrentNodePopup);
        driveButton.onClick.AddListener(StartDrivingGame);
        showPopupButton.gameObject.SetActive(true);
        driveButton.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!popupManager.IsPopupOpen && !isMoving)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveToNode(currentNode + 1);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveToNode(currentNode - 1);
        }
        UpdateButtonVisibility();
    }

    void InitializeNodes()
    {
        if (mainNodesParent == null)
        {
            Debug.LogError("MainNodesParent is not assigned!");
            return;
        }

        mainNodes = new Transform[mainNodesParent.childCount];
        for (int i = 0; i < mainNodesParent.childCount; i++)
        {
            mainNodes[i] = mainNodesParent.GetChild(i);
        }

        mainNodeIndices = new int[mainNodes.Length];
        for (int i = 0; i < mainNodes.Length; i++)
        {
            mainNodeIndices[i] = FindClosestPointIndex(mainNodes[i].position);
        }
    }

    void MoveToNode(int targetNode)
    {
        if (targetNode < 0 || targetNode >= mainNodes.Length || isMoving) return;
        isMovingForward = targetNode > currentNode;
        StartCoroutine(isMovingForward ? MoveToNextNode(targetNode) : MoveToPreviousNode(targetNode));
    }

    IEnumerator MoveToNextNode(int targetNode)
    {
        isMoving = true;
        showPopupButton.gameObject.SetActive(false);
        driveButton.gameObject.SetActive(false);

        int startIndex = mainNodeIndices[currentNode];
        int targetIndex = mainNodeIndices[targetNode];

        for (int i = startIndex; i <= targetIndex; i++)
        {
            yield return MoveAlongPath(i);
        }

        currentNode = targetNode;
        isMoving = false;
        showPopupButton.gameObject.SetActive(true);
        driveButton.gameObject.SetActive(true);
    }

    IEnumerator MoveToPreviousNode(int targetNode)
    {
        isMoving = true;
        showPopupButton.gameObject.SetActive(false);
        driveButton.gameObject.SetActive(false);

        int startIndex = mainNodeIndices[currentNode];
        int targetIndex = mainNodeIndices[targetNode];

        for (int i = startIndex; i >= targetIndex; i--)
        {
            yield return MoveAlongPath(i);
        }

        currentNode = targetNode;
        isMoving = false;
        showPopupButton.gameObject.SetActive(true);
        driveButton.gameObject.SetActive(true);
    }

    IEnumerator MoveAlongPath(int pointIndex)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = path.GetPosition(pointIndex);
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / speed;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = EaseInOut(time / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            // Wheel rotation only (no directional rotation)
            float wheelDirection = isMovingForward ? 1 : -1;
            float rotationAmount = wheelSpinSpeed * Time.deltaTime * -wheelDirection;
            frontWheel.transform.Rotate(Vector3.forward, rotationAmount);
            rearWheel.transform.Rotate(Vector3.forward, rotationAmount);

            yield return null;
        }
    }

    void ShowCurrentNodePopup()
    {
        slideManager.SetCurrentTopic(currentNode);
        popupManager.ShowPopup();
    }

    public void UpdateButtonVisibility()
    {
        bool shouldShowButtons = !popupManager.IsPopupOpen && !isMoving;
        showPopupButton.gameObject.SetActive(shouldShowButtons);
        driveButton.gameObject.SetActive(shouldShowButtons);
    }

    void StartDrivingGame()
    {
        PlayerPrefs.SetInt("CurrentUpgrade", currentNode);
        PlayerPrefs.Save();
        SceneManager.LoadScene("DrivingScene");
    }

    float EaseInOut(float t) => t * t * (3f - 2f * t);

    int FindClosestPointIndex(Vector3 position)
    {
        if (path.positionCount == 0)
        {
            Debug.LogError("LineRenderer has no points!");
            return -1;
        }

        int closestIndex = 0;
        float closestDistance = Vector3.Distance(position, path.GetPosition(0));

        for (int i = 1; i < path.positionCount; i++)
        {
            float distance = Vector3.Distance(position, path.GetPosition(i));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }
}