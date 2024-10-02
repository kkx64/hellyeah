using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 100f;
    public float sensY = 100f;
    public float smoothing = 5f;

    public Transform orientation;
    public Transform camHolder;

    private float xRotation;
    private float yRotation;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    private CinemachineCamera cam;
    private float originalFov;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponent<CinemachineCamera>();
        originalFov = cam.Lens.FieldOfView;
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
    }

    public void DoFov(float mult)
    {
        DOTween.To(
            () => cam.Lens.FieldOfView,
            x => cam.Lens.FieldOfView = x,
            originalFov * mult,
            0.25f
        );
    }

    public void DoTilt(float zTilt)
    {
        DOTween.To(() => cam.Lens.Dutch, x => cam.Lens.Dutch = x, zTilt, 0.25f);
    }
}
