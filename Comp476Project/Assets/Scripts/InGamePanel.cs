using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGamePanel : MonoBehaviour
{
    public Text PlayerKind;

    public GameObject controlContainer;
    public List<Button> unitControls;
    public ColorBlock buttonColors;

    public bool controlLocked;


    // Use this for initialization
    void Start()
    {
        controlLocked = false;

        controlContainer = GameObject.Find("Unit Commands");

        for(int i = 1; i <= controlContainer.transform.childCount; i++)
        {
            unitControls.Add(GameObject.Find("Unit " + i).GetComponent<Button>());
        }



        
    }


    // Update is called once per frame
    void Update()
    {
        // Disable all buttons until you place the chosen unit
        if (controlLocked == true)
        {
            for (int i = 0; i < unitControls.Count; i++)
            {
                unitControls[i].interactable = false;
            }
        }
        else
        {
            for (int i = 0; i < unitControls.Count; i++)
            {
                
                unitControls[i].interactable = true;

                // Also, reset their colors back to white
                buttonColors = unitControls[i].colors;
                buttonColors.normalColor = Color.white;
                buttonColors.disabledColor = Color.white;
                unitControls[i].colors = buttonColors;
            }
        }
    }




    public void selectUnit(int ID)
    {
        buttonColors = unitControls[ID - 1].colors;
        buttonColors.normalColor = Color.green;
        buttonColors.disabledColor = Color.green;
        unitControls[ID - 1].colors = buttonColors;

        controlLocked = true;

        
    }



}