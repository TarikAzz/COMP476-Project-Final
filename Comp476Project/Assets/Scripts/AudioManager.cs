using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Defined in the Inspector
    public AudioSource source;
    public AudioClip sniper_fire;
    public AudioClip unit_placement;
    public AudioClip trap_stun;
    public AudioClip lightning;

    public AudioClip win;
    public AudioClip lose;












    // Play the Sniper Fire sound effect
    public void playSniperFire()
    {
        source.clip = sniper_fire;
        source.Play();
    }

    // Play the Unit Placement sound effect
    public void playUnitPlacement()
    {
        source.clip = unit_placement;
        source.Play();
    }

    // Play the Trap Stun sound effect
    public void playTrapStun()
    {
        source.clip = trap_stun;
        source.Play();
    }

    // Play the Lightning sound effect
    public void playLightning()
    {
        source.clip = lightning;
        source.Play();
    }

    // Play the Win sound effect
    public void playWin()
    {
        source.clip = win;
        source.Play();
    }

    // Play the Lose sound effect
    public void playLose()
    {
        source.clip = lose;
        source.Play();
    }





}