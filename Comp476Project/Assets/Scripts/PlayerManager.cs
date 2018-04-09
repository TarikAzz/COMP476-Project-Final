using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// The maximum health for every character
    /// </summary>
    public float CharactersMaxHealth;

    /// <summary>
    /// The current health of characters in the list
    /// </summary>
    public SyncListFloat CharactersHealth;

    /// <summary>
    /// The damage characters take per second when spotted
    /// </summary>
    public float DamagePerSecond;

    /// <summary>
    /// Tracks how many characters are currently selected
    /// </summary>
    public int Selected_Characters;

    /// <summary>
    /// Reference to the Audio Manager (CANNOT GET IT THROUGH INSPECTOR)
    /// </summary>
    public AudioManager audioManager;

    /// <summary>
    /// The lighting.
    /// </summary>
    public GameObject lightning;

    /// <summary>
    /// Disable controls once game is over
    /// </summary>
    public bool DisableControls;

    /// <summary>
    /// Sprites for Win and Lose screens (defined in the Inspector)
    /// </summary>
    public Sprite winImage;
    public Sprite loseImage;

    /// <summary>
    /// TEMPORARY! Used to sync animations the opponent's animations.
    /// </summary>
    List<Vector3> otherPlayersPositions = new List<Vector3> { new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3() };

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

                _inGamePanel.EndGameGroup.gameObject.SetActive(false);
                _inGamePanel.ReadyButton.gameObject.SetActive(false);
                _inGamePanel.SetupGroup.gameObject.SetActive(true);
                _setupTimer = MainManager.SetupTime;

                // Play setup ticking noise
                audioManager.playSetup();

                var selectedCharacterPositions =
                    (from character in Characters select character.transform.position).ToArray();
                var center = Vector3.zero;

                foreach (var position in selectedCharacterPositions)
                {
                    center += position;
                }

                center /= selectedCharacterPositions.Length;

                if (_cameraOffset == Vector3.zero)
                {
                    _cameraOffset = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z / (Kind == PlayerKind.Defender ? 1.25f : 1.5f));
                }

                Camera.main.transform.position = center + _cameraOffset;
                Camera.main.fieldOfView = Camera.main.GetComponent<RtsCamera>().minFov;
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

            if (_infiltratorsInGoalZone >= MainManager.CharactersNeededToWin)
            {
                CmdEndGame(PlayerKind.Infiltrator);
            }
        }
    }

    /// <summary>
    /// The number of dead infiltrators
    /// </summary>
    public int InfiltratorDead
    {
        get
        {
            return _infiltratorDead;
        }
        set
        {
            _infiltratorDead = value;

            if (_infiltratorDead >= MainManager.CharactersNeededToWin)
            {
                CmdEndGame(PlayerKind.Defender);
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
    /// InfiltratorDead backing field
    /// </summary>
    private int _infiltratorDead;

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

    /// <summary>
    /// The Camera offset
    /// </summary>
    private Vector3 _cameraOffset = Vector3.zero;

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

    void Awake()
    {
        CharactersHealth = new SyncListFloat();

        lightning = GameObject.Find("Lightning");

        // Get audio functionalities (CANNOT GET IT THROUGH INSPECTOR)
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        for (var i = 0; i < Characters.Count; i++)
        {
            Characters[i].PlayerManager = this;
        }
    }

    public override void OnStartServer()
    {
        for (var i = 0; i < Characters.Count; i++)
        {
            CharactersHealth.Add(CharactersMaxHealth);
        }
    }

    /// <summary>
    /// Intializes most of the variables when the player connects to the network
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        // Assigns a role to the player, depending on if they joined first or not
        Kind = NetworkManager.singleton.numPlayers == 1 ? PlayerKind.Defender : PlayerKind.Infiltrator;

        for (var i = 0; i < Characters.Count; i++)
        {
            Characters[i].gameObject.SetActive(false);
        }

        switch (Kind)
        {
            case PlayerKind.Infiltrator:
                transform.localPosition = MainManager.InfiltratorSpawn.position;
                gameObject.tag = "InfiltratorPlayer";
                break;

            case PlayerKind.Defender:
                transform.localPosition = MainManager.DefenderSpawn.position;
                gameObject.tag = "DefenderPlayer";
                break;
        }

        for (var i = 0; i < Characters.Count; i++)
        {
            Characters[i].Colorize(Color.blue);
            Characters[i].gameObject.SetActive(true);
        }

        _inGamePanel = FindObjectOfType<InGamePanel>();
        _inGamePanel.PlayerKindText.text = Kind.ToString();
        _inGamePanel.PlayerManager = this;

        _setupBarriers = FindObjectsOfType<Barrier>();
    }

    bool doTaggingOnce = true;
    bool doModelsOnce = true;

    /// <summary>
    /// Updates character selection and camera movement
    /// </summary>
    void Update()
    {
        // Changed this to work properly.
        if (!isLocalPlayer)
        {
            return;
        }

        if (!GameReady)
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

                // Stop the setup sound and play game start sound effect
                audioManager.stopSetup();
                audioManager.playGameStart();
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
        CountSelections();

        #region Visibility

        // Tags all players once we know they are present.
        if (doTaggingOnce)
        {
            // Manages the visibility.
            if (Kind == PlayerKind.Infiltrator)
            {
                if (Characters.Count > 0)
                {
                    Characters.ForEach(c => c.tag = "Bad");
                }

                GameObject otherPlayer = GameObject.FindGameObjectWithTag("OtherPlayer");

                for (int i = 0; i < 5; i++)
                {
                    otherPlayer.transform.GetChild(i).tag = "Good";
                }
            }
            else
            {
                if (Characters.Count > 0)
                {
                    Characters.ForEach(c => c.tag = "Good");
                }

                GameObject otherPlayer = GameObject.FindGameObjectWithTag("OtherPlayer");

                for (int i = 0; i < 5; i++)
                {
                    otherPlayer.transform.GetChild(i).tag = "Bad";
                }
            }

            doTaggingOnce = false;
        }

        if (Kind == PlayerKind.Defender)
        {
            var infiltrators = GameObject.FindGameObjectsWithTag("Bad");

            foreach (var infiltrator in infiltrators)
            {
                //bool isSpotted = (infiltrator.GetComponent<Character>().IsSpotted);

                // Show visibility on lightning flash
                if (lightning.GetComponent<Lightning>().showInfiltrators)
                {
                    infiltrator.gameObject.GetComponent<Character>().ToggleVisibility(true);
                    infiltrator.GetComponent<Character>().IsSpotted = true;
                }
                else
                {
                    infiltrator.gameObject.GetComponent<Character>().ToggleVisibility(false);
                    infiltrator.GetComponent<Character>().IsSpotted = false;
                }
            }
        }
        #endregion

        // Changes the models.
        if (doModelsOnce)
        {
            ChangeChars();
            doModelsOnce = false;
        }

        // Temporary
        ApplyOpponentAnimations();
    }

    /// <summary>
    /// TEMPORARY Runs the animaitons for the other player.
    /// </summary>
    void ApplyOpponentAnimations()
    {
        GameObject[] otherCharacters;

        if (Kind == PlayerKind.Defender)
        {
            otherCharacters = GameObject.FindGameObjectsWithTag("Bad");
        }
        else
        {
            otherCharacters = GameObject.FindGameObjectsWithTag("Good");
        }
        
        for (int i = 0; i < otherCharacters.Length; i++)
        {
            if (otherPlayersPositions[i] != otherCharacters[i].transform.position)
            {
                otherCharacters[i].GetComponent<Character>().IsMoving = true;
                otherPlayersPositions[i] = otherCharacters[i].transform.position;
            }
            else
            {
                otherCharacters[i].GetComponent<Character>().IsMoving = false;
            }
        }
    }

    /// <summary>
    /// Chnages the models depending on the player type.
    /// </summary>
    void ChangeChars()
    {
        GameObject[] otherCharacters;

        if (Kind == PlayerKind.Defender)
        {
            otherCharacters = GameObject.FindGameObjectsWithTag("Bad");
        }
        else
        {
            otherCharacters = GameObject.FindGameObjectsWithTag("Good");
        }

        for (var i = 0; i < otherCharacters.Length; i++)
        {
            otherCharacters[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
            otherCharacters[i].gameObject.transform.GetChild(4).gameObject.SetActive(true);
        }

        // Infiltrator or has 5 children since the FOV is absent.
        for (int i = 0; i < Characters.Count; i++)
        {
            Characters[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
            Characters[i].gameObject.transform.GetChild(4).gameObject.SetActive(true);
        }
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

        lightning.GetComponent<Lightning>().hasGameStarted = true;
        GameOn = true;
        _inGamePanel.SetupGroup.SetActive(false);
    }

    /// <summary>
    /// Deactivate FOV if player is infiltrator
    /// </summary>
    public void DeactivateFOV()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        CmdDeactivateFOV();
    }

    /// <summary>
    /// The command to DeactivateFOV
    /// </summary>
    [Command]
    public void CmdDeactivateFOV()
    {
        RpcDeactivateFOV();
    }

    /// <summary>
    /// The RPC call to DeactivateFOV
    /// </summary>
    [ClientRpc]
    public void RpcDeactivateFOV()
    {
        for (var i = 0; i < Characters.Count; i++)
        {
            if (Characters[i].GetComponent<FieldOfView>() == null)
            {
                continue;
            }

            Destroy(Characters[i].GetComponent<FieldOfView>().viewMeshFilter.gameObject);
            Destroy(Characters[i].GetComponent<FieldOfView>());
        }
    }

    /// <summary>
    /// Assign damage to a specific character and handles elimination
    /// </summary>
    /// <param name="character">The damaged character</param>
    public void AssignDamage(Character character)
    {
        if (CharactersHealth[Characters.IndexOf(character)] <= 0)
        {
            return;
        }

        CharactersHealth[Characters.IndexOf(character)] -= DamagePerSecond * Time.deltaTime;
        RpcUpdateCharacterHealth(Characters.IndexOf(character));
    }

    /// <summary>
    /// Assign heavy damage to a specific character and handles elimination (used by Defender's Sniper)
    /// </summary>
    /// <param name="character">The damaged character</param>
    public void AssignBurstDamage(Character character)
    {
        if (CharactersHealth[Characters.IndexOf(character)] <= 0)
        {
            return;
        }

        CharactersHealth[Characters.IndexOf(character)] -= 9999;
        RpcUpdateCharacterHealth(Characters.IndexOf(character));
    }

    /// <summary>
    /// Tells the client to update the character's health info
    /// </summary>
    /// <param name="characterIndex">The index of the character to update</param>
    [ClientRpc]
    private void RpcUpdateCharacterHealth(int characterIndex)
    {
        if (CharactersHealth[characterIndex] <= 0)
        {
            RemoveCharacter(characterIndex);
            return;
        }

        Characters[characterIndex].UpdateHealthBar(CharactersHealth[characterIndex] / CharactersMaxHealth);
    }

    /// <summary>
    /// Removes a player from the list. Ends the game is the Infiltrator character count is small than CharactersNeededToWin
    /// </summary>
    /// <param name="characterIndex">The index of the character to remove</param>
    public void RemoveCharacter(int characterIndex)
    {
        Characters[characterIndex].DestroyTargetIndicators();
        Characters[characterIndex].gameObject.SetActive(false);
        InfiltratorDead++;
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
    /// Selects the characters using the left mouse button.
    /// Changes behaviour based on if the shift button is held.
    /// </summary>
    /// <param name="isShift">Option bool used to determine if shift was pressed</param>
    /// <author>Tarik</author>
    private void CharacterSelection(bool isShift)
    {
        // Had to add this to fix the shift select not working. 
        // In Update another condition was added for this check, so it didn't work anymore.
        if (!isLocalPlayer)
        {
            return;
        }

        RaycastHit hit;

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            return;
        }

        var hitCharacter = hit.collider.GetComponent<Character>();

        if (hitCharacter == null || !OwnsCharacter(hitCharacter))
        {
            // just uncommented to test fixing it.
            if (!isShift)
            {
                Characters.ForEach(c => c.Deselect());
            }
            return;
        }

        //just uncommented to test fixing it.
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

        #region OriginalSelectionCode
        //if (isShift)
        //{
        //    RaycastHit hit;

        //    if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        //    {
        //        return;
        //    }
        //    var hitCharacter = hit.collider.GetComponent<Character>();

        //    if (hitCharacter == null || !OwnsCharacter(hitCharacter))
        //    {
        //        return;
        //    }

        //    if (hitCharacter.IsSelected)
        //    {
        //        hitCharacter.Deselect();
        //    }
        //    else
        //    {
        //        hitCharacter.Select();
        //    }

        //}
        //else
        //{
        //    RaycastHit hit;

        //    if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        //    {
        //        return;
        //    }

        //    var hitCharacter = hit.collider.GetComponent<Character>();

        //    if (hitCharacter == null || !OwnsCharacter(hitCharacter))
        //    {
        //        Characters.ForEach(c => c.Deselect());
        //        return;
        //    }

        //    Characters.ForEach(c => c.Deselect());
        //    hitCharacter.Select();
        //}
        #endregion
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
    /// Counts how many characters are selected
    /// </summary>
    /// <author>Jonathan</author>
    private void CountSelections()
    {
        // Initialize counter
        int counter = 0;

        // Count selected characters
        for (int i = 0; i < Characters.Count; i++)
        {
            if (Characters[i].GetComponent<Character>().IsSelected)
            {
                counter++;
            }
        }

        // Set the final answer
        Selected_Characters = counter;
    }

    /// <summary>
    /// Ends the game after a player wins
    /// </summary>
    /// <param name="winningPlayer">The winning player kind</param>
    [Command]
    public void CmdEndGame(PlayerKind winningPlayer)
    {
        MainManager.EndGame(winningPlayer);
    }

    //[Command]
    //void CmdAnimate()
    //{

    //    // Plays the character animations.
    //    if ((_navMeshAgent.velocity.magnitude > 0f))
    //    {
    //        //print("Moving");
    //        transform.GetChild(4).gameObject.GetComponent<NetworkAnimator>().animator.SetBool("Move", true);
    //    }
    //    else
    //    {
    //        transform.GetChild(4).gameObject.GetComponent<Animator>().SetBool("Move", false);
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="winningPlayer"></param>
    public void EndGame(PlayerKind winningPlayer)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        DisableControls = true;

        // Determine Win or Lose sound effects and icons
        if (Kind == winningPlayer)
        {
            audioManager.playWin();
            _inGamePanel.EndGameImage.sprite = winImage;
        }
        else
        {
            audioManager.playLose();
            _inGamePanel.EndGameImage.sprite = loseImage;
        }

        // Enable view
        _inGamePanel.EndGameGroup.SetActive(true);
    }
}