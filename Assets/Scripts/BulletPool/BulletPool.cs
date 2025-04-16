using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletType
{
    public GameObject bulletPrefab;
    public int poolSize;
}

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] private GameObject bulletParent;

    [SerializeField] private List<BulletType> bulletTypes = new();
    private Dictionary<GameObject, List<GameObject>> bulletPools = new Dictionary<GameObject, List<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        foreach (var bulletType in bulletTypes)
        {
            List<GameObject> bulletList = new List<GameObject>();

            for (int i = 0; i < bulletType.poolSize; i++)
            {
                GameObject bullet = Instantiate(bulletType.bulletPrefab, bulletParent.transform);
                bullet.SetActive(false);
                bulletList.Add(bullet);
            }

            bulletPools.Add(bulletType.bulletPrefab, bulletList);
        }
    }

    public GameObject GetBullet(GameObject bulletPrefab)
    {
        if (bulletPools.ContainsKey(bulletPrefab))
        {
            foreach (GameObject bullet in bulletPools[bulletPrefab])
            {
                if (!bullet.activeInHierarchy)
                {
                    bullet.SetActive(true);
                    return bullet;
                }
            }

            // If no inactive bullets are available, instantiate a new one
            GameObject newBullet = Instantiate(bulletPrefab);
            bulletPools[bulletPrefab].Add(newBullet);
            return newBullet;
        }
        else Debug.LogError("Bullet prefab not found in pool: " + bulletPrefab.name);

        return null;
    }

    public void ReturnBullet(GameObject bullet, GameObject bulletPrefab)
    {
        if (bulletPools.ContainsKey(bulletPrefab))
        {
            bullet.SetActive(false);
            bullet.transform.position = bulletParent.transform.position;
            bullet.transform.rotation = bulletParent.transform.rotation;
        }
    }
}