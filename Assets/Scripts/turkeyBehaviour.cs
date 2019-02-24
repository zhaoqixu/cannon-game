using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turkeyBehaviour : MonoBehaviour {

    float posX = 5f;
    float posY = 1.2f;
    public Transform[] points;
    LineRenderer[] lines;
    Transform head;
    Transform body;

    public float gravity = -0.032f;
    public float initialVelocity = -0.01f;
    public float restitution = 0.5f;
    Vector3[] currentPos;
    Vector3[] previousPos;
    public float[] lengthConstraints; //length to vertex i + 1
    float lengthTolerance;
    public float[] angleConstraints; //angle i-1, i, i+1
    float angleTolerance;
    public float[] velX;
    public float[] velY;
    Vector2 Velocity;
    int angle;
    float windSpeed;
    windBehaviour wind;
    bool windOff = true;
    bool firstMove = true;
    public bool collision = false;
    bool inTheAir = false;
    int direction = 1; // 1 left, -1 right
    bool updateOn = true;
    bool onLeftHill = false;
    bool onRightHill = false;
    bool onPeak = false;
    bool jumpable = true;

    //mountain vertices
    Vector2 leftBot = new Vector2(10f, 1f);
    Vector2 leftTop = new Vector2(14f, 6f);
    Vector2 rightBot = new Vector2(20f, 1f);
    Vector2 rightTop = new Vector2(16f, 6f);
    //epsilon for collision detection
    float epsilon = 0.02f;

    // Use this for initialization
    void Start()
    {

        wind = GameObject.FindGameObjectWithTag("cannons").GetComponent<windBehaviour>();
        windSpeed = 0;
        posX = Random.Range(1f, 9f);
        transform.position = new Vector3(posX, posY, 0);
        Velocity.x = 0;
        Velocity.y = 0;
        points = new Transform[19];
        lines = new LineRenderer[18];
        currentPos = new Vector3[19];
        previousPos = new Vector3[19];
        lengthConstraints = new float[19];
        lengthTolerance = 0.005f;
        angleConstraints = new float[19];
        angleTolerance = 0.01f;
        velX = new float[19]; //this does not store velocity, rather it is just used to calculate inertia for each point
        velY = new float[19];
        head = transform.Find("Head");
        body = transform.Find("Body");

        draw(true);
        buildConstraints();

        float dir = Random.Range(-1f, 1f);
        if (dir < 0)
        {
            flipTurkey(-1);
        }

        int jump = Random.Range(1, 14);
        InvokeRepeating("Jump", jump, 7f);
    }

    // Update is called once per frame
    void Update()
    {

        if (updateOn)
        {
            Move();
            // flip if reach the left wall
            if (points[2].position.x < 0.5 )
            {
                Velocity.x = 0;
                Velocity.y = 0;
                flipTurkeyAndMove(-1);
            }

            // flip if reach the mountain left slope
            if (points[2].position.x > 10 && points[2].position.x < 11 && !inTheAir)
            {
                Velocity.x = 0;
                Velocity.y = 0;
                flipTurkeyAndMove(1);
            }

            // flip if reach the mountain right slope
            if (points[2].position.x < rightBot.x && points[2].position.x > rightBot.x - 1 && !inTheAir)
            {
                Velocity.x = 0;
                Velocity.y = 0;
                flipTurkeyAndMove(-1);
            }

            // destroy if reach the right bound
            if (points[11].position.x > 30)
            {
                Destroy(gameObject);
            }

            // reset inTheAir and firstMove if turkey back to the ground
            if (inTheAir && points[15].position.y <= 1.2 && velY[18] <= 0)
            {
                inTheAir = false;
                firstMove = true;
                Velocity.x = 0;
                Velocity.y = 0;
            }

            // if turkey rear reaches the left wall
            if (points[9].position.x <= 0.5f || points[10].position.x <=0.5f)
            {
                firstMove = true;
                direction = -1;
                Velocity.x = 0;
                Velocity.y = 0;
            }

            // if turkey reach the right ground, disable jumping
            if (points[9].position.x >= rightBot.x) {
                jumpable = false;
            }

            // apply constraints of verlet
            ApplyDistanceConstraints();
            ApplyAngleConstraints();
        }

        // check if collide the left slope
        if (Vector2.Distance(points[17].position, leftTop) + Vector2.Distance(points[17].position, leftBot) <= Vector2.Distance(leftTop, leftBot) + epsilon)
        {
            Velocity.x = 0;
            Velocity.y = 0;
            updateOn = false;
            onLeftHill = true;
            inTheAir = false;
            if (direction == -1)
            {
                flipTurkey(1);
            }
        }

        // if on left slope, turn back and return to the ground
        if (onLeftHill)
        {
            Velocity.x = 0;
            Velocity.y = 0;
            for (int i = 0; i < 19; i++)
            {
                velX[i] = direction * initialVelocity * Time.deltaTime / 20;
                velY[i] = velX[i] * 5 / 4;
                points[i].position = new Vector3(points[i].position.x + velX[i], points[i].position.y + velY[i], 0);
            }
            if (!inTheAir && points[15].position.y <= 1.05)
            {
                onLeftHill = false;
                updateOn = true;
                firstMove = true;
                inTheAir = false;
                for (int i = 0; i<19; i++)
                {
                    currentPos[i].x = points[i].position.x;
                    currentPos[i].y = points[i].position.y;
                }
            }
        }

        // check if collide the peak
        if (Vector2.Distance(points[17].position, leftTop) + Vector2.Distance(points[17].position, rightTop) <= Vector2.Distance(leftTop, rightTop) + epsilon)
        {
            Velocity.x = 0;
            Velocity.y = 0;
            updateOn = false;
            onPeak = true;
            inTheAir = false;
            if (direction == 1)
            {
                flipTurkey(-1);
            }
        }

        if (onPeak)
        {

            for (int i = 0; i < 19; i++)
            {
                velX[i] = direction * initialVelocity * Time.deltaTime / 20;
                points[i].position = new Vector3(points[i].position.x + velX[i], points[i].position.y, 0);
            }
            if (!inTheAir && points[15].position.x > rightTop.x)
            {
                onRightHill = true;
                onPeak = false;
                inTheAir = false;
            }
        }

        // check if collide the right slope
        if (Vector2.Distance(points[10].position, rightTop) + Vector2.Distance(points[10].position, rightBot) <= Vector2.Distance(rightBot, rightTop) + epsilon)
        {
            Velocity.x = 0;
            Velocity.y = 0;
            updateOn = false;
            onRightHill = true;
            inTheAir = false;
            if (direction == 1)
            {
                flipTurkey(-1);
            }
        }

        if (onRightHill)
        {
            for (int i = 0; i < 19; i++)
            {
                velX[i] = direction * initialVelocity * Time.deltaTime / 20;
                velY[i] = - velX[i] * 5 / 4;
                points[i].position = new Vector3(points[i].position.x + velX[i], points[i].position.y + velY[i], 0);
            }
            if (points[15].position.y <= 1.05)
            {
                onRightHill = false;
                updateOn = true;
                firstMove = true;
                inTheAir = false;
                for (int i = 0; i < 19; i++)
                {
                    currentPos[i].x = points[i].position.x;
                    currentPos[i].y = points[i].position.y;
                }
            }
        }

        draw(false);
    }

    
    // verlet updates 
    void Move()
    {

        if (points[0].position.y > 6f) windSpeed = wind.getWind() * 0.2f * Time.deltaTime / 1.25f; //get wind
        else windSpeed = 0;

        for (int i = 0; i < 19; i++)
        {
            if (i == 8) i = 18;

            //apply initial velocity on first movement, inertia afterwards
            if (!firstMove)
            {
                velX[i] = points[i].position.x - previousPos[i].x + Velocity.x;
                velY[i] = points[i].position.y - previousPos[i].y + Velocity.y;
            }
            else
            {
                velX[i] = direction * initialVelocity * Time.deltaTime / 20;
                if (inTheAir)
                {
                    // do nothing
                } else {
                    velY[i] = 0;
                }
            }

            //verlet formulas
            currentPos[i].x = points[i].position.x + velX[i] + windSpeed;
            if (inTheAir)
            {
                velY[i] = velY[i] + gravity * Time.deltaTime * 5;
                currentPos[i].y = points[i].position.y + velY[i];
            }

            previousPos[i].x = points[i].position.x;
            previousPos[i].y = points[i].position.y;


            points[i].position = new Vector3(currentPos[i].x, currentPos[i].y, 0);
        }
        firstMove = false;

    }

    // add a speed in positive y direction to jump
    void Jump()
    {
        if (jumpable == true)
        {
            inTheAir = true;
            firstMove = true;
            for (int i = 0; i < 19; i++)
            {
                if (i == 8) i = 18;

                velY[i] = -15 * initialVelocity * Time.deltaTime / 20;
            }
        }
    }

    // flip the turkey direction
    void flipTurkeyAndMove(int dir)
    {
        float flipAxis = points[12].position.x;
        for (int i = 0; i < 19; i++)
        {

            float newPosX = points[i].position.x + 2 * (flipAxis - points[i].position.x);
            points[i].position = new Vector3(points[i].position.x + 2 * (flipAxis - points[i].position.x), points[i].position.y, 0); ;
        }
        firstMove = true;
        direction = dir;
        Move();
    }

    // flip the turkey direction
    void flipTurkey(int dir)
    {
        float flipAxis = points[12].position.x;
        for (int i = 0; i < 19; i++)
        {
            float newPosX = points[i].position.x + 2 * (flipAxis - points[i].position.x);
            points[i].position = new Vector3(points[i].position.x + 2 * (flipAxis - points[i].position.x), points[i].position.y, 0); ;
        }
        firstMove = true;
        direction = dir;
    }

    // apply length constraints on turkey
    void ApplyDistanceConstraints()
    {
        //move body verlets to satisfy constraints based on movement of head verlets

        for (int i = 7; i < 9; i++) //top half
        {
            distanceConstraints(i+1, i);
        }

        // point 13
        distanceConstraints(13, 0);

        for (int i = 13; i > 8; i--) //bottom half
        {
            distanceConstraints(i - 1, i);
        }

        // point 14
        distanceConstraints(14, 11);

        // point 15
        distanceConstraints(15, 14);

        // point 16
        distanceConstraints(16, 12);

        // point 17
        distanceConstraints(17, 16);
    }

    void distanceConstraints(int curPt, int prePt)
    {
        float maxL = lengthConstraints[curPt] * (1 + lengthTolerance);
        float minL = lengthConstraints[curPt] * (1 - lengthTolerance);
        float distance = (points[prePt].position - points[curPt].position).magnitude;
        if (distance < minL)
        {
            while (distance < minL)
            {
                points[curPt].position = Vector3.MoveTowards(points[curPt].position, points[prePt].position, -0.01f);
                distance = (points[prePt].position - points[curPt].position).magnitude;
            }
        }
        else if (distance > maxL)
        {
            while (distance > maxL)
            {
                points[curPt].position = Vector3.MoveTowards(points[curPt].position, points[prePt].position, 0.01f);
                distance = (points[prePt].position - points[curPt].position).magnitude;
            }
        }
    }

    // apply angular constrtaints on turkey
    void ApplyAngleConstraints()
    {
        //move body verlets to satisfy constraints based on movement of head verlets

        anglularConstraintsNonConvex(7);
        anglularConstraintsConvexTop(8);
        anglularConstraintsConvexTop(9);


        // point 0
        if (true)
        {
            float maxA = angleConstraints[0] * (1 + angleTolerance);
            float minA = angleConstraints[0] * (1 - angleTolerance);
            float angle = Vector3.Angle(points[13].position - points[0].position, points[1].position - points[0].position);

            if (angle < minA)
            {
                while (angle < minA)
                {
                    points[13].RotateAround(points[0].position, new Vector3(0, 0, -direction), 0.5f);
                    angle = Vector3.Angle(points[13].position - points[0].position, points[1].position - points[0].position);
                }
            }
            else if (angle > maxA)
            {
                while (angle > maxA)
                {
                    points[13].RotateAround(points[0].position, new Vector3(0, 0, direction), 0.5f);
                    angle = Vector3.Angle(points[13].position - points[0].position, points[1].position - points[0].position);
                }
            }
        }

        // point 13
        if (true)
        {
            float maxA = angleConstraints[13] * (1 + angleTolerance);
            float minA = angleConstraints[13] * (1 - angleTolerance);
            float angle = Vector3.Angle(points[12].position - points[13].position, points[0].position - points[13].position);

            if (angle < minA)
            {
                while (angle < minA)
                {
                    points[12].RotateAround(points[13].position, new Vector3(0, 0, -direction), 0.5f);
                    angle = Vector3.Angle(points[12].position - points[13].position, points[0].position - points[13].position);
                }
            }
            else if (angle > maxA)
            {
                while (angle > maxA)
                {
                    points[12].RotateAround(points[13].position, new Vector3(0, 0, direction), 0.5f);
                    angle = Vector3.Angle(points[12].position - points[13].position, points[0].position - points[13].position);
                }
            }
        }

        anglularConstraintsConvexBot(12);
        anglularConstraintsConvexBot(11);
        anglularConstraintsConvexBot(10);
        anglularConstraintsConvexBot(9);

        // point 14
        if (true) {
            float maxA = angleConstraints[14] * (1 + angleTolerance);
            float minA = angleConstraints[14] * (1 - angleTolerance);
            float angle = Vector3.Angle(points[14].position - points[11].position, points[12].position - points[11].position);
            if (angle < minA)
            {
                while (angle < minA)
                {
                    points[14].RotateAround(points[11].position, new Vector3(0, 0, direction), 0.5f);
                    angle = Vector3.Angle(points[14].position - points[11].position, points[12].position - points[11].position);
                }
            }
            else if (angle > maxA)
            {
                while (angle > maxA)
                {
                    points[14].RotateAround(points[11].position, new Vector3(0, 0, -direction), 0.5f);
                    angle = Vector3.Angle(points[14].position - points[11].position, points[12].position - points[11].position);
                }
            }
        }

        // point 15
        if (true)
        {
            float maxA = angleConstraints[15] * (1 + angleTolerance);
            float minA = angleConstraints[15] * (1 - angleTolerance);
            float angle = Vector3.Angle(points[15].position - points[14].position, points[11].position - points[14].position);
            if (angle < minA)
            {
                while (angle < minA)
                {
                    points[15].RotateAround(points[14].position, new Vector3(0, 0, direction), 0.5f);
                    angle = Vector3.Angle(points[15].position - points[14].position, points[11].position - points[14].position);
                }
            }
            else if (angle > maxA)
            {
                while (angle > maxA)
                {
                    points[15].RotateAround(points[14].position, new Vector3(0, 0, -direction), 0.5f);
                    angle = Vector3.Angle(points[15].position - points[14].position, points[11].position - points[14].position);
                }
            }
        }

        // point 16
        if (true)
        {
            float maxA = angleConstraints[16] * (1 + angleTolerance);
            float minA = angleConstraints[16] * (1 - angleTolerance);
            float angle = Vector3.Angle(points[16].position - points[12].position, points[11].position - points[12].position);
            if (angle < minA)
            {
                while (angle < minA)
                {
                    points[16].RotateAround(points[12].position, new Vector3(0, 0, -direction), 0.5f);
                    angle = Vector3.Angle(points[16].position - points[12].position, points[11].position - points[12].position);
                }
            }
            else if (angle > maxA)
            {
                while (angle > maxA)
                {
                    points[16].RotateAround(points[12].position, new Vector3(0, 0, direction), 0.5f);
                    angle = Vector3.Angle(points[16].position - points[12].position, points[11].position - points[12].position);
                }
            }
        }

        // point 17
        if (true)
        {
            float maxA = angleConstraints[17] * (1 + angleTolerance);
            float minA = angleConstraints[17] * (1 - angleTolerance);
            float angle = Vector3.Angle(points[17].position - points[16].position, points[12].position - points[16].position);
            if (angle < minA)
            {
                while (angle < minA)
                {
                    points[17].RotateAround(points[16].position, new Vector3(0, 0, direction), 0.5f);
                    angle = Vector3.Angle(points[17].position - points[16].position, points[12].position - points[16].position);
                }
            }
            else if (angle > maxA)
            {
                while (angle > maxA)
                {
                    points[17].RotateAround(points[16].position, new Vector3(0, 0, -direction), 0.5f);
                    angle = Vector3.Angle(points[17].position - points[16].position, points[12].position - points[16].position);
                }
            }
        }


    }

    void anglularConstraintsNonConvex(int i)
    {
        float maxA = angleConstraints[i] * (1 + angleTolerance);
        float minA = angleConstraints[i] * (1 - angleTolerance);
        float angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
        //rotate on z axis through its neighbour to satisfy angle constraints
        if (angle < minA)
        {
            while (angle < minA)
            {
                points[i + 1].RotateAround(points[i].position, new Vector3(0, 0, -direction), 0.5f);
                angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
            }
        }
        else if (angle > maxA)
        {
            while (angle > maxA)
            {
                points[i + 1].RotateAround(points[i].position, new Vector3(0, 0, direction), 0.5f);
                angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
            }
        }
    }

    void anglularConstraintsConvexTop(int i)
    {
        float maxA = angleConstraints[i] * (1 + angleTolerance);
        float minA = angleConstraints[i] * (1 - angleTolerance);
        float angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
        //rotate on z axis through its neighbour to satisfy angle constraints
        if (angle < minA)
        {
            while (angle < minA)
            {
                points[i + 1].RotateAround(points[i].position, new Vector3(0, 0, direction), 0.5f);
                angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
            }
        }
        else if (angle > maxA)
        {
            while (angle > maxA)
            {
                points[i + 1].RotateAround(points[i].position, new Vector3(0, 0, -direction), 0.5f);
                angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
            }
        }
    }

    void anglularConstraintsConvexBot(int i)
    {
        float maxA = angleConstraints[i] * (1 + angleTolerance);
        float minA = angleConstraints[i] * (1 - angleTolerance);
        float angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
        //rotate on z axis through its neighbour to satisfy angle constraints
        if (angle < minA)
        {
            while (angle < minA)
            {
                points[i - 1].RotateAround(points[i].position, new Vector3(0, 0, -direction), 0.5f);
                angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
            }
        }
        else if (angle > maxA)
        {
            while (angle > maxA)
            {
                points[i - 1].RotateAround(points[i].position, new Vector3(0, 0, direction), 0.5f);
                angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
            }
        }
    }

    // draw lines between vertex
    void draw(bool init)
    {
        //if (init) Debug.Log("INIT GOAT");
        for (int i = 0; i < 14; i++)
        {

            if (init) //init is true if this is the first time we are drawing. in this case we find all the points and positions
            {
                if (i >= 8 && i <= 13) points[i] = body.Find("" + i);
                else points[i] = head.Find("" + i);
                currentPos[i] = points[i].position;
                previousPos[i] = points[i].position;
            }

            if (i > 0)
            {
                if (init) lines[i - 1] = points[i - 1].gameObject.AddComponent<LineRenderer>(); //if init then give each point a line renderer
                lines[i - 1].startColor = Color.black;
                lines[i - 1].startWidth = 0.05f;
                lines[i - 1].endColor = Color.black;
                lines[i - 1].endWidth = 0.05f;
                lines[i - 1].positionCount = 2;
                lines[i - 1].SetPosition(0, points[i - 1].position);
                lines[i - 1].SetPosition(1, points[i].position);
                lines[i - 1].material.color = Color.black;
                lines[i - 1].numCapVertices = 1;
            }
        }

        //special cases for last head line and also legs
        if (init) lines[13] = points[13].gameObject.AddComponent<LineRenderer>();
        lines[13].startColor = Color.black;
        lines[13].startWidth = 0.05f;
        lines[13].endColor = Color.black;
        lines[13].endWidth = 0.05f;
        lines[13].positionCount = 2;
        lines[13].SetPosition(0, points[13].position);
        lines[13].SetPosition(1, points[0].position);
        lines[13].material.color = Color.black;
        lines[13].numCapVertices = 1;



        if (init) //init legs and eye
        {
            points[14] = body.Find("leg1");
            points[15] = body.Find("leg11");

            points[16] = body.Find("leg2");
            points[17] = body.Find("leg22");

            points[18] = head.Find("eye");

            for (int i = 14; i < 19; i++)
            {
                currentPos[i] = points[i].position;
                previousPos[i] = points[i].position;
            }
        }

        // draw leg1
        if (init) lines[14] = points[14].gameObject.AddComponent<LineRenderer>();
        lines[14].startColor = Color.black;
        lines[14].startWidth = 0.05f;
        lines[14].endColor = Color.black;
        lines[14].endWidth = 0.05f;
        lines[14].positionCount = 2;
        lines[14].SetPosition(0, points[14].position);
        lines[14].SetPosition(1, points[11].position);
        lines[14].material.color = Color.black;
        lines[14].numCapVertices = 1;

        if (init) lines[15] = points[15].gameObject.AddComponent<LineRenderer>();
        lines[15].startColor = Color.black;
        lines[15].startWidth = 0.05f;
        lines[15].endColor = Color.black;
        lines[15].endWidth = 0.05f;
        lines[15].positionCount = 2;
        lines[15].SetPosition(0, points[15].position);
        lines[15].SetPosition(1, points[14].position);
        lines[15].material.color = Color.black;
        lines[15].numCapVertices = 1;

        // draw leg2
        if (init) lines[16] = points[16].gameObject.AddComponent<LineRenderer>();
        lines[16].startColor = Color.black;
        lines[16].startWidth = 0.05f;
        lines[16].endColor = Color.black;
        lines[16].endWidth = 0.05f;
        lines[16].positionCount = 2;
        lines[16].SetPosition(0, points[16].position);
        lines[16].SetPosition(1, points[12].position);
        lines[16].material.color = Color.black;
        lines[16].numCapVertices = 1;

        if (init) lines[17] = points[17].gameObject.AddComponent<LineRenderer>();
        lines[17].startColor = Color.black;
        lines[17].startWidth = 0.05f;
        lines[17].endColor = Color.black;
        lines[17].endWidth = 0.05f;
        lines[17].positionCount = 2;
        lines[17].SetPosition(0, points[17].position);
        lines[17].SetPosition(1, points[16].position);
        lines[17].material.color = Color.black;
        lines[17].numCapVertices = 1;
    }

    // generate verlet constraints
    void buildConstraints()
    {
        int h = 0;
        for (int i = 0; i < 19; i++)
        {
            if (i == 0) h = 13;
            else h = i - 1;

            //special cases for legs and eye
            if (i == 14)
            {
                lengthConstraints[i] = (points[i].GetComponent<LineRenderer>().GetPosition(0) - points[11].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
                angleConstraints[i] = 90;
                continue;
            }

            if (i == 15)
            {
                lengthConstraints[i] = (points[i].GetComponent<LineRenderer>().GetPosition(0) - points[14].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
                angleConstraints[i] = 145;
                continue;
            }

            if (i == 16)
            {
                lengthConstraints[i] = (points[i].GetComponent<LineRenderer>().GetPosition(0) - points[12].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
                angleConstraints[i] = 90;
                continue;
            }

            if (i == 17)
            {
                lengthConstraints[i] = (points[i].GetComponent<LineRenderer>().GetPosition(0) - points[16].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
                angleConstraints[i] = 145;
                continue;
            }

            if (i == 18)
            {
                lengthConstraints[i] = (points[i].position - points[5].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
                angleConstraints[i] = 90;
                continue;
            }

            //set its length to next neighbour, and angle based on surrounding verlets
            lengthConstraints[i] = (points[i].GetComponent<LineRenderer>().GetPosition(1) - points[i].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
            angleConstraints[i] = Vector3.Angle(points[h].GetComponent<LineRenderer>().GetPosition(0) - points[h].GetComponent<LineRenderer>().GetPosition(1),
                points[i].GetComponent<LineRenderer>().GetPosition(1) - points[i].GetComponent<LineRenderer>().GetPosition(0));
        }
    }

    // change turkey velocity
    public void changeVelocity(Vector2 vel)
    {
        for (int i=0; i< 19; i++)
        {
            if (i == 8) i = 18;
            Velocity.x = vel.x / 10000;
            Velocity.y = vel.y / 10000;
        }
    }

}
