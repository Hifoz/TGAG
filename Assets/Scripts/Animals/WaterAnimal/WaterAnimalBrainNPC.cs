using UnityEngine;
using System.Collections;

public class WaterAnimalBrainNPC : AnimalBrainNPC {

    override public float roamDist { get { return 50f; } }

    override public float slowSpeed { get { return 10f; } }
    override public float fastSpeed { get { return 40f; } }

    /// <summary>
    /// Moves the animal in world space
    /// </summary>
    override public void move() {
        state.desiredSpeed = slowSpeed;
        float dist = Vector3.Distance(new Vector3(state.transform.position.x, 0, state.transform.position.z), roamCenter);
        Vector3 toCenter = roamCenter - state.transform.position;
        toCenter.y = 0;
        if (dist > roamDist && Vector3.Angle(toCenter, state.desiredHeading) > 90) {
            state.desiredHeading = -state.desiredHeading;
            state.desiredHeading = Quaternion.AngleAxis(80 * Random.Range(-1f, 1f), Vector3.up) * state.desiredHeading;
        }        
    }
}
