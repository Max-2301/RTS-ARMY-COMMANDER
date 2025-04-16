using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;

public class UnitUI : MonoBehaviour
{
    private GameObject ui;

    private GameObject uiExtra;
    public Unit unit { set; private get; }

    [SerializeField] private string uiPrefabPath;
    private LineRenderer lineRenderer;
    private NavMeshAgent agent;

    [SerializeField] private float uiHeight = 10f;

    private List<Image> attackPanels;
    [SerializeField] private Color color, attackColor;
    private void Start()
    {
        DataContainer data = (DataContainer)Resources.Load("Prefabs/UI/UIData");
        ui = data.UnitUIObject;

        Transform formation = transform.GetChild(0);
        ui = Instantiate(ui, formation.position, formation.rotation, formation);
        ui.transform.SetLocalPositionAndRotation(new Vector3(0, uiHeight, 0), Quaternion.Euler(90, 0, 0));

        color = data.unitUIColor;
        attackColor = data.unitUIAttackColor;
        UIData uiData = ui.GetComponent<UIData>();
        attackPanels = uiData.attackPanels;
        ChangeUIAttackStatus(true);


        uiExtra = ui.transform.GetChild(0).gameObject;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")) { color = Color.green };

        agent = GetComponent<UnitBehaviour>().GetAgent();

        ChangeUIState(false);
    }

    public void ChangeUIState(bool enabled)
    {
        uiExtra.SetActive(enabled);
        lineRenderer.enabled = enabled;
    }

    public void ChangeUIAttackStatus(bool enabled)
    {
        foreach (Image panel in attackPanels)
        {
            if (enabled)
                panel.color = attackColor;
            
            else
                panel.color = color;
        }
    }

    public void UpdateUI()
    {
        if (agent == null || lineRenderer == null)
            return;

        NavMeshPath path = agent.path;
        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPositions(path.corners);
    }
}