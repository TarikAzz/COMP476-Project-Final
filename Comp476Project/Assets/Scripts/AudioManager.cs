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
    public AudioClip alarm;
    public AudioClip win;
    public AudioClip lose;
    
    // These are used to play the alarm sound effect when any infiltrator is spotted
    public GameObject[] infiltrators;
    public float alarmFrequency;
    public bool waitForAlarm;
    public double lastTimeAlarmPlayed;

    // Use this for initialization
    void Start()
    {
        alarmFrequency = 0.5f;
        waitForAlarm = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Always get updated list of active infiltrators
        infiltrators = GameObject.FindGameObjectsWithTag("Bad");

        // Only check for spotted infiltrator when it can play an alarm
        if (waitForAlarm == false)
        {
            for (int i = 0; i < infiltrators.Length; i++)
            {
                if (infiltrators[i].GetComponent<Character>().IsSpotted)
                {
                    waitForAlarm = true;
                    playAlarm();
                }
            }

            // Always update time since last played alarm
            lastTimeAlarmPlayed = Network.time;
        }
        else
        {
            // Halt alarm via the allowed frequency
            if (Network.time > lastTimeAlarmPlayed + alarmFrequency)
            {
                waitForAlarm = false;
            }
        }
    }





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

    // Play the Alarm sound effect
    public void playAlarm()
    {
        source.clip = alarm;
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