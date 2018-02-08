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
    public GameObject TargetIndicatorPrefab;
    public float MaxVelocity;
    public float MaxAcceleration;

    public GameObject Target { get; set; }

    private NetworkStartPosition[] _spawnPoints;
    private Vector3 _targetPosition;
    private Vector3 _velocity;
    private Vector3 _acceleration;

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

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if(Target != null)
                {
                    Destroy(Target.gameObject);
                }

                Target = Instantiate(TargetIndicatorPrefab, hit.point, Quaternion.identity);
            }
        }

        if (Target != null)
        {
            _targetPosition = Target.transform.position;
            _targetPosition.y = transform.position.y;

            _acceleration = Vector3.Normalize(_targetPosition - transform.position) * MaxAcceleration;

            var potentialVelocity = _velocity + (Time.deltaTime * _acceleration);
            _velocity = potentialVelocity.magnitude <= MaxVelocity ? potentialVelocity : MaxVelocity * Vector3.Normalize(potentialVelocity);

            transform.position = transform.position + _velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(_velocity, Vector3.up);
        }
    }

    void OnTriggerEnter(Collider otherCollider)
    {
        if(otherCollider.gameObject == Target.gameObject)
        {
            Destroy(Target.gameObject);
            Target = null;
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
