using UnityEngine;
using System.Collections;

public class LandAnimalPlayer : LandAnimal {

    override protected void move() {
        heading = Camera.main.transform.forward;
        heading.y = 0;
        heading.Normalize();
        transform.LookAt(transform.position - heading);

        if (Input.GetKey(KeyCode.W)) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                speed = runSpeed;
            } else {
                speed = walkSpeed;
            }
        } else {
            speed = 0;
        }

        transform.position += heading * speed * Time.deltaTime;

        RaycastHit hit;
        Vector3 groundTarget;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit)) {
            groundTarget = hit.point + Vector3.up * (skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH) / 2f);            
        } else {
            groundTarget = transform.position + Vector3.down * (skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH) / 2f);
        }

        if (Vector3.Distance(groundTarget, transform.position) > 0.1f) {
            Vector3 fallDir = (groundTarget - transform.position);
            if (fallDir.magnitude < 1f) {
                fallDir.Normalize();
            } else {
                fallDir /= 2f;
            }
            transform.position += fallDir * Time.deltaTime * 3f;
        }
    }
}
