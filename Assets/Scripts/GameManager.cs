using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> disableList = new(), enableList = new();

    private void Start()
    {
        foreach (GameObject obj in enableList)
        {
            obj.SetActive(false);
        }
    }
    public void StartBattle()
    {
        foreach (GameObject obj in disableList)
        {
            obj.SetActive(false);
        }
        foreach (GameObject obj in enableList)
        {
            obj.SetActive(true);
        }
    }
}
