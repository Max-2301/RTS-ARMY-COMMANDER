using Unity.AI.Navigation;
using UnityEngine;

public class SurfaceIDInfo : MonoBehaviour
{
    private static SurfaceIDInfo instance;
    public static SurfaceIDInfo Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SurfaceIDInfo");
                instance = go.AddComponent<SurfaceIDInfo>();
            }
            return instance;
        }
    }

    [SerializeField] private NavMeshSurface troopSurface;
    [SerializeField] private NavMeshSurface unitSurface;

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

    public int GetTroopSurfaceID()
    {
        return troopSurface.agentTypeID;
    }

    public int GetUnitSurfaceID()
    {
        return unitSurface.agentTypeID;
    }
}