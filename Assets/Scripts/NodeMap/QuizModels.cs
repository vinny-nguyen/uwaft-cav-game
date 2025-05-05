using System;
using UnityEngine;

namespace NodeMap.Quiz
{
    /// <summary>
    /// Root container for all quiz data
    /// </summary>
    [Serializable]
    public class QuizData
    {
        public NodeQuiz[] nodes;
        
        /// <summary>
        /// Find a node quiz by its ID
        /// </summary>
        public NodeQuiz FindNodeQuizById(int nodeId)
        {
            if (nodes == null) return null;
            
            foreach (var node in nodes)
            {
                if (node.nodeId == nodeId) return node;
            }
            
            return null;
        }
    }

    /// <summary>
    /// Quiz data specific to a node
    /// </summary>
    [Serializable]
    public class NodeQuiz
    {
        public int nodeId;
        public QuizQuestion[] questions;
    }

    /// <summary>
    /// Individual quiz question with possible answers
    /// </summary>
    [Serializable]
    public class QuizQuestion
    {
        public string questionText;
        public string[] options;
        public int correctAnswerIndex;
        
        /// <summary>
        /// Check if the given answer index is correct
        /// </summary>
        public bool IsCorrectAnswer(int answerIndex)
        {
            return answerIndex == correctAnswerIndex;
        }
    }
}