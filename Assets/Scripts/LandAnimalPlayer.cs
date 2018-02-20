using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LandAnimalPlayer : LandAnimal {
    // Values are needed in the CVDTs for calculating order priority
    public static ThreadSafeVector3 playerPos = new ThreadSafeVector3();
    public static ThreadSafeVector3 playerRot = new ThreadSafeVector3();
    public static ThreadSafeVector3 playerSpeed = new ThreadSafeVector3();

    /// <summary>
    /// Function for that lets the player control the animal
    /// </summary>
    override protected void move() {
        if (!Input.GetKey(KeyCode.LeftAlt)) {
            desiredHeading = Camera.main.transform.forward;
            desiredHeading.y = 0;
            desiredHeading.Normalize();
        }

        Vector3 finalHeading = Vector3.zero;
        desiredSpeed = 0;

        if (Input.GetKey(KeyCode.W)) {
            finalHeading += desiredHeading;
            setSpeed();
        }
        if (Input.GetKey(KeyCode.A)) {
            finalHeading += Quaternion.AngleAxis(-90, Vector3.up) * desiredHeading;
            setSpeed();
        }
        if (Input.GetKey(KeyCode.D)) {
            finalHeading += Quaternion.AngleAxis(90, Vector3.up) * desiredHeading;
            setSpeed();
        }
        if (Input.GetKey(KeyCode.S)) {
            finalHeading -= desiredHeading;
            setSpeed();
        }

        if (finalHeading != Vector3.zero) {
            desiredHeading = finalHeading;
        }        

        transform.LookAt(transform.position - heading);

        Vector3 velocity = spineHeading.normalized * speed;
        GetComponent<Rigidbody>().velocity = velocity + gravity;

        playerPos.set(transform.position);
        playerRot.set(transform.rotation * Vector3.forward);
        playerSpeed.set(velocity);
    }

    /// <summary>
    /// Sets speed based on input
    /// </summary>
    private void setSpeed() {
        if (Input.GetKey(KeyCode.LeftShift)) {
            desiredSpeed = runSpeed;
        } else {
            desiredSpeed = walkSpeed;
        }
    }
}
