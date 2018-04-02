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

    // Damage infiltrators as they remain in the light
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Bad")
        {
            col.gameObject.GetComponent<Character>().TakeDamage();
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