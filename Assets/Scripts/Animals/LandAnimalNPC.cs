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
        transform.position = pos;
        roamCenter = pos;
        roamCenter.y = 0;

        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit)) {
            Vector3 groundTarget = hit.point + Vector3.up * 10;
            transform.position = groundTarget;
            desiredHeading = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            return true;
        }
        return false;        
    }

    /// <summary>
    /// Function for when this animal used to be a player
    /// </summary>
    public void takeOverPlayer() {
        roamCenter = transform.position + new Vector3(Random.Range(2f, 5f), 100, Random.Range(2f, 5f));
        RaycastHit hit;
        if (Physics.Raycast(new Ray(roamCenter, Vector3.down), out hit)) {
            roamCenter = hit.point;    
        }

        desiredHeading = transform.rotation * Vector3.forward;
        desiredHeading.y = 0;
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
            GetComponent<Rigidbody>().velocity = spineHeading * speed + gravity;
        } else {
            GetComponent<Rigidbody>().velocity = gravity;
        }
    }
}
