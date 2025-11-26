using System;
using System.Collections.Generic;
using UnityEngine;

// Data structure for quiz questions loaded from JSON
[Serializable]
public class QuizData
{
    public List<QuizQuestion> questions = new();
}

// Represents a single multiple-choice quiz question
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
