using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class MyNetworkManager : NetworkManager
{
    // Defined in Inspector
    public Canvas TitleScreen;
    public Text IP_Address;

    // Used to detect when matchmaker is ON
    public bool Matchmaking;

    // Use this for initialization
    void Start()
    {
        // Hide HUD at the start, title screen handles networking options
        ToggleNetworkHUD(false);
    }
    
    // Update is called once per frame
    void Update()
    {
        // A hack to "override" the Disable Match Maker button
        if(Matchmaking && matchMaker == null)
        {
            Matchmaking = false;
            TitleScreen.enabled = true;
            ToggleNetworkHUD(false);
        }
    }

    // Listener for the Create Session button
    public void CreateSession()
    {
        TitleScreen.enabled = false;
        ToggleNetworkHUD(true);
        StartHost();
    }

    // Listener for the Join Session button
    public void JoinSession()
    {
        // Have the default address set to localhost
        if (IP_Address.text == "")
        {
            networkAddress = "localhost";
        }
        else
        {
            networkAddress = IP_Address.text;
        }

        TitleScreen.enabled = false;
        ToggleNetworkHUD(true);
        StartClient();
    }

    // Listener for the Match Maker button
    public void MatchMaker()
    {
        TitleScreen.enabled = false;
        ToggleNetworkHUD(true);
        Matchmaking = true;
        StartMatchMaker();
    }

    // Override to bring back the title screen and to disable the default HUD again
    public override void OnStopServer()
    {
        TitleScreen.enabled = true;
        ToggleNetworkHUD(false);
        HandleDisconnection();
        base.OnStopServer();
    }

    // Override to bring back the title screen and to disable the default HUD again
    public override void OnStopClient()
    {
        TitleScreen.enabled = true;
        ToggleNetworkHUD(false);
        HandleDisconnection();
        base.OnStopClient();
    }

    // Handles disconnection
    private void HandleDisconnection()
    {
        Camera.main.transform.position = new Vector3(0f, 84f, 65f);
        Camera.main.fieldOfView = Camera.main.GetComponent<RtsCamera>().maxFov;

        foreach (var barrier in FindObjectsOfType<Barrier>())
        {
            barrier.GetComponent<MeshRenderer>().enabled = true;
        }

        foreach (var mainManager in FindObjectsOfType<MainManager>())
        {
            mainManager.DefenderReady = false;
            mainManager.InfiltratorReady = false;
        }
    }

    // Toggle the default HUD appearence
    public void ToggleNetworkHUD(bool state)
    {
        gameObject.GetComponent<NetworkManagerHUD>().showGUI = state;
    }
}