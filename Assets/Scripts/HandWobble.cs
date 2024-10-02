using UnityEngine;

public class HandRunningMotion : MonoBehaviour
{
    [Header("Running Motion Settings")]
    public float baseAmplitude = 0.1f;
    public float maxAmplitude = 0.3f;
    public float baseFrequency = 5f;
    public float maxFrequency = 10f;
    public float maxSpeed = 10f; // The speed at which max amplitude and frequency are reached
    public Vector3 motionDirection = Vector3.forward; // Direction of the running motion
    public float offset = 0f; // Offset in radians, use PI for opposite hand

    [Header("Position Settings")]
    public float recenterSpeed = 5f;

    private Vector3 startPosition;
    private float motionTime;
    private Vector3 previousPosition;
    private float currentAmplitude;
    private float currentFrequency;

    private void Start()
    {
        startPosition = transform.localPosition;
        previousPosition = transform.position;
        currentAmplitude = baseAmplitude;
        currentFrequency = baseFrequency;
    }

    private void Update()
    {
        Vector3 movementDelta = (transform.position - previousPosition) / Time.deltaTime;
        float movementMagnitude = movementDelta.magnitude;

        // Adjust motion based on speed
        float speedFactor = Mathf.Clamp01(movementMagnitude / maxSpeed);
        currentAmplitude = Mathf.Lerp(baseAmplitude, maxAmplitude, speedFactor);
        currentFrequency = Mathf.Lerp(baseFrequency, maxFrequency, speedFactor);

        // Calculate running motion
        motionTime += Time.deltaTime * currentFrequency;
        float motionValue = Mathf.Sin(motionTime + offset) * currentAmplitude;
        Vector3 motion = motionDirection.normalized * motionValue;

        // Apply transformations
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            startPosition + motion,
            Time.deltaTime * recenterSpeed
        );

        previousPosition = transform.position;
    }
}
