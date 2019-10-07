using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Answer
{
    public string answerText;
    public bool isCorrect;
}

[System.Serializable]
public class Question
{
    public string questionText;
    public Answer[] answers;
}
