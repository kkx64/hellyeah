using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float swingSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air,
        grappling,
        swinging,
    }

    public bool sliding;
    public bool crouching;
    public bool wallrunning;

    public bool freeze;

    public bool activeGrapple;
    public bool swinging;

    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_mode;

    PlayerCam playerCam;
    PlayerStamina playerStamina;

    [Header("State Multipliers")]
    public float walkingMultiplier = 1f;
    public float sprintingMultiplier = 1f;
    public float wallrunningMultiplier = 1f;
    public float crouchingMultiplier = 1f;
    public float slidingMultiplier = 1f;
    public float airMultiplierState = 1f;
    public float grapplingMultiplier = 1f;
    public float swingingMultiplier = 1f;

    [Header("Attacks")]
    public float slideKickForce = 10f;
    public int minSlideKickDamage = 10;
    public int maxSlideKickDamage = 20;
    public float knockbackPower = 2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
        playerCam = FindAnyObjectByType<PlayerCam>();
        playerStamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            playerHeight * 0.5f + 0.2f,
            whatIsGround
        );

        MyInput();
        SpeedControl();
        StateHandler();
        TextStuff();

        // handle drag
        if (grounded && !activeGrapple)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
        {
            transform.localScale = new Vector3(
                transform.localScale.x,
                crouchYScale,
                transform.localScale.z
            );
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            crouching = true;
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(
                transform.localScale.x,
                startYScale,
                transform.localScale.z
            );

            crouching = false;
        }
    }

    private void StateHandler()
    {
        if (grounded && state == MovementState.air)
        {
            playerCam.DoShake(rb.linearVelocity.magnitude / 5f, 0.2f);
        }
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        // Mode - Swinging
        else if (swinging)
        {
            state = MovementState.swinging;
            moveSpeed = swingSpeed;
        }
        // Mode - Grappling
        else if (activeGrapple)
        {
            state = MovementState.grappling;
            moveSpeed = sprintSpeed;
        }
        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            // increase speed by one every second
            if (OnSlope() && rb.linearVelocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else if (grounded)
                desiredMoveSpeed = sprintSpeed;
        }
        // Mode - Crouching
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        // Mode - Sprinting
        else if (
            grounded
            && Input.GetKey(sprintKey)
            && !playerStamina.IsRegenerating()
            && playerStamina.HasStamina(0.1f)
        )
        {
            if (playerStamina.UseStamina(Time.deltaTime * playerStamina.staminaDepletionRate))
            {
                state = MovementState.sprinting;
                desiredMoveSpeed = sprintSpeed;
            }
        }
        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // Mode - Air
        else
        {
            moveSpeed = 100f;
            state = MovementState.air;
        }

        // check if desired move speed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());

            print("Lerp Started!");
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time +=
                    Time.deltaTime
                    * speedIncreaseMultiplier
                    * slopeIncreaseMultiplier
                    * slopeAngleIncrease;
            }
            else
            {
                var multiplier = 1f;
                switch (state)
                {
                    case MovementState.walking:
                        multiplier = walkingMultiplier;
                        break;
                    case MovementState.sprinting:
                        multiplier = sprintingMultiplier;
                        break;
                    case MovementState.wallrunning:
                        multiplier = wallrunningMultiplier;
                        break;
                    case MovementState.crouching:
                        multiplier = crouchingMultiplier;
                        break;
                    case MovementState.sliding:
                        multiplier = slidingMultiplier;
                        break;
                    case MovementState.air:
                        multiplier = airMultiplierState;
                        break;
                    case MovementState.grappling:
                        multiplier = grapplingMultiplier;
                        break;
                    case MovementState.swinging:
                        multiplier = swingingMultiplier;
                        break;
                }

                time += Time.deltaTime * speedIncreaseMultiplier * multiplier;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (activeGrapple)
            return;
        if (swinging)
            return;
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // in air
        else if (!grounded)
            rb.AddForce(
                moveDirection.normalized * moveSpeed * 10f * airMultiplier,
                ForceMode.Force
            );

        // turn gravity off while on slope
        if (!wallrunning)
            rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrapple || swinging)
            return;
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (
            Physics.Raycast(
                transform.position,
                Vector3.down,
                out slopeHit,
                playerHeight * 0.5f + 0.3f,
                whatIsGround
            )
        )
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private bool enableMovementOnNextTouch;

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.linearVelocity = velocityToSet;
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    public Vector3 CalculateJumpVelocity(
        Vector3 startPoint,
        Vector3 endPoint,
        float trajectoryHeight
    )
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(
            endPoint.x - startPoint.x,
            0f,
            endPoint.z - startPoint.z
        );

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ =
            displacementXZ
            / (
                Mathf.Sqrt(-2 * trajectoryHeight / gravity)
                + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity)
            );

        return velocityXZ + velocityY;
    }

    private void TextStuff()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (text_speed != null)
        {
            if (OnSlope())
                text_speed.SetText("Speed: " + Round(rb.linearVelocity.magnitude, 1));
            else
                text_speed.SetText("Speed: " + Round(flatVel.magnitude, 1));
        }
        if (text_mode != null)
            text_mode.SetText(state.ToString());
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!grounded && sliding)
        {
            var enemy = collision.gameObject.GetComponent<CoreEnemy>();
            if (enemy != null)
            {
                int slideKickDamage = Mathf.RoundToInt(
                    Mathf.Lerp(
                        minSlideKickDamage,
                        maxSlideKickDamage,
                        rb.linearVelocity.magnitude / slideSpeed
                    )
                );
                enemy.TakeDamage(slideKickDamage);
                var enemyDirection = (enemy.transform.position - transform.position).normalized;
                if (enemy.GetType() == typeof(GroundEnemy))
                    // Apply force if the enemy is a GroundEnemy
                    ((GroundEnemy)enemy).ApplyRigidbodyForce(enemyDirection * slideKickForce);
                // Bounce player upwards and back
                Vector3 bounceDirection = -enemyDirection;
                bounceDirection.y = 0.5f;
                rb.AddForce(-rb.linearVelocity, ForceMode.Impulse);
                rb.AddForce(bounceDirection * knockbackPower, ForceMode.Impulse);
            }
        }
    }
}
