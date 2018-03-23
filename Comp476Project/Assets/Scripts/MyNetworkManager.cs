using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager
{
    public GameObject MainManagerPrefab;

    public override void OnStartServer()
    {
        /*base.OnStartServer();

        var manager = Instantiate(MainManagerPrefab);
        NetworkServer.Spawn(manager);*/
    }
}
