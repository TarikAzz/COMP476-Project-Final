using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using UnityEngine.UI;
#pragma warning disable 1587

/// <summary>
/// A character controlled by a player
/// </summary>
public class Character : MonoBehaviour
{
    #region Public variables

    [Header("Bullet")]

    /// <summary>
    /// The prefab of the bullet to be shot
    /// </summary>
    public GameObject BulletPrefab;

    /// <summary>
    /// Holds the spawn position of the bullet to be shot
    /// </summary>
    public Transform BulletSpawn;

    /// <summary>
    /// The velocity of the shot bullet
    /// </summary>
    public float BulletVelocity;

    /// <summary>
    /// The time before the bullet is destroyed after being shot
    /// </summary>
    public float BulletLifeTime;

    /// <summary>
    /// The damage the bullet inflicts on the character it collides with after being shot
    /// </summary>
    public float BulletDamage;

    [Header("Health")]

    /// <summary>
    /// The health UI compnent
    /// </summary>
    public Image HealthBar;

    /// <summary>
    /// The maximum health value
    /// </summary>
    public float MaxHealth;

    [Header("Movement")]

    /// <summary>
    /// The prefab of the indicator used to show the character's movement
    /// </summary>
    public GameObject IndicatorPrefab;

    /// <summary>
    /// The time a character wait after reaching a patrol node
    /// </summary>
    public float PatrolCooldown;

    /// <summary>
    /// Whether the character loops back on the patrol's path after reaching its last node or executes it in reverse
    /// </summary>
    public bool LoopPatrol;

    /// <summary>
    /// The duration of the character being stunned (if stunned)
    /// </summary>
    public float StunDuration;

    /// <summary>
    /// Keep track of network's time as you are NOT stunned
    /// </summary>
    public double NotStunnedTillNow;

    #endregion

    #region Public Properties

    /// <summary>
    /// The manager who owns the character
    /// </summary>
    public PlayerManager PlayerManager { get; set; }

    /// <summary>
    /// Whether or not the character is currently selected
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Whether or not the character has just been selected
    /// </summary>
    public bool JustSelected { get; set; }

    /// <summary>
    /// Whether or not the infiltrating character has been spotted by the defender
    /// </summary>
    public bool IsSpotted { get; set; }

    /// <summary>
    /// To determine if defender unit is stunned by infiltrator trap
    /// </summary>
    public bool IsStunned;

    #endregion

    #region Private variables

    /// <summary>
    /// The current target
    /// </summary>
    private Indicator _target;

    /// <summary>
    /// The chased target
    /// </summary>
    private Transform _chaseTarget;

    /// <summary>
    /// The chase routine
    /// </summary>
    private Coroutine _chaseRoutine;

    /// <summary>
    /// The step on the current patrol
    /// </summary>
    private int _patrolStep;

    /// <summary>
    /// Is the character going back around the patrol
    /// </summary>
    private bool _reversePatrol;

    /// <summary>
    /// The list of indicators for the patrol
    /// </summary>
    private List<Indicator> _patrolIndicators;

    /// <summary>
    /// The NavMeshAgent component
    /// </summary>
    public NavMeshAgent _navMeshAgent;

    #endregion

    /// <summary>
    /// Initilizes the components and the pertinent members
    /// </summary>
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _patrolIndicators = new List<Indicator>();
    }

    /// <summary>
    /// Updates the target, listening to inputs from the user
    /// </summary>
    void Update()
    {
        if (PlayerManager == null || !PlayerManager.GameReady)
        {
            return;
        }

        // If the character has just been selected, don't update this frame
        if (JustSelected)
        {
            JustSelected = false;
            return;
        }

        // Shift-right-click to set a patrol for the character
        if (PlayerManager.Kind == PlayerManager.PlayerKind.Defender && IsSelected && Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(1) && IsStunned == false)
        {
            RaycastHit originHit;
            RaycastHit targetHit;

            var originHitSuccess = Physics.Raycast(transform.position, Vector3.down, out originHit);
            var targetHitSuccess = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out targetHit);

            // Create indicators and initiate patrol if both the clicked ground and the ground under the character is walkable
            if (originHitSuccess && targetHitSuccess)
            {
                var targetWalkable = targetHit.collider.GetComponent<Walkable>();

                if (targetWalkable != null && targetHit.collider.GetComponent<Walkable>())
                {
                    // Disallow movement on other surface than the neutral zone for the defender player on setup
                    var defenderSetup =
                        !PlayerManager.GameOn &&
                        PlayerManager.Kind == PlayerManager.PlayerKind.Defender &&
                        targetWalkable.Kind != Walkable.WalkableKind.Neutral;

                    if (!defenderSetup)
                    {
                        CreatePatrolIndicators(originHit, targetHit);
                    }
                }
            }
        }
        // Right-click to set simple movement target
        else if (IsSelected && Input.GetMouseButtonDown(1) && IsStunned == false)
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                var walkable = hit.collider.GetComponent<Walkable>();

                // Create indicators and initiate movement if a walkable surface is detected on click
                if (walkable != null && !walkable.IsWall(hit.normal))
                {
                    // Disallow movement on other surface than the start zone for the Infiltrator player on setup
                    var infiltratorSetup =
                        !PlayerManager.GameOn &&
                        PlayerManager.Kind == PlayerManager.PlayerKind.Infiltrator &&
                        walkable.Kind != Walkable.WalkableKind.Start;

                    // Disallow movement on other surface than the neutral zone for the defender player on setup
                    var defenderSetup =
                        !PlayerManager.GameOn &&
                        PlayerManager.Kind == PlayerManager.PlayerKind.Defender &&
                        walkable.Kind != Walkable.WalkableKind.Neutral;

                    if (!infiltratorSetup && !defenderSetup)
                    {
                        CreateTargetIndicator(hit);
                    }
                }
            }
        }

        // Handle pathfinding when the current target is reached
        if (_target != null && Vector3.Distance(transform.position, _target.transform.position) <= _navMeshAgent.stoppingDistance)
        {
            // If the character is on patrol, set the target to the next patrol indicator
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

            // If not on patrol, simply destroy the target
            DestroyTargetIndicators();

            if (IsSpotted)
            {
                ToggleVisibility(true);
            }
        }

        // When stunned, free itself when stun duration passes (and go back to being hidden)
        if(IsStunned)
        {
            if(Network.time > NotStunnedTillNow + StunDuration)
            {
                IsStunned = false;
                IsSpotted = false;
            }
        }
        // When not stunned, track up-to-date time
        else
        {
            NotStunnedTillNow = Network.time;
        }

        if (_chaseTarget != null && _chaseRoutine != null && !GetComponent<FieldOfView>().visibleTargets.Contains(_chaseTarget))
        {
            StopChase();
        }
    }

    /// <summary>
    /// Tell the player manager to assign damage to this character
    /// </summary>
    public void TakeDamage()
    {
        if (PlayerManager == null)
        {
            return;
        }

        PlayerManager.AssignDamage(this);
    }

    /// <summary>
    /// Tell the player managre to assign heavy damage to this character (caused by Defender's Sniper)
    /// </summary>
    public void TakeSniperDamage()
    {
        if (PlayerManager == null)
        {
            return;
        }

        PlayerManager.AssignBurstDamage(this);
    }

    /// <summary>
    /// Update the graphical healthbar above the character to reflect its health
    /// </summary>
    /// <param name="ratio">The ratio of current/max health</param>
    public void UpdateHealthBar(float ratio)
    {
        HealthBar.fillAmount = ratio;
    }

    /// <summary>
    /// Colors the character
    /// </summary>
    /// <param name="color">The color to be applied</param>
    public void Colorize(Color color)
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();

        for (var i = 0; i < renderers.Length; i++)
        {
            if (GetComponent<FieldOfView>() != null && renderers[i] == GetComponent<FieldOfView>().viewMeshFilter.GetComponent<MeshRenderer>())
            {
                continue;
            }
            renderers[i].material.color = color;
        }
    }

    /// <summary>
    /// Selects the character
    /// </summary>
    public void Select()
    {
        if (PlayerManager == null || !PlayerManager.isLocalPlayer)
        {
            return;
        }

        IsSelected = true;
        JustSelected = true;

        Colorize(Color.cyan);

        ShowTargetIndicators();
    }

    /// <summary>
    /// Deselects the character
    /// </summary>
    public void Deselect()
    {
        if (PlayerManager == null || !PlayerManager.isLocalPlayer)
        {
            return;
        }

        IsSelected = false;
        Colorize(Color.blue);
        HideTargetIndicators();
    }

    /// <summary>
    /// Starts chasing a character
    /// </summary>
    /// <param name="target">The chased character</param>
    public void Chase(Transform target)
    {
        if (_chaseTarget != null && _chaseTarget == target)
        {
            return;
        }

        if (_chaseRoutine != null)
        {
            StopCoroutine(_chaseRoutine);
        }
        
        _chaseTarget = target;
        _chaseRoutine = StartCoroutine(FollowTarget(target));
    }

    /// <summary>
    /// Stops chasing a character
    /// </summary>
    public void StopChase()
    {
        StopCoroutine(_chaseRoutine);
        _chaseRoutine = null;

        _navMeshAgent.SetDestination(_target.transform.position);

        _chaseTarget = null;
    }

    /// <summary>
    /// Follow a target, recomputing a path at a fixed interval
    /// </summary>
    /// <param name="target">The target</param>
    private IEnumerator FollowTarget(Transform target)
    {
        var previousTargetPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity);

        while (Vector3.SqrMagnitude(transform.position - target.position) > 0.1f)
        {
            if (Vector3.SqrMagnitude(previousTargetPosition - target.position) > 0.1f)
            {
                _navMeshAgent.SetDestination(target.position);
                previousTargetPosition = target.position;
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }

    /// <summary>
    /// Set the immidiate target of the character
    /// </summary>
    /// <param name="target">The indicator to be the target</param>
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

    /// <summary>
    /// Set a new target along the patrol path with a delay
    /// </summary>
    /// <param name="target">The new target</param>
    private IEnumerator SetPatrolTargetWithDelay(Indicator target)
    {
        SetTarget(target);

        yield return new WaitForSeconds(PatrolCooldown);

        _navMeshAgent.SetDestination(target.transform.position);
    }

    /// <summary>
    /// Spawns an indicator for the target
    /// </summary>
    /// <param name="hit">The raycast information</param>
    private void CreateTargetIndicator(RaycastHit hit)
    {
        StopAllCoroutines();
        DestroyTargetIndicators();

        var centerOffset = GetSelectionCenterOffset();

        if (Physics.OverlapSphere(hit.point - centerOffset, 0.1f).Length > 1)
        {
            Deselect();
            return;
        }

        _target = Instantiate(IndicatorPrefab, hit.point - centerOffset + Vector3.up * 0.01f, Quaternion.identity).GetComponent<Indicator>();
        _target.transform.up = hit.normal;
        _target.transform.Rotate(new Vector3(90f, 0f, 0f));
        SetTarget(_target);

        _navMeshAgent.SetDestination(hit.point - centerOffset);
    }

    /// <summary>
    /// Spawns an indicators for the patrol path being created
    /// </summary>
    /// <param name="originHit">The raycast information for the origin of the patrol</param>
    /// <param name="targetHit">The raycast information for the new patrol target</param>
    private void CreatePatrolIndicators(RaycastHit originHit, RaycastHit targetHit)
    {
        var newPatrolNode = (Indicator)null;

        var centerOffset = GetSelectionCenterOffset();

        if (Physics.OverlapSphere(targetHit.point - centerOffset, 0.1f).Length > 1)
        {
            Deselect();
            return;
        }

        // If the patrol list is empty, create an indicator at its origin
        if (_patrolIndicators.Count == 0)
        {
            StopAllCoroutines();
            DestroyTargetIndicators();
            
            newPatrolNode = Instantiate(IndicatorPrefab, originHit.point + Vector3.up * 0.01f, Quaternion.identity).GetComponent<Indicator>();
            newPatrolNode.transform.up = originHit.normal;
            newPatrolNode.transform.Rotate(new Vector3(90f, 0f, 0f));

            _patrolIndicators.Add(newPatrolNode);

            _navMeshAgent.SetDestination(targetHit.point - centerOffset);

            _patrolStep = 1;
        }
        
        // Create an indicator for ground clicks
        newPatrolNode = Instantiate(IndicatorPrefab, targetHit.point - centerOffset + Vector3.up * 0.01f, Quaternion.identity).GetComponent<Indicator>();
        newPatrolNode.transform.up = targetHit.normal;
        newPatrolNode.transform.Rotate(new Vector3(90f, 0f, 0f));
        _patrolIndicators.Add(newPatrolNode);

        if (_patrolIndicators.Count == 2)
        {
            SetTarget(newPatrolNode);
        }
    }

    /// <summary>
    /// Gets the center offset from the center of mass of the selected characters to the character's position
    /// </summary>
    /// <returns>The center offset</returns>
    private Vector3 GetSelectionCenterOffset()
    {
        var selectedCharacterPositions = (from character in PlayerManager.Characters where character.IsSelected select character.transform.position).ToArray();
        var centerOffset = Vector3.zero;

        if (selectedCharacterPositions.Length > 1)
        {
            var center = Vector3.zero;

            foreach (var position in selectedCharacterPositions)
            {
                center += position;
            }

            center /= selectedCharacterPositions.Length;

            centerOffset = center - transform.position;
        }

        return centerOffset;
    }

    /// <summary>
    /// Makes the character's movement indicator visible
    /// </summary>
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

    /// <summary>
    /// Makes the character's movement indicator invisible
    /// </summary>
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

    /// <summary>
    /// Destroys all the character's movement indicators
    /// </summary>
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

    /// <summary>
    /// Toggles visibility of the spotted character and its components.
    /// </summary>
    /// <param name="enabled">Determines if enabled or not</param>
    public void ToggleVisibility(bool enabled)
    {
        Renderer[] rs = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in rs)
        {
            r.enabled = enabled;
        }

        gameObject.transform.GetChild(2).gameObject.SetActive(enabled);
    }

}
