using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LandAnimalPlayer : LandAnimal {
    bool jumping = false;

    protected override void Start() {
        base.Start();
        isPlayer = true;
    }

    /// <summary>
    /// Function for that lets the player control the animal
    /// </summary>
    override protected void move() {
        Debug.Log(grounded);
        desiredSpeed = 0;

        if (!Input.GetKey(KeyCode.LeftAlt)) {
            desiredHeading = Camera.main.transform.forward;
            desiredHeading.y = 0;
            desiredHeading.Normalize();
        }

        Vector3 finalHeading = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) {
            finalHeading += desiredHeading;
        }
        if (Input.GetKey(KeyCode.A)) {
            finalHeading += Quaternion.AngleAxis(-90, Vector3.up) * desiredHeading;
        }
        if (Input.GetKey(KeyCode.D)) {
            finalHeading += Quaternion.AngleAxis(90, Vector3.up) * desiredHeading;
        }
        if (Input.GetKey(KeyCode.S)) {
            finalHeading -= desiredHeading;
        }

        if (!jumping && grounded && Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(jump());
        }

        if (finalHeading != Vector3.zero) {
            desiredHeading = finalHeading;
            setSpeed();
        }

        Vector3 velocity;
        if (grounded || inWater) {
           velocity = spineHeading.normalized * speed;
        } else {
            velocity = heading.normalized * speed;
        }
        rb.velocity = velocity + gravity;
        transform.LookAt(transform.position + heading);
    }

    /// <summary>
    /// Sets speed based on input
    /// </summary>
    private void setSpeed() {
        if (!grounded && !inWater) {
            desiredSpeed = 0;
        } else {
            if (Input.GetKey(KeyCode.LeftShift)) {
                desiredSpeed = runSpeed;
            } else {
                desiredSpeed = walkSpeed;
            }
        }
    }

    private IEnumerator jump() {
        jumping = true;
        gravity += -Physics.gravity * 2f;
        yield return new WaitForSeconds(1.0f);
        jumping = false;
    }

    override protected void OnCollisionEnter(Collision collision) {
        base.OnCollisionEnter(collision);
        gravity = Vector3.zero;
        jumping = false;
    }
}
