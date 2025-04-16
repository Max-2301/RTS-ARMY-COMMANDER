using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    private readonly Dictionary<GameObject, (UnitBehaviour behaviour, UnitUI ui)> units = new();
    private readonly Dictionary<GameObject, (UnitBehaviour behaviour, UnitUI ui)> enemyUnits = new();
    private static UnitManager instance;
    public static UnitManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("UnitManager");
                instance = go.AddComponent<UnitManager>();
            }
            return instance;
        }
    }

    [SerializeField] private int updateDelay = 10, animatorUpdateDelay = 50;
    [SerializeField] private float animatorDisableDistance = 50f;
    private Camera mainCamera;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        mainCamera = Camera.main;
    }

    public void StartUnitLogic()
    {
        UpdateUnits();
        UpdateAnimators();
    }

    public void AddUnit(GameObject unit)
    {
        Debug.Log("Adding unit: " + unit.name);
        UnitBehaviour behaviour = unit.GetComponent<UnitBehaviour>();
        UnitUI ui = unit.GetComponent<UnitUI>();
        if (behaviour != null && ui != null && !units.ContainsKey(unit))
        {
            units.Add(unit, (behaviour, ui));
        }
    }

    public void AddEnemyUnit(GameObject unit)
    {
        UnitBehaviour behaviour = unit.GetComponent<UnitBehaviour>();
        UnitUI ui = unit.GetComponent<UnitUI>();
        if (behaviour != null && ui != null && !enemyUnits.ContainsKey(unit))
        {
            enemyUnits.Add(unit, (behaviour, ui));
        }
    }

    public void RemoveUnit(GameObject unit)
    {
        if (units.ContainsKey(unit))
        {
            units.Remove(unit);
            Destroy(unit);
        }
    }

    public void RemoveEnemyUnit(GameObject unit)
    {
        if (enemyUnits.ContainsKey(unit))
        {
            enemyUnits.Remove(unit);
            Destroy(unit);
        }
    }

    async void UpdateUnits()
    {
        while (true)
        {
            if (units != null && units.Count > 0)
            {
                foreach (var unit in units)
                {
                    unit.Value.behaviour.UpdateUnitBehaviour();
                    unit.Value.ui.UpdateUI();
                    await Task.Delay(updateDelay);
                }
            }

            if (enemyUnits != null && enemyUnits.Count > 0)
            {
                foreach (var unit in enemyUnits)
                {
                    unit.Value.behaviour.UpdateUnitBehaviour();
                    unit.Value.ui.UpdateUI();
                    await Task.Delay(updateDelay);
                }
            }

            await Task.Yield();
        }
    }

    async void UpdateAnimators()
    {
        while (true)
        {
            CheckAndToggleAnimators(units);
            CheckAndToggleAnimators(enemyUnits);
            await Task.Delay(animatorUpdateDelay);
            await Task.Yield();
        }
    }

    public void CheckAndToggleAnimators(Dictionary<GameObject, (UnitBehaviour behaviour, UnitUI ui)> units)
    {
        foreach (var unit in units)
        {
            if (unit.Key.gameObject == null) continue;
            float distance = Vector3.Distance(mainCamera.transform.position, unit.Key.transform.GetChild(0).position);
            if (distance > animatorDisableDistance)
            {
                unit.Value.behaviour.EnableAllAnimators(false);
            }
            else
            {
                unit.Value.behaviour.EnableAllAnimators(true);
            }
        }
    }

    public List<GameObject> GetUnits()
    {
        return new List<GameObject>(units.Keys);
    }

    public List<GameObject> GetEnemyUnits()
    {
        return new List<GameObject>(enemyUnits.Keys);
    }

    public bool CheckForUnit(GameObject unit)
    {
        if (units.ContainsKey(unit) || enemyUnits.ContainsKey(unit)) return true;
        return false;
    }
}