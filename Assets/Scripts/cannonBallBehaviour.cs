using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cannonBallBehaviour : MonoBehaviour {

    //kinematic motion
    public Vector2 Velocity;
    public Vector2 Position;

    public float initialVelocity = 10f;
    [Range(0.5f, 0.95f)]
    public float restitution = 0.8f;

    //firing angle
    int angle;

    //cannon ball initial position
    float InitPosX = 27.93f;
    float InitPosY = 1.8f;

    //forces
    public float gravity = -200f;
    float windSpeed = 0;
    windBehaviour wind;

    //mountain vertices
    Vector2 leftBot = new Vector2(10f, 1f);
    Vector2 leftTop = new Vector2(14f, 6f);
    Vector2 rightBot = new Vector2(20f, 1f);
    Vector2 rightTop = new Vector2(16f, 6f);

    //mountain slope
    float rightSlope;
    float leftSlope;

    //epsilon for collision detection
    float epsilon = 0.05f;

    bool bounce = false;

    turkeyManager turkeyManager;
    List<GameObject> turkeyList;

    // Use this for initialization
    void Start () {
        transform.position = new Vector3(InitPosX, InitPosY, 0);
        wind = GameObject.FindGameObjectWithTag("cannons").GetComponent<windBehaviour>();
        angle = GameObject.FindGameObjectWithTag("cannons").GetComponent<cannonBehaviour>().getCannonAngle(); //find firing angle

        // compute mountain slope
        rightSlope = (rightBot.y - rightTop.y) / (rightBot.x - rightTop.x);
        leftSlope = (leftBot.y - leftTop.y) / (leftBot.x - leftTop.x);

        //set appropriate initial velocity
        Velocity.x = -initialVelocity * Mathf.Cos(Mathf.Deg2Rad * angle);
        Velocity.y = initialVelocity * Mathf.Sin(Mathf.Deg2Rad * angle);

        turkeyManager = GameObject.Find("TurkeyManager").GetComponent<turkeyManager>();
        turkeyList = turkeyManager.turkeyList;
    }
	
	// Update is called once per frame
	void Update() {

        Move();

        // discard cannon ball if encounters the screen boundary or ground
        if (Position.x > 31 || Position.x < -1 || Position.y > 13 || Position.y < 1)
        {
            Destroy(gameObject);
        }

        // check left wall collision and bounce
        if (Position.x <= 1 + epsilon)
        {
            Velocity.x = -Velocity.x * restitution;
            Velocity.y = Velocity.y * restitution;
            gameObject.transform.Translate(Velocity * Time.deltaTime);
            Position = gameObject.transform.position;
        }

        // check right slope collision and bounce
        if (Vector2.Distance(Position, rightTop) + Vector2.Distance(Position, rightBot) <= Vector2.Distance(rightTop, rightBot) + epsilon)
        {
            bounce = true;
            if (Velocity.x < 0)
            {
                Velocity.x = -Velocity.x * restitution;
            }
            else
            {
                //Velocity.x = Velocity.x;
            }
            Velocity.y = -Velocity.y * restitution;
            gameObject.transform.Translate(Velocity * Time.deltaTime);
            Position = gameObject.transform.position;
        }

        // check peak collision and bounce
        if (Vector2.Distance(Position, rightTop) + Vector2.Distance(Position, leftTop) <= Vector2.Distance(rightTop, leftTop) + epsilon)
        {
            bounce = true;

            //Velocity.x = Velocity.x * restitution;
            Velocity.y = -Velocity.y * restitution;
            gameObject.transform.Translate(Velocity * Time.deltaTime);
            Position = gameObject.transform.position;
            if (Velocity.x == 0 && System.Math.Abs(Velocity.y) <= 0.1)
            {
                Destroy(gameObject);
            }
        }

        // check left slop collision and bounce
        if (Vector2.Distance(Position, leftTop) + Vector2.Distance(Position, leftBot) <= Vector2.Distance(leftTop, leftBot) + epsilon)
        {
            bounce = true;
            if (Velocity.x < 0)
            {
               // Velocity.x = Velocity.x;
            }
            else
            {
                Velocity.x = -Velocity.x * restitution;
            }
            Velocity.y = Velocity.y * restitution;
            gameObject.transform.Translate(Velocity * Time.deltaTime);
            Position = gameObject.transform.position;
        }

        // check collision with turkeys
        for (int i = 0; i<turkeyList.Count; i++)
        {
            GameObject obj = turkeyList[i];
            turkeyBehaviour turkey = obj.GetComponent<turkeyBehaviour>();
            for (int j = 0; j < 19; j++)
            {
                Vector2 turkeyPos = new Vector2(turkey.points[j].position.x, turkey.points[j].position.y);
                if (Vector2.Distance(Position, turkeyPos) <= 0.5)
                {
                    turkey.changeVelocity(Velocity);
                    Destroy(gameObject);
                    break;
                }
            }
        }

    }

    void Move()
    {
        windSpeed = wind.getWind() * 0.2f;
       // Debug.Log(Time.deltaTime);
        if (Position.y > 6.0 && !bounce)
        {
            Velocity.x = Velocity.x + windSpeed;
        }

        Velocity.y = Velocity.y + gravity * Time.deltaTime * 50;
        gameObject.transform.Translate(Velocity * Time.deltaTime);
        Position = gameObject.transform.position;
    }
}
