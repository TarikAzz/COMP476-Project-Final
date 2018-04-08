using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MyNetworkManager : NetworkManager
{
    // Defined in Inspector
    public Canvas TitleScreen;
    public Text IP_Address;

    // Use this for initialization
    void Start()
    {
        ToggleNetworkHUD(false);
        
    }








    // Update is called once per frame
    void Update()
    {

    }




    public void ToggleNetworkHUD(bool state)
    {
        gameObject.GetComponent<NetworkManagerHUD>().showGUI = state;
    }




    


    public void CreateSession()
    {
        TitleScreen.enabled = false;
        ToggleNetworkHUD(true);
        StartHost();
    }



    public void JoinSession()
    {
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




    public void MatchMaker()
    {
        TitleScreen.enabled = false;
        ToggleNetworkHUD(true);
        StartMatchMaker();
    }






    public override void OnStopServer()
    {
        TitleScreen.enabled = true;
        HandleDisconnection();
        base.OnStopServer();
    }

    public override void OnStopClient()
    {
        TitleScreen.enabled = true;
        HandleDisconnection();
        base.OnStopClient();
    }



    




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
}
