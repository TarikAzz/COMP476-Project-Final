using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    void OnCollisionEnter(Collision col)
    {
        // TEMP
        if(col.gameObject.tag == "Infiltrator")
        {
            // ...
        }
        else if (col.gameObject.tag == "Defender")
        {
            // ...
        }

    }
}