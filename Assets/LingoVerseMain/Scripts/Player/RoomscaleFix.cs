using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(XROrigin))]
public class RoomscaleFix : MonoBehaviour
{
    private CharacterController characterController;
    private XROrigin xROrigin;

    public GameObject startSpawnPosition;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        xROrigin = GetComponent<XROrigin>();

        // set the position of the player to the start spawn position
        transform.position = startSpawnPosition.transform.position;
    }

    void FixedUpdate()
    {
        characterController.height = xROrigin.CameraInOriginSpaceHeight + 0.15f;

        Vector3 centerPoint = transform.InverseTransformPoint(xROrigin.Camera.transform.position);

        characterController.center = new Vector3(centerPoint.x, characterController.height / 2 + characterController.skinWidth, centerPoint.z);

        characterController.Move(new Vector3(0.001f, -0.001f, 0.001f));
        characterController.Move(new Vector3(-0.001f, -0.001f, -0.001f));
    }
}
