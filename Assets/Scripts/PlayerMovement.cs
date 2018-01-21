using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 10f;
    public float gravity = 6f;
    public float flyingSmoothTime = 1.2f;
    public float groundedSmoothTime = 0.1f;
    public float flapStrength = 0.05f;

    private CharacterController characterController;


    // Movement
    Vector3 currentSpeed;
    Vector3 currentVelocity;

    // Rotation
    float yaw;
    float pitch;
    Vector3 rotation;
    Vector3 rotationSmoothVelocity;


	// Use this for initialization
	void Start () {
        characterController = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {

        updateRotation();
        updateMovement();



	}

    /// <summary>
    /// Updates the movement of the player
    /// </summary>
    private void updateMovement() {
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        float smoothTime = groundedSmoothTime;
        if (!characterController.isGrounded) {
            smoothTime = flyingSmoothTime;

            Mathf.Clamp01(inputDir.y);
        }


        Vector3 moveDir = transform.TransformDirection(inputDir.x, 0, inputDir.y);

        if (characterController.isGrounded) {
            moveDir.y = 0;
        }
        moveDir.Normalize();

        Vector3 targetSpeed = moveDir * moveSpeed;
        currentSpeed = Vector3.SmoothDamp(currentSpeed, targetSpeed, ref currentVelocity, smoothTime);

        if (Input.GetKey(KeyCode.Space)) {
            currentSpeed.y += flapStrength;
        }

        Vector3 velocity = currentSpeed - Vector3.up * gravity * (1.1f - currentSpeed.magnitude/moveSpeed);

        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Updates the rotation of the player
    /// </summary>
    private void updateRotation() {
        yaw += Input.GetAxis("Mouse X");
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y"), -80, 80);

        rotation = Vector3.SmoothDamp(rotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, 0);


        transform.eulerAngles = rotation;

    }

}
