using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Custom inspector for SlideDeck to make manual slide management easier.
/// Provides utilities for reordering, inserting, and managing slides.
/// </summary>
[CustomEditor(typeof(SlideDeck))]
public class SlideDeckEditor : Editor
{
    private SerializedProperty slidesProperty;
    
    private void OnEnable()
    {
        slidesProperty = serializedObject.FindProperty("slides");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SlideDeck deck = (SlideDeck)target;
        
        // Header
        EditorGUILayout.LabelField("Slide Deck", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Utilities section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Sort by Number"))
        {
            SortSlidesByNumber(deck);
        }
        if (GUILayout.Button("Validate Keys"))
        {
            ValidateKeys(deck);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Remove Null Slides"))
        {
            RemoveNullSlides(deck);
        }
        if (GUILayout.Button("Clear All"))
        {
            if (EditorUtility.DisplayDialog("Clear All Slides",
                "Are you sure you want to remove all slides from this deck?",
                "Yes", "Cancel"))
            {
                deck.slides.Clear();
                EditorUtility.SetDirty(deck);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Stats
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Slides: {deck.slides.Count}");
        int nullCount = deck.slides.Count(s => s.slidePrefab == null);
        if (nullCount > 0)
        {
            EditorGUILayout.LabelField($"Null Prefabs: {nullCount}", EditorStyles.miniLabel);
        }
        int duplicateKeys = deck.slides.GroupBy(s => s.key).Where(g => g.Count() > 1).Count();
        if (duplicateKeys > 0)
        {
            EditorGUILayout.LabelField($"⚠ Duplicate Keys: {duplicateKeys}", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Default inspector for slides list
        EditorGUILayout.PropertyField(slidesProperty, true);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void SortSlidesByNumber(SlideDeck deck)
    {
        deck.slides = deck.slides
            .OrderBy(s => ExtractNumber(s.key))
            .ToList();
        
        EditorUtility.SetDirty(deck);
        Debug.Log($"Sorted {deck.slides.Count} slides by number");
    }
    
    private void ValidateKeys(SlideDeck deck)
    {
        int issues = 0;
        
        // Check for null or empty keys
        for (int i = 0; i < deck.slides.Count; i++)
        {
            if (string.IsNullOrEmpty(deck.slides[i].key))
            {
                Debug.LogWarning($"Slide at index {i} has empty key");
                issues++;
            }
        }
        
        // Check for duplicates
        var duplicates = deck.slides
            .GroupBy(s => s.key)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        
        foreach (var key in duplicates)
        {
            Debug.LogWarning($"Duplicate key found: {key}");
            issues++;
        }
        
        if (issues == 0)
        {
            Debug.Log("✓ All keys are valid and unique");
        }
        else
        {
            Debug.LogWarning($"Found {issues} key validation issues");
        }
    }
    
    private void RemoveNullSlides(SlideDeck deck)
    {
        int beforeCount = deck.slides.Count;
        deck.slides = deck.slides.Where(s => s.slidePrefab != null).ToList();
        int removed = beforeCount - deck.slides.Count;
        
        if (removed > 0)
        {
            EditorUtility.SetDirty(deck);
            Debug.Log($"Removed {removed} null slide(s)");
        }
        else
        {
            Debug.Log("No null slides found");
        }
    }
    
    private int ExtractNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
            return int.MaxValue;
        
        var match = System.Text.RegularExpressions.Regex.Match(text, @"(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
        {
            return number;
        }
        
        return int.MaxValue;
    }
}
