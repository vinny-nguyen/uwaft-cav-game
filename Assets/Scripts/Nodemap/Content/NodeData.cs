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
    public string upgradeText;           // “Upgraded tires improve grip...”
    public Sprite upgradeFrame;          // Frame sprite to swap on upgrade
    public Sprite upgradeTire;           // Tire sprite to swap on upgrade
}
