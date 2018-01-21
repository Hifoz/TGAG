using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform target;
    public float targetDistance = 5;
    public float cameraHeight = 0.75f;


    // Rotation
    float yaw;
    float pitch;
    Vector3 rotation;
    Vector3 rotationSmoothVelocity;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
        yaw += Input.GetAxis("Mouse X");
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y"), -80, 80);

        rotation = Vector3.SmoothDamp(rotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, 0);


        transform.eulerAngles = rotation;

        this.transform.position = target.position - transform.forward * targetDistance + transform.TransformDirection(0, cameraHeight, 0);

    }
}
