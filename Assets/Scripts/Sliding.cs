using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;
    public float scaleLerpTime = 0.3f;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    [Header("Cooldown")]
    public float slideCooldown = 2f; // Cooldown duration in seconds
    private float cooldownTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (
            Input.GetKeyDown(slideKey)
            && (horizontalInput != 0 || verticalInput != 0)
            && cooldownTimer <= 0
        )
        {
            StartSlide();
        }

        if (Input.GetKeyUp(slideKey) && pm.sliding)
        {
            StopSlide();
        }
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
        {
            SlidingMovement();
        }
    }

    private void StartSlide()
    {
        if (pm.wallrunning || pm.swinging || pm.activeGrapple)
        {
            return;
        }

        pm.sliding = true;

        StartCoroutine(LerpScale(playerObj.localScale.y, slideYScale, 0.2f));
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection =
            orientation.forward * verticalInput + orientation.right * horizontalInput;

        // sliding normal
        if (!pm.OnSlope())
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        // sliding down a slope
        else if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        pm.sliding = false;
        cooldownTimer = slideCooldown; // Start cooldown

        StartCoroutine(LerpScale(playerObj.localScale.y, startYScale, scaleLerpTime));
    }

    private IEnumerator LerpScale(float startScale, float endScale, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            float newScale = Mathf.Lerp(startScale, endScale, time / duration);
            playerObj.localScale = new Vector3(
                playerObj.localScale.x,
                newScale,
                playerObj.localScale.z
            );
            time += Time.deltaTime;
            yield return null;
        }
        playerObj.localScale = new Vector3(
            playerObj.localScale.x,
            endScale,
            playerObj.localScale.z
        );
    }
}
