﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCameraNew : MonoBehaviour
{
   // private GameObject Player;
    private Camera playerCam;
    // private SpawnPortal portalSpawner;

    public Camera portalCam;
    public GameObject otherPortal;
    public Transform otherPortalCam;

    [SerializeField] private float nearClipOffset = 0.05f;
    [SerializeField] private float nearClipLimit = 0.2f;

    private void Start()
    {
     //   Player = GameObject.FindGameObjectWithTag("Player");
        playerCam = Camera.main;

        //   portalSpawner = Player.GetComponent<SpawnPortal>();
    }

    private void LateUpdate()
    {
        UpdatePortalCameras();
    }

    /// <summary>
    /// Handles the relative position, rotation and clipping planes of the portal cameras
    /// </summary>
    private void UpdatePortalCameras()
    {
        //if (portalSpawner.BothPortalsSpawned())
        //{
        /*  if (this.CompareTag("PortalLeft"))
          {
              Debug.Log("PortalLeft");
              otherPortal = GameObject.FindGameObjectWithTag("PortalRight");
              Debug.Log(otherPortal);
          }
          else if (this.CompareTag("PortalRight"))
          {
              Debug.Log("PortalRight");
              otherPortal = GameObject.FindGameObjectWithTag("PortalLeft");
              Debug.Log(otherPortal);
          }

          portalCam = this.GetComponentInChildren<Camera>();
          otherPortalCam = otherPortal.transform.GetChild(0);
  */
        SetPortalCamPositionAndRotation();
        SetViewFrustrum();
        // }
    }

    /// <summary>
    /// Sets the opposite portal's camera position and rotation relative to the players from this portal.
    /// </summary>
    /// Sebastian Lague: https://www.youtube.com/watch?v=cWpFZbjtSQg
    private void SetPortalCamPositionAndRotation()
    {
        Matrix4x4 m = otherPortal.transform.localToWorldMatrix
                             * this.transform.worldToLocalMatrix
                             * playerCam.transform.localToWorldMatrix;

        Quaternion rotation = Quaternion.Euler(0, 180, 0) * m.rotation;

        Vector3 relativePos = this.transform.InverseTransformPoint(playerCam.transform.position);
        relativePos = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativePos;
        Vector3 position = otherPortal.transform.TransformPoint(relativePos);

        otherPortalCam.SetPositionAndRotation(position, rotation);
    }

    /// <summary>
    /// Sets the view frustrum of the portal's camera to prevent redering objects behind the portal
    /// </summary>
    /// Sebastian Lague: https://www.youtube.com/watch?v=cWpFZbjtSQg
    private void SetViewFrustrum()
    {
        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCam.transform.position));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCam.projectionMatrix = playerCam.projectionMatrix;
        }
    }
}