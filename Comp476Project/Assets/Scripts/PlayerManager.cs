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
    #region Enum
    
    /// <summary>
    /// The different roles a player can play
    /// </summary>
    public enum PlayerKind
    {
        Infiltrator,
        Defender
    }

    #endregion

    #region Public variables

    /// <summary>
    /// The sensitivity of the camera controls
    /// </summary>
    public float MouseSensitivity;
	
	/// <summary>
    /// The characters owned by the manager
    /// </summary>
    public List<Character> Characters;

    #endregion

    #region Public properties

    /// <summary>
    /// The player's role
    /// </summary>
    public PlayerKind Kind { get; set; }

    #endregion

    #region Private variables

    /// <summary>
    /// The player's HUD
    /// </summary>
    private InGamePanel _inGamePanel;

    #endregion

    /// <summary>
    /// Intializes most of the variables when the player connects to the network
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        for (var i = 0; i < Characters.Count; i++)
        {
            Characters[i].Owner = this;
            Characters[i].Colorize(Color.blue);
        }

        // Assigns a role to the player, depending on if they joined first or not
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

        // If shift and left click is pressed.
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            RectangleSelect();

            CharacterSelection(true);
        }
        else if (Input.GetMouseButtonDown(0)) 
        {
            CharacterSelection(false);
        }

        RectangleSelect();
        
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

    /// <summary>
    /// Selects the characters using the left mouse button.
    /// Changes behaviour based on if the shift button is held.
    /// </summary>
    /// <param name="isShift">Option bool used to determine if shift was pressed</param>
    /// <author>Tarik</author>
    void CharacterSelection(bool isShift = false)
    {
        RaycastHit hit;

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
        {
            return;
        }

        var hitCharacter = hit.collider.GetComponent<Character>();

        if (hitCharacter == null || !OwnsCharacter(hitCharacter))
        {
            if (!isShift)
            {
                Characters.ForEach(c => c.Deselect());
            }
            return;
        }

        if (!isShift)
        {
            Characters.ForEach(c => c.Deselect());
            hitCharacter.Select();
        }
        else
        {
            if (hitCharacter.IsSelected)
            {
                hitCharacter.Deselect();
            }
            else
            {
                hitCharacter.Select();
            }
        }
    }
    
    /// Draws a rectangle from the mouse drag and selects the characters within it.
    /// </summary>
    /// <author>Tarik</author>
    void RectangleSelect()
    {
        // Iterate through all characters to select them in the rectangle selection.
        foreach (var character in Characters)
        {
            bool isRectanlgeSelcted = GetComponent<MouseSelection>().IsWithinSelectionBounds(character.gameObject);

            if (isRectanlgeSelcted)
            {
                character.Select();
            }
        }
    }
}
