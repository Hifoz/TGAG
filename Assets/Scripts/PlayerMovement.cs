using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 5f;
    public float mass = .2f;

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
        Vector3 moveDir = transform.TransformDirection(inputDir.x, 0, inputDir.y);

        Vector3 targetSpeed = moveDir * moveSpeed;
        currentSpeed = Vector3.SmoothDamp(currentSpeed, targetSpeed, ref currentVelocity, 1);

        Vector3 velocity = currentSpeed + Physics.gravity * mass;
        if (Input.GetKey(KeyCode.Space))
            velocity.y += 5;


        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Updates the rotation of the player
    /// </summary>
    private void updateRotation() {
        yaw += Input.GetAxis("Mouse X");
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y"), -80, 80);
        Debug.Log(pitch);

        rotation = Vector3.SmoothDamp(rotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, 0);


        transform.eulerAngles = rotation;

    }

}
