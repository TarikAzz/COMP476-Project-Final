using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MainManager : NetworkBehaviour
{
    [SyncVar]
    public bool DefenderReady;

    [SyncVar]
    public bool InfiltratorReady;
    
    public void PlayerReady(PlayerManager.PlayerKind playerKind)
    {
        if (!isServer)
            return;
        
        RpcPlayerReady(playerKind);
    }
    
    [ClientRpc]
    public void RpcPlayerReady(PlayerManager.PlayerKind playerKind)
    {
        switch (playerKind)
        {
            case PlayerManager.PlayerKind.Defender:
                DefenderReady = true;
                break;

            case PlayerManager.PlayerKind.Infiltrator:
                InfiltratorReady = true;
                break;
        }

        if (!DefenderReady || !InfiltratorReady)
        {
            return;
        }

        var playerManagers = FindObjectsOfType<PlayerManager>();

        foreach (var playerManager in playerManagers)
        {
            playerManager.GameReady = true;
        }
    }
}
