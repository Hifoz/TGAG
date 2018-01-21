using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform target;
    public float targetDistance = 5;
    public float cameraHeight = 0.75f;


    private float yaw;
    private float pitch;
    private Vector3 rotation;
    private Vector3 rotationSmoothVelocity;


    /// <summary>
    /// Update the rotation of the camera
    /// </summary>
	void LateUpdate () {
        yaw += Input.GetAxis("Mouse X");
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y"), -80, 80);

        rotation = Vector3.SmoothDamp(rotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, 0);

        transform.eulerAngles = rotation;

        this.transform.position = target.position - transform.forward * targetDistance + transform.TransformDirection(0, cameraHeight, 0);

    }
}
