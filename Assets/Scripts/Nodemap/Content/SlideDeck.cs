using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Content/Slide Deck")]
public class SlideDeck : ScriptableObject
{
    public List<SlideRef> slides = new();
}

[System.Serializable]
public class SlideRef
{
    public string key;               // Unique slide ID (used by quizzes)
    public GameObject slidePrefab;   // The prefab with your manual layout
}
