using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UnitSpawner : NetworkBehaviour
{
    // Get a reference to the InGamePanel
    public InGamePanel unitSelector
    {
        get
        {
            if (_unitSelector == null)
            {
                _unitSelector = FindObjectOfType<InGamePanel>();
            }

            return _unitSelector;
        }
    }

    // Defined in the Inspector (sniper is an ability, not a prefab)
    public GameObject lamp;
    public GameObject trap;
    public GameObject cam;

    // Track how many of each unit have been spawned
    public int lamps_spawned;
    public int traps_spawned;
    public int cameras_spawned;

    // Reference to the Audio Manager (defined in Inspector)
    public AudioManager audioManager;

    private InGamePanel _unitSelector;

    // Update is called once per frame
    void Update ()
    {
        if (unitSelector == null)
        {
            return;
        }

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
                if(_unitSelector.PlayerManager.Kind == PlayerManager.PlayerKind.Defender)
                {
                    // Spawn unit (sniper is not a spawnable unit, so only need to define cases 1 - 3)
                    switch (unitSelector.buttonSelected)
                    {
                        case 1:
                            {
                                unit = Instantiate(lamp, hit.point, Quaternion.identity);

                                // Play unit placement sound effect
                                audioManager.playUnitPlacement();

                                NetworkServer.Spawn(unit);
                                lamps_spawned++;
                            }
                            break;

                        case 2:
                            {
                                unit = Instantiate(cam, hit.point, Quaternion.identity);

                                // Play unit placement sound effect
                                audioManager.playUnitPlacement();

                                NetworkServer.Spawn(unit);
                                cameras_spawned++;
                            }
                            break;

                        case 3:
                            {
                                unit = Instantiate(trap, hit.point, Quaternion.identity);

                                // Play unit placement sound effect
                                audioManager.playUnitPlacement();

                                NetworkServer.Spawn(unit);
                                traps_spawned++;
                            }
                            break;
                    }
                }

                // Once unit has been spawned, unlock the controls again
                unitSelector.controlLocked = false;
            }
        }
    }
}