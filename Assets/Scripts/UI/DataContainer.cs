using UnityEngine;

[CreateAssetMenu(fileName = "DataContainer", menuName = "Scriptable Objects/DataContainer")]
public class DataContainer : ScriptableObject
{
    public GameObject UnitUIObject;
    public Color unitUIAttackColor, unitUIColor;
}
