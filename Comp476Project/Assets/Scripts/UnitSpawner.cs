using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public InGamePanel unitSelector;

	// Use this for initialization
	void Start ()
    {
        unitSelector = GameObject.Find("InGamePanel").GetComponent<InGamePanel>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            print(mousePos);

            // Now use a ray to detect if mouse click is on plane's collider to spawn unit.
            // ...
        }
    }
}
