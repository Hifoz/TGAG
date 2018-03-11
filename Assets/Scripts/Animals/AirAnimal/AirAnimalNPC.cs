using UnityEngine;
using System.Collections;

public class AirAnimalNPC : AirAnimal {
    public const float roamDistance = 50;

    float timer = 0;
    Range<float> phaseRange = new Range<float>(10f, 10f);
    float phaseDuration;
    bool flying;
    bool isAscending = false;

    override protected void Start() {
        base.Start();
        desiredSpeed = walkSpeed;
        phaseDuration = Random.Range(phaseRange.Min, phaseRange.Max);
        flying = 0 == Random.Range(0, 2);
        if (flying) {
            desiredSpeed = flySpeed;
        } else {
            desiredSpeed = walkSpeed;
        }
    }

    /// <summary>
    /// Moves the animal in world space
    /// </summary>
    override protected void move() {
        timer += Time.deltaTime;
        if (timer >= phaseDuration) {
            timer = 0;
            phaseDuration = Random.Range(phaseRange.Min, phaseRange.Max);
            flying = !flying;
            if (flying) {
                desiredSpeed = flySpeed;
            } else {
                desiredSpeed = walkSpeed;
            }
        }
                
        if (grounded) {
            desiredSpeed = walkSpeed;
        } else {
            desiredSpeed = flySpeed;
        }
        
        if (grounded && flying) {
            tryAscend();
        } else if (!grounded && !flying) {
            timer = 0;
            desiredSpeed = 0;
        }

        if (!isAscending) {
            float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), roamCenter);
            Vector3 toCenter = roamCenter - transform.position;
            toCenter.y = 0;
            if (dist > roamDistance && Vector3.Angle(toCenter, desiredHeading) > 90) {
                desiredHeading = -desiredHeading;
                desiredHeading = Quaternion.AngleAxis(80 * Random.Range(-1f, 1f), Vector3.up) * desiredHeading;
            }
        }
        transform.LookAt(transform.position + heading);
        Vector3 velocity;
        if (grounded) {
            velocity = spineHeading.normalized * speed;
        } else {
            velocity = heading.normalized * speed;
        }
        rb.velocity = velocity + gravity;
    }

    /// <summary>
    /// Tries to ascend
    /// </summary>
    /// <returns>success flag</returns>
    private bool tryAscend() {
        if (!isAscending) {
            StartCoroutine(ascend());
        }
        return !isAscending;
    }

    /// <summary>
    /// Makes the NPC fly straight up
    /// </summary>
    /// <returns></returns>
    private IEnumerator ascend() {
        isAscending = true;
        Vector3 originalHeading = desiredHeading;
        
        tryLaunch();        
        for (float t = 0; t <= 1f; t += Time.deltaTime / 2f) {
            desiredHeading = Vector3.up;
            yield return 0;
        }
        desiredHeading = originalHeading;
        isAscending = false;
    }
}
