using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    // Value needed in the CVDTs for calculating order priority
    public static ThreadSafeVector3 cameraDir = new ThreadSafeVector3();


    public Transform target;
    public float targetDistance = 5;
    public float cameraHeight = 0.75f;

    public GameObject underwaterOverlay;
    private int inWater;


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

        if (Input.GetKeyDown(KeyCode.L)) {
            toggleMouse();
        }

        if(inWater > 0 && underwaterOverlay.activeInHierarchy == false)
            underwaterOverlay.SetActive(true);
        else if(inWater == 0 && underwaterOverlay.activeInHierarchy == true)
            underwaterOverlay.SetActive(false);


        cameraDir.set(transform.rotation * Vector3.forward);
    }

    /// <summary>
    /// Toggles the crusor lockstate when called.
    /// </summary>
    private void toggleMouse() {
        if (Cursor.lockState != CursorLockMode.Locked) {
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            Cursor.lockState = CursorLockMode.None;
        }

    }

    private void OnTriggerEnter(Collider other) {
        if (other.name == "waterChunk")
            inWater += 1;
    }

    private void OnTriggerExit(Collider other) {
        if (other.name == "waterChunk")
            inWater -= 1;
    }
}
