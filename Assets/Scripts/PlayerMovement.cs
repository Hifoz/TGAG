using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    public float walkingSpeed = 10f;
    public float flyingSpeed = 10f;
    public float gravity = 6f;
    public float flyingSmoothTime = 1.2f;
    public float groundedSmoothTime = 0.1f;
    public float flapStrength = 0.05f;

    private CharacterController characterController;

    // Movement
    private Vector3 currentSpeed;
    private Vector3 currentVelocity;

	void Start () {
        characterController = GetComponent<CharacterController>();
	}
	
	void Update () {
        updateMovement();
        updateRotation();
	}

    /// <summary>
    /// Updates the movement of the player
    /// </summary>
    private void updateMovement() {
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        float moveSpeed = walkingSpeed;
        float smoothTime = groundedSmoothTime;
        if (!characterController.isGrounded) {
            moveSpeed = flyingSpeed;
            smoothTime = flyingSmoothTime;
        }


        Vector3 moveDir = Camera.main.transform.TransformDirection(inputDir.x, 0, inputDir.y);
        if (characterController.isGrounded) {
            moveDir.y = 0;
        }
        moveDir.Normalize();


        Vector3 targetSpeed = moveDir * moveSpeed;
        currentSpeed = Vector3.SmoothDamp(currentSpeed, targetSpeed, ref currentVelocity, smoothTime);


        if (Input.GetKey(KeyCode.Space)) {
            if (!characterController.isGrounded)
                currentSpeed.y += flapStrength;
            else
                currentSpeed.y += 4;
        }

        Vector3 velocity = currentSpeed - Vector3.up * gravity * (1.1f - currentSpeed.magnitude/moveSpeed);

        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Updates the rotation of the player
    /// </summary>
    private void updateRotation() {
        if (currentSpeed.magnitude < 0.1f)
            return;

        Quaternion rotation = Quaternion.LookRotation(currentSpeed);
        transform.rotation = rotation;

    }


    private void OnTriggerEnter(Collider other) {
        if (other.name == "waterChunk")
            Debug.Log("Entering water");
    }

    private void OnTriggerExit(Collider other) {
        if (other.name == "waterChunk")
            Debug.Log("Leaving water");
    }
}
