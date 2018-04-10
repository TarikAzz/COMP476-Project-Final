using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpyCamera : NetworkBehaviour
{
	void Update ()
	{
	    if (!isServer)
	    {
	        return;
	    }

	    RpcRotate();
	}

    [ClientRpc]
    public void RpcRotate()
    {
        transform.Rotate(0f, 0.25f, 0f);
    }
}