using UnityEngine;

public class UnitSwitcher : MonoBehaviour
{
    private GridPlacementSystem gridPlacementSystem;
    private void Start()
    {
        gridPlacementSystem = GridPlacementSystem.Instance;
    }
    public void SwitchUnit(Unit unit)
    {
        gridPlacementSystem.SwitchUnit(unit);
    }
}