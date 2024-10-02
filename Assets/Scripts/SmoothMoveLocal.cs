using UnityEngine;

public class SmoothMoveLocal : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;

    private void Update()
    {
        if (target == null)
            return;

        Vector3 targetLocalPosition = transform.InverseTransformPoint(target.position);
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            targetLocalPosition,
            ref velocity,
            smoothTime,
            speed
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetSmoothTime(float newSmoothTime)
    {
        smoothTime = newSmoothTime;
    }
}
