using System.Collections;
using DG.Tweening;
using UnityEngine;

public class StaticEnemy : MonoBehaviour
{
    public Transform player;
    public Transform enemyTurret; // The part of the enemy that rotates
    public float detectionRange = 10f;
    public float rotationSpeed = 5f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private float nextFireTime;
    bool isTrackingPlayer = false;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (enemyTurret == null)
        {
            enemyTurret = transform;
        }
        StartCoroutine(StartRandomRotate());
        nextFireTime = Time.time;
    }

    void Update()
    {
        if (player == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            isTrackingPlayer = true;
            // Rotate towards the player
            Vector3 directionToPlayer = player.position - enemyTurret.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            enemyTurret.rotation = Quaternion.Slerp(
                enemyTurret.rotation,
                lookRotation,
                Time.deltaTime * rotationSpeed
            );

            // Check if it's time to fire
            if (Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            isTrackingPlayer = false;
        }
    }

    void Fire()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
        else
        {
            Debug.LogWarning("Projectile prefab or fire point not set on " + gameObject.name);
        }
    }

    // Optionally, add this method to visualize the detection range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    IEnumerator StartRandomRotate()
    {
        yield return new WaitForSeconds(Random.Range(2f, 4f));

        if (isTrackingPlayer)
        {
            StartCoroutine(StartRandomRotate());
            yield break;
        }

        enemyTurret
            .DOLocalRotate(
                new Vector3(Random.Range(-15, 15), Random.Range(0, 360), Random.Range(-15, 15)),
                1f
            )
            .OnComplete(() => StartCoroutine(StartRandomRotate()));
    }
}
