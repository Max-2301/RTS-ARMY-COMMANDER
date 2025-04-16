using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitPlacement : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints = new();
    [SerializeField] private List<Unit> units = new();
    [SerializeField] private int unitCount = 5;

    [SerializeField] private GameObject unitParent;

    [SerializeField] private string troopLayer, enemyLayer;
    private void Start()
    {
        List<Transform> points = new();
        if (spawnPoints.Count < unitCount)
        {
            Debug.LogError("Not enough spawn points for the number of units.");
            return;
        }
        for(int i = 0; i < unitCount; i++)
        {
            int randomIndex = Random.Range(0, units.Count);
            Unit unit = units[randomIndex];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            while (points.Contains(spawnPoint))
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            }
            GameObject unitObj = Instantiate(unitParent, spawnPoint.position, spawnPoint.rotation);
            UnitSetUp unitSetUp = unitObj.GetComponent<UnitSetUp>();
            unitSetUp.unit = unit;
            unitSetUp.troopLayer = troopLayer;
            unitSetUp.enemyLayer = enemyLayer;

            UnitBehaviour unitBehaviour = unitObj.AddComponent<UnitBehaviour>();
            unitBehaviour.unit = unit;

            unitObj.AddComponent<UnitAttackBehaviour>();

            unitObj.AddComponent<UnitHealth>();

            UnitUI unitUI = unitObj.AddComponent<UnitUI>();
            unitUI.unit = unit;

            UnitManager.Instance.AddEnemyUnit(unitObj);
        }
    }
}
