using System.Collections;
using UnityEngine;

public class BareHandsWeapon : CoreWeapon
{
    public float punchForce = 10f;
    public float punchCooldown = 0.5f;
    public float punchRange = 2f;
    public float punchRadius = 1f;
    public int punchDamage = 10;
    public LayerMask punchableLayers;
    private bool canPunch = true;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetButtonDown(fireKey))
        {
            if (canPunch)
            {
                StartCoroutine(Punch());
            }
        }
    }

    IEnumerator Punch()
    {
        canPunch = false;
        animator.SetTrigger("Punch");

        Vector3 punchOrigin = transform.position + transform.forward * punchRange / 2f;
        RaycastHit[] hits = Physics.SphereCastAll(
            punchOrigin,
            punchRadius,
            transform.forward,
            punchRange,
            punchableLayers
        );

        foreach (RaycastHit hit in hits)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (rb.position - transform.position).normalized;
                rb.AddForce(direction * punchForce, ForceMode.Impulse);

                var groundEnemy = hit.collider.GetComponent<GroundEnemy>();
                if (groundEnemy != null)
                {
                    groundEnemy.ApplyRigidbodyForce(direction * punchForce);
                }
            }

            CoreEnemy enemy = hit.collider.GetComponent<CoreEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(punchDamage);
            }
        }

        yield return new WaitForSeconds(punchCooldown);
        canPunch = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 punchOrigin = transform.position + transform.forward * punchRange / 2f;
        Gizmos.DrawWireSphere(punchOrigin, punchRadius);
    }
}
