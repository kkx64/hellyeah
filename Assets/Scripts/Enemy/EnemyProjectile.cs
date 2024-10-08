using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 20;
    public float speed = 20f;
    public float lifetime = 2f;

    Rigidbody rb;

    private void Start()
    {
        Destroy(gameObject, lifetime);
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
