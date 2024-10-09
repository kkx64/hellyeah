using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy : MonoBehaviour
{
    Rigidbody rb;
    public float depthBefSub;
    public float displacementAmt;
    public int floaters;

    public float waterDrag;
    public float waterAngularDrag;
    public WaterSurface water;
    WaterSearchParameters Search;
    WaterSearchResult SearchResult;

    void Start()
    {
        water = FindAnyObjectByType<WaterSurface>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // rb.AddForceAtPosition(
        //     Physics.gravity / floaters,
        //     transform.position,
        //     ForceMode.Acceleration
        // );

        Search.startPositionWS = transform.position;

        water.ProjectPointOnWaterSurface(Search, out SearchResult);

        if (transform.position.y < SearchResult.projectedPositionWS.y)
        {
            float displacementMulti =
                Mathf.Clamp01(
                    SearchResult.projectedPositionWS.y - transform.position.y / depthBefSub
                ) * displacementAmt;
            rb.AddForceAtPosition(
                new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMulti, 0f),
                transform.position,
                ForceMode.Acceleration
            );
            rb.AddForce(
                displacementMulti * -rb.linearVelocity * waterDrag * Time.fixedDeltaTime,
                ForceMode.VelocityChange
            );
            rb.AddTorque(
                displacementMulti * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime,
                ForceMode.VelocityChange
            );
        }
    }
}
