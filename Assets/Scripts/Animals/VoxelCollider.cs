using UnityEngine;
using System.Collections;

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
        Vector3 vel = Vector3.zero;
        int[] axis = new int[] { 1, 0, 2 };
        for (int i = 0; i < axis.Length; i++) {
            vel[axis[i]] = rb.velocity[axis[i]];
            Vector3 nextPos = transform.position + vel * Time.fixedDeltaTime;
            if (VoxelPhysics.isSolid(VoxelPhysics.voxelAtPos(nextPos))) {
                vel[axis[i]] = 0;
            }
        }
        rb.velocity = vel;
    }
}
