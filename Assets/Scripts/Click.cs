using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;

public class Click : MonoBehaviour
{
    public string serverIP = "192.168.1.108";
    public System.Int32 serverPort = 8000;
    private TcpClient socketConnection; 	
	private Thread clientReceiveThread; 	
    private string uid;
    private bool start = false;
    // Start is called before the first frame update
    void Start()
    {
        Button btn = this.GetComponent<Button>();
		btn.onClick.AddListener(onClick);
        uid = SystemInfo.deviceUniqueIdentifier;   
        ConnectToTcpServer();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (start==true)
        {
            SceneManager.LoadScene("Game");
        }
    }

     void onClick()
    {
        
    }

    private void ConnectToTcpServer () { 		
		try {  			
			clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
			clientReceiveThread.IsBackground = true; 			
			clientReceiveThread.Start();  		
		} 		
		catch (Exception e) { 			
			Debug.Log("On client connect exception " + e); 		
		} 	
	}  	

	private void ListenForData() { 		
		try { 	
			socketConnection = new TcpClient(serverIP, serverPort);  			
			Byte[] bytes = new Byte[1024]; 
            SendMessage("ready:"+uid);           
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
                        if (serverMessage.Contains("start:"))
                        {
                            start = true;
                        } 
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}  

    private void SendMessage(string message) {         
		if (socketConnection == null) {             
			return;         
		}  		
		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				string clientMessage = message; 				
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Message sent: " + message);             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	} 
}
