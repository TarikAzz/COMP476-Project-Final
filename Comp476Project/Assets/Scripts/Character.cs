using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject BulletPrefab;
    public Transform BulletSpawn;
    public float BulletVelocity;
    public float BulletLifeTime;
    public float BulletDamage;

    [Header("Health")]
    public Image HealthBar;
    public float MaxHealth;

    [Header("Movement")]
    public GameObject TargetIndicatorPrefab;
    public GameObject PatrolIndicatorPrefab;
    public Renderer Level;
    public float PatrolCooldown;
    public float MinVelocity;
    public float MaxVelocity;
    public float MaxAcceleration;

    public PlayerManager Owner { get; set; }
    public int OwnerId { get; set; }
    public bool IsSelected { get; set; }
    public bool JustSelected { get; set; }

    private GameObject _target;
    private GameObject _patrolOrigin;
    private GameObject _patrolTarget;
    private Vector3 _targetPosition;
    private Vector3 _velocity;
    private Vector3 _acceleration;
    private float _topBounds;
    private float _rightBounds;
    private float _bottomBounds;
    private float _leftBounds;







    // JONATHAN'S PART
    public bool spotted;
    public GameObject[] lamps;








    void Start()
    {
        var moveAreaX = Level.bounds.size.x / 2;
        var moveAreaZ = Level.bounds.size.z / 2;
        var center = Level.bounds.center;

        _topBounds = center.z + moveAreaZ;
        _rightBounds = center.x + moveAreaX;
        _bottomBounds = center.z - moveAreaZ;
        _leftBounds = center.x - moveAreaX;



        // JONATHAN'S PART
        lamps = GameObject.FindGameObjectsWithTag("Lamp");



    }

    void Update()
    {





        // JONATHAN'S PART
        for (int i = 0; i < lamps.Length; i++)
        {
            if (Vector3.Distance(transform.position, lamps[i].transform.position) <= lamps[i].GetComponent<Lamp>().range)
            {
                spotted = true;
            }
            else
            {
                spotted = false;
            }
        }






        if (JustSelected)
        {
            JustSelected = false;
            return;
        }

        if (IsSelected && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                StopAllCoroutines();
                DestroyTargetIndicators();

                _target = Instantiate(TargetIndicatorPrefab, hit.point, Quaternion.identity);
            }
        }

        if (IsSelected && Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                StopAllCoroutines();
                DestroyTargetIndicators();

                var targetOriginPosition = transform.position;
                targetOriginPosition.y = hit.point.y;

                _patrolTarget = Instantiate(PatrolIndicatorPrefab, hit.point, Quaternion.identity);
                _patrolOrigin = Instantiate(PatrolIndicatorPrefab, targetOriginPosition, Quaternion.identity);

                _target = _patrolTarget;
            }
        }

        if (_target != null)
        {
            _targetPosition = _target.transform.position;
            _targetPosition.y = transform.position.y;

            PursueTarget();
        }
        else if (_velocity.magnitude > MinVelocity)
        {
            PursueTarget();
        }

        RestrictPosition();
    }

    void OnTriggerEnter(Collider otherCollider)
    {
        if (_target != null && otherCollider.gameObject == _target.gameObject)
        {
            if (_patrolTarget != null && _patrolOrigin != null)
            {
                if (_target == _patrolTarget)
                {
                    StartCoroutine(SetPatrolTargetWithDelay(_patrolOrigin));
                }
                else if (_target == _patrolOrigin)
                {
                    StartCoroutine(SetPatrolTargetWithDelay(_patrolTarget));
                }

                return;
            }

            Destroy(_target.gameObject);
            _target = null;

            _targetPosition = transform.position - transform.forward * 5f;
        }
    }

    public void Colorize(Color color)
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();

        for (var i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = color;
        }
    }

    public void Select()
    {
        if (Owner != null)
        {
            foreach (var character in Owner.Characters)
            {
                if (character == this)
                {
                    continue;
                }

                character.Deselect();
            }
        }

        IsSelected = true;
        JustSelected = true;

        Colorize(Color.cyan);
    }

    public void Deselect()
    {
        IsSelected = false;
        Colorize(Color.blue);
    }

    private IEnumerator SetPatrolTargetWithDelay(GameObject target)
    {
        _target = null;

        yield return new WaitForSeconds(PatrolCooldown);

        _target = target;
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

    private void DestroyTargetIndicators()
    {
        GameObject[] targetIndicators = { _target, _patrolOrigin, _patrolTarget };

        for (var i = 0; i < targetIndicators.Length; i++)
        {
            if (targetIndicators[i] != null)
            {
                Destroy(targetIndicators[i].gameObject);
            }
        }
    }
}
