using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDetection : MonoBehaviour
{
    // Reveal infiltrators when entering the light
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Bad")
        {
            col.gameObject.GetComponent<Character>().IsSpotted = true;
        }
    }

    // Hide infiltrators when leaving the light
    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Bad")
        {
            col.gameObject.GetComponent<Character>().IsSpotted = false;
        }
    }
}