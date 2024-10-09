using System.Collections;
using UnityEngine;

public class ThrowWeapon : CoreWeapon
{
    public GameObject projectilePrefab;
    public Animator animator;
    public Transform firePoint;
    public float chargeTime = 1f;
    public float projectileForce = 1000f;
    public float reloadTime = 1f;
    public float fireDelay = 0f;
    public Vector3 randomThrowAngularVelocity = new Vector3(0, 0, 0);

    private bool isReloading = false;
    private bool isReadyToFire = true;
    private float chargeStartTime;
    private bool isCharging = false;

    private void Update()
    {
        HandleFiring();
    }

    private void HandleFiring()
    {
        if (isReadyToFire && !isReloading && currentAmmo > 0)
        {
            if (Input.GetButtonDown(fireKey))
            {
                StartCharging();
            }
            else if (Input.GetButton(fireKey))
            {
                ContinueCharging();
            }
            else if (Input.GetButtonUp(fireKey))
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

        if (UseAmmo())
        {
            GameObject projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation
            );
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * projectileForce);
                rb.angularVelocity = new Vector3(
                    Random.Range(-randomThrowAngularVelocity.x, randomThrowAngularVelocity.x),
                    Random.Range(-randomThrowAngularVelocity.y, randomThrowAngularVelocity.y),
                    Random.Range(-randomThrowAngularVelocity.z, randomThrowAngularVelocity.z)
                );
            }
        }

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

    public override bool UseAmmo()
    {
        if (base.UseAmmo())
        {
            // Additional logic for using ammo can be added here if needed
            return true;
        }
        return false;
    }

    public override void RefillAmmo()
    {
        base.RefillAmmo();
        // Additional logic for refilling ammo can be added here if needed
    }
}
