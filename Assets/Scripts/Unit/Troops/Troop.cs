using JetBrains.Annotations;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "Troop", menuName = "Scriptable Objects/Troop")]
public class Troop : ScriptableObject
{
    public int cost;
    public float attackTime;
    public float range;
    public int dmg;
    public int health;
    public float speed;

    public GameObject model;
    [Tooltip("How Many Rows of the troop")][Min(1)]public int troopWidth;
    [Tooltip("Ammount of troops in a row1")][Min(1)] public int troopDepth;
    public int ammount;
    public float spreadW;
    public float spreadD;

    public float height, width;

    public float detectionRange;
    public enum TroopType
    {
        ranged,
        melee,
        longRange
    }
    public TroopType troopType;

    public GameObject projectile;
    public float bulletAngle;
    public float bulletSpeed;
    public float aoeRadius;

    private void OnValidate()
    {
        ammount = troopWidth * troopDepth;
    }
}
