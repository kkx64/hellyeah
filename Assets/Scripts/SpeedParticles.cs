using UnityEngine;

public class ParticleEffectFromVelocityDirection : MonoBehaviour
{
    public ParticleSystem windParticleSystem; // Assign your particle system in the inspector
    public Transform player; // Assign your player or object whose speed you're tracking
    public float maxSpeed = 50f; // The maximum speed to base the intensification on
    public float minSpeed = 0f; // The minimum speed

    public float distanceInFrontOfPlayer = 10f; // Distance the particle system will be in front of the player
    public float smoothingSpeed = 5f; // How smoothly the particle system moves to the new position

    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;

    private Vector3 lastPosition; // To track player's position from the previous frame
    private float currentSpeed; // To store calculated speed
    private Vector3 movementDirection; // To store player's movement direction

    void Start()
    {
        // Get references to the particle system modules
        mainModule = windParticleSystem.main;
        emissionModule = windParticleSystem.emission;

        // Initialize lastPosition with the player's initial position
        lastPosition = player.position;
    }

    void Update()
    {
        // Calculate the player's speed based on position delta
        Vector3 displacement = player.position - lastPosition;
        currentSpeed = displacement.magnitude / Time.deltaTime;

        // Calculate movement direction (normalized vector)
        movementDirection = displacement.normalized;

        // Update lastPosition for the next frame
        lastPosition = player.position;

        // Normalize speed value between 0 and 1
        float normalizedSpeed = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);

        // Adjust particle properties based on speed
        AdjustParticleSystem(normalizedSpeed);

        // Place the particle system in the direction of movement and make it face the player
        MoveAndAlignParticles();
    }

    void AdjustParticleSystem(float normalizedSpeed)
    {
        // Adjust particle speed based on movement speed
        mainModule.startSpeed = Mathf.Lerp(10f, 30f, normalizedSpeed);

        // Adjust emission rate based on movement speed
        emissionModule.rateOverTime = Mathf.Lerp(10f, 100f, normalizedSpeed);

        // Optional: Adjust lifetime or other properties
        mainModule.startLifetime = Mathf.Lerp(0.5f, 1f, normalizedSpeed);
    }

    void MoveAndAlignParticles()
    {
        if (movementDirection != Vector3.zero)
        {
            // Place the particle system in front of the player, relative to movement direction
            Vector3 targetPosition = player.position + movementDirection * distanceInFrontOfPlayer;

            // Smoothly move the particle system to the target position
            windParticleSystem.transform.position = Vector3.Lerp(
                windParticleSystem.transform.position,
                targetPosition,
                Time.deltaTime * smoothingSpeed
            );

            // Make the particle system face the player
            windParticleSystem.transform.LookAt(player);
        }
    }
}
