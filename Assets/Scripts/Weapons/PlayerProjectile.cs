using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public int damage = 20;
    public float lifetime = 5f;
    public float speed = 20f;
    public float randomizeAngularVelocity;
    public bool destroyOnHit = true;

    Rigidbody rb;

    private void Start()
    {
        Destroy(gameObject, lifetime);
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        rb.angularVelocity = Random.insideUnitSphere * randomizeAngularVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CoreEnemy enemy = collision.gameObject.GetComponent<CoreEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (destroyOnHit)
            Destroy(gameObject);
    }
}
