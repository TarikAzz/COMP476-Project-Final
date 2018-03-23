using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UnitSpawner : NetworkBehaviour
{/*
    // Get a reference to the InGamePanel
    public InGamePanel unitSelector;

    // Defined in the Inspector

    // Missing the rest...
    public GameObject lamp;

    // Use this for initialization
    public override void OnStartLocalPlayer()
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

                // Defender Units
                if(unitSelector.PlayerKind.text == "Defender")
                {
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
                }

                // Infiltrator Units
                else if (unitSelector.PlayerKind.text == "Infiltrator")
                {
                    // Spawn unit
                    
                    switch (unitSelector.buttonSelected)
                    {
                        // Missing the rest...
                        case 1:
                            break;
                    }
                }
                
                

                // Once unit has been spawned, unlock the controls again
                unitSelector.controlLocked = false;
            }
        }
    }*/
}