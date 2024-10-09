using UnityEngine;

public class FlyingEnemy : CoreEnemy
{
    public Transform player;
    public float moveSpeed = 5f;
    public float rotationSpeed = 3f;

    public LayerMask whatIsPlayer;

    // Patrolling
    public Vector3 flyPoint;
    bool flyPointSet;
    public float flyPointRange = 10f;

    // Attacking
    public float timeBetweenAttacks = 2f;
    bool alreadyAttacked;
    public GameObject projectile;

    // States
    public float sightRange = 15f;
    public float attackRange = 8f;
    public bool playerInSightRange;
    public bool playerInAttackRange;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
            Patrolling();
        else if (playerInSightRange && !playerInAttackRange)
            ChasePlayer();
        else if (playerInAttackRange && playerInSightRange)
            AttackPlayer();
    }

    private void Patrolling()
    {
        if (!flyPointSet)
            SearchFlyPoint();

        if (flyPointSet)
        {
            Vector3 direction = (flyPoint - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * rotationSpeed
            );
            transform.position = Vector3.MoveTowards(
                transform.position,
                flyPoint,
                moveSpeed * Time.deltaTime
            );
        }

        Vector3 distanceToFlyPoint = transform.position - flyPoint;

        // Fly point reached
        if (distanceToFlyPoint.magnitude < 1f)
            flyPointSet = false;
    }

    private void SearchFlyPoint()
    {
        // Calculate random point in range (including Y axis for flying)
        float randomX = Random.Range(-flyPointRange, flyPointRange);
        float randomY = Random.Range(-flyPointRange / 2, flyPointRange / 2); // Limit vertical range
        float randomZ = Random.Range(-flyPointRange, flyPointRange);

        flyPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y + randomY,
            transform.position.z + randomZ
        );

        flyPointSet = true;
    }

    private void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * rotationSpeed
        );
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );
    }

    private void AttackPlayer()
    {
        // Make sure enemy doesn't move
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * rotationSpeed
        );

        if (!alreadyAttacked)
        {
            // Attack code here
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity)
                .GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
