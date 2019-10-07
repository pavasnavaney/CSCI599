using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public Question[] questions;
    public GameObject mainCanvas;
    public GameObject resultCanvas;
    public Text resultText;
    public Text player1Score;
    public Text timer;
    [SerializeField] private Text questionText;
    [SerializeField] private Text answerText1;
    [SerializeField] private Text answerText2;
    [SerializeField] private Text answerText3;
    [SerializeField] private Text answerText4;
    private int round = 0;
    private int score = 0;
    private float roundTime = 10f;
    
    void Start() {
        resultCanvas.SetActive(false);
        setQuestion();
        InvokeRepeating("showTimer", 1.0f, 1.0f);
    }

    void Update() {
        player1Score.text = score.ToString() + "\nStarter";
        timer.text = roundTime.ToString();   
    }
    
    void setQuestion() {
        questionText.text = questions.ToList<Question>()[round].questionText;
        answerText1.text = questions.ToList<Question>()[round].answers[0].answerText;
        answerText2.text = questions.ToList<Question>()[round].answers[1].answerText;
        answerText3.text = questions.ToList<Question>()[round].answers[2].answerText;
        answerText4.text = questions.ToList<Question>()[round].answers[3].answerText;
    }
    
    public void checkAnswer(Button button) {
        if(questions.ToList<Question>()[round].answers[int.Parse(button.name)-1].isCorrect) {
            print("coo");
            StartCoroutine(showResult());
            resultCanvas.GetComponent<Image>().color = new Color32(28,156,15,255);
            resultText.text = "You got it!\n\n+10";
            score += 10;

        } else {
            print("wrong");
            StartCoroutine(showResult());
            resultCanvas.GetComponent<Image>().color = new Color32(156,26,16,255);
            resultText.text = "Whoopsie!";
        }
    }
    
    IEnumerator showResult () {
        resultCanvas.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        resultCanvas.SetActive(false);
        round += 1;
        roundTime = 10f;
        if(round == questions.Length) {
            round = 0;
        }
        setQuestion();
     }

     void showTimer() {
        roundTime--;
        if(roundTime <= 0) {
            StartCoroutine(showResult());
            resultCanvas.GetComponent<Image>().color = new Color32(16,81,156,255);
            resultText.text = "Time out!";
        }
    }
 
}
