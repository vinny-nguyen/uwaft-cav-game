[System.Serializable]
public class QuizData
{
    public NodeQuiz[] nodes;
}

[System.Serializable]
public class NodeQuiz
{
    public int nodeId;
    public QuizQuestion[] questions;
}

[System.Serializable]
public class QuizQuestion
{
    public string questionText;
    public string[] options;
    public int correctAnswerIndex;
}