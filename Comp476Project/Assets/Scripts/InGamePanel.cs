using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class InGamePanel : MonoBehaviour
{
    #region UI components

    public Image PlayerKind;
    public Text PlayerKindText;
    public Text GameStateText;
    public GameObject SetupGroup;
    public Image SetupTimerImage;
    public Button ReadyButton;
    public GameObject EndGameGroup;
    public Text EndGameMessage;

    #endregion

    #region Public variables

    // The object to retrieve all the UI buttons from
    public GameObject controlContainer;

    // The object to retrieve the toggle button (separated from unit container)
    public GameObject toggleContainer;

    // The list of all UI buttons
    public List<Button> unitControls;

    // The toggle button itself
    public Button toggleControl;

    // Label for sniper's cooldown
    public Text cooldownLabel;

    // Image overlay used to show red flash on sniper kill
    public Image killImage;

    // Determine when to flash screen on sniper kill
    public bool startFlash;

    // The boolean to check both states of the toggle button
    public bool isToggleCycled;

    // Used to access all the characters for the toggle feature to adjust their values
    public PlayerManager characterContainer;

    // Used to handle the green highlight of the button when selecting a unit
    public ColorBlock buttonColors;

    // When an unit is selected, you cannot select anything else until you spawn that unit
    public bool controlLocked;

    // Track the unit ID of what you selected for the UnitSpawner class to know
    public int buttonSelected;

    // Only load the unit UI once player kind is determined (Defender or Infiltrator)
    public bool UI_Loaded;

    // Game Info sprites (all defined in the Inspector)
    public Sprite defender;
    public Sprite infiltrator;

    // UI button sprites for Defender (all defined in the Inspector)
    public Sprite lamp;
    public Sprite cam;
    public Sprite trap;
    public Sprite sniper;

    // UI button sprites for the toggle (all defined in the Inspector)
    public Sprite cycle;
    public Sprite back_forth;

    // Unit limitations
    public int lampCapacity;
    public int cameraCapacity;
    public int trapCapacity;

    // Needed for the sniper ability
    public List<GameObject> spottedInfiltrators;

    // Sniper cooldown
    public int sniperCooldown;

    // Keep track of network's time from last time you used the sniper
    public double lastTimeSniped;

    // Check if sniper is available
    public bool isSniperReady;

    #endregion

    #region Public Properties

    public PlayerManager PlayerManager { get; set; }

    #endregion
    
    // Use this for initialization
    void Start()
    {
        UI_Loaded = false;
        isToggleCycled = true;
        startFlash = false;

        // The quantity limit of each unit
        lampCapacity = 4;
        cameraCapacity = 3;
        trapCapacity = 5;

        // Handle sniper cooldown mechanic
        sniperCooldown = 20;
        isSniperReady = true;
    }


    // Update is called once per frame
    void Update()
    {
        if ((PlayerManager == null || !PlayerManager.isLocalPlayer) && transform.parent.gameObject.activeSelf)
        {
            transform.parent.gameObject.SetActive(false);
        }

        if (PlayerManager == null)
        {
            return;
        }

        // Only load the unit UI once player kind has been determined
        if (UI_Loaded == false)
        {
            // Outside the "if" statement because it works for both players
            toggleContainer = GameObject.Find("Toggle Command");
            toggleControl = GameObject.Find("Toggle").GetComponent<Button>();

            // Defender's UI
            if (PlayerManager.Kind == PlayerManager.PlayerKind.Defender)
            {
                controlLocked = false;
                controlContainer = GameObject.Find("Unit Commands");

                // Display Defender icon
                PlayerKind.sprite = defender;

                Color32 temp_color = PlayerKind.color;
                temp_color.a = 255;
                PlayerKind.color = temp_color;

                // Add all unit buttons to the list, while modifying each one
                for (int i = 1; i <= controlContainer.transform.childCount; i++)
                {
                    unitControls.Add(GameObject.Find("Unit " + i).GetComponent<Button>());

                    // Set button's image to the sprite according to the unit ID
                    switch (i)
                    {
                        case 1:
                            unitControls[i - 1].image.sprite = lamp;
                            break;
                        case 2:
                            unitControls[i - 1].image.sprite = cam;
                            break;
                        case 3:
                            unitControls[i - 1].image.sprite = trap;
                            break;
                        case 4:
                            unitControls[i - 1].image.sprite = sniper;
                            break;
                    }

                    // Just to make buttons visible after UI is loaded (mainly for setting alpha to 255)
                    unitControls[i - 1].image.color = new Color32(255, 255, 255, 255);
                }
            }

            // Infiltrator's UI (disable view for unit commands since infiltrator can't spawn anything)
            else if (PlayerManager.Kind == PlayerManager.PlayerKind.Infiltrator)
            {
                controlContainer = GameObject.Find("Unit Commands");
                controlContainer.SetActive(false);

                // Display Infiltrator icon
                PlayerKind.sprite = infiltrator;

                Color32 temp_color = PlayerKind.color;
                temp_color.a = 255;
                PlayerKind.color = temp_color;
            }

            // Once UI is loaded, don't update this anymore
            UI_Loaded = true;
        }

        // If UI is loaded, then keep checking to lock/unlock buttons and coloring as game is running
        else
        {
            // Only enable controls once game is ready
            if (PlayerManager.GameReady)
            {
                // Enable toggle control
                toggleControl.interactable = true;

                // Acess the Player Manager script
                characterContainer = GameObject.Find("PlayerManager(Clone)").GetComponent<PlayerManager>();

                if (PlayerManager.Kind == PlayerManager.PlayerKind.Defender)
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
                            if ((i + 1 == 1 && GameObject.FindGameObjectsWithTag("Lamp").Length >= lampCapacity) ||
                                (i + 1 == 2 && GameObject.FindGameObjectsWithTag("Camera").Length >= cameraCapacity) ||
                                (i + 1 == 3 && GameObject.FindGameObjectsWithTag("Trap").Length >= trapCapacity) ||
                                 i + 1 == 4 && !isSniperReady)
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

                // Always update sprites depending on toggle's state
                if (isToggleCycled)
                {
                    toggleControl.image.sprite = cycle;

                    // Set all character to cycle
                    for (int i = 0; i < characterContainer.Characters.Count; i++)
                    {
                        characterContainer.Characters[i].LoopPatrol = true;
                    }
                }
                else
                {
                    toggleControl.image.sprite = back_forth;

                    // Set all character to reverse
                    for (int i = 0; i < characterContainer.Characters.Count; i++)
                    {
                        characterContainer.Characters[i].LoopPatrol = false;
                    }
                }

                // Always check the sniper's cooldown state
                SniperCooldown();

                // Flash screen when triggered to do so
                if (startFlash == true)
                {
                    StartCoroutine(ScreenFlash());
                }
            }
        }
    }


    public void UI_Ready()
    {
        ReadyButton.interactable = false;
        PlayerManager.PlayerReady();
    }


    // Select its ID and make button green
    public void selectUnit(int ID)
    {
        // No need for button color change for the sniper ability (which is ID 4)
        if(ID != 4)
        {
            // Highlight button green, and assign button selected's ID
            buttonColors = unitControls[ID - 1].colors;
            buttonColors.normalColor = Color.green;
            buttonColors.disabledColor = Color.green;
            unitControls[ID - 1].colors = buttonColors;
            buttonSelected = ID;
            controlLocked = true;
        }
        else
        {
            // Snipe!
            OneHitKO();
        }
    }


    // Switch between the toggle states when user clicks button
    public void togglePathState()
    {
        if (isToggleCycled)
        {
            isToggleCycled = false;
        }
        else
        {
            isToggleCycled = true;
        }
    }


    // Have sniper kill a random infiltrator that is spotted
    void OneHitKO()
    {
        // Get a hold of all the infiltrators
        GameObject[] infiltrators = GameObject.FindGameObjectsWithTag("Bad");

        // Out of those, keep track of the spotted ones in the list
        for (int i = 0; i < infiltrators.Length; i++)
        {
            if (infiltrators[i].GetComponent<Character>().IsSpotted == true)
            {
                spottedInfiltrators.Add(infiltrators[i]);
            }
        }

        // Only check if there are any spotted infiltrators
        if(spottedInfiltrators.Count > 0)
        {
            GameObject target = spottedInfiltrators[UnityEngine.Random.Range(0, spottedInfiltrators.Count)];

            // Kill the character
            target.GetComponent<Character>().TakeSniperDamage();
            
            // Flash screen
            startFlash = true;

            // Clear list once finished
            spottedInfiltrators.Clear();
        }

        // Sniper not ready anymore, cooldown begins
        isSniperReady = false;
    }


    // Updates sniper availability based on cooldown and time passed
    public void SniperCooldown()
    {
        // When sniper is not ready, check when the cooldown is over
        if (!isSniperReady)
        {
            // +1 to make cooldown label display stop at 1, not 0
            cooldownLabel.text = ((int)((sniperCooldown + lastTimeSniped) - Network.time) + 1).ToString();

            if (Network.time > sniperCooldown + lastTimeSniped)
            {
                isSniperReady = true;
                cooldownLabel.text = "";
            }
        }
        // When ready, always check latest time
        else
        {
            lastTimeSniped = Network.time;
        }
    }


    // Handle red flash on screen
    public IEnumerator ScreenFlash()
    {
        Color32 color = killImage.color;

        color.a = 150;
        killImage.color = color;
        yield return new WaitForSeconds(0.075f);

        color.a = 0;
        killImage.color = color;

        startFlash = false;
    }
}