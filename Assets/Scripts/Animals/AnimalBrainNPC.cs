using UnityEngine;
using System.Collections;

public abstract class AnimalBrainNPC : AnimalBrain {

    protected Vector3 roamCenter;
    virtual public float roamDist { get { return 50f; } }
    virtual public Vector3 RoamCenter { get { return roamCenter; } }

/// <summary>
/// Spawns the animal at position
/// </summary>
/// <param name="pos">Vector3 pos</param>
override public void Spawn(Vector3 pos) {
        state.transform.position = pos;
        roamCenter = pos;
        roamCenter.y = 0;
        state.desiredHeading = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
    }

    /// <summary>
    /// Function for when this animal used to be a player
    /// </summary>
    public void takeOverPlayer() {
        roamCenter = state.transform.position + new Vector3(Random.Range(2f, 5f), 100, Random.Range(2f, 5f));
        roamCenter.y = 0;
        RaycastHit hit;
        if (Physics.Raycast(new Ray(roamCenter, Vector3.down), out hit)) {
            roamCenter = hit.point;
        }

        state.desiredHeading = state.transform.rotation * Vector3.forward;
        state.desiredHeading.y = 0;
    }

    override public void OnCollisionEnter() {
        avoidObstacle();
    }

    /// <summary>
    /// Tries to avoid obstacle
    /// </summary>
    protected virtual void avoidObstacle() {
        int layerMask = 1 << 8;
        Ray ray = new Ray(state.transform.position, state.desiredHeading);
        VoxelRayCastHit hit = VoxelPhysics.rayCast(ray, 3f, VoxelRayCastTarget.SOLID, 1f);
        if (VoxelPhysics.isSolid(hit.type) || Physics.Raycast(ray, 2f, layerMask)) {
            state.desiredHeading = -state.desiredHeading;
        }
    }
}
