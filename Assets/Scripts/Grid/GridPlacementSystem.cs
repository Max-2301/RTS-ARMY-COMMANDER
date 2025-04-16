using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridPlacementSystem : MonoBehaviour
{
    private static GridPlacementSystem instance;
    public static GridPlacementSystem Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GridPlacementSystem");
                instance = go.AddComponent<GridPlacementSystem>();
            }
            return instance;
        }
    }

    [SerializeField] private LayerMask gridPlaceLayer;
    [SerializeField] private GameObject placeObject;
    [SerializeField] private Unit unit;
    private GridSystem currentGrid;
    [SerializeField] private int points = 1000;

    [SerializeField] private Material ghostMaterial;
    private Color originalColor;

    private InputManager inputManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SwitchUnit(unit);
        originalColor = ghostMaterial.color;
    }

    private void OnEnable()
    {
        inputManager = InputManager.Instance;
        inputManager.SelectPerformed += OnSelectPerformed;
        inputManager.CancelPerformed += OnCancelPerformed;
    }

    /// <summary>
    /// Reset the material color to it's original color for next start
    /// </summary>
    private void OnDisable()
    {
        inputManager.SelectPerformed -= OnSelectPerformed;
        inputManager.CancelPerformed -= OnCancelPerformed;

        ghostMaterial.color = originalColor;
    }

    private void Update()
    {
        CheckMousePositionOnGrid();
    }

    private void CheckMousePositionOnGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 1000);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, gridPlaceLayer))
        {
            currentGrid = hit.collider.gameObject.GetComponent<GridSystem>();
            Transform pos = currentGrid.GetGridPoint(hit.point);
            PlacePiece(pos);
        }
        else
        {
            currentGrid = null;
            PlacePiece(transform);
        }
    }

    private void PlacePiece(Transform pos)
    {
        if (placeObject != null && placeObject.transform != pos) placeObject.transform.position = pos.position;
    }

    private void OnSelectPerformed()
    {
        if (currentGrid != null)
        {
            int cost = currentGrid.SpawnUnitAndGetCost(placeObject.transform, unit, points);
            points -= cost;
            CheckCost();
        }
    }

    private void OnCancelPerformed()
    {
        if (currentGrid != null)
        {
            int cost = currentGrid.SpawnUnitAndGetCost(placeObject.transform, null, points);
            points -= cost;
            CheckCost();
        }
    }

    public async void SwitchUnit(Unit unit)
    {
        this.unit = unit;
        await GhostUnitSpawner.Instance.MakeGhostUnit(placeObject.transform, new List<Unit> { unit });
    }

    private void CheckCost()
    {
        if (unit.cost > points) ghostMaterial.color = Color.red;
        else ghostMaterial.color = originalColor;
    }
}