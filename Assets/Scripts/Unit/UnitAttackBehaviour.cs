using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class UnitAttackBehaviour : MonoBehaviour, IRemovable
{
    private Dictionary<GameObject, (Troop troop, GameObject target, NavMeshAgent agent)> attackData = new();
    private Dictionary<GameObject, (float time, Coroutine timer)> attackTimerData = new();

    private UnitHealth enemyUnitHealth;
    private UnitBehaviour enemyUnitBehaviour;

    private UnitBehaviour unitbehaviour;

    public LayerMask targetLayer;

    private void Start()
    {
        unitbehaviour = GetComponent<UnitBehaviour>();
    }

    public void UpdateAttackInRange()
    {
        if (!UnitManager.Instance.CheckForUnit(enemyUnitHealth.gameObject))
        {
            enemyUnitBehaviour = null;
            enemyUnitHealth = null;
            return;
        }
        List<GameObject> keys = new(attackData.Keys);
        foreach (GameObject key in keys)
        {
            if (attackTimerData[key].timer != null) continue;
            if (!enemyUnitBehaviour.HasTroop(attackData[key].target))
            {
                unitbehaviour.UpdateTroopStatus(key, UnitBehaviour.TroopStatus.moving);
                attackData[key] = (attackData[key].troop, enemyUnitBehaviour.GetRandomTroop(), attackData[key].agent);
            }
            GameObject troop = key;
            GameObject target = attackData[key].target;
            NavMeshAgent agent = attackData[key].agent;

            if (Vector2.Distance(troop.transform.position, target.transform.position) > attackData[key].troop.range)
            {
                agent.SetDestination(target.transform.position);
                continue;
            }
            else
            {
                agent.ResetPath();
            }
            TroopAttackLogic(troop);
        }
    }

    private void TroopAttackLogic(GameObject troop)
    {
        Troop.TroopType troopType = attackData[troop].troop.troopType;
        switch (troopType)
        {
            case Troop.TroopType.melee:
                attackTimerData[troop] = (attackTimerData[troop].time, StartCoroutine(MeleeTroopAttack(troop)));
                break;
            case Troop.TroopType.ranged:
                attackTimerData[troop] = (attackTimerData[troop].time, StartCoroutine(TroopAttackTimer(troop)));
                RangedTroopAttack(troop);
                break;
            case Troop.TroopType.longRange:
                attackTimerData[troop] = (attackTimerData[troop].time, StartCoroutine(TroopAttackTimer(troop)));
                LongRangeTroopAttack(troop);
                break;
        }
    }

    private IEnumerator TroopAttackTimer(GameObject troop)
    {
        unitbehaviour.UpdateTroopStatus(troop, UnitBehaviour.TroopStatus.attacking);
        yield return new WaitForSeconds(attackTimerData[troop].time);
        ResetTimer(troop);
    }

    private void ResetTimer(GameObject troop)
    {
        attackTimerData[troop] = (attackData[troop].troop.attackTime, null);
    }

    private IEnumerator MeleeTroopAttack(GameObject troop)
    {
        unitbehaviour.UpdateTroopStatus(troop, UnitBehaviour.TroopStatus.attacking);
        yield return new WaitForSeconds(attackTimerData[troop].time);
        AttackDamage(attackData[troop].target, attackData[troop].troop.dmg);
        ResetTimer(troop);
    }

    private void RangedTroopAttack(GameObject troop)
    {
        GameObject bullet = BulletPool.Instance.GetBullet(attackData[troop].troop.projectile);
        if (bullet != null)
        {
            bullet.transform.position = troop.transform.position + new Vector3(0, 1, 0);
            bullet.transform.LookAt(attackData[troop].target.transform.position);
            StartCoroutine(BulletLogic(bullet, troop));
        }
    }

    private void LongRangeTroopAttack(GameObject troop)
    {
        GameObject bullet = BulletPool.Instance.GetBullet(attackData[troop].troop.projectile);
        Debug.Log(bullet);
        if (bullet != null)
        {
            bullet.transform.position = troop.transform.position + new Vector3(0,3,0);
            bullet.transform.LookAt(attackData[troop].target.transform.position);
            bullet.transform.Rotate(attackData[troop].troop.bulletAngle, 0,0);
            StartCoroutine(AoeBulletCollision(bullet, troop));
        }
    }

    private IEnumerator BulletLogic(GameObject bullet, GameObject troop)
    {
        Transform target = attackData[troop].target.transform;
        while (Vector3.Distance(bullet.transform.position, target.position) > 0.1f && target != null)
        {
            bullet.transform.position = Vector3.MoveTowards(bullet.transform.position, target.position, attackData[troop].troop.bulletSpeed * Time.deltaTime);
            bullet.transform.LookAt(target.position);
            yield return new WaitForFixedUpdate();
        }
        enemyUnitHealth.TakeDamage(target.gameObject, attackData[troop].troop.dmg);
        BulletPool.Instance.ReturnBullet(bullet, attackData[troop].troop.projectile);
    }

    private readonly int maxColliders = 40;
    private IEnumerator AoeBulletCollision(GameObject bullet, GameObject troop)
    {
        ParticleCollisionTracker tracker = bullet.GetComponent<ParticleCollisionTracker>();
        tracker.ResetCollision();

        // Wait until the particle collides with something
        while (!tracker.hasCollided)
        {
            yield return new WaitForFixedUpdate();
        }
        Vector3 hitPos = tracker.lastCollisionPoint;
        Collider[] colliders = new Collider[maxColliders];
        Physics.OverlapSphereNonAlloc(hitPos, attackData[troop].troop.aoeRadius, colliders, targetLayer);
        foreach (Collider collider in colliders)
        {
            if (collider == null) continue;
            enemyUnitHealth.TakeDamage(collider.gameObject, attackData[troop].troop.dmg);
        }
        yield return new WaitForSeconds(0.2f);
        Debug.Log("returning bullet");
        BulletPool.Instance.ReturnBullet(bullet, attackData[troop].troop.projectile);
    }

    private void AttackDamage(GameObject enemyTroop, int dmg)
    {
        enemyUnitHealth.TakeDamage(enemyTroop, dmg);
    }

    public void AddTroop(GameObject troop, Troop troopSCR, NavMeshAgent agent)
    {
        if (!attackData.ContainsKey(troop))
        {
            attackData.Add(troop, (troopSCR, null, agent));
            attackTimerData.Add(troop, (troopSCR.attackTime, null));
        }
    }

    public void RemoveTroop(GameObject troop)
    {
        if (attackData.ContainsKey(troop))
        {
            attackData.Remove(troop);
        }
    }

    public void SetDataTroop(GameObject troop, GameObject target)
    {
        if (attackData.ContainsKey(troop))
        {
            var data = attackData[troop];
            data.target = target;
            attackData[troop] = data;
        }
    }

    public Dictionary<GameObject, (Troop troop, GameObject target, NavMeshAgent agent)> GetData()
    {
        return attackData;
    }

    public void SetEnemyUnit(Transform enemyUnit)
    {
        if (enemyUnit == null)
        {
            enemyUnitHealth = null;
            enemyUnitBehaviour = null;
            return;
        }
        enemyUnitHealth = enemyUnit.GetComponent<UnitHealth>();
        enemyUnitBehaviour = enemyUnit.GetComponent<UnitBehaviour>();
    }
}