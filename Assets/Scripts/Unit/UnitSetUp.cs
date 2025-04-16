using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class UnitSetUp : MonoBehaviour
{
    public Unit unit { private get; set; }

    [SerializeField] private GameObject emptyTroop;

    private List<List<Transform>>[] troopSpots;
    private List<List<GameObject>>[] troops;

    private Dictionary<GameObject, (Transform, Troop, NavMeshAgent, UnitBehaviour.TroopStatus, Animator)> troopsLinkedToSpots = new();

    [SerializeField] private GameObject troopSpot;

    public string troopLayer, enemyLayer;
    private void Start()
    {
        if (troopLayer == null) troopLayer = "Troops";
        troopSpots = new List<List<Transform>>[unit.troops.Count];
        troops = new List<List<GameObject>>[unit.troops.Count];
        Transform formation = Instantiate(unit.formation, transform.position, transform.rotation, transform).transform;

        for (int i = 0; i < unit.troops.Count; i++)
        {
            Transform formationSpot = formation.GetChild(i).transform;
            Troop troop = unit.troops[i];
            troopSpots[i] = new List<List<Transform>>();
            troops[i] = new List<List<GameObject>>();

            for (int w = 0; w < troop.troopWidth; w++)
            {
                troopSpots[i].Add(new List<Transform>());
                troops[i].Add(new List<GameObject>());

                for (int d = 0; d < troop.troopDepth; d++)
                {
                    //Calculate position to center around the formation spot
                    float offsetX = troop.spreadW * (w - (troop.troopWidth - 1) / 2f); //Center formation horizontally
                    float offsetZ = troop.spreadD * (d - (troop.troopDepth - 1) / 2f); //Center formation vertically

                    Transform troopSpot = Instantiate(this.troopSpot, formationSpot.position + new Vector3(offsetX, 0, offsetZ), formationSpot.rotation, formationSpot).transform;
                    troopSpots[i][w].Add(troopSpot);

                    GameObject spawnedTroop = Instantiate(emptyTroop, troopSpot.position, troopSpot.rotation, transform);
                    troops[i][w].Add(spawnedTroop);

                    GameObject model = Instantiate(troop.model, spawnedTroop.transform.position, spawnedTroop.transform.rotation, spawnedTroop.transform);
                    model.layer = LayerMask.NameToLayer(troopLayer);

                    Animator anim = null;
                    if (model.TryGetComponent<Animator>(out Animator animator))
                    {
                        anim = animator;
                    }

                    troopsLinkedToSpots.Add(spawnedTroop, (troopSpot, troop, null, UnitBehaviour.TroopStatus.standing, anim));
                }
            }
        }
        if (TryGetComponent<UnitBehaviour>(out var unitBehaviour))
        {
            unitBehaviour.SetTroops(troopsLinkedToSpots);
            unitBehaviour.SetEnemyLayer(enemyLayer);
        }

    }

    public int GetCost()
    {
        return unit.cost;
    }

    private void OnDestroy()
    {
        UnitManager.Instance.RemoveUnit(gameObject);
    }
}
