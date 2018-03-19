using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UnitSpawner : NetworkBehaviour
{
    public InGamePanel unitSelector;

    // Prefab linked through the Inspector
    public GameObject lamp;

	// Use this for initialization
	void Start ()
    {
        unitSelector = GameObject.Find("InGamePanel").GetComponent<InGamePanel>();
	}
	
	// Update is called once per frame
	void Update ()
    {

        if (Input.GetMouseButtonDown(0) && unitSelector.controlLocked == true)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                GameObject unit = null;

                switch (unitSelector.buttonSelected)
                {
                    case 1:
                    {
                        unit = Instantiate(lamp, hit.point, Quaternion.identity);
                        NetworkServer.Spawn(unit);
                    }
                    break;

                    // Missing the rest...

                }
                



                unitSelector.controlLocked = false;
            }

        }

    }
}