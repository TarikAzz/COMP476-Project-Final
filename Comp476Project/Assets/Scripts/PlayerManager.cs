using System;
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
    /// The characters owned by the manager
    /// </summary>
    public List<Character> Characters;
    
    #endregion

    #region Public properties

    /// <summary>
    /// The player's role
    /// </summary>
    public PlayerKind Kind { get; set; }

    /// <summary>
    /// Whether or not the actual game is ongoing
    /// </summary>
    public bool GameOn { get; set; }

    /// <summary>
    /// Whether or not the actual game is ready for setup
    /// </summary>
    public bool GameReady
    {
        get
        {
            return _gameReady;
        }
        set
        {
            _gameReady = value;

            if (_gameReady)
            {
                if (!isLocalPlayer)
                {
                    return;
                }

                _inGamePanel.ReadyButton.gameObject.SetActive(false);
                _setupTimer = MainManager.SetupTime;
            }
        }
    }

    /// <summary>
    /// The number of infiltrating characters currently in the Goal Zone
    /// On set : ends the game if the value is greater than or equal CharactersNeededToWin
    /// </summary>
    public int InfiltratorsInGoalZone
    {
        get
        {
            return _infiltratorsInGoalZone;
        }
        set
        {
            _infiltratorsInGoalZone = value;

            if (value >= MainManager.CharactersNeededToWin)
            {
                EndGame(PlayerKind.Infiltrator);
            }
        }
    }

    #endregion

    #region Private variables

    /// <summary>
    /// The player's HUD
    /// </summary>
    private InGamePanel _inGamePanel;
    
    /// <summary>
    /// InfiltratorsInGoalZone backing field
    /// </summary>
    private int _infiltratorsInGoalZone;

    /// <summary>
    /// GameReady backing field
    /// </summary>
    private bool _gameReady;

    /// <summary>
    /// The timer counting down the setup time
    /// </summary>
    private float _setupTimer;

    /// <summary>
    /// The barriers preventing the infiltrator to step through the level on setup
    /// </summary>
    private Barrier[] _setupBarriers;

    /// <summary>
    /// MainManager backing field
    /// </summary>
    private MainManager _mainManager;

    #endregion

    #region Private properties

    /// <summary>
    /// The main game manager
    /// </summary>
    private MainManager MainManager
    {
        get
        {
            if (_mainManager == null)
            {
                _mainManager = FindObjectOfType<MainManager>();
            }

            return _mainManager;
        }
    }

    #endregion

    /// <summary>
    /// Intializes most of the variables when the player connects to the network
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        for (var i = 0; i < Characters.Count; i++)
        {
            Characters[i].PlayerManager = this;
            Characters[i].Colorize(Color.blue);
        }

        // Assigns a role to the player, depending on if they joined first or not
        Kind = NetworkManager.singleton.numPlayers == 1 ? PlayerKind.Defender : PlayerKind.Infiltrator;

        switch (Kind)
        {
            case PlayerKind.Infiltrator:
                transform.position = MainManager.InfiltratorSpawn.position;
                break;
            case PlayerKind.Defender:
                transform.position = MainManager.DefenderSpawn.position;
                break;
        }

        _inGamePanel = FindObjectOfType<InGamePanel>();
        _inGamePanel.PlayerKindText.text = Kind.ToString();
        _inGamePanel.PlayerManager = this;

        _setupBarriers = FindObjectsOfType<Barrier>();
    }

    /// <summary>
	/// Updates character selection and camera movement
    /// </summary>
    void Update()
    {
        if (!isLocalPlayer && !GameReady)
        {
            return;
        }
        
        if (_setupTimer > 0)
        {
            _setupTimer -= Time.deltaTime;

            _inGamePanel.SetupTimerImage.fillAmount = _setupTimer / MainManager.SetupTime;

            if (_setupTimer <= 0)
            {
                _inGamePanel.SetupTimerImage.fillAmount = 0;
                StartGame();
            }
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
    }

    /// <summary>
    /// Passes the ready function
    /// </summary>
    public void PlayerReady()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        CmdPlayerReady(Kind);
    }

    /// <summary>
    /// Tells the main manager that this player is ready
    /// </summary>
    /// <param name="playerKind">The player kind</param>
    [Command]
    public void CmdPlayerReady(PlayerKind playerKind)
    {
        MainManager.PlayerReady(playerKind);
    }

    /// <summary>
    /// Starts the actual game, allowing the infiltrator through the level
    /// </summary>
    public void StartGame()
    {
        foreach (var barrier in _setupBarriers)
        {
            barrier.GetComponent<MeshRenderer>().enabled = false;
        }

        GameOn = true;
        _inGamePanel.GameStateText.text = "Go!";
    }

    /// <summary>
    /// Ends the game, reseting the level and other variable to their standby states
    /// </summary>
    public void EndGame()
    {
        foreach (var barrier in _setupBarriers)
        {
            barrier.GetComponent<MeshRenderer>().enabled = true;
        }

        GameOn = false;
    }
    
    /// <summary>
    /// Whether the manager owns a specific character
    /// </summary>
    /// <param name="character">The character</param>
    /// <returns>The manager's ownership of the character</returns>
    public bool OwnsCharacter(Character character)
    {
        return character.PlayerManager != null && character.PlayerManager.isLocalPlayer;
    }

    /// <summary>
    /// Removes a player from the list and destroys it. Ends the game is the Infiltrator character count is small than CharactersNeededToWin
    /// </summary>
    /// <param name="character">The character to remove</param>
    public void RemoveCharacter(Character character)
    {
        Characters.Remove(character);
        character.gameObject.SetActive(false);
        //Destroy(character.gameObject);

        if (Kind == PlayerKind.Infiltrator && Characters.Count < MainManager.CharactersNeededToWin)
        {
            EndGame(PlayerKind.Defender);
        }
    }

    /// <summary>
    /// Selects the characters using the left mouse button.
    /// Changes behaviour based on if the shift button is held.
    /// </summary>
    /// <param name="isShift">Option bool used to determine if shift was pressed</param>
    /// <author>Tarik</author>
    private void CharacterSelection(bool isShift = false)
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
    private void RectangleSelect()
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

    /// <summary>
    /// Ends the game after a player wins
    /// </summary>
    /// <param name="winningPlayer">The winning player kind</param>
    private void EndGame(PlayerKind winningPlayer)
    {
        Debug.Log(winningPlayer + " wins!");
    }
}
