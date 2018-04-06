using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

public class Lightning : NetworkBehaviour
{
    /// <summary>
    /// Determines if the game has started.
    /// </summary>
    public bool hasGameStarted = false;

    /// <summary>
    /// Determines if the infiltrators should be shown.
    /// </summary>
    public bool showInfiltrators = false;

    /// <summary>
    /// Calculate the random time.
    /// </summary>
    bool calculateRandomTime = true;

    /// <summary>
    /// Determines if the flash should show.
    /// </summary>
    bool isFlashed;

    /// <summary>
    /// Determines if the lightning is calming.
    /// </summary>
    bool isCalming;

    /// <summary>
    /// Locks the flash.
    /// </summary>
    bool locker = true;

    /// <summary>
    /// Not used.
    /// </summary>
    Stopwatch sw = new Stopwatch();

    /// <summary>
    /// The light.
    /// </summary>
    Light light;

    /// <summary>
    /// The random time.
    /// </summary>
    [SyncVar]
    int randomTime;

    /// <summary>
    /// The last time checked.
    /// </summary>
    [SyncVar]
    double lastCheckedTime;

    /// <summary>
    /// The other last time checked.
    /// </summary>
    [SyncVar]
    double otherTime;

    /// <summary>
    /// Reference to the Audio Manager (CANNOT GET IT THROUGH INSPECTOR)
    /// </summary>
    public AudioManager audioManager;

    /// <summary>
    /// Unity's Start.
    /// </summary>
    void Start()
    {
        light = GetComponent<Light>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    /// <summary>
    /// Unity's Update method.
    /// </summary>
    void Update()
    {
        if (hasGameStarted)
        {
            if (calculateRandomTime)
            {
                randomTime = Random.Range(30, 70);

                calculateRandomTime = false;

                lastCheckedTime = Network.time;

                print("Lighting time is: " + randomTime);
            }
            else
            {
                CmdDoLightning(randomTime);
            }
        }
    }

    /// <summary>
    /// Performs the lightning flash.
    /// </summary>
    /// <param name="time">The time between flashes.</param>
    //[Command]
    void CmdDoLightning(int time)
    {
        sw.Start();

        if ((Network.time > time + lastCheckedTime) && locker)
        {
            isFlashed = true;

            // Play lightning sound effect
            audioManager.playLightning();

            locker = false;
            sw.Stop();
            sw.Reset();
        }

        if (isFlashed)
        {

            light.intensity = 1.5f;
            isFlashed = false;
            isCalming = true;

            showInfiltrators = true;

            otherTime = Network.time;
        }
        
        if (isCalming && Network.time > otherTime + 1.5)
        {
            light.intensity -= 0.1f;

            if (light.intensity <= 0)
            {
                showInfiltrators = false;
                light.intensity = 0.0f;

                isCalming = false;
                locker = true;

                sw.Stop();
                sw.Reset();

                calculateRandomTime = true;
            }
        }
    }
}
