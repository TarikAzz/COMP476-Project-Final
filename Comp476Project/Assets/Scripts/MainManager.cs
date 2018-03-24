using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MainManager : NetworkBehaviour
{
    /// <summary>
    /// The object holding the position for the defender's spawn position
    /// </summary>
    public Transform DefenderSpawn;

    /// <summary>
    /// The object holding the position for the infiltrator's spawn position
    /// </summary>
    public Transform InfiltratorSpawn;

    /// <summary>
    /// The number of characters the infiltrating player needs to bring to the goal zone to win the game
    /// </summary>
    public int CharactersNeededToWin;

    /// <summary>
    /// The time before the game starts when defender and infiltrator can start setting up their stuff
    /// </summary>
    public float SetupTime;

    /// <summary>
    /// Is the defending player ready
    /// </summary>
    [SyncVar]
    public bool DefenderReady;

    /// <summary>
    /// Is the infiltrating player ready
    /// </summary>
    [SyncVar]
    public bool InfiltratorReady;
    
    /// <summary>
    /// Sends the ready message from the server
    /// </summary>
    /// <param name="playerKind">The player that is ready</param>
    public void PlayerReady(PlayerManager.PlayerKind playerKind)
    {
        if (!isServer)
        {
            return;
        }
            
        RpcPlayerReady(playerKind);
    }

    /// <summary>
    /// Sends the end message from the server
    /// </summary>
    /// <param name="winningPlayer">the winning player</param>
    public void EndGame(PlayerManager.PlayerKind winningPlayer)
    {
        if (!isServer)
        {
            return;
        }

        RpcEndGame(winningPlayer);
    }

    /// <summary>
    /// Sends the end message back to the client
    /// </summary>
    /// <param name="winningPlayer">the winning player</param>
    [ClientRpc]
    public void RpcEndGame(PlayerManager.PlayerKind winningPlayer)
    {
        var allCharacters = FindObjectsOfType<Character>();

        foreach (var character in allCharacters)
        {
            character.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sends the ready message back to the client
    /// </summary>
    /// <param name="playerKind">The player that is ready</param>
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
