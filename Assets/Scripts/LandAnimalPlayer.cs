using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class LandAnimalPlayer : LandAnimal {
    // Values are needed in the CVDTs for calculating order priority
    public static ThreadSafeVector3 playerPos = new ThreadSafeVector3();
    public static ThreadSafeVector3 playerRot = new ThreadSafeVector3();
    public static ThreadSafeVector3 playerSpeed = new ThreadSafeVector3();
    public float groundForce = 5f;

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
        //transform.position += heading.normalized * speed * Time.deltaTime;
        Vector3 velocity = heading.normalized * speed;
        velocity.y = GetComponent<Rigidbody>().velocity.y;
        GetComponent<Rigidbody>().velocity = velocity;

        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit)) {
            float legLen = skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH);
            float dist2ground = Vector3.Distance(hit.point, transform.position);
            if (dist2ground < legLen/2) {
                GetComponent<Rigidbody>().AddForce(Vector3.up * Mathf.Pow((legLen - dist2ground)/legLen, 2) * groundForce);
            }
        }

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
