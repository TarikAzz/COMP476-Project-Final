using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Trap : NetworkBehaviour
{
    // Defined in Inspector
    public GameObject StunParticles;

    // Keep track of all Infiltrators for Defender traps
    public GameObject[] Infiltrators;


    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Only run this if it's a Defender Trap
        if (tag == "Trap")
        {
            Infiltrators = GameObject.FindGameObjectsWithTag("Bad");

            // Scan all infiltrators to see if any are within range to be spotted
            for (int i = 0; i < Infiltrators.Length; i++)
            {
                if (Vector3.Distance(transform.position, Infiltrators[i].transform.position) <= 8f)
                {
                    Infiltrators[i].GetComponent<Character>().IsSpotted = true;
                }
            }
        }
    }


    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Good" && tag == "iTrap")
        {
            col.gameObject.GetComponent<Character>().IsStunned = true;
            col.gameObject.GetComponent<Character>()._navMeshAgent.SetDestination(transform.position);

            GameObject effect = Instantiate(StunParticles, col.gameObject.transform.position, Quaternion.identity);
            NetworkServer.Spawn(effect);
        }
    }
}