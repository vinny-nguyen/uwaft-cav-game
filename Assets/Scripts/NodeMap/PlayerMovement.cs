// using UnityEngine;
// using System.Collections;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;
// using System.Collections.Generic;
// using System;

// public class PlayerMovement : MonoBehaviour
// {
//     [Header("Path Settings")]
//     public LineRenderer path;
//     public Transform mainNodesParent;
//     public float speed = 3f;

//     [Header("UI References")]
//     public PopupManager popupManager;
//     public SlideManager slideManager;
//     public Button showPopupButton;
//     public Button driveButton;

//     [Header("Car Components")]
//     public WheelLogic frontWheel;
//     public WheelLogic rearWheel;
//     public float wheelSpinSpeed = 200f;

//     [Header("Node Settings")]
//     public ParticleSystem completionParticles;

//     [Header("Animation Settings")]
//     public float pulseIntensity = 0.2f;
//     public float pulseSpeed = 5f;
//     public float shakeDuration = 0.5f;
//     public float shakeMagnitude = 0.1f; // Increased for more visible shake

//     private Dictionary<int, SpriteRenderer> nodeRenderers = new Dictionary<int, SpriteRenderer>();
//     private bool isMoving = false;
//     private bool isMovingForward = true;
//     private Transform[] mainNodes;
//     private int[] mainNodeIndices;
//     private HashSet<int> completedNodes = new HashSet<int>();

   
//     [Header("Main Menu")]
//     public Button mainMenuButton;
//     public string mainMenuSceneName = "MainMenu"; // Set this to your actual main menu scene name

//     public int CurrentNode { get; private set; }

//     void Start()
//     {

//         if (mainMenuButton != null)
//         {
//             mainMenuButton.onClick.AddListener(GoToMainMenu);
//         }

//         CurrentNode = 0;
//         InitializeNodes();
//         showPopupButton.onClick.AddListener(ShowCurrentNodePopup);
//         driveButton.onClick.AddListener(StartDrivingGame);
//         showPopupButton.gameObject.SetActive(true);
//         driveButton.gameObject.SetActive(true);

//         for (int i = 0; i < mainNodes.Length; i++)
//         {
//             var renderer = mainNodes[i].GetComponent<SpriteRenderer>();
//             if (renderer != null) nodeRenderers.Add(i, renderer);
//         }
//         UpdateNodeVisuals();
//     }

//     void Update()
//     {
//         if (!popupManager.IsPopupOpen && !isMoving)
//         {
//             if (Input.GetKeyDown(KeyCode.RightArrow))
//             {
//                 TryMoveToNode(CurrentNode + 1);
//             }
//             if (Input.GetKeyDown(KeyCode.LeftArrow))
//             {
//                 TryMoveToNode(CurrentNode - 1);
//             }
//         }
//         UpdateButtonVisibility();
//     }

//     public void GoToMainMenu()
//     {
//         // Optional: Save progress if needed
//         PlayerPrefs.Save();

//         // Load main menu scene
//         SceneManager.LoadScene(mainMenuSceneName);
//     }

//     void TryMoveToNode(int targetNode)
//     {
//         if (targetNode < 0 || targetNode >= mainNodes.Length || isMoving)
//             return;

//         if (targetNode > CurrentNode)
//         {
//             // Check if previous nodes are completed
//             if (CurrentNode > 0 && !completedNodes.Contains(CurrentNode - 1))
//             {
//                 StartCoroutine(ShakeNode(mainNodes[CurrentNode - 1]));
//                 return;
//             }

//             if (!completedNodes.Contains(CurrentNode))
//             {
//                 StartCoroutine(ShakeNode(mainNodes[CurrentNode])); // Shake current node instead of pulse
//                 return;
//             }
//         }

//         isMovingForward = targetNode > CurrentNode;
//         StartCoroutine(isMovingForward ? MoveToNextNode(targetNode) : MoveToPreviousNode(targetNode));
//     }

//     IEnumerator PulseNode(SpriteRenderer renderer)
//     {
//         Color originalColor = renderer.color;
//         float timer = 0f;

//         while (timer < 1f)
//         {
//             timer += Time.deltaTime * pulseSpeed;
//             float pulseValue = Mathf.PingPong(timer, pulseIntensity);
//             renderer.color = new Color(
//                 originalColor.r + pulseValue,
//                 originalColor.g + pulseValue,
//                 originalColor.b,
//                 originalColor.a
//             );
//             yield return null;
//         }
//         renderer.color = originalColor;
//     }

//     IEnumerator ShakeNode(Transform nodeTransform)
//     {
//         Vector3 originalPos = nodeTransform.localPosition;
//         float elapsed = 0f;

//         // More pronounced shake effect
//         while (elapsed < shakeDuration)
//         {
//             // More dramatic shaking with larger magnitude
//             float x = originalPos.x + Mathf.Sin(Time.time * 50f) * shakeMagnitude;
//             float y = originalPos.y + Mathf.Sin(Time.time * 45f) * shakeMagnitude * 0.7f;

//             nodeTransform.localPosition = new Vector3(x, y, originalPos.z);
//             elapsed += Time.deltaTime;
//             yield return null;
//         }
//         nodeTransform.localPosition = originalPos;
//     }

//     void InitializeNodes()
//     {
//         if (mainNodesParent == null)
//         {
//             Debug.LogError("MainNodesParent is not assigned!");
//             return;
//         }

//         mainNodes = new Transform[mainNodesParent.childCount];
//         for (int i = 0; i < mainNodesParent.childCount; i++)
//         {
//             mainNodes[i] = mainNodesParent.GetChild(i);
//         }

//         mainNodeIndices = new int[mainNodes.Length];
//         for (int i = 0; i < mainNodes.Length; i++)
//         {
//             mainNodeIndices[i] = FindClosestPointIndex(mainNodes[i].position);
//         }
//     }

//     IEnumerator MoveToNextNode(int targetNode)
//     {
//         isMoving = true;
//         showPopupButton.gameObject.SetActive(false);
//         driveButton.gameObject.SetActive(false);

//         int startIndex = mainNodeIndices[CurrentNode];
//         int targetIndex = mainNodeIndices[targetNode];

//         for (int i = startIndex; i <= targetIndex; i++)
//         {
//             yield return MoveAlongPath(i);
//         }

//         CurrentNode = targetNode;
//         isMoving = false;
//         UpdateNodeVisuals();
//         showPopupButton.gameObject.SetActive(true);
//         driveButton.gameObject.SetActive(true);
//     }

//     IEnumerator MoveToPreviousNode(int targetNode)
//     {
//         isMoving = true;
//         showPopupButton.gameObject.SetActive(false);
//         driveButton.gameObject.SetActive(false);

//         int startIndex = mainNodeIndices[CurrentNode];
//         int targetIndex = mainNodeIndices[targetNode];

//         for (int i = startIndex; i >= targetIndex; i--)
//         {
//             yield return MoveAlongPath(i);
//         }

//         CurrentNode = targetNode;
//         isMoving = false;
//         UpdateNodeVisuals();
//         showPopupButton.gameObject.SetActive(true);
//         driveButton.gameObject.SetActive(true);
//     }

//     public void CompleteNode(int nodeIndex)
//     {
//         completedNodes.Add(nodeIndex);
//         Sprite completedSprite = Resources.Load<Sprite>($"Nodes/node_{nodeIndex + 1}_complete");

//         if (nodeRenderers.TryGetValue(nodeIndex, out SpriteRenderer renderer) && completedSprite != null)
//         {
//             StartCoroutine(AnimateNodeCompletion(renderer, completedSprite));

//             if (completionParticles != null)
//             {
//                 completionParticles.transform.position = renderer.transform.position;
//                 completionParticles.Play();
//             }
//         }

//         UpdateNodeVisuals();
//     }

//     void UpdateNodeVisuals()
//     {
//         foreach (var kvp in nodeRenderers)
//         {
//             string state;
//             if (kvp.Key == CurrentNode)
//                 state = "current";
//             else if (completedNodes.Contains(kvp.Key))
//                 state = "complete";
//             else
//                 state = "incomplete";

//             Sprite sprite = Resources.Load<Sprite>($"Nodes/node_{kvp.Key + 1}_{state}");
//             if (sprite != null) kvp.Value.sprite = sprite;
//         }
//     }

//     IEnumerator MoveAlongPath(int pointIndex)
//     {
//         Vector3 startPos = transform.position;
//         Vector3 targetPos = path.GetPosition(pointIndex);
//         float distance = Vector3.Distance(startPos, targetPos);
//         float duration = distance / speed;
//         float time = 0f;

//         while (time < duration)
//         {
//             time += Time.deltaTime;
//             float t = EaseInOut(time / duration);
//             transform.position = Vector3.Lerp(startPos, targetPos, t);

//             float wheelDirection = isMovingForward ? 1 : -1;
//             float rotationAmount = wheelSpinSpeed * Time.deltaTime * -wheelDirection;
//             frontWheel.transform.Rotate(Vector3.forward, rotationAmount);
//             rearWheel.transform.Rotate(Vector3.forward, rotationAmount);

//             yield return null;
//         }
//     }

//     void ShowCurrentNodePopup()
//     {
//         slideManager.SetCurrentTopic(CurrentNode);
//         popupManager.ShowPopup();
//     }

//     public void UpdateButtonVisibility()
//     {
//         bool shouldShowButtons = !popupManager.IsPopupOpen && !isMoving;
//         showPopupButton.gameObject.SetActive(shouldShowButtons);
//         driveButton.gameObject.SetActive(shouldShowButtons);
//     }

//     void StartDrivingGame()
//     {
//         PlayerPrefs.SetInt("CurrentUpgrade", CurrentNode);
//         PlayerPrefs.Save();
//         SceneManager.LoadScene("DrivingScene");
//     }

//     IEnumerator AnimateNodeCompletion(SpriteRenderer renderer, Sprite newSprite)
//     {
//         float duration = 0.5f;
//         float time = 0f;
//         Color startColor = renderer.color;
//         Vector3 startScale = renderer.transform.localScale;

//         while (time < duration)
//         {
//             time += Time.deltaTime;
//             renderer.color = Color.Lerp(startColor, Color.green, time / duration);
//             renderer.transform.localScale = Vector3.Lerp(startScale, startScale * 1.2f, time / duration);
//             yield return null;
//         }

//         renderer.sprite = newSprite;
//         renderer.color = Color.white;
//         renderer.transform.localScale = startScale;
//     }

//     float EaseInOut(float t) => t * t * (3f - 2f * t);

//     int FindClosestPointIndex(Vector3 position)
//     {
//         if (path.positionCount == 0)
//         {
//             Debug.LogError("LineRenderer has no points!");
//             return -1;
//         }

//         int closestIndex = 0;
//         float closestDistance = Vector3.Distance(position, path.GetPosition(0));

//         for (int i = 1; i < path.positionCount; i++)
//         {
//             float distance = Vector3.Distance(position, path.GetPosition(i));
//             if (distance < closestDistance)
//             {
//                 closestDistance = distance;
//                 closestIndex = i;
//             }
//         }
//         return closestIndex;
//     }
// }