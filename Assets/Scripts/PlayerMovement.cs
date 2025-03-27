using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Add this for scene management


public class PlayerMovement : MonoBehaviour
{
    public LineRenderer path;
    public Transform mainNodesParent;
    public PopupManager popupManager;
    public SlideManager slideManager;
    public Button showPopupButton;
    public Button driveButton; // Assign this in Inspector


    private int currentNode = 0;
    private bool isMoving = false;
    public float speed = 3f;

    private Transform[] mainNodes;
    private int[] mainNodeIndices;

    void Start()
    {
        InitializeNodes();
        showPopupButton.onClick.AddListener(ShowCurrentNodePopup);
        driveButton.onClick.AddListener(StartDrivingGame);

        // Show both buttons at start
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
    }

    void StartDrivingGame()
    {
        // Save the current node (upgrade level) to PlayerPrefs
        PlayerPrefs.SetInt("CurrentUpgrade", currentNode);
        PlayerPrefs.Save();

        // Load the driving scene
        SceneManager.LoadScene("DrivingScene"); // Replace with your actual scene name
    }

    void InitializeNodes()
    {
        if (mainNodesParent == null)
        {
            UnityEngine.Debug.LogError("MainNodesParent is not assigned!");
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

        StartCoroutine(targetNode > currentNode ?
            MoveToNextNode(targetNode) :
            MoveToPreviousNode(targetNode));
    }

    IEnumerator MoveToNextNode(int targetNode)
    {
        isMoving = true;
        // Hide both buttons while moving
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
        // Show both buttons when movement completes
        showPopupButton.gameObject.SetActive(true);
        driveButton.gameObject.SetActive(true);
    }

    IEnumerator MoveToPreviousNode(int targetNode)
    {
        isMoving = true;
        // Hide both buttons while moving
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
        // Show both buttons when movement completes
        showPopupButton.gameObject.SetActive(true);
        driveButton.gameObject.SetActive(true);
    }

    void ShowCurrentNodePopup()
    {
        slideManager.SetCurrentTopic(currentNode);
        popupManager.ShowPopup();
    }

    IEnumerator MoveAlongPath(int pointIndex)
    {
        Vector3 start = transform.position;
        Vector3 target = path.GetPosition(pointIndex);
        float distance = Vector3.Distance(start, target);
        float duration = distance / speed;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = EaseInOut(time / duration);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
    }

    float EaseInOut(float t) => t * t * (3f - 2f * t);

    int FindClosestPointIndex(Vector3 position)
    {
        if (path.positionCount == 0)
        {
            UnityEngine.Debug.LogError("LineRenderer has no points!");
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