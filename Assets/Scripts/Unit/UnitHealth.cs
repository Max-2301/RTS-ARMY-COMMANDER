using System.Collections.Generic;
using UnityEngine;

public class UnitHealth : MonoBehaviour, IRemovable, IDamagable
{
    private Dictionary<GameObject, int> healthData = new();
    
    private int health;

    private UnitBehaviour unitBehaviour;
    private UnitAttackBehaviour unitAttackBehaviour;

    private void Start()
    {
        unitBehaviour = GetComponent<UnitBehaviour>();
        unitAttackBehaviour = GetComponent<UnitAttackBehaviour>();
    }
    public void RemoveTroop(GameObject troop)
    {
        healthData.Remove(troop);
        unitBehaviour.RemoveTroop(troop);
        unitAttackBehaviour.RemoveTroop(troop);
        troop.SetActive(false);
    }

    public void TakeDamage(GameObject troop, int damage)
    {
        if (!healthData.ContainsKey(troop)) return;
        int h = healthData[troop] - damage;
        if (h < 0)
        {
            RemoveTroop(troop);
            health -= h + damage;
        }
        else
        {
            healthData[troop] = h;
            health -= damage;
        }
        if (health <= 0)
        {
            UnitManager.Instance.RemoveUnit(gameObject);
            Destroy(gameObject);
        }
    }

    public void AddTroop(GameObject troop, int health)
    {
        healthData.Add(troop, health);
        this.health += health;
    }
}
