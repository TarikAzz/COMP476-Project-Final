using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Audio Sources added in Start method (it's messy via Inspector)
    public AudioSource source_Sniper;
    public AudioSource source_Unit;
    public AudioSource source_Trap;
    public AudioSource source_Lightning;
    public AudioSource source_Alarm;
    public AudioSource source_Win;
    public AudioSource source_Lose;

    // Defined in the Inspector
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
        // Each gets its own audio source so that sounds can overlap each other
        source_Sniper = gameObject.AddComponent<AudioSource>();
        source_Unit = gameObject.AddComponent<AudioSource>();
        source_Trap = gameObject.AddComponent<AudioSource>();
        source_Lightning = gameObject.AddComponent<AudioSource>();

        // Lowered its volume because the alarm itself is loud
        source_Alarm = gameObject.AddComponent<AudioSource>();
        source_Alarm.volume = 0.25f;

        source_Win = gameObject.AddComponent<AudioSource>();
        source_Lose = gameObject.AddComponent<AudioSource>();

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

    // Play the Alarm sound effect
    public void playAlarm()
    {
        source_Alarm.clip = alarm;
        source_Alarm.Play();
    }

    // Play the Win sound effect
    public void playWin()
    {
        source_Win.clip = win;
        source_Win.Play();
    }

    // Play the Lose sound effect
    public void playLose()
    {
        source_Lose.clip = lose;
        source_Lose.Play();
    }





    



}