using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// JONATHAN'S PART
public class Lamp : MonoBehaviour
{
    public GameObject point_light;
    public float range;


	// Use this for initialization
	void Start ()
    {
        point_light = GameObject.Find("Head/Point Light");
        range = point_light.GetComponent<Light>().range;
	}
	
	// Update is called once per frame
	void Update ()
    {
		// i love milk
	}
}