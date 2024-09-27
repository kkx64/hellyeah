using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Animator animator;
    public Transform firePoint;
    public ParticleSystem smokeParticles;
    public float chargeTime = 1f; // Time needed to hold before fully charged
    public float projectileForce = 1000f;

    private bool isReloading = false;
    private bool isReadyToFire = true;
    private float chargeStartTime;
    private bool isCharging = false;

    private void Update()
    {
        HandleFiring();
        HandleReloading();
    }

    private void HandleFiring()
    {
        if (isReadyToFire && !isReloading)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                StartCharging();
            }
            else if (Input.GetButton("Fire1"))
            {
                ContinueCharging();
            }
            else if (Input.GetButtonUp("Fire1"))
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
            animator.SetBool("HoldingDownFire", false);
            animator.SetBool("FullyCharged", false);
            Fire(holdTime >= chargeTime);
            isCharging = false;
        }
    }

    private void HandleReloading()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReadyToFire && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    private void Fire(bool fullyCharged)
    {
        if (isReloading || !isReadyToFire)
            return;

        animator.SetTrigger("Fire");
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        float force = fullyCharged ? projectileForce * 1.5f : projectileForce;
        rb.AddForce(firePoint.forward * force);

        smokeParticles.Stop();
        isReadyToFire = false;
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        animator.SetBool("Reloading", true);
        smokeParticles.Play();

        yield return new WaitForSeconds(1.5f);

        animator.SetBool("Reloading", false);
        isReloading = false;
        isReadyToFire = true;
    }
}
