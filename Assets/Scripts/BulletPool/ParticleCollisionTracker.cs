using UnityEngine;

public class ParticleCollisionTracker : MonoBehaviour
{
    public bool hasCollided = false;
    public Vector3 lastCollisionPoint;

    private ParticleSystem ps;
    private ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[16];

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        int count = ps.GetCollisionEvents(other, collisionEvents);
        if (count > 0)
        {
            hasCollided = true;
            lastCollisionPoint = collisionEvents[0].intersection;
        }
    }

    public void ResetCollision()
    {
        hasCollided = false;
    }
}