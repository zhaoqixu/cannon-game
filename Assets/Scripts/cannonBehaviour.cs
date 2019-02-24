using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cannonBehaviour : MonoBehaviour
{

    GameObject cannonBall;
    GameObject cannon;

    public int minAngle = 0;
    public int maxAngle = 90;
    public int cannonAngle = 45;

    // Use this for initialization
    void Start()
    { //load appropriate resources
        cannon = GameObject.FindGameObjectWithTag("cannon") as GameObject;
        cannonBall = Resources.Load("Cannon Ball") as GameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow) && cannonAngle <= 80) {
            cannonAngle += 10;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && cannonAngle >= 10) {
            cannonAngle -= 10;
        }

        cannon.transform.localRotation = Quaternion.Euler(0, -180, cannonAngle);
        
        if (Input.GetKeyDown(KeyCode.Space)) //shoot on space
        {
            fireBall();
        }
    }

    public int getCannonAngle() //public get
    {
        return cannonAngle;
    }

    void fireBall() //fire cannonball (simply instantiate the object and it's scripts will handle the rest)
    {
        GameObject ball = Instantiate(cannonBall) as GameObject;
    }
}
