using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DemoScene : MonoBehaviour
{
    public Slider slider;

    private void Start()
    {
        Transform camT = Camera.main.transform;
        slider.onValueChanged.AddListener((value) => SetXRotation(camT, value));
    }

    private void SetXRotation(Transform t, float angle)
    {
        t.localEulerAngles = new Vector3(angle, t.localEulerAngles.y, t.localEulerAngles.z);
    }
}
