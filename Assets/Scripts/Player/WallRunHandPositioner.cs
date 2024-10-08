using UnityEngine;

public class WallRunHandPositioner : MonoBehaviour
{
    public float maxCheckDistance = 0.5f;
    public LayerMask wallLayer;
    public float maxDistance = 0.1f; // Maximum distance from the wall
    public Vector3 sphereCenterOffset; // Offset for the center of the sphere
    private Vector3 originalLocalPosition;
    PlayerMovement playerMovement;
    public float lerpSpeed = 5f; // Speed of the lerp

    private void Start()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        originalLocalPosition = transform.localPosition; // Store the original local position
    }

    private void LateUpdate()
    {
        if (playerMovement.wallrunning || playerMovement.state == PlayerMovement.MovementState.air)
        {
            Vector3 sphereCenter =
                transform.position + transform.TransformDirection(sphereCenterOffset);
            Collider[] hitColliders = Physics.OverlapSphere(
                sphereCenter,
                maxCheckDistance,
                wallLayer
            );
            if (hitColliders.Length > 0)
            {
                Vector3 hitPoint = hitColliders[0].ClosestPoint(sphereCenter);
                Vector3 direction = (hitPoint - sphereCenter).normalized;
                Vector3 targetPosition = hitPoint + direction * maxDistance;
                Vector3 localTargetPosition = transform.parent.InverseTransformPoint(
                    targetPosition
                );

                // Limit the distance by the max distance from original local position
                if (Vector3.Distance(originalLocalPosition, localTargetPosition) > maxDistance)
                {
                    localTargetPosition =
                        originalLocalPosition
                        + (localTargetPosition - originalLocalPosition).normalized * maxDistance;
                }

                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    localTargetPosition,
                    Time.deltaTime * lerpSpeed
                );
            }
            else
            {
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    originalLocalPosition,
                    Time.deltaTime * lerpSpeed
                ); // Lerp back to original local position if no wall is found
            }
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalLocalPosition,
                Time.deltaTime * lerpSpeed
            ); // Lerp back to original local position if not wallrunning or in air
        }
    }
}
