using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int playerPool = randomNumber(0,3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private static int randomNumber(int min, int max)
    {
        System.Random random = new System.Random(); 
        return random.Next(min, max);

    }
}
