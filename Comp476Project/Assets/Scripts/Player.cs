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
    public Renderer Level;
    public float MinVelocity;
    public float MaxVelocity;
    public float MaxAcceleration;

    public GameObject Target { get; set; }

    private NetworkStartPosition[] _spawnPoints;
    private Vector3 _targetPosition;
    private Vector3 _velocity;
    private Vector3 _acceleration;
    private float _topBounds;
    private float _rightBounds;
    private float _bottomBounds;
    private float _leftBounds;

    void Start()
    {
        if (isLocalPlayer)
        {
            _spawnPoints = FindObjectsOfType<NetworkStartPosition>();

            var moveAreaX = Level.bounds.size.x / 2;
            var moveAreaZ = Level.bounds.size.z / 2;
            var center = Level.bounds.center;

            _topBounds = center.z + moveAreaZ;
            _rightBounds = center.x + moveAreaX;
            _bottomBounds = center.z - moveAreaZ;
            _leftBounds = center.x - moveAreaX;
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

            PursueTarget();
        }
        else if(_velocity.magnitude > MinVelocity)
        {
            PursueTarget();
        }

        RestrictPosition();
    }
    
    void OnTriggerEnter(Collider otherCollider)
    {
        if(otherCollider.gameObject == Target.gameObject)
        {
            Destroy(Target.gameObject);
            Target = null;

            _targetPosition = transform.position - transform.forward * 5f;
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

    private void PursueTarget()
    {
        _acceleration = Vector3.Normalize(_targetPosition - transform.position) * MaxAcceleration;

        var potentialVelocity = _velocity + (Time.deltaTime * _acceleration);
        _velocity = potentialVelocity.magnitude <= MaxVelocity ? potentialVelocity : MaxVelocity * Vector3.Normalize(potentialVelocity);

        transform.position = transform.position + _velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(_velocity, Vector3.up);
    }

    private void RestrictPosition()
    {
        if (transform.position.z > _topBounds)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, _topBounds);
        }
        if (transform.position.x > _rightBounds)
        {
            transform.position = new Vector3(_rightBounds, transform.position.y, transform.position.z);
        }
        if (transform.position.z < _bottomBounds)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, _bottomBounds);
        }
        if (transform.position.x < _leftBounds)
        {
            transform.position = new Vector3(_leftBounds, transform.position.y, transform.position.z);
        }
    }
}
