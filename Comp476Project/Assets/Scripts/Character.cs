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
        
        if (_target != null && Vector3.Distance(transform.position, _target.transform.position) <= _navMeshAgent.stoppingDistance)
        {
            if (_patrolTarget != null && _patrolOrigin != null)
            {
                if (_target == _patrolTarget)
                {
                    _target = null;
                    StartCoroutine(SetPatrolTargetWithDelay(_patrolOrigin));
                }
                else if (_target == _patrolOrigin)
                {
                    _target = null;
                    StartCoroutine(SetPatrolTargetWithDelay(_patrolTarget));
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

    private IEnumerator SetPatrolTargetWithDelay(GameObject target)
    {
        yield return new WaitForSeconds(PatrolCooldown);

        _navMeshAgent.SetDestination(target.transform.position);
        _target = target;
    }

    private void CreateTargetIndicator(RaycastHit hit)
    {
        StopAllCoroutines();
        DestroyTargetIndicators();

        _target = Instantiate(TargetIndicatorPrefab, hit.point + Vector3.up * 0.01f, Quaternion.identity);
        _target.transform.up = hit.normal;
        _target.transform.Rotate(new Vector3(90f, 0f, 0f));

        _navMeshAgent.SetDestination(hit.point);
    }

    private void CreatePatrolIndicators(RaycastHit originHit, RaycastHit targetHit)
    {
        StopAllCoroutines();
        DestroyTargetIndicators();

        _patrolOrigin = Instantiate(PatrolIndicatorPrefab, originHit.point + Vector3.up * 0.01f, Quaternion.identity);
        _patrolOrigin.transform.up = originHit.normal;
        _patrolOrigin.transform.Rotate(new Vector3(90f, 0f, 0f));

        _patrolTarget = Instantiate(PatrolIndicatorPrefab, targetHit.point + Vector3.up * 0.01f, Quaternion.identity);
        _patrolTarget.transform.up = targetHit.normal;
        _patrolTarget.transform.Rotate(new Vector3(90f, 0f, 0f));

        _navMeshAgent.SetDestination(targetHit.point);
        _target = _patrolTarget;
    }

    private void ShowTargetIndicators()
    {
        GameObject[] targetIndicators = { _target, _patrolOrigin, _patrolTarget };

        for (var i = 0; i < targetIndicators.Length; i++)
        {
            if (targetIndicators[i] != null)
            {
                targetIndicators[i].GetComponent<SpriteRenderer>().enabled = true;
            }
        }
    }

    private void HideTargetIndicators()
    {
        GameObject[] targetIndicators = { _target, _patrolOrigin, _patrolTarget };

        for (var i = 0; i < targetIndicators.Length; i++)
        {
            if (targetIndicators[i] != null)
            {
                targetIndicators[i].GetComponent<SpriteRenderer>().enabled = false;
            }
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
