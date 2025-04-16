using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionBox : MonoBehaviour
{
    Camera myCam;

    [SerializeField]
    RectTransform boxVisual;

    Rect selectionBox;

    Vector2 startPosition;
    Vector2 endPosition;

    private bool holding = false, adding = false;
    InputManager inputManager;

    private void Start()
    {
        myCam = Camera.main;
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    private void OnEnable()
    {
        inputManager = InputManager.Instance;
        inputManager.HoldStarted += OnDragSelectStarted;
        inputManager.HoldReleased += OnDragSelectReleased;
        inputManager.AddPerformed += AddingStarted;
        inputManager.AddCanceled += AddingReleased;
    }

    private void OnDisable()
    {
        inputManager.HoldStarted -= OnDragSelectStarted;
        inputManager.HoldReleased -= OnDragSelectReleased;
        inputManager.AddPerformed -= AddingStarted;
        inputManager.AddCanceled -= AddingReleased;
    }

    private void Update()
    {
        if (holding)
        {
            endPosition = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }
    }

    private void OnDragSelectStarted()
    {
        if (!adding) return;
        startPosition = Input.mousePosition;
        selectionBox = new Rect();
        holding = true;
    }

    private void OnDragSelectReleased()
    {
        if (holding)
        {
            holding = false;
            SelectUnits();
            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();
        }
    }

    private void AddingStarted()
    {
        adding = true;
    }

    private void AddingReleased()
    {
        adding = false;
    }

    void DrawVisual()
    {
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;

        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        boxVisual.position = boxCenter;

        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));
        boxVisual.sizeDelta = boxSize;
    }

    void DrawSelection()
    {
        if (Input.mousePosition.x < startPosition.x)
        {
            selectionBox.xMin = Input.mousePosition.x;
            selectionBox.xMax = startPosition.x;
        }
        else
        {
            selectionBox.xMin = startPosition.x;
            selectionBox.xMax = Input.mousePosition.x;
        }

        if (Input.mousePosition.y < startPosition.y)
        {
            selectionBox.yMin = Input.mousePosition.y;
            selectionBox.yMax = startPosition.y;
        }
        else
        {
            selectionBox.yMin = startPosition.y;
            selectionBox.yMax = Input.mousePosition.y;
        }
    }

    void SelectUnits()
    {
        foreach (var unit in UnitManager.Instance.GetUnits())
        {
            if (selectionBox.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
            {
                UnitSelectionManager.Instance.DragSelect(unit);
            }
        }
    }
}