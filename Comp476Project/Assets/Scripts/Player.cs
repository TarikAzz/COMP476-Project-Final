using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : Character
{
    [Header("Bullet")]
    public GameObject BulletPrefab;
    public Transform BulletSpawn;
    public float BulletVelocity;
    public float BulletLifeTime;
    public float BulletDamage;

    [Header("Movement")]
    public float MovementSpeed;
    public float TurnSpeed;

    private NetworkStartPosition[] _spawnPoints;

    void Start()
    {
        if (isLocalPlayer)
        {
            _spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * MovementSpeed;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * TurnSpeed;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdFire();
        }
    }

    public override void OnStartLocalPlayer()
    {
        Colorize(Color.blue);
    }

    public override void Die()
    {
        RpcRespawn();
    }

    [Command]
    private void CmdFire()
    {
        var bullet = Instantiate(BulletPrefab, BulletSpawn.position, BulletSpawn.rotation).GetComponent<Bullet>();
        bullet.Initialize(BulletDamage, BulletVelocity);

        NetworkServer.Spawn(bullet.gameObject);

        Destroy(bullet.gameObject, BulletLifeTime);
    }

    [ClientRpc]
    private void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            var spawnPoint = Vector3.zero;
            
            if (_spawnPoints != null && _spawnPoints.Length > 0)
            {
                spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)].transform.position;
            }
            
            transform.position = spawnPoint;
        }
    }
}
