using UnityEngine;
using System.Collections;

public class AirAnimalBrainNPC : AnimalBrainNPC {
    override public float roamDist { get { return 100f; } }

    float timer = 0;
    Range<float> phaseRange = new Range<float>(10f, 10f);
    float phaseDuration;
    bool flying;
    bool isAscending = false;

    override public float slowSpeed { get { return 5f; } }
    override public float fastSpeed { get { return 30f; } }

    public override void Spawn(Vector3 pos) {
        base.Spawn(pos);

        phaseDuration = Random.Range(phaseRange.Min, phaseRange.Max);
        flying = 0 == Random.Range(0, 2);
        if (flying) {
            state.desiredSpeed = fastSpeed;
        } else {
            state.desiredSpeed = slowSpeed;
        }
    }

    /// <summary>
    /// Moves the animal in world space
    /// </summary>
    override public void move() {
        timer += Time.deltaTime;
        if (timer >= phaseDuration) {
            timer = 0;
            phaseDuration = Random.Range(phaseRange.Min, phaseRange.Max);
            flying = !flying;
            if (flying) {
                state.desiredSpeed = fastSpeed;
            } else {
                state.desiredSpeed = slowSpeed;
            }
        }

        if (state.grounded || state.inWater) {
            state.desiredSpeed = slowSpeed;
        } else {
            state.desiredSpeed = fastSpeed;
        }

        if ((state.grounded || state.inWater) && flying) {
            actions["ascend"]();
        } else if (!(state.grounded || state.inWater) && !flying) {
            timer = 0;
            state.desiredSpeed = 0;
        }

        if (!isAscending) {
            float dist = Vector3.Distance(new Vector3(state.transform.position.x, 0, state.transform.position.z), roamCenter);
            Vector3 toCenter = roamCenter - state.transform.position;
            toCenter.y = 0;
            if (dist > roamDist && Vector3.Angle(toCenter, state.desiredHeading) > 90) {
                state.desiredHeading = -state.desiredHeading;
                state.desiredHeading = Quaternion.AngleAxis(80 * Random.Range(-1f, 1f), Vector3.up) * state.desiredHeading;
            }
        }
        state.transform.LookAt(state.transform.position + state.heading);
        Vector3 velocity;
        if (state.grounded || state.inWater) {
            velocity = state.spineHeading.normalized * state.speed;
        } else {
            velocity = state.heading.normalized * state.speed;
        }
        state.rb.velocity = velocity + state.gravity;
    }

    
}
