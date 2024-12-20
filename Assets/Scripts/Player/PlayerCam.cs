using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 100f;
    public float sensY = 100f;
    public float lookSmoothing = 5f;
    public float fovSmoothing = 5f;

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

    public NoiseSettings noiseSettings;

    Rigidbody playerRb;
    PlayerMovement playerMovement;
    CinemachineBasicMultiChannelPerlin noise;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponent<CinemachineCamera>();
        originalFov = cam.Lens.FieldOfView;
        playerRb = orientation.GetComponent<Rigidbody>();
        playerMovement = orientation.GetComponent<PlayerMovement>();
        noise =
            cam.GetCinemachineComponent(CinemachineCore.Stage.Noise)
            as CinemachineBasicMultiChannelPerlin;
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
            lookSmoothing * Time.deltaTime
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
            Time.deltaTime * fovSmoothing
        );

        if (
            playerMovement.state != PlayerMovement.MovementState.wallrunning
            && playerMovement.state != PlayerMovement.MovementState.air
        )
        {
            // Add lean when turning and running left and right
            float leanAmount = Mathf.Lerp(0, 10, speedNormalized); // Adjust the lean amount as needed
            if (Input.GetKey(KeyCode.A))
            {
                DoTilt(leanAmount, 0.2f);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                DoTilt(-leanAmount, 0.2f);
            }
            else
            {
                DoTilt(0, 0.2f);
            }
        }

        if (noise != null)
        {
            float shakeIntensity = Mathf.Lerp(0, 1, speedNormalized); // Adjust the range as needed
            if (
                playerMovement.state == PlayerMovement.MovementState.sprinting
                || playerMovement.state == PlayerMovement.MovementState.walking
            )
            {
                noiseSettings.PositionNoise[0].Y.Amplitude = shakeIntensity;
                noiseSettings.PositionNoise[0].X.Amplitude = shakeIntensity;
            }
            else
            {
                noiseSettings.PositionNoise[0].Y.Amplitude = 0;
                noiseSettings.PositionNoise[0].X.Amplitude = 0;
            }

            noise.AmplitudeGain = shakeIntensity;
            noise.FrequencyGain = shakeIntensity * 4;
        }
    }

    public void DoFov(float mult)
    {
        DOTween.To(() => fovMultiplier, x => fovMultiplier = x, mult, 0.25f);
    }

    public void DoTilt(float zTilt, float tiltSpeed = 0.25f)
    {
        DOTween.To(() => cam.Lens.Dutch, x => cam.Lens.Dutch = x, zTilt, tiltSpeed);
    }

    public void SetNewRotation(float newRotX, float newRotY)
    {
        xRotation = newRotX;
        yRotation = newRotY;
    }

    public void DoShake(float intensity, float duration)
    {
        var perlin =
            cam.GetCinemachineComponent(CinemachineCore.Stage.Noise)
            as CinemachineBasicMultiChannelPerlin;
        perlin.AmplitudeGain = intensity;
        perlin.FrequencyGain = intensity * 4;

        // Reset shake after duration
        DOTween.To(() => perlin.AmplitudeGain, x => perlin.AmplitudeGain = x, 0, duration);
        DOTween.To(() => perlin.FrequencyGain, x => perlin.FrequencyGain = x, 0, duration);
    }
}
