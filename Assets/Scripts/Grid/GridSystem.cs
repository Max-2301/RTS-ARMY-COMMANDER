using UnityEngine;
using System.Collections.Generic;

public class GridSystem : MonoBehaviour
{
    [SerializeField] private int points = 100;

    [SerializeField] private GameObject tileObject;
    [SerializeField] private GameObject gridParent;
    [SerializeField] private float tileSize = 10;

    private float width, height;

    [SerializeField] private int horizontalTiles = 30, verticalTiles = 10;

    private Transform[,] gridPlaces;
    private GameObject[,] placedTroops;

    [SerializeField] private GameObject unitParent;

    [SerializeField] private string troopLayer, enemyLayer;
    void Start()
    {
        gridPlaces = new Transform[horizontalTiles, verticalTiles];
        placedTroops = new GameObject[horizontalTiles, verticalTiles];
        MakeGrid();
    }

    private void MakeGrid()
    {
        float gridWidth, gridHeight;

        Renderer gridRenderer = GetComponent<Renderer>();
        gridWidth = tileSize / 10 * horizontalTiles;
        gridHeight = tileSize / 10 * verticalTiles;

        gridRenderer.transform.localScale = new Vector3(gridWidth, 0, gridHeight);
        width = gridRenderer.bounds.size.x;
        height = gridRenderer.bounds.size.z;

        for (int i = 0; i < horizontalTiles; i++)
        {
            for (int j = 0; j < verticalTiles; j++)
            {
                float xPos = i * tileSize + tileSize / 2;
                float zPos = j * tileSize + tileSize / 2;

                GameObject spawnedTile = Instantiate(tileObject, new Vector3(transform.position.x - width / 2 + xPos, transform.position.y, transform.position.z - height / 2 + zPos), Quaternion.identity, gridParent.transform);
                gridPlaces[i, j] = spawnedTile.transform;
            }
        }
    }

    public Transform GetGridPoint(Vector3 pos)
    {
        Transform target = null;
        float closestDistanceSqur = Mathf.Infinity;
        foreach (Transform t in gridPlaces)
        {
            Vector3 directionToTarget = t.position - pos;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqur)
            {
                closestDistanceSqur = dSqrToTarget;
                target = t;
                if (closestDistanceSqur < tileSize / 2) return target;
            }
        }
        return target;
    }

    public int SpawnUnitAndGetCost(Transform point, Unit unit, int points)
    {
        int cost = 0;
        if (unit != null) cost = unit.cost;

        for (int i = 0; i < horizontalTiles; i++)
        {
            for (int j = 0; j < verticalTiles; j++)
            {
                if (gridPlaces[i, j].position == point.position)
                {
                    if (unit == null)
                    {
                        if (placedTroops[i, j] != null)
                        {
                            cost -= placedTroops[i, j].GetComponent<UnitSetUp>().GetCost();
                            Destroy(placedTroops[i, j]);
                            placedTroops[i, j] = null;
                        }
                        return cost;
                    }
                    else
                    {
                        if (placedTroops[i, j] != null)
                        {
                            cost -= placedTroops[i, j].GetComponent<UnitSetUp>().GetCost();
                            if (points - cost < 0) return 0;
                            Destroy(placedTroops[i, j]);
                        }
                        if (points - cost < 0) return 0;

                        GameObject unitObj = Instantiate(unitParent, gridPlaces[i, j].position, gridPlaces[i, j].rotation);
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

                        placedTroops[i, j] = unitObj;

                        UnitManager.Instance.AddUnit(unitObj);

                        return cost;
                    }
                }
            }
        }
        return cost;
    }
}