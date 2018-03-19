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
    public float PatrolCooldown;
    public bool LoopPatrol;

    public PlayerManager Owner { get; set; }
    public int OwnerId { get; set; }
    public bool IsSelected { get; set; }
    public bool JustSelected { get; set; }

    private Indicator _target;
    private int _patrolStep;
    private bool _reversePatrol;
    private List<Indicator> _patrolIndicators;
    private NavMeshAgent _navMeshAgent;

    // JONATHAN'S PART
    public bool spotted;
    public GameObject[] lamps;
    
    void Start()
    {
        // JONATHAN'S PART
        lamps = GameObject.FindGameObjectsWithTag("Lamp");

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _patrolIndicators = new List<Indicator>();
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

        if (IsSelected && Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            RaycastHit originHit;
            RaycastHit targetHit;

            var originHitSuccess = Physics.Raycast(transform.position, Vector3.down, out originHit);
            var targetHitSuccess = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out targetHit);

            if (originHitSuccess && targetHitSuccess)
            {
                if (originHit.collider.GetComponent<Walkable>() != null && targetHit.collider.GetComponent<Walkable>())
                {
                    CreatePatrolIndicators(originHit, targetHit);
                }
            }
        }
        else if (IsSelected && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                var walkable = hit.collider.GetComponent<Walkable>();

                if (walkable != null && walkable.IsWall(hit.normal))
                {
                    CreateTargetIndicator(hit);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoopPatrol = !LoopPatrol;
        }
        
        if (_target != null && Vector3.Distance(transform.position, _target.transform.position) <= _navMeshAgent.stoppingDistance)
        {
            if (_patrolIndicators.Count > 0)
            {
                if (!LoopPatrol)
                {
                    if (_patrolStep == _patrolIndicators.Count - 1)
                    {
                        _reversePatrol = true;
                    }
                    else if (_patrolStep == 0 && !LoopPatrol)
                    {
                        _reversePatrol = false;
                    }
                }
                
                SetTarget(null);

                StartCoroutine(SetPatrolTargetWithDelay(_patrolIndicators[_patrolStep]));

                if (_reversePatrol)
                {
                    if (_patrolStep == 0)
                    {
                        _patrolStep = _patrolIndicators.Count - 1;
                    }
                    else
                    {
                        _patrolStep--;
                    }
                }
                else
                {
                    if (_patrolStep == _patrolIndicators.Count - 1)
                    {
                        _patrolStep = 0;
                    }
                    else
                    {
                        _patrolStep++;
                    }
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

        ShowTargetIndicators();
    }

    public void Deselect()
    {
        IsSelected = false;
        Colorize(Color.blue);
        HideTargetIndicators();
    }

    private void SetTarget(Indicator target)
    {
        if (_target != null)
        {
            _target.GetComponent<Indicator>().IsTarget = false;
        }

        if (target != null)
        {
            target.GetComponent<Indicator>().IsTarget = true;
        }
        
        _target = target;
    }

    private IEnumerator SetPatrolTargetWithDelay(Indicator target)
    {
        SetTarget(target);

        yield return new WaitForSeconds(PatrolCooldown);

        _navMeshAgent.SetDestination(target.transform.position);
    }

    private void CreateTargetIndicator(RaycastHit hit)
    {
        StopAllCoroutines();
        DestroyTargetIndicators();

        _target = Instantiate(TargetIndicatorPrefab, hit.point + Vector3.up * 0.01f, Quaternion.identity).GetComponent<Indicator>();
        _target.transform.up = hit.normal;
        _target.transform.Rotate(new Vector3(90f, 0f, 0f));
        SetTarget(_target);

        _navMeshAgent.SetDestination(hit.point);
    }

    private void CreatePatrolIndicators(RaycastHit originHit, RaycastHit targetHit)
    {
        var newPatrolNode = (Indicator) null;

        if (_patrolIndicators.Count == 0)
        {
            StopAllCoroutines();
            DestroyTargetIndicators();

            newPatrolNode = Instantiate(TargetIndicatorPrefab, originHit.point + Vector3.up * 0.01f, Quaternion.identity).GetComponent<Indicator>();
            newPatrolNode.transform.up = originHit.normal;
            newPatrolNode.transform.Rotate(new Vector3(90f, 0f, 0f));

            _patrolIndicators.Add(newPatrolNode);

            _navMeshAgent.SetDestination(targetHit.point);

            _patrolStep = 1;
        }

        newPatrolNode = Instantiate(TargetIndicatorPrefab, targetHit.point + Vector3.up * 0.01f, Quaternion.identity).GetComponent<Indicator>();
        newPatrolNode.transform.up = targetHit.normal;
        newPatrolNode.transform.Rotate(new Vector3(90f, 0f, 0f));
        _patrolIndicators.Add(newPatrolNode);

        if (_patrolIndicators.Count == 2)
        {
            SetTarget(newPatrolNode);
        }
    }
    
    private void ShowTargetIndicators()
    {
        if (_patrolIndicators.Count != 0)
        {
            for (var i = 0; i < _patrolIndicators.Count; i++)
            {
                if (_patrolIndicators[i] != null)
                {
                    _patrolIndicators[i].GetComponent<SpriteRenderer>().enabled = true;
                }
            }
        }
        
        if (_target != null)
        {
            _target.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void HideTargetIndicators()
    {
        if (_patrolIndicators.Count != 0)
        {
            for (var i = 0; i < _patrolIndicators.Count; i++)
            {
                if (_patrolIndicators[i] != null)
                {
                    _patrolIndicators[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }

        if (_target != null)
        {
            _target.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    private void DestroyTargetIndicators()
    {
        if (_patrolIndicators.Count != 0)
        {
            for (var i = 0; i < _patrolIndicators.Count; i++)
            {
                if (_patrolIndicators[i] != null)
                {
                    Destroy(_patrolIndicators[i].gameObject);
                }
            }
        }

        if (_target != null)
        {
            Destroy(_target.gameObject);
        }

        _patrolIndicators.Clear();
        _patrolStep = 0;
    }
}
