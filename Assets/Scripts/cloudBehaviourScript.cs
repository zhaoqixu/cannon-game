using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cloudBehaviourScript : MonoBehaviour {

    Vector2 Velocity;
    float windSpeed;
    windBehaviour wind;

    // Use this for initialization
    void Start () {
        wind = GameObject.FindGameObjectWithTag("cannons").GetComponent<windBehaviour>();

    }

    // Update is called once per frame
    void Update () {
        windSpeed = wind.getWind();
        Velocity.x = windSpeed;
        Velocity.y = 0;
        gameObject.transform.Translate(Velocity * Time.deltaTime);
    }
}
