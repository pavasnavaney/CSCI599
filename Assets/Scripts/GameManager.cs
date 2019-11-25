using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.IO; 
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine.Networking;
using System.Threading.Tasks;
// using UnityEditor;

public class GameManager : MonoBehaviour
{
    public Camera mainCamera;
    public Camera secCamera;
    private Question q = null;
    private Data userData = null;
    public GameObject mainCanvas;
    public GameObject resultCanvas;
    public GameObject resBack;
    public GameObject endCanvas;
    public Text resultText;
    public Text playerName;
    public Text playerScore;
    public Text playerTitle;
    public Text otherPlayerName;
    public Text otherPlayerScore;
    public Text otherPlayerTitle;
    public Text timer;
    public Image logo;
    public Image loadingImage;
    public Text loadingText;
    public Image timerBar;
    public Image playerPic;
    public Image otherPlayerPic;
    public GameObject wrongSign;
    public GameObject playerWrongSign;
    public Slider playerSlider;
    public Slider otherPlayerSlider;
	
	//User
	[SerializeField] public InputField emailInputField, usernameInputField;
	private string email="",username="";
	
    [SerializeField] private Text questionText;
    [SerializeField] private Text answerText1;
    [SerializeField] private Text answerText2;
    [SerializeField] private Text answerText3;
    [SerializeField] private Text answerText4;
    [SerializeField] private Button startBtn;
    private int round = 0;
    private int score = 0;
    private int otherScore = 0;
    private static float roundTime = 20f;
    private float roundTimer = roundTime;

    private string serverIP = "69.51.20.25";
    private System.Int32 serverPort = 8000;
    private TcpClient socketConnection; 	
	private Thread clientThread; 	
   
    byte[] data = new byte[1024];
    string receiveMsg = "";
    bool conReady = false;
    private bool questionSet = false;
    private string uid;
    private string otherUid = "";
    private int status = 0;
    private bool wrong = false;
    private bool ready;
    Sprite defaultimg;
    private string filePath;
    private string userdata;

    IDictionary<string, string> dict = new Dictionary<string, string>()
    {
        {"0","Explorer"},
        {"460c00440b52418fbb084cfebe8d0b12","Achiever"},
        {"ebadfd2b6c0597ca351bafe1c32c9691", "Champion"}
        
    };


    void Start() {
        filePath = Path.Combine(Application.dataPath, "userdata.json");
        readJSON();
        connectToServer();
        defaultimg = Resources.Load<Sprite>("default_pic");
        mainCamera.enabled = false;
        secCamera.enabled = true;
        loadingImage.enabled = false;
        loadingText.enabled = false;
		
        startBtn.GetComponent<Button>().onClick.AddListener(() => { startGameButton(); }); 
        // readJSON();
        // resultCanvas.SetActive(false);
        uid = SystemInfo.deviceUniqueIdentifier; 
        InvokeRepeating("showTimer", 1.0f, 1.0f);

        Debug.Log("server ip: " + serverIP + "    server port: " + serverPort);
        
    }

    void Update() {
        if (status > 0)
        {
            playerName.text = uid.Substring(0,5);
            playerScore.text = score.ToString();
            playerTitle.text = dict.ContainsKey(uid) ? dict[uid] : dict["0"];
            otherPlayerName.text = otherUid.Length > 5 ? otherUid.Substring(0,5) : otherUid;
            otherPlayerScore.text = otherScore.ToString();
            otherPlayerTitle.text = dict.ContainsKey(otherUid) ? dict[otherUid] : dict["0"];
            var oimg = Resources.Load<Sprite>(otherUid);
            var pimg = Resources.Load<Sprite>(uid);
                
                if (pimg != null) {
                    playerPic.sprite = pimg;
                } else {
                    playerPic.sprite = defaultimg;
                }
            if (oimg != null) {
                otherPlayerPic.sprite = oimg;
            } else {
                otherPlayerPic.sprite = defaultimg;
            }
            playerSlider.value = score;
            otherPlayerSlider.value = otherScore;
            timer.text = roundTimer.ToString();
            timerBar.fillAmount = roundTimer/roundTime;
            if (status == 1)
            {
                roundTimer = roundTime;
                timerBar.fillAmount = roundTimer/roundTime;
                secCamera.enabled = false;
                mainCamera.enabled = true;
                loadingImage.enabled = false;
                loadingText.enabled = false;
                startBtn.enabled = false;
                logo.enabled = false;
                status = 2;
            } else if(status == 3)
            {
                StartCoroutine(showResult());
                resultText.color = new Color32(156,26,16,255);
                resultText.text = "other player got it!";
                status = -1;
            } else if(status == 4)
            {
                wrongSign.SetActive(true);
            } else if(status == 5)
            {
                endCanvas.SetActive(true);
                socketConnection.Close();
                clientThread.Abort();
            }
        } else
        {
            if(status == -5) {
                loadingImage.transform.Rotate(0, 0, 3);
            }
            
             
        }
    
        if (q != null && questionSet == false) {
            setQuestion();
            wrongSign.SetActive(false);
            playerWrongSign.SetActive(false);
        }
    }

    public void restart() {

    }
    
    void setQuestion() {
        questionText.text = q.question.Replace("???", "'");
        answerText1.text = q.answerChoices[0];
        answerText2.text = q.answerChoices[1];
        answerText3.text = q.answerChoices[2];
        answerText4.text = q.answerChoices[3];
        questionSet = true;
        GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach(GameObject go in gos)
        {
            if(go.layer == 10) {
                go.GetComponent<Image>().color =new Color32(79,79,79,255);
                go.GetComponent<Button>().interactable = true;
            }
        }
    }
    
    void checkAnswer(Button button) {
        if(!wrong) {
            if(int.Parse(button.name)-1 == q.correctAnswerIndex) {
                score += 10;
                StartCoroutine(showResult());
                resBack.GetComponent<Image>().color = new Color32(68,162,64,255);
                // resultText.color = new Color32(78,255,46,255);
                resultText.text = "Correct!\n\n+10";
                userData.score = score;
                userData.correctAnswers += 1;
				button.GetComponent<Image>().color = new Color32(0,255,0,255);
                sendMessage("next:"+score);
            } else {
                wrong = true;
                StartCoroutine(showResult());
                resBack.GetComponent<Image>().color = new Color32(186,58,58,255);
                // resultText.color = new Color32(156,26,16,255);
                resultText.text = "Incorrect!";
                userData.wrongAnswers += 1;
                GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
                foreach(GameObject go in gos)
                {
                    if(go.layer == 10) {
                        go.GetComponent<Button>().interactable = false;
                    }
                }
                button.GetComponent<Image>().color = new Color32(255,0,0,255);
                playerWrongSign.SetActive(true);
                sendMessage("wrong:"+score);
            }
        }
    }
    
    IEnumerator showResult () {
        resultCanvas.SetActive(true);
        yield return new WaitForSeconds(1.1f);
        resultCanvas.SetActive(false);
        if(!wrong) {
            round += 1;
            roundTimer = roundTime;
        }
     }

     void showTimer() {
         roundTimer = roundTimer;
        if(otherUid.Length > 1 ) {
            roundTimer--;
            // sendMessage("time:" + roundTimer);
            if(roundTimer <= 0) {
                if (!wrong)
                {
                    StartCoroutine(showResult());
                    resultText.color = new Color32(16,81,156,255);
                    resultText.text = "Time out!";
                }
                
                sendMessage("timeout:" + score);
            }
         }
    }

    private void connectToServer () { 		
		try {  			
			clientThread = new Thread (new ThreadStart(listenForData)); 			
			clientThread.IsBackground = true; 			
			clientThread.Start();  		
		} 		
		catch (Exception e) { 			
			Debug.Log("On client connect exception " + e); 		
		} 	
	}  	

    void startGameButton() {
        email=emailInputField.text;
		username=usernameInputField.text;
		// PostToDatabase();
		sendMessage("ready:"+uid);  
		ready = true;
		status = -5;
		loadingImage.enabled = true;
		loadingText.enabled = true;
		userData.matches += 1;
    }
	
	
	private void PostToDatabase()
	{
		userdata="{\"email\":\"" +email+"\",\"username\":\""+username+"\",\"returnSecureToken\":true}";
        print(userdata);
        StartCoroutine(postData());
	}

    IEnumerator postData() {
        using (UnityWebRequest www = UnityWebRequest.Put("https://csci599-brainiton-4add5.firebaseio.com/users/" + username + ".json", userdata))
        {
            yield return www.Send();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
        }
    }

	private void listenForData() { 		
		try { 	
			socketConnection = new TcpClient(serverIP, serverPort);  			
			Byte[] bytes = new Byte[1024];    
                          
			while (true) { 				
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData); 						
						Debug.Log("message received: " + serverMessage);
                        if (serverMessage.Contains("next:"))
                        {
                            // roundTimer = roundTime;
                            otherScore = Int32.Parse(serverMessage.Split(':')[1]);
                            status = 3;
                        } else if (serverMessage.Contains("answerChoices"))
                        {
                            q = JsonUtility.FromJson<Question>(serverMessage);
                            wrong = false;
                            questionSet = false;
                            status = 1;
                            userData.timeSpent += (roundTime - roundTimer);
                        // } else if(serverMessage.Contains("start:"))
                        // {
                        //     status = 1;
                        } else if(serverMessage.Contains("wrong:"))
                        {
                            status = 4;
                        } else if(serverMessage.Contains("players:"))
                        {
                            string temp = serverMessage.Split(':')[1];
                            if (uid!=temp)
                            {
                                otherUid = temp;
                            } else
                            {
                                otherUid = serverMessage.Split(':')[2];
                            } 
                            
                            // otherPlayerName =  
                        } else if(serverMessage.Contains("finish"))
                        {
                            status = 5;
                        }

                        
                        // setQuestion();
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}  	

    private void sendMessage(string message) {         
		if (socketConnection == null) {             
			return;         
		}  		
		try { 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				string clientMessage = message; 				
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Message sent: " + message);             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	} 

    void readJSON() {
        if(File.Exists(filePath)) {
            string dataAsJson = File.ReadAllText(filePath); 
            userData = JsonUtility.FromJson<Data>(dataAsJson);
        } else {
            Debug.LogError("Cannot Find Data file.");
        }
    }
    
    void writeJSON() {
        string itemsToJSON = "";
        itemsToJSON = JsonUtility.ToJson(userData, true);
        File.WriteAllText(filePath, itemsToJSON);
    }

    void OnApplicationQuit()
    {
        socketConnection.Close();
        clientThread.Abort();
        userData.timeSpent += (roundTime - roundTimer);
        writeJSON();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

 
}
