using UnityEngine;
using System.Collections;

public class LandAnimalNPC : LandAnimal {

    public const float roamDistance = 50;

    override protected void Start() {
        base.Start();
        desiredSpeed = walkSpeed;

        //LandAnimalSkeleton skeleton = new LandAnimalSkeleton(transform);
        //skeleton.generateInThread();
        //setSkeleton(skeleton);
    }

    /// <summary>
    /// Moves the animal in world space
    /// </summary>
    override protected void move() {
        float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), roamCenter);
        Vector3 toCenter = roamCenter - transform.position;
        toCenter.y = 0;
        if (dist > roamDistance && Vector3.Angle(toCenter, desiredHeading) > 90) {
            desiredHeading = -desiredHeading;
            desiredHeading = Quaternion.AngleAxis(80 * Random.Range(-1f, 1f), Vector3.up) * desiredHeading;
        }
        transform.LookAt(transform.position + heading);
        if (grounded) {
            rb.velocity = spineHeading * speed + gravity;
        } else {
            rb.velocity = gravity;
        }
    }
}
