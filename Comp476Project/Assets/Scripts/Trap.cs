using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Trap : NetworkBehaviour
{
    // Defined in Inspector
    public GameObject StunParticles;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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