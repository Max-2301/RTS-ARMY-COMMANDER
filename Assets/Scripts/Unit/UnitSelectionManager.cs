using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    private static UnitSelectionManager instance;
    public static UnitSelectionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("UnitSelectionManager");
                instance = go.AddComponent<UnitSelectionManager>();
            }
            return instance;
        }
    }

    [SerializeField] private LayerMask troopLayer, enemyLayer, groundLayer;
    [SerializeField] private Transform parent;
    [SerializeField] private float minSpacing = 20; // Spacing between ghost units

    private List<GameObject> selectedUnits = new();
    private List<GameObject> ghostUnits = new();
    private bool holding = false;
    private bool rotating = false;
    private bool adding = false;
    private Vector3 rotationPos;

    private InputManager inputManager;

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
    }

    private void OnEnable()
    {
        inputManager = InputManager.Instance;
        inputManager.SelectCanceled += Clicked;
        inputManager.HoldPerformed += HoldStarted;
        inputManager.HoldReleased += HoldReleased;
        inputManager.RotateStarted += RotateStarted;
        inputManager.RotateReleased += RotateReleased;
        inputManager.AddPerformed += AddStarted;
        inputManager.AddCanceled += AddCanceled;
        inputManager.AttackEnablePerformed += SetAttackStatusUnit;
    }

    private void OnDisable()
    {
        inputManager.SelectCanceled -= Clicked;
        inputManager.HoldPerformed -= HoldStarted;
        inputManager.HoldReleased -= HoldReleased;
        inputManager.RotateStarted -= RotateStarted;
        inputManager.RotateReleased -= RotateReleased;
        inputManager.AddPerformed -= AddStarted;
        inputManager.AddCanceled -= AddCanceled;
        inputManager.AttackEnablePerformed -= SetAttackStatusUnit;
    }

    private void Update()
    {
        if (holding && !adding)
        {
            if (ghostUnits.Count == 1 || rotating)
                RotateGhosts();
            else
                UpdateGhostUnits();
        }
    }

    private void Clicked()
    {
        if (holding) return;
        if (selectedUnits.Count > 0)
        {
            GameObject enemy = CheckForEnemyUnit();
            if (enemy != null)
            {
                SetAttackTarget(enemy.transform);
                return;
            }
        }

        if (adding)
        {
            AddUnit();
        }
        else
        {
            GameObject unit = AddUnit();
            if (unit == null)
            {
                Cancel();
            }
        }
    }

    private GameObject CheckForEnemyUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer))
        {
            return hit.transform.root.gameObject;
        }
        return null;
    }

    private void HoldStarted()
    {
        if (adding) return;
        holding = true;
        if (selectedUnits.Count > 0 && !adding)
        {
            parent.position = GetMouseWorldPosition();
            CreateGhostUnits();
        }
    }

    private void HoldReleased()
    {
        if (holding && !adding)
        {
            holding = false;
            SetTargets();
            ClearUnits();
            ClearGhostUnits();
        }
    }

    private void RotateStarted()
    {
        rotationPos = GetMouseWorldPosition();
        rotating = true;
    }

    private void RotateReleased()
    {
        rotating = false;
    }

    private void AddStarted()
    {
        adding = true;
    }

    private void AddCanceled()
    {
        adding = false;
    }

    private void Cancel()
    {
        holding = false;
        ClearUnits();
        ClearGhostUnits();
    }

    private void SetTargets()
    {
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            if (selectedUnits[i].TryGetComponent<UnitBehaviour>(out var behaviour))
            {
                if (ghostUnits.Count > i)
                {
                    behaviour.SetUnitTarget(ghostUnits[i].transform, false);
                }
            }
        }
    }

    private void SetAttackTarget(Transform target)
    {
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            if (selectedUnits[i].TryGetComponent<UnitBehaviour>(out var behaviour))
            {
                behaviour.SetUnitTarget(target, true);
            }
        }
    }

    private void SetAttackStatusUnit()
    {
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            if (selectedUnits[i].TryGetComponent<UnitBehaviour>(out var behaviour))
            {
                behaviour.SetAutoAttack();
            }
        }
    }

    private GameObject AddUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, troopLayer))
        {
            GameObject parent = hit.transform.root.gameObject;
            if (!selectedUnits.Contains(parent))
            {
                ActivateUI(parent);
                selectedUnits.Add(parent);
                return parent;
            }
        }
        return null;
    }

    private async void CreateGhostUnits()
    {
        List<Unit> units = new();
        foreach (GameObject unit in selectedUnits)
        {
            if (unit.TryGetComponent<UnitBehaviour>(out var unitBehaviour))
                units.Add(unitBehaviour.unit);
        }
        ghostUnits.AddRange(await GhostUnitSpawner.Instance.MakeGhostUnit(parent, units));
    }

    private void UpdateGhostUnits()
    {
        Vector3 currentMousePosition = GetMouseWorldPosition();
        Vector3 dragDirection = (currentMousePosition - parent.position).normalized;

        for (int i = 0; i < ghostUnits.Count; i++)
        {
            float ghostDistance = Mathf.Max(Vector3.Distance(parent.position, currentMousePosition), minSpacing);
            Vector3 ghostPosition = parent.position + dragDirection * (i % 2 == 0 ? 1 : -1) * (i / 2 + 1) * ghostDistance;
            ghostUnits[i].transform.position = ghostPosition;
        }
    }

    private void RotateGhosts()
    {
        Vector3 currentMousePosition = GetMouseWorldPosition();
        Vector3 direction = currentMousePosition - rotationPos;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        foreach (GameObject ghost in ghostUnits)
        {
            Transform transform = ghost.transform;
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y + angle, transform.rotation.z);
        }
    }

    private void ClearUnits()
    {
        if (selectedUnits.Count == 0) return;
        DeactivateUI();
        selectedUnits.Clear();
    }

    private void ClearGhostUnits()
    {
        if (ghostUnits.Count == 0) return;
        GhostUnitSpawner.Instance.ClearGhostUnits();
        foreach (GameObject ghostUnit in ghostUnits)
        {
            Destroy(ghostUnit);
        }
        ghostUnits.Clear();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    public void DragSelect(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            ActivateUI(unit);
            selectedUnits.Add(unit);
        }
    }

    public bool HasUnits()
    {
        return selectedUnits.Count > 0;
    }

    private void ActivateUI(GameObject unit)
    {
        if (unit.TryGetComponent<UnitUI>(out UnitUI unitUI))
        {
            unitUI.ChangeUIState(true);
        }
    }

    private void DeactivateUI()
    {
        // Deactivate the UI for the selected units
        foreach (GameObject unit in selectedUnits)
        {
            if (unit.TryGetComponent<UnitUI>(out UnitUI unitUI))
            {
                unitUI.ChangeUIState(false);
            }
        }
    }
}