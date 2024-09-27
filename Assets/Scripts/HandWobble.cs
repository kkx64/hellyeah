using UnityEngine;

public class HandWobble : MonoBehaviour
{
    [Header("Wobble Settings")]
    public float baseWobbleAmount = 0.05f;
    public float maxWobbleAmount = 0.2f;
    public float wobbleSpeed = 2f;
    public float wobbleSpeedMultiplier = 1.5f;
    public float recenterSpeed = 5f;

    [Header("Movement Settings")]
    public float maxTilt = 20f;
    public float tiltSpeed = 5f;
    public float moveThreshold = 0.1f;
    public float maxSpeed = 10f; // The speed at which max wobble is reached

    [Header("Z-Position Settings")]
    public float minZPosition = -0.1f;
    public float maxZPosition = 0.1f;
    public float zPositionSpeed = 3f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float wobbleTime;
    private Vector3 previousPosition;
    private Vector2 tiltVelocity;
    private float currentZPosition;
    private float currentWobbleAmount;
    private float currentWobbleSpeed;

    private void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        previousPosition = transform.position;
        currentZPosition = startPosition.z;
        currentWobbleAmount = baseWobbleAmount;
        currentWobbleSpeed = wobbleSpeed;
    }

    private void Update()
    {
        Vector3 movementDelta = (transform.position - previousPosition) / Time.deltaTime;
        float movementMagnitude = new Vector2(movementDelta.x, movementDelta.y).magnitude;

        // Adjust wobble based on speed
        float speedFactor = Mathf.Clamp01(movementMagnitude / maxSpeed);
        currentWobbleAmount = Mathf.Lerp(baseWobbleAmount, maxWobbleAmount, speedFactor);
        currentWobbleSpeed = wobbleSpeed * (1f + speedFactor * (wobbleSpeedMultiplier - 1f));

        // Wobble effect
        wobbleTime += Time.deltaTime * currentWobbleSpeed;
        Vector3 wobble = new Vector3(
            Mathf.Sin(wobbleTime) * currentWobbleAmount,
            Mathf.Sin(wobbleTime * 0.8f) * currentWobbleAmount,
            0
        );

        // Tilt based on movement
        Vector2 tiltTarget = Vector2.zero;
        if (movementMagnitude > moveThreshold)
        {
            tiltTarget =
                new Vector2(-movementDelta.y, movementDelta.x).normalized * maxTilt * speedFactor;
        }
        tiltVelocity = Vector2.Lerp(tiltVelocity, tiltTarget, Time.deltaTime * tiltSpeed);

        // Z-position adjustment
        float zTarget = Mathf.Lerp(minZPosition, maxZPosition, speedFactor);
        currentZPosition = Mathf.Lerp(currentZPosition, zTarget, Time.deltaTime * zPositionSpeed);

        // Apply transformations
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            startPosition + wobble + Vector3.forward * currentZPosition,
            Time.deltaTime * recenterSpeed
        );
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            startRotation * Quaternion.Euler(tiltVelocity.x, tiltVelocity.y, 0),
            Time.deltaTime * recenterSpeed
        );

        previousPosition = transform.position;
    }
}
