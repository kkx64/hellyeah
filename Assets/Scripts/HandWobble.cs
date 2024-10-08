using UnityEngine;

public class HandAnimation : MonoBehaviour
{
    public Transform leftHand;
    public Transform rightHand;
    public Transform player; // Reference to the player's transform
    public float maxVerticalBobAmount = 0.1f; // Maximum vertical movement
    public float maxHorizontalBobAmount = 0.05f; // Maximum forward/backward movement
    public float minBobSpeed = 5f; // Minimum bob speed when player is stationary
    public float maxBobSpeed = 15f; // Maximum bob speed when player is at max speed
    public float playerMaxSpeed = 10f; // The player's maximum speed

    private Vector3 leftHandStartPos;
    private Vector3 rightHandStartPos;
    private float bobTime;

    Rigidbody playerRb;
    PlayerMovement playerMovement;

    private void Start()
    {
        playerRb = player.GetComponent<Rigidbody>();
        playerMovement = player.GetComponent<PlayerMovement>();
        if (leftHand != null)
            leftHandStartPos = leftHand.localPosition;
        if (rightHand != null)
            rightHandStartPos = rightHand.localPosition;
    }

    private void Update()
    {
        if (
            player == null
            || (
                !playerMovement.state.Equals(PlayerMovement.MovementState.walking)
                && !playerMovement.state.Equals(PlayerMovement.MovementState.sprinting)
            )
        )
            return;

        // Calculate player's current speed
        float playerSpeed = playerRb.linearVelocity.magnitude;

        // Calculate bob speed based on player speed
        float bobSpeed = Mathf.Lerp(minBobSpeed, maxBobSpeed, playerSpeed / playerMaxSpeed);

        // Update bob time
        bobTime += Time.deltaTime * bobSpeed;

        // Calculate vertical offset
        float verticalOffset = Mathf.Sin(bobTime) * maxVerticalBobAmount;

        // Calculate horizontal (forward/backward) offset
        float horizontalOffset = Mathf.Cos(bobTime) * maxHorizontalBobAmount;

        // Apply offsets to hands
        if (leftHand != null)
        {
            leftHand.localPosition =
                leftHandStartPos + new Vector3(horizontalOffset, verticalOffset, 0f);
        }

        if (rightHand != null)
        {
            rightHand.localPosition =
                rightHandStartPos + new Vector3(-horizontalOffset, -verticalOffset, 0f);
        }

        // Optionally, add some rotation to the hands for more realism
        float rotationAmount = Mathf.Sin(bobTime) * 1f;
        if (leftHand != null)
            leftHand.localRotation = Quaternion.Euler(0f, 0f, rotationAmount);
        if (rightHand != null)
            rightHand.localRotation = Quaternion.Euler(0f, 0f, -rotationAmount);
    }
}
