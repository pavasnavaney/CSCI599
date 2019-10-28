using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;

public class Connection : MonoBehaviour
{
    public string serverIP = "192.168.1.108";
    public System.Int32 serverPort = 8000;
    public TcpClient socketConnection; 	
	public Thread clientReceiveThread; 
    // Start is called before the first frame update
    void Start()
    {
        
    // ConnectToTcpServer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnectToTcpServer () { 		
		try {  			
			clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
			clientReceiveThread.IsBackground = false; 			
			clientReceiveThread.Start();  		
		} 		
		catch (Exception e) { 			
			Debug.Log("On client connect exception " + e); 		
		} 	
	}  	

	public void ListenForData() { 		
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
                        } else if (serverMessage.Contains("answerChoices"))
                        {
                            // q = JsonUtility.FromJson<Question>(serverMessage);
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
}
