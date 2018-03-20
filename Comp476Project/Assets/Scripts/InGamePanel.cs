using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGamePanel : MonoBehaviour
{
    public Text PlayerKind;

    // The object to retrieve all the UI buttons from
    public GameObject controlContainer;

    // The list of all UI buttons
    public List<Button> unitControls;

    // Used to handle the green highlight of the button when selecting a unit
    public ColorBlock buttonColors;

    // When an unit is selected, you cannot select anything else until you spawn that unit
    public bool controlLocked;

    // Track the unit ID of what you selected for the UnitSpawner class to know
    public int buttonSelected;

    // Only load the unit UI once player kind is determined (Defender or Infiltrator)
    public bool UI_Loaded;

    // UI button sprites for Defender (all defined in the Inspector)
    public Sprite guard;
    public Sprite lamp;
    public Sprite cam;
    public Sprite trap;
    public Sprite sniper;

    // UI button sprites for Infiltrator (all defined in the Inspector)
    // ...

    // Unit limitations
    public int guardCapacity;
    public int lampCapacity;
    public int cameraCapacity;
    public int trapCapacity;
    public int sniperCapacity;



    // Use this for initialization
    void Start()
    {
        UI_Loaded = false;

        // RANDOM VALUES FOR TESTING, NOT FINALIZED!
        guardCapacity = 5;
        lampCapacity = 8;
        cameraCapacity = 7;
        trapCapacity = 4;
        sniperCapacity = 2;
    }


    // Update is called once per frame
    void Update()
    {
        // Only load the unit UI once player kind has been determined
        if(UI_Loaded == false)
        {
            // Defender's UI
            if (PlayerKind.text == "Defender")
            {
                controlLocked = false;
                controlContainer = GameObject.Find("Unit Commands");

                // Add all unit buttons to the list, while modifying each one
                for (int i = 1; i <= controlContainer.transform.childCount; i++)
                {
                    unitControls.Add(GameObject.Find("Unit " + i).GetComponent<Button>());
                    
                    // Set button's image to the sprite according to the unit ID
                    switch(i)
                    {
                        case 1:
                            unitControls[i-1].image.sprite = guard;
                            break;
                        case 2:
                            unitControls[i-1].image.sprite = lamp;
                            break;
                        case 3:
                            unitControls[i-1].image.sprite = cam;
                            break;
                        case 4:
                            unitControls[i-1].image.sprite = trap;
                            break;
                        case 5:
                            unitControls[i-1].image.sprite = sniper;
                            break;
                    }

                    // Just to make buttons visible after UI is loaded (mainly for setting alpha to 255)
                    unitControls[i - 1].image.color = new Color32 (255, 255, 255, 255);
                }

                // Once UI is loaded, don't update this anymore
                UI_Loaded = true;
            }

            // Infiltrator's UI
            else if (PlayerKind.text == "Infiltrator")
            {
                // ...

                UI_Loaded = true;
            }
        }

        // If UI is loaded, then keep checking to lock/unlock buttons and coloring as game is running
        else
        {
            // Disable all buttons until you place the chosen unit
            if (controlLocked == true)
            {
                for (int i = 0; i < unitControls.Count; i++)
                {
                    // Lock all buttons
                    unitControls[i].interactable = false;
                }
            }
            else
            {
                for (int i = 0; i < unitControls.Count; i++)
                {
                    // If a specific unit has reached its limit, lock it and gray out the button
                    if((i + 1 == 1 && GameObject.FindGameObjectsWithTag("Guard").Length >= guardCapacity) ||
                        (i + 1 == 2 && GameObject.FindGameObjectsWithTag("Lamp").Length >= lampCapacity) ||
                        (i + 1 == 3 && GameObject.FindGameObjectsWithTag("Camera").Length >= cameraCapacity) ||
                        (i + 1 == 4 && GameObject.FindGameObjectsWithTag("Trap").Length >= trapCapacity) ||
                        (i + 1 == 5 && GameObject.FindGameObjectsWithTag("Sniper").Length >= sniperCapacity))
                    {
                        unitControls[i].interactable = false;
                        buttonColors = unitControls[i].colors;
                        buttonColors.normalColor = Color.gray;
                        buttonColors.disabledColor = Color.gray;
                        unitControls[i].colors = buttonColors;
                    }

                    else
                    {
                        // Unlock button
                        unitControls[i].interactable = true;

                        // Also, reset its color back to white (selecting a button makes it green)
                        buttonColors = unitControls[i].colors;
                        buttonColors.normalColor = Color.white;
                        buttonColors.disabledColor = Color.white;
                        unitControls[i].colors = buttonColors;
                    }
                }
            }
        }
    }
    
    // Select its ID and make button green
    public void selectUnit(int ID)
    {
        // Highlight button green, and assign button selected's ID
        buttonColors = unitControls[ID - 1].colors;
        buttonColors.normalColor = Color.green;
        buttonColors.disabledColor = Color.green;
        unitControls[ID - 1].colors = buttonColors;
        buttonSelected = ID;
        controlLocked = true;
    }
}