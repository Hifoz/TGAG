using UnityEngine;
using System.Collections;

public class LandAnimalNPC : LandAnimal {

    public const float roamDistance = 40;
    private Vector3 roamCenter;

    /// <summary>
    /// Spawns the animal at position
    /// </summary>
    /// <param name="pos">Vector3 pos</param>
    public bool Spawn(Vector3 pos) {
        transform.rotation = Quaternion.identity;
        transform.localRotation = Quaternion.identity;
        if (skeleton != null) {
            foreach (Bone bone in skeleton.getBones(BodyPart.ALL)) {
                bone.bone.rotation = Quaternion.identity;
                bone.bone.localRotation = Quaternion.identity;
            }
        }

        transform.position = pos;
        roamCenter = pos;
        roamCenter.y = 0;

        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit)) {
            Vector3 groundTarget = hit.point + Vector3.up * skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH) / 2f;
            transform.position = groundTarget;
            desiredHeading = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            return true;
        }
        return false;        
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
        transform.LookAt(transform.position - heading);
        GetComponent<Rigidbody>().velocity = heading * speed + gravity;
    }
}
