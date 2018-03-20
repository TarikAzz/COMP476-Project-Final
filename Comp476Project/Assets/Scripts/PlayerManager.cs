using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    public enum PlayerKind
    {
        Infiltrator,
        Defender
    }

    public float MouseSensitivity;
    public List<Character> Characters;

    public PlayerKind Kind { get; set; }

    private int _id;
    private InGamePanel _inGamePanel;

    public override void OnStartLocalPlayer()
    {
        for (var i = 0; i < Characters.Count; i++)
        {
            _id = NetworkManager.singleton.numPlayers;
            Characters[i].OwnerId = _id;
            Characters[i].Owner = this;
            Characters[i].Colorize(Color.blue);
        }

        Kind = NetworkManager.singleton.numPlayers == 1 ? PlayerKind.Defender : PlayerKind.Infiltrator;

        if (Kind == PlayerKind.Infiltrator)
        {
            /*var newCameraPosition = Camera.main.transform.position;
            newCameraPosition.z = -newCameraPosition.z;

            var newCameraRotation = Quaternion.Euler(55f, 180f, 0f);

            Camera.main.transform.position = newCameraPosition;
            Camera.main.transform.rotation = newCameraRotation;*/
        }

        _inGamePanel = FindObjectOfType<InGamePanel>();
        _inGamePanel.PlayerKind.text = Kind.ToString();
    }

    /// <summary>
    /// The Update method.
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

    /// <summary>
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
