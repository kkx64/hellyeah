using System.Collections;
using UnityEngine;

public class ThrowWeapon : CoreWeapon
{
    public GameObject projectilePrefab;
    public Animator animator;
    public Transform firePoint;
    public float chargeTime = 1f; // Time needed to hold before fully charged
    public float projectileForce = 1000f;
    public float reloadTime = 1f;

    private bool isReloading = false;
    private bool isReadyToFire = true;
    private float chargeStartTime;
    private bool isCharging = false;

    public float fireDelay = 0f;

    public Vector3 randomThrowAngularVelocity = new Vector3(0, 0, 0);

    private void Update()
    {
        HandleFiring();
    }

    private void HandleFiring()
    {
        if (isReadyToFire && !isReloading)
        {
            if (Input.GetButtonDown(fireKey.ToString()))
            {
                StartCharging();
            }
            else if (Input.GetButton(fireKey.ToString()))
            {
                ContinueCharging();
            }
            else if (Input.GetButtonUp(fireKey.ToString()))
            {
                ReleaseFire();
            }
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        animator.SetBool("HoldingDownFire", true);
    }

    private void ContinueCharging()
    {
        if (isCharging)
        {
            float holdTime = Time.time - chargeStartTime;
            if (holdTime >= chargeTime)
            {
                animator.SetBool("FullyCharged", true);
            }
        }
    }

    private void ReleaseFire()
    {
        if (isCharging)
        {
            float holdTime = Time.time - chargeStartTime;
            if (isReloading || !isReadyToFire)
                return;
            animator.SetBool("HoldingDownFire", false);
            animator.SetBool("FullyCharged", false);
            StartCoroutine(Fire(holdTime >= chargeTime));
            isCharging = false;
        }
    }

    private IEnumerator Fire(bool fullyCharged)
    {
        isReadyToFire = false;
        animator.SetTrigger("Fire");
        yield return new WaitForSeconds(fireDelay);
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        float force = fullyCharged ? projectileForce * 1.5f : projectileForce;
        rb.angularVelocity = new Vector3(
            Random.Range(-randomThrowAngularVelocity.x, randomThrowAngularVelocity.x),
            Random.Range(-randomThrowAngularVelocity.y, randomThrowAngularVelocity.y),
            Random.Range(-randomThrowAngularVelocity.z, randomThrowAngularVelocity.z)
        );
        rb.AddForce(firePoint.forward * force);

        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        yield return new WaitForSeconds(0.5f);
        isReloading = true;
        animator.SetBool("Reloading", true);

        yield return new WaitForSeconds(reloadTime);

        animator.SetBool("Reloading", false);
        isReloading = false;
        isReadyToFire = true;
    }
}
