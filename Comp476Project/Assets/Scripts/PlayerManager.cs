using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    public enum PlayerKind
    {
        Infiltrator,
        Defender
    }

    public Character[] Characters;

    private int _id;
    private PlayerKind _kind;
    private InGamePanel _inGamePanel;

    public override void OnStartLocalPlayer()
    {
        for (var i = 0; i < Characters.Length; i++)
        {
            _id = NetworkManager.singleton.numPlayers;
            Characters[i].OwnerId = _id;
            Characters[i].Owner = this;
            Characters[i].Colorize(Color.blue);
        }

        _kind = NetworkManager.singleton.numPlayers == 1 ? PlayerKind.Defender : PlayerKind.Infiltrator;

        if (_kind == PlayerKind.Infiltrator)
        {
            /*var newCameraPosition = Camera.main.transform.position;
            newCameraPosition.z = -newCameraPosition.z;

            var newCameraRotation = Quaternion.Euler(55f, 180f, 0f);

            Camera.main.transform.position = newCameraPosition;
            Camera.main.transform.rotation = newCameraRotation;*/
        }

        _inGamePanel = FindObjectOfType<InGamePanel>();
        _inGamePanel.PlayerKind.text = _kind.ToString();
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
        return character.Owner != null && character.Owner.isLocalPlayer;
    }
}
