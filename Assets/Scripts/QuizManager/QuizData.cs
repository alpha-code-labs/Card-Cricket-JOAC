using UnityEngine;

[CreateAssetMenu(fileName = "New Quiz Data", menuName = "Quiz System/Quiz Data")]
public class QuizData : ScriptableObject
{
    [System.Serializable]
    public class Question
    {
        [Header("Question Content")]
        public string questionText;

        [Header("Answer Options")]
        public string[] options = new string[4];

        [Header("Correct Answer")]
        public int correctAnswerIndex;
    }

    [Header("Quiz Configuration")]
    public int totalQuestions = 50;
    public int maxWrongAnswers = 5;

    [Header("Questions List")]
    public Question[] questions;

    [Header("Background Assets")]
    public AudioClip backgroundMusic;
    public Sprite backgroundImage;
}