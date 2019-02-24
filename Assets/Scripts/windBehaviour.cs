using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windBehaviour : MonoBehaviour {

    float windSpeed;
    public float maxSpeed = 1f;

    void Start () {
        windSpeed = 0;
        InvokeRepeating("randomWind", 0f, 0.5f); //randomize wind starting now, repeating every half a second
    }

    void randomWind()
    {
        windSpeed = Random.Range(-maxSpeed, maxSpeed + 0.1f);
    }

    public float getWind()
    {
        return windSpeed;
    }
}
