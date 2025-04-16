using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Scriptable Objects/Unit")]
public class Unit : ScriptableObject
{
    public List<Troop> troops;
    public int cost;
    [Tooltip("Should have spots for all troops in list")]public GameObject formation;

    public float speed;

    public float attackRange;
    public float detectionRange;

    public void OnValidate()
    {
        int c = 0;

        float minSpeed = float.MaxValue;
        float maxRange = 0;
        float maxAttackRnage = 0;
        for (int i = 0; i < troops.Count; i++)
        {
            c += troops[i].cost * troops[i].ammount;
            if (troops[i].speed < minSpeed)
            {
                minSpeed = troops[i].speed;
            }
            if (troops[i].detectionRange > maxRange)
            {
                maxRange = troops[i].detectionRange;
            }
            if (troops[i].range > maxAttackRnage)
            {
                maxAttackRnage = troops[i].range;
            }
        }
        cost = c;

        speed = minSpeed == float.MaxValue ? 0 : minSpeed;
        detectionRange = maxRange;
        attackRange = maxAttackRnage;
    }
}
