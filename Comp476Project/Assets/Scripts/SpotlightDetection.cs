using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightDetection : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    // Reveal infiltrators when entering the spotlight
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Bad")
        {
            col.gameObject.GetComponent<Character>().IsSpotted = true;
        }
    }

    // Hide infiltrators when leaving the spotlight
    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Bad")
        {
            col.gameObject.GetComponent<Character>().IsSpotted = false;
        }
    }
}