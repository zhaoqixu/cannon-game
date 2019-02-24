using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turkeyManager : MonoBehaviour {

    GameObject turkey;
    public List<GameObject> turkeyList = new List<GameObject>();
	// Use this for initialization
	void Start () {
        turkey = Resources.Load("Turkey") as GameObject;
        for (int i = 0; i < 5; i++)
        {
            GameObject tk = Instantiate(turkey) as GameObject;
            turkeyList.Add(tk);
        }
    }

    // Update is called once per frame
    void Update () {
        for (int i = 0; i < turkeyList.Count; i++)
        {
            if (turkeyList[i] == null)
            {
                turkeyList[i] = Instantiate(turkey) as GameObject;
            }
        }
    }
}
