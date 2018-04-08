using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

public class Animate : NetworkBehaviour
{
    /// <summary>
    /// Unity's update method.
    /// </summary>
    void Update()
    {
        AnimateMovements();
    }

    /// <summary>
    /// Animates the characters.
    /// </summary>
    void AnimateMovements()
    {
        if ((transform.parent.GetComponent<Character>().IsMoving))
        {
            gameObject.GetComponent<Animator>().SetBool("Move", true);
        }
        else 
        {
            gameObject.GetComponent<Animator>().SetBool("Move", false);
        }
    }
}
