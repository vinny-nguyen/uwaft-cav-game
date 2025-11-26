using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Content/Node Data")]
public class NodeData : ScriptableObject
{
    [Header("General Info")]
    public int id;
    public string title;

    [Header("Learning Content")]
    public SlideDeck slideDeck;          // List of slide prefabs (manual layout)
    public TextAsset quizJson;           // JSON file with quiz questions
    public List<GameObject> miniGamePrefabs; // 0–N mini-game prefabs

    [Header("Upgrade Info (optional)")]
    public string upgradeText;           // "Upgraded tires improve grip..."
    public SpriteRenderer upgradeFrame;  // Frame SpriteRenderer to copy sprite from on upgrade
    public SpriteRenderer upgradeTire;   // Tire SpriteRenderer to copy sprite from on upgrade
    
    [Header("Driving Experience")]
    [Tooltip("Name of the driving scene to load for this node (e.g., 'DrivingScene1')")]
    public string drivingSceneName;      // Scene to load when playing this node's driving game
}
