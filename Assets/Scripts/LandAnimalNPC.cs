using UnityEngine;
using System.Collections;

public class LandAnimalNPC : LandAnimal {

    public const float roamDistance = 40;
    private Vector3 roamCenter;

    /// <summary>
    /// Spawns the animal at position
    /// </summary>
    /// <param name="pos">Vector3 pos</param>
    public void Spawn(Vector3 pos) {
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
        }

        heading = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        transform.LookAt(transform.position - heading);
    }

    /// <summary>
    /// Moves the animal in world space
    /// </summary>
    override protected void move() {
        float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), roamCenter);
        Vector3 toCenter = roamCenter - transform.position;
        toCenter.y = 0;
        if (dist > roamDistance && Vector3.Angle(toCenter, heading) > 90) {
            heading = -heading;
            heading = Quaternion.AngleAxis(80 * Random.Range(-1f, 1f), Vector3.up) * heading;
            transform.LookAt(transform.position - heading);
        }
        transform.position += heading * walkSpeed * Time.deltaTime;
        Debug.DrawLine(transform.position, transform.position + heading * 10, Color.blue);

        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit)) {
            Vector3 groundTarget = hit.point + Vector3.up * (skeleton.getBodyParameter<float>(BodyParameter.LEG_LENGTH) / 2f);
            if (Vector3.Distance(groundTarget, transform.position) > 0.1f) {
                transform.position += (groundTarget - transform.position).normalized * Time.deltaTime * 3f;
            }
        } else {
            transform.position = new Vector3(0, -1000, 0);
        }
    }
}
