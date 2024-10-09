using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : CoreEnemy
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround,
        whatIsPlayer;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public float sightRange,
        attackRange;
    public bool playerInSightRange,
        playerInAttackRange;

    Rigidbody rb;

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerMovement>().transform;
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
            Patroling();
        if (playerInSightRange && !playerInAttackRange)
            ChasePlayer();
        if (playerInAttackRange && playerInSightRange)
            AttackPlayer();
    }

    private void Patroling()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            if (!walkPointSet)
                SearchWalkPoint();

            if (walkPointSet)
                agent.SetDestination(walkPoint);

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            //Walkpoint reached
            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        if (agent.enabled && agent.isOnNavMesh)
            agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            //Make sure enemy doesn't move
            agent.SetDestination(transform.position);

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Keep only the horizontal direction
            transform.rotation = Quaternion.LookRotation(direction);

            if (!alreadyAttacked)
            {
                ///Attack code here
                Instantiate(
                    projectile,
                    transform.position,
                    Quaternion.LookRotation(player.position - transform.position)
                );

                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
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

    public void ApplyRigidbodyForce(Vector3 force)
    {
        // Disable the NavMeshAgent to allow physics interactions
        agent.enabled = false;

        // Apply the force to the Rigidbody
        rb.AddForce(force, ForceMode.Impulse);

        // Start coroutine to re-enable the NavMeshAgent after ensuring the enemy is grounded
        StartCoroutine(WaitUntilGrounded());
    }

    private IEnumerator WaitUntilGrounded()
    {
        // Wait for at least 2 seconds
        yield return new WaitForSeconds(2f);

        // Wait until the enemy is grounded
        while (!IsGrounded())
        {
            yield return null;
        }

        // Re-enable the NavMeshAgent
        agent.enabled = true;
    }

    private bool IsGrounded()
    {
        // Check if the enemy is grounded
        return Physics.Raycast(transform.position, Vector3.down, agent.height + 0.1f, whatIsGround);
    }
}
