using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GhostUnitSpawner : MonoBehaviour
{
    private static GhostUnitSpawner instance;
    public static GhostUnitSpawner Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GhostUnitManager");
                instance = go.AddComponent<GhostUnitSpawner>();
            }
            return instance;
        }
    }

    [SerializeField] private GameObject ghostParent;
    private List<GameObject> ghostUnits = new();
    [SerializeField] private Material ghostMaterial;

    private Coroutine ghostCoroutine;
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
    public async Task<List<GameObject>> MakeGhostUnit(Transform parent, List<Unit> units)
    {
        ClearGhostUnits();
        ghostCoroutine = StartCoroutine(GhostUnitCoroutine(parent, units));
        await WaitUntilCoroutineEnds();
        return ghostUnits;
    }

    private IEnumerator GhostUnitCoroutine(Transform parent, List<Unit> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            GameObject spawnedGhost = Instantiate(ghostParent, parent.position, parent.rotation, parent);
            ghostUnits.Add(spawnedGhost);
            UnitSetUp ghostSetUp = spawnedGhost.GetComponent<UnitSetUp>();
            ghostSetUp.unit = units[i];
            yield return new WaitUntil(() => ghostSetUp.didStart);
            foreach (Renderer renderer in spawnedGhost.GetComponentsInChildren<Renderer>())
            {
                renderer.material = ghostMaterial;
            }
        }
        ghostCoroutine = null;
    }

    private async Task WaitUntilCoroutineEnds()
    {
        while (ghostCoroutine != null)
        {
            await Task.Yield();
        }
    }

    public void ClearGhostUnits()
    {
        foreach (GameObject ghostUnit in ghostUnits)
        {
            Destroy(ghostUnit);
        }
        ghostUnits.Clear();
    }
}
