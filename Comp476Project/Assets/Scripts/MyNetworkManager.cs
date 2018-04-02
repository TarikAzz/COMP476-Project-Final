using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager
{
    public override void OnStopServer()
    {
        HandleDisconnection();

        base.OnStopServer();
    }

    public override void OnStopClient()
    {
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
