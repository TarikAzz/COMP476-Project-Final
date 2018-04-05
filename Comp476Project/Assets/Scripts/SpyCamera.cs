using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpyCamera : NetworkBehaviour
{
	void Update ()
	{
	    CmdRotate();
	}

    [Command]
    public void CmdRotate()
    {
        transform.Rotate(0f, 0.25f, 0f);
    }
}