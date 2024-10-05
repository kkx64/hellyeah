using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 100f;
    public float sensY = 100f;
    public float smoothing = 5f;

    public float maxFovMultiplier = 1.5f;
    public float minFovMultiplier = 1f;

    public float minSpeed = 10f;
    public float maxSpeed = 40f;

    public Transform orientation;
    public Transform camHolder;

    private float xRotation;
    private float yRotation;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    private CinemachineCamera cam;
    private float originalFov;
    private float fovMultiplier = 1f; // New field to store FOV multiplier

    Rigidbody playerRb;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponent<CinemachineCamera>();
        originalFov = cam.Lens.FieldOfView;
        playerRb = orientation.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Get smooth mouse input
        Vector2 targetMouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );
        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            targetMouseDelta,
            ref currentMouseDeltaVelocity,
            smoothing * Time.deltaTime
        );

        // Apply sensitivity
        float mouseX = currentMouseDelta.x * sensX * Time.deltaTime;
        float mouseY = currentMouseDelta.y * sensY * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate cam and orientation
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        // Lerp FOV based on Rigidbody speed
        float speed = playerRb.linearVelocity.magnitude;
        float speedNormalized = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        float targetFovMultiplier = Mathf.Lerp(minFovMultiplier, maxFovMultiplier, speedNormalized);
        cam.Lens.FieldOfView = Mathf.Lerp(
            cam.Lens.FieldOfView,
            originalFov * targetFovMultiplier * fovMultiplier, // Apply the combined multiplier
            Time.deltaTime * smoothing
        );
    }

    public void DoFov(float mult)
    {
        DOTween.To(() => fovMultiplier, x => fovMultiplier = x, mult, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        DOTween.To(() => cam.Lens.Dutch, x => cam.Lens.Dutch = x, zTilt, 0.25f);
    }
}
