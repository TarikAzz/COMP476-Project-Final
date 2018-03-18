using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
    public float PatrolCooldown;

    public PlayerManager Owner { get; set; }
    public int OwnerId { get; set; }
    public bool IsSelected { get; set; }
    public bool JustSelected { get; set; }

    private GameObject _target;
    private GameObject _patrolOrigin;
    private GameObject _patrolTarget;
    private NavMeshAgent _navMeshAgent;

    // JONATHAN'S PART
    public bool spotted;
    public GameObject[] lamps;
    
    void Start()
    {
        // JONATHAN'S PART
        lamps = GameObject.FindGameObjectsWithTag("Lamp");

        _navMeshAgent = GetComponent<NavMeshAgent>();
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

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                var walkable = hit.collider.GetComponent<Walkable>();

                if (walkable != null && walkable.IsWall(hit.normal))
                {
                    StopAllCoroutines();
                    DestroyTargetIndicators();

                    _target = Instantiate(TargetIndicatorPrefab, hit.point, Quaternion.identity);
                    
                    _navMeshAgent.SetDestination(hit.point);
                }
            }
        }

        if (IsSelected && Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.collider.GetComponent<Walkable>() != null)
                {
                    StopAllCoroutines();
                    DestroyTargetIndicators();

                    var targetOriginPosition = transform.position;
                    targetOriginPosition.y = hit.point.y;

                    _patrolTarget = Instantiate(PatrolIndicatorPrefab, hit.point, Quaternion.identity);
                    _patrolOrigin = Instantiate(PatrolIndicatorPrefab, targetOriginPosition, Quaternion.identity);

                    _navMeshAgent.SetDestination(hit.point);
                }
            }
        }

        if (Vector3.Distance(transform.position, _navMeshAgent.destination) <= _navMeshAgent.stoppingDistance)
        {
            if (_patrolTarget != null && _patrolOrigin != null)
            {
                if (_target == _patrolTarget)
                {
                    StartCoroutine(SetPatrolTargetWithDelay(_patrolOrigin.transform.position));
                }
                else if (_target == _patrolOrigin)
                {
                    StartCoroutine(SetPatrolTargetWithDelay(_patrolTarget.transform.position));
                }

                return;
            }

            DestroyTargetIndicators();
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

    private IEnumerator SetPatrolTargetWithDelay(Vector3 target)
    {
        yield return new WaitForSeconds(PatrolCooldown);

        _navMeshAgent.SetDestination(target);
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
