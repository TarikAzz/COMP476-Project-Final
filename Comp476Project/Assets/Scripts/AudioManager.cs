using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    // Audio Sources added in Start method (it's messy via Inspector)
    public AudioSource source_MainTheme;
    public AudioSource source_Setup;
    public AudioSource source_GameStart;
    public AudioSource source_Sniper;
    public AudioSource source_Unit;
    public AudioSource source_Trap;
    public AudioSource source_Lightning;
    public AudioSource source_Win;
    public AudioSource source_Lose;
    public AudioSource source_CharacterSelection;
    public AudioSource source_CharacterOk;

    // Defined in the Inspector
    public AudioClip main_theme;
    public AudioClip setup;
    public AudioClip game_start;
    public AudioClip sniper_fire;
    public AudioClip unit_placement;
    public AudioClip trap_stun;
    public AudioClip lightning;
    public AudioClip win;
    public AudioClip lose;

    /// <summary>
    /// The infiltrator select sounds
    /// </summary>
    public AudioClip infiltratorSelect1;
    public AudioClip infiltratorSelect2;
    public AudioClip infiltratorSelect3;
    public AudioClip infiltratorSelect4;

    /// <summary>
    /// The defender select sounds
    /// </summary>
    public AudioClip defenderSelect1;
    public AudioClip defenderSelect2;
    public AudioClip defenderSelect3;
    public AudioClip defenderSelect4;

    /// <summary>
    /// The infiltrator ok sounds
    /// </summary>
    public AudioClip infiltratorOk1;
    public AudioClip infiltratorOk2;
    public AudioClip infiltratorOk3;
    public AudioClip infiltratorOk4;

    /// <summary>
    /// The defender ok sounds
    /// </summary>
    public AudioClip defenderOk1;
    public AudioClip defenderOk2;
    public AudioClip defenderOk3;
    public AudioClip defenderOk4;

    List<AudioClip> infiltratorSelectList;
    List<AudioClip> defenderSelectList = new List<AudioClip>();
    List<AudioClip> infiltratorOkList = new List<AudioClip>();
    List<AudioClip> defenderOkList = new List<AudioClip>();

    private float barkCooldown = 4f;
    private bool canBark;

    // Use this for initialization
    void Start()
    {
        // Each gets its own audio source so that sounds can overlap each other
        source_MainTheme = gameObject.AddComponent<AudioSource>();
        source_MainTheme.volume = 0.6f;
        source_MainTheme.loop = true;

        source_Setup = gameObject.AddComponent<AudioSource>();
        source_Setup.loop = true;

        source_GameStart = gameObject.AddComponent<AudioSource>();
        source_Sniper = gameObject.AddComponent<AudioSource>();
        source_Unit = gameObject.AddComponent<AudioSource>();
        source_Trap = gameObject.AddComponent<AudioSource>();
        source_Lightning = gameObject.AddComponent<AudioSource>();
        source_Win = gameObject.AddComponent<AudioSource>();
        source_Lose = gameObject.AddComponent<AudioSource>();

        source_CharacterSelection = gameObject.AddComponent<AudioSource>();
        source_CharacterOk = gameObject.AddComponent<AudioSource>();

        // Start playing the main theme
        playMainTheme();

        infiltratorSelectList = new List<AudioClip> { infiltratorSelect1, infiltratorSelect2, infiltratorSelect3, infiltratorSelect4 };
        defenderSelectList = new List<AudioClip> { defenderSelect1, defenderSelect2, defenderSelect3, defenderSelect4 };

        infiltratorOkList = new List<AudioClip> { infiltratorOk1, infiltratorOk2, infiltratorOk3, infiltratorOk4 };
        defenderOkList = new List<AudioClip> { defenderOk1, defenderOk2, defenderOk3, defenderOk4 };

        source_CharacterOk.playOnAwake = false;
        source_CharacterOk.loop = false;

        source_CharacterSelection.playOnAwake = false;
        source_CharacterSelection.loop = false;

        canBark = true;

    }

    // Play the Main Theme music
    public void playMainTheme()
    {
        // Play main theme once loaded
        if (source_MainTheme != null)
        {
            source_MainTheme.clip = main_theme;
            source_MainTheme.Play();
        }

    }

    // Play the Setup sound effect
    public void playSetup()
    {
        source_Setup.clip = setup;
        source_Setup.Play();
    }

    // Stop the Setup sound effect
    public void stopSetup()
    {
        source_Setup.Stop();
    }

    // Play the Game Start sound effect
    public void playGameStart()
    {
        source_GameStart.clip = game_start;
        source_GameStart.Play();
    }

    // Play the Sniper Fire sound effect
    public void playSniperFire()
    {
        source_Sniper.clip = sniper_fire;
        source_Sniper.Play();
    }

    // Play the Unit Placement sound effect
    public void playUnitPlacement()
    {
        source_Unit.clip = unit_placement;
        source_Unit.Play();
    }

    // Play the Trap Stun sound effect
    public void playTrapStun()
    {
        source_Trap.clip = trap_stun;
        source_Trap.Play();
    }

    // Play the Lightning sound effect
    public void playLightning()
    {
        source_Lightning.clip = lightning;
        source_Lightning.Play();
    }

    // Play the Win sound effect
    public void playWin()
    {
        // Only for Win and Lose
        StopEverything();

        source_Win.clip = win;
        source_Win.Play();
    }

    // Play the Lose sound effect
    public void playLose()
    {
        // Only for Win and Lose
        StopEverything();

        source_Lose.clip = lose;
        source_Lose.Play();
    }

    public void playCharacterSelection(bool isInfiltrator)
    {
        int randomIndex = Random.Range(0, infiltratorSelectList.Count);

        if (isInfiltrator)
        {
            source_CharacterSelection.PlayOneShot(infiltratorSelectList[randomIndex]);
        }
        else
        {
            //source_CharacterSelection.clip = defenderSelectList[randomIndex];
            source_CharacterSelection.PlayOneShot(defenderSelectList[randomIndex]);
        }
        canBark = true;

        //source_CharacterSelection.Play();
    }

    public void playCharacterOk(bool isInfiltrator)
    {
        //int randomIndex = Random.Range(0, infiltratorOkList.Count);
        if (canBark)
        {
            if (isInfiltrator)
            {
                source_CharacterSelection.PlayOneShot(infiltratorOkList[0]);
            }
            else
            {
                source_CharacterSelection.PlayOneShot(defenderOkList[0]);
            }
            canBark = false;
            Invoke("EnableBark",barkCooldown);
        }
       

       // source_CharacterOk.Play();
    }

    // Stop all audio (used for when game is over)
    public void StopEverything()
    {
        source_MainTheme.Stop();
        source_Sniper.Stop();
        source_Unit.Stop();
        source_Trap.Stop();
        source_Lightning.Stop();
    }

    private void EnableBark()
    {
        canBark = true;
    }
}