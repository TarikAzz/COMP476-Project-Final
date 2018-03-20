using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UnitSpawner : NetworkBehaviour
{
    // Get a reference to the InGamePanel
    public InGamePanel unitSelector;

    // Defined in the Inspector
    public GameObject lamp;

	// Use this for initialization
	void Start ()
    {
        unitSelector = GameObject.Find("InGamePanel").GetComponent<InGamePanel>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Whenever you click and the controls were locked (meaning a unit was selected)
        if (Input.GetMouseButtonDown(0) && unitSelector.controlLocked == true)
        {
            // Shoot a ray, and see if it hits land (can't spawn units outside of the map)
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // If it hits land
            if (Physics.Raycast(ray, out hit))
            {
                GameObject unit = null;

                // Spawn unit
                switch (unitSelector.buttonSelected)
                {
                    // Missing the rest...

                    case 2:
                    {
                        unit = Instantiate(lamp, hit.point, Quaternion.identity);
                        NetworkServer.Spawn(unit);
                    }
                    break;

                    // Missing the rest...

                }
                

                // Once unit has been spawned, unlock the controls again
                unitSelector.controlLocked = false;
            }
        }
    }
}