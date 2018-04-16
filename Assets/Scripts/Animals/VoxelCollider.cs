using UnityEngine;

/// <summary>
/// Collider that collides with the voxel world.
/// </summary>
public class VoxelCollider : MonoBehaviour {
    private Rigidbody rb;
    private Animal animal;

    private BlockData.BlockType lastVoxel = BlockData.BlockType.NONE;

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        animal = GetComponent<Animal>();
    }

    private void FixedUpdate() {
        physicsMessages();
        preventCollision();
    }

    /// <summary>
    /// Triggers OnVoxel/Enter/Stay/Exit
    /// </summary>
    private void physicsMessages() {
        BlockData.BlockType voxelAtPos = VoxelPhysics.voxelAtPos(transform.position);

        if (VoxelPhysics.isSolid(voxelAtPos)) { //Unstuck
            transform.position += Vector3.up;
        }

        if (voxelAtPos == lastVoxel) {
            animal.OnVoxelStay(lastVoxel);
        } else if (voxelAtPos != lastVoxel) {
            animal.OnVoxelEnter(voxelAtPos);
            animal.OnVoxelExit(lastVoxel);
        }
        lastVoxel = voxelAtPos;
    }

    /// <summary>
    /// Adjusts velocity to avoid collision, last step in velocity calculations
    /// </summary>
    protected void preventCollision() {
        bool collision = false;
        Vector3 vel = Vector3.zero;
        for (int i = 0; i < 3; i++) {
            vel[i] = rb.velocity[i];
            Vector3 nextPos = transform.position + vel * Time.fixedDeltaTime;
            if (VoxelPhysics.isSolid(VoxelPhysics.voxelAtPos(nextPos))) {
                vel[i] = 0;
                collision = true;
            }
        }
        rb.velocity = vel;

        if (collision) {
            animal.OnVoxelCollisionEnter(VoxelPhysics.voxelAtPos(transform.position + rb.velocity * Time.fixedDeltaTime));
        }
    }
}
