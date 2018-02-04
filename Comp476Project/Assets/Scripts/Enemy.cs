using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Enemy : Character
{
    public override void Die()
    {
        Destroy(gameObject);
    }
}
