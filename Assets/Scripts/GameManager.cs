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
public class GameManager : MonoBehaviour
{
    public Camera mainCamera;
    public Camera secCamera;
    private Question q = null;
    public GameObject mainCanvas;
    public GameObject resultCanvas;
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
    public Slider playerSlider;
    public Slider otherPlayerSlider;
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

    private string serverIP = "127.0.0.1";
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

    IDictionary<string, string> dict = new Dictionary<string, string>()
        {
            {"0","Explorer"},
            {"460c00440b52418fbb084cfebe8d0b12","Achiever"},
            {"ebadfd2b6c0597ca351bafe1c32c9691", "Champion"}
            
        };


    void Start() {
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
            otherPlayerName.text = otherUid.Substring(0,5);
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
                resultCanvas.GetComponent<Image>().color = new Color32(156,26,16,255);
                resultText.text = "other player got it!";
                // status = -1;
            } else if(status == 4)
            {
                wrongSign.SetActive(true);
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
        }
    }
    
    void setQuestion() {
        questionText.text = q.question.Replace("???", "'");
        answerText1.text = q.answerChoices[0];
        answerText2.text = q.answerChoices[1];
        answerText3.text = q.answerChoices[2];
        answerText4.text = q.answerChoices[3];
        questionSet = true;
    }
    
    void checkAnswer(Button button) {
        if(!wrong) {
            if(int.Parse(button.name)-1 == q.correctAnswerIndex) {
                score += 10;
                sendMessage("next:"+score);
                StartCoroutine(showResult());
                resultCanvas.GetComponent<Image>().color = new Color32(28,156,15,255);
                resultText.text = "You got it!\n\n+10";
            } else {
                wrong = true;
                StartCoroutine(showResult());
                resultCanvas.GetComponent<Image>().color = new Color32(156,26,16,255);
                resultText.text = "Whoopsie!";
                
                sendMessage("wrong:"+score);
            }
        }
    }
    
    IEnumerator showResult () {
        resultCanvas.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        resultCanvas.SetActive(false);
        if(!wrong) {
            round += 1;
            roundTimer = roundTime;
        }
     }

     void showTimer() {
        roundTimer--;
        // sendMessage("time:" + roundTimer);
        if(roundTimer <= 0) {
            StartCoroutine(showResult());
            resultCanvas.GetComponent<Image>().color = new Color32(16,81,156,255);
            resultText.text = "Time out!";
            sendMessage("timeout:" + score);
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
        sendMessage("ready:"+uid);  
        ready = true;
        status = -5;
        loadingImage.enabled = true;
        loadingText.enabled = true;
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
                        // } else if(serverMessage.Contains("start:"))
                        // {
                        //     status = 1;
                        } else if(serverMessage.Contains("wrong:"))
                        {
                            status = 4;
                        }else if(serverMessage.Contains("players:"))
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

    void OnApplicationQuit()
    {
        socketConnection.Close();
        clientThread.Abort();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

 
}
