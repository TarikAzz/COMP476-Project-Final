using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    public Character[] Characters;

    public override void OnStartLocalPlayer()
    {
        for (var i = 0; i < Characters.Length; i++)
        {
            Characters[i].Owner = this;
            Characters[i].Colorize(Color.blue);
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                return;
            }

            var hitCharacter = hit.collider.GetComponent<Character>();

            if (hitCharacter == null || !OwnsCharacter(hitCharacter))
            {
                return;
            }

            if (hitCharacter.IsSelected)
            {
                hitCharacter.Deselect();
            }
            else
            {
                hitCharacter.Select();
            }
        }
    }

    public bool OwnsCharacter(Character character)
    {
        for (var i = 0; i < Characters.Length; i++)
        {
            if (Characters[i] != character)
            {
                return true;
            }
        }

        return false;
    }
}
