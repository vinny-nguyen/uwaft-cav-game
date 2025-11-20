using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure for quiz questions loaded from JSON.
/// </summary>
[Serializable]
public class QuizData
{
    public List<QuizQuestion> questions = new();
}

/// <summary>
/// Represents a single multiple-choice quiz question.
/// </summary>
[Serializable]
public class QuizQuestion
{
    [Tooltip("The question text to display")]
    public string question;

    [Tooltip("List of answer options (no letter prefixes like 'A.')")]
    public List<string> options = new();

    [Tooltip("Index of the correct answer (0-based)")]
    public int correctIndex;

    [Tooltip("Slide key to jump to for review when answered incorrectly")]
    public string relatedSlideKey;
}
