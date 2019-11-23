using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spinWheel : MonoBehaviour
{
 	public float speed = 400; 
 	public bool isSpin = true;
 	private bool onclick = false;
 	private int rand;
 	public GameObject pointer;
    void Update()
    {
    	if(onclick) {
    		Rotate();	
    	}				   
    }


    public void Rotate ()
    {
    	rand = Random.Range(1,2);
    	transform.Rotate(0, 0, speed * Time.deltaTime * rand);
    	if(isSpin == false && speed > 0) {
    		Stop();
    	}
    }

    public void Stop() {
    	speed--;
    	if(speed <= 0) {
    		pointer.GetComponent<BoxCollider>().enabled = true;
    		speed = 0;
    	}
    }

    public void spinthewheel() {
    	isSpin = false;
    	onclick = true;
    }
}