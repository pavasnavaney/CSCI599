using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class RandomGenerator : MonoBehaviour
{
    public string category;
    private QuestionData[] questions;
    private QuestionData[] output;
    public string[] json;
    public Random number;
    public string range2;

  void Start() {
    // Set up the Editor before calling into the realtime database.
    FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://YOUR-FIREBASE-APP.firebaseio.com/");

    // Get the root reference location of the database.
    DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
    GetQuestions(category); //we have to input the category here
  }
  public void GetQuestions(string category){
          Firebase.Database.FirebaseDatabase dbInstance = Firebase.Database.FirebaseDatabase.DefaultInstance;
          
          dbInstance.GetReference(category).GetValueAsync().ContinueWith(task => {
                    if (task.IsFaulted) {
                        Debug.Log("Error in connecting to firebase")
                    }
                    else if (task.IsCompleted) {
                      DataSnapshot snapshot = task.Result;
                      
                      questions=snapshot.Children
                      Debug.Log(""+questions[0].question[0]+"kk"+questions[0].answerChoices) //testing the structure of db

                        // for adding random questions in json format
                      for (int i = 0; i < 10; i++)
                      {
                        number=Random.Range(0, questions.length)    //random number
                        output.question=questions[number].question[0]
                        output.answers=question[number].answerChoices
                        output.correctIndex=question[number].correctAnswerIndex
                        json[i] = JsonUtility.ToJson(output)
                      }

                      // write to json file
                      string filePath = Application.dataPath + '/StreamingAssets/output.json';
                      File.WriteAllText (filePath, dataAsJson);

                    //for ref
                    //   foreach ( DataSnapshot question in snapshot.Children){
                    //     IDictionary dictUser = (IDictionary)question.Value;
                    //     Debug.Log ("" + dictUser["question"] + " - " + dictUser["answerchoices"]);
                    //   }
                    }
          });
}
}
