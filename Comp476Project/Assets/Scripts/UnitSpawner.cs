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
		
	}
}
