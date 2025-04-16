using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.XR;
using System.Linq;

public class UnitBehaviour : MonoBehaviour, IRemovable
{
    private Transform target;
    private Transform enemyTarget = null;
    private UnitBehaviour enemyUnitBehaviour;

    public Unit unit;

    private NavMeshAgent agent;

    public enum TroopStatus
    {
        standing,
        moving,
        attacking
    }

    protected Dictionary<GameObject, (Transform target, Troop troop, NavMeshAgent agent, TroopStatus troopStatus, Animator troopAnimator)> behaviourData = new();
    protected Dictionary<GameObject, (Coroutine rotating, bool isFacingTarget)> faceTargetData = new();
    private readonly float rotationSpeed = 5f;

    private UnitAttackBehaviour unitAttackBehaviour;
    private UnitHealth unitHealth;
    private UnitUI unitUI;

    private bool autoAttack = false;
    private bool attackInitiated = false;

    public LayerMask enemyLayer;
    public enum UnitStatus
    {
        moving,
        attack
    }
    private UnitStatus behaviour = UnitStatus.moving;

    private void Start()
    {
        unitAttackBehaviour = GetComponent<UnitAttackBehaviour>();
        unitAttackBehaviour.targetLayer = enemyLayer;
        unitHealth = GetComponent<UnitHealth>();
        unitUI = GetComponent<UnitUI>();

        InitializeTarget();
        InitializeAgent();
        InitializeTroops();
    }

    private void InitializeTarget()
    {
        target = new GameObject("Target" + unit.name).transform;
        target.position = transform.position;
        target.parent = transform;
    }

    private void InitializeAgent()
    {
        agent = transform.GetChild(0).AddComponent<NavMeshAgent>();
        agent.agentTypeID = SurfaceIDInfo.Instance.GetUnitSurfaceID();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.speed = unit.speed;
        faceTargetData.Add(agent.gameObject, (null, false));
    }

    private void InitializeTroops()
    {
        List<GameObject> keys = new(behaviourData.Keys);
        foreach (GameObject troop in keys)
        {
            NavMeshAgent agent = troop.AddComponent<NavMeshAgent>();
            agent.speed = behaviourData[troop].troop.speed;
            behaviourData[troop] = (behaviourData[troop].target, behaviourData[troop].troop, agent, behaviourData[troop].troopStatus, behaviourData[troop].troopAnimator);
            faceTargetData.Add(troop, (null, false));
            
            unitAttackBehaviour.AddTroop(troop, behaviourData[troop].troop, agent);

            unitHealth.AddTroop(troop, behaviourData[troop].troop.health);
        }
    }

    public void SetNewTarget(Vector3 pos, NavMeshAgent agent)
    {
        NavMeshPath path = new();
        agent.CalculatePath(pos, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(pos);
        }
        else
        {
            Debug.LogWarning("Invalid path to target position.");
        }

        if (IsDestinationReached(agent) && !faceTargetData[agent.gameObject].isFacingTarget)
        {
            Transform t = target;
            if (behaviourData.ContainsKey(agent.gameObject))
            {
                t = behaviourData[agent.gameObject].target;
            }
            if (faceTargetData.ContainsKey(agent.gameObject) && faceTargetData[agent.gameObject].rotating != null)
            {
                StopCoroutine(faceTargetData[agent.gameObject].rotating);
            }
            faceTargetData[agent.gameObject] = (StartCoroutine(FaceTarget(agent, t)), HasFinishedRotating(agent, t));
        }
    }

    public void SetTroops(Dictionary<GameObject, (Transform, Troop, NavMeshAgent, TroopStatus, Animator)> troopsLinkedToSpots)
    {
        this.behaviourData = troopsLinkedToSpots;
    }

    public void UpdateUnitBehaviour()
    {
        if (autoAttack && !UnitManager.Instance.CheckForUnit(enemyTarget.gameObject))
        {
            FindClosestEnemy();
        }
        UpdateBehaviour();
    }

    private void FindClosestEnemy()
    {
        float closestDistance = float.MaxValue;
        Transform closestEnemy = null;

        foreach (GameObject enemyUnit in UnitManager.Instance.GetEnemyUnits())
        {
            float distance = Vector3.Distance(agent.transform.position, enemyUnit.transform.position);
            if (distance < unit.detectionRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemyUnit.transform;
            }
        }

        if (closestEnemy != null)
        {
            enemyTarget = closestEnemy;
            unitAttackBehaviour.SetEnemyUnit(closestEnemy);
            behaviour = UnitStatus.attack;
        }
        else
        {
            unitAttackBehaviour.SetEnemyUnit(null);
            behaviour = UnitStatus.moving;
        }
    }

    private void UpdateBehaviour()
    {
        switch (behaviour)
        {
            case UnitStatus.moving:
                attackInitiated = false;
                UpdateDestination();
                break;
            case UnitStatus.attack:
                UpdateAttackBehaviour();
                break;
        }
    }
    private void UpdateDestination()
    {
        List<GameObject> keys = new(behaviourData.Keys);

        foreach (GameObject troop in keys)
        {
            UpdateTroopStatusBasedOnVelocity(troop);
            UpdateTroopAnimations(troop);
            Vector3 target = behaviourData[troop].target.position;
            SetNewTarget(target, behaviourData[troop].agent);
        }
    }

    private void UpdateTroopStatusBasedOnVelocity(GameObject troop)
    {
        if (behaviourData[troop].troopStatus == TroopStatus.attacking) return;
        var t = behaviourData[troop];
        if (!t.agent) return;
        t.troopStatus = t.agent.velocity.sqrMagnitude > 0.1f ? TroopStatus.moving : TroopStatus.standing;
        behaviourData[troop] = (t.target, t.troop, t.agent, t.troopStatus, t.troopAnimator);
    }

    private void UpdateAttackBehaviour()
    {
        if (UnitManager.Instance.CheckForUnit(enemyTarget.gameObject))
        {
            float distanceToEnemy = Vector3.Distance(agent.transform.position, enemyTarget.position);
            if (distanceToEnemy > unit.attackRange)
            {
                HandleOutOfRange();
            }
            else
            {
                HandleInRange();
            }
        }
        else
        {
            behaviour = UnitStatus.moving;
            enemyUnitBehaviour = null;
            SetNewTarget(target.position, agent);
        }
    }

    private void HandleOutOfRange()
    {
        UpdateDestination();
        attackInitiated = false;
    }

    private void HandleInRange()
    {
        if (!attackInitiated)
        {
            agent.ResetPath();
            attackInitiated = true;
            SetAttackTargets();
        }
        else
        {
            unitAttackBehaviour.UpdateAttackInRange();
        }
    }

    private void SetAttackTargets()
    {
        List<GameObject> keys = new(unitAttackBehaviour.GetData().Keys);
        foreach (GameObject key in keys)
        {
            if (behaviourData.ContainsKey(key))
            {
                Debug.Log(enemyUnitBehaviour);
                GameObject target = enemyUnitBehaviour.GetRandomTroop();
                unitAttackBehaviour.SetDataTroop(key, target);
            }
        }
    }

    public void UpdateTroopStatus(GameObject troop, TroopStatus status)
    {
        if (behaviourData.ContainsKey(troop))
        {
            var troopData = behaviourData[troop];
            troopData.troopStatus = status;
            behaviourData[troop] = troopData;
        }
        UpdateTroopAnimations(troop);
    }

    private bool IsDestinationReached(NavMeshAgent agent)
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    private IEnumerator FaceTarget(NavMeshAgent agent, Transform target)
    {
        faceTargetData[agent.gameObject] = (faceTargetData[agent.gameObject].rotating, false);
        while (!HasFinishedRotating(agent, target))
        {
            Vector3 direction = (target.position - agent.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
        faceTargetData[agent.gameObject] = (null, true);
    }

    private bool HasFinishedRotating(NavMeshAgent agent, Transform target)
    {
        Vector3 direction = (target.position - agent.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        return Quaternion.Angle(agent.transform.rotation, lookRotation) < 0.1f;
    }

    public NavMeshAgent GetAgent()
    {
        return agent;
    }

    public void SetUnitTarget(Transform pos, bool enemy)
    {
        if (enemy)
        {
            enemyTarget = pos;
            enemyUnitBehaviour = pos.GetComponent<UnitBehaviour>();
            unitAttackBehaviour.SetEnemyUnit(pos);
            behaviour = UnitStatus.attack;
        }
        else
        {
            target.position = pos.position;
            enemyTarget = null;
            enemyUnitBehaviour = null;
            behaviour = UnitStatus.moving;
        }
        SetNewTarget(pos.position, agent);
    }

    public void UpdateTroopAnimations(GameObject troopKey)
    {
        var troop = behaviourData[troopKey];
        Animator animator = troop.troopAnimator;
        if (animator.enabled)
        {
            switch (troop.troopStatus)
            {
                case TroopStatus.standing:
                    SetAnimator(animator, "Move", false);
                    SetAnimator(animator, "Attack", false);
                    SetAnimator(animator, "Stand", true);
                    break;
                case TroopStatus.moving:
                    SetAnimator(animator, "Stand", false);
                    SetAnimator(animator, "Attack", false);
                    SetAnimator(animator, "Move", true);
                    break;
                case TroopStatus.attacking:
                    SetAnimator(animator, "Stand", false);
                    SetAnimator(animator, "Move", false);
                    SetAnimator(animator, "Attack", true);
                    break;
            }
        }
    }

    private void SetAnimator(Animator animator, string trigger, bool enable)
    {
        animator.SetBool(trigger, enable);
    }

    public void EnableAllAnimators(bool enabled)
    {
        foreach (var troop in behaviourData.Values)
        {
            if (troop.troopAnimator.enabled == enabled) break;
            troop.troopAnimator.enabled = enabled;
        }
    }

    public void SetAutoAttack()
    {
        autoAttack = !autoAttack;
        unitUI.ChangeUIAttackStatus(autoAttack);
    }

    public GameObject GetRandomTroop()
    {
        if (behaviourData.Count == 0)
        {
            return null;
        }
        int rand = Random.Range(0, behaviourData.Keys.Count);
        return behaviourData.ElementAt(rand).Key;
    }

    public bool HasTroop(GameObject troop)
    {
        return behaviourData.ContainsKey(troop);
    }

    public void RemoveTroop(GameObject troop)
    {
        if (behaviourData.ContainsKey(troop))
        {
            behaviourData.Remove(troop);
        }
        if (faceTargetData.ContainsKey(troop))
        {
            if (faceTargetData[troop].rotating != null) StopCoroutine(faceTargetData[troop].rotating);
            faceTargetData.Remove(troop);
        }
    }

    public void SetEnemyLayer(string layerName)
    {
        enemyLayer = LayerMask.GetMask(layerName);
    }
}