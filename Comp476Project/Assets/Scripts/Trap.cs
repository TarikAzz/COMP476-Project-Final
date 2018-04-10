using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Trap : NetworkBehaviour
{
    // Defined in Inspector
    public GameObject StunParticles;
    public GameObject TrapBase;
    public GameObject TrapHead;

    // Get the panel for traps to determine player kind
    public InGamePanel panel;

    // Reference to the Audio Manager (CANNOT GET IT THROUGH INSPECTOR)
    public AudioManager audioManager;

    // So that the sound for the stun effect only plays once (can trigger multiple times on collision)
    public bool lockStunSound;

    // Use this for initialization
    void Start()
    {
        panel = GameObject.Find("InGamePanel").GetComponent<InGamePanel>();

        // Get audio functionalities (CANNOT GET IT THROUGH INSPECTOR)
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        lockStunSound = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Show traps if you are the Defender (they're invisible by default)
        if (panel.PlayerKindText.text == "Defender")
        {
            TrapBase.GetComponent<MeshRenderer>().enabled = true;
            TrapHead.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    // When an infiltrator touches a trap
    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Bad")
        {
            if(lockStunSound == false)
            {
                // Play trap stun sound effect
                audioManager.playTrapStun();
                lockStunSound = true;
            }

            // Reveal the trap
            TrapBase.GetComponent<MeshRenderer>().enabled = true;
            TrapHead.GetComponent<MeshRenderer>().enabled = true;

            // Stun, reveal, and lock infiltrator in place throughout the stun duration
            col.gameObject.GetComponent<Character>().IsStunned = true;
            col.gameObject.GetComponent<Character>().IsSpotted = true;
            col.gameObject.GetComponent<Character>()._navMeshAgent.SetDestination(transform.position);

            // Show stun effect (via a command to resolve NetworkServer spawn error)
            CmdCallParticles(col.gameObject);

            // Trap gets destroyed after 2 seconds
            Destroy(gameObject, 2f);
        }
    }

    // Spawn stun particles to the server
    [Command]
    void CmdCallParticles(GameObject obj)
    {
        NetworkServer.Spawn(Instantiate(StunParticles, obj.gameObject.transform.position, Quaternion.identity));
    }
}