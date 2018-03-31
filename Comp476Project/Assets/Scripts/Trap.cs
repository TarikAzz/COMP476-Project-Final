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

    // Use this for initialization
    void Start()
    {
        panel = GameObject.Find("InGamePanel").GetComponent<InGamePanel>();
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
            // Reveal the trap
            TrapBase.GetComponent<MeshRenderer>().enabled = true;
            TrapHead.GetComponent<MeshRenderer>().enabled = true;

            // Stun, reveal, and lock infiltrator in place throughout the stun duration
            col.gameObject.GetComponent<Character>().IsStunned = true;
            col.gameObject.GetComponent<Character>().IsSpotted = true;
            col.gameObject.GetComponent<Character>()._navMeshAgent.SetDestination(transform.position);

            // Show stun effect
            NetworkServer.Spawn(Instantiate(StunParticles, col.gameObject.transform.position, Quaternion.identity));

            // Trap gets destroyed after 2 seconds
            Destroy(gameObject, 2f);
        }
    }
}