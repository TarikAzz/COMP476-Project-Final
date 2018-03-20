using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// The object responsible for managing the player characters
/// </summary>
public class PlayerManager : NetworkBehaviour
{
    /// <summary>
    /// The different roles a player can play
    /// </summary>
    public enum PlayerKind
    {
        Infiltrator,
        Defender
    }

    /// <summary>
    /// The sensitivity of the camera controls
    /// </summary>
    public float MouseSensitivity;

    /// <summary>
    /// The characters owned by the manager
    /// </summary>
    public Character[] Characters;

    /// <summary>
    /// The player's role
    /// </summary>
    public PlayerKind Kind { get; set; }
    
    /// <summary>
    /// The player's HUD
    /// </summary>
    private InGamePanel _inGamePanel;

    /// <summary>
    /// Intializes most of the variables when the player connects to the network
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        for (var i = 0; i < Characters.Length; i++)
        {
            Characters[i].Owner = this;
            Characters[i].Colorize(Color.blue);
        }

        Kind = NetworkManager.singleton.numPlayers == 1 ? PlayerKind.Defender : PlayerKind.Infiltrator;
        
        _inGamePanel = FindObjectOfType<InGamePanel>();
        _inGamePanel.PlayerKind.text = Kind.ToString();
    }

    /// <summary>
    /// Updates character selection and camera movement
    /// </summary>
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                return;
            }

            var hitCharacter = hit.collider.GetComponent<Character>();

            if (hitCharacter == null || !OwnsCharacter(hitCharacter))
            {
                return;
            }

            if (hitCharacter.IsSelected)
            {
                hitCharacter.Deselect();
            }
            else
            {
                hitCharacter.Select();
            }
        }

        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");
        
        if (Input.GetMouseButton(2))
        {
            Camera.main.transform.Translate(new Vector3(-mouseX, 0.0f, -mouseY) * MouseSensitivity, Space.World);
        }
    }

    /// <summary>
    /// Whether the manager owns a specific character
    /// </summary>
    /// <param name="character">The character</param>
    /// <returns>The manager's ownership of the character</returns>
    public bool OwnsCharacter(Character character)
    {
        return character.Owner != null && character.Owner.isLocalPlayer;
    }
}
