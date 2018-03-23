using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MainManager : NetworkBehaviour
{
    /// <summary>
    /// The manager instance
    /// </summary>
    public static MainManager Instance;

    /// <summary>
    /// The readyness of the players
    /// </summary>
    public bool[] PlayersReady;

    public override void OnStartServer()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Debug.Log("initialized");
        
        PlayersReady = new bool[2];
    }

    public void SendReady()
    {
        for(var i = 0; i < PlayersReady.Length; i++)
        {
            if(!PlayersReady[i])
            {
                PlayersReady[i] = true;
                return;
            }
        }

        var playerManagers = FindObjectsOfType<PlayerManager>();

        foreach(var playerManager in playerManagers)
        {
            playerManager.GameReady = true;
        }
    }
}
