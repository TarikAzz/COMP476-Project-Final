using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpyCamera : MonoBehaviour
{
    public Vector3 rotation;

    // Use this for initialization
    void Start ()
    {
        rotation = transform.rotation.eulerAngles;
    }
	
	// Update is called once per frame
	void Update ()
    {
        rotation.y += 0.3f;
        transform.rotation = Quaternion.Euler(rotation);
    }
}