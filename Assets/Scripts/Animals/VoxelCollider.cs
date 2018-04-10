using UnityEngine;
using System.Collections;

public class VoxelCollider : MonoBehaviour {
    private Vector3 oldPos;
    private Rigidbody rb;
    private Animal animal;

    private BlockData.BlockType lastVoxel = BlockData.BlockType.NONE;

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        oldPos = transform.position;
        animal = GetComponent<Animal>();
    }

    // Update is called once per frame
    void Update() {
        if (VoxelPhysics.Ready) {
            BlockData.BlockType voxelAtPos = VoxelPhysics.voxelAtPos(transform.position);
            if (VoxelPhysics.isSolid(voxelAtPos)) {
                transform.position = transform.position + (oldPos - transform.position).normalized * 0.2f;
            }

            if (voxelAtPos == lastVoxel) {
                animal.OnVoxelStay(lastVoxel);
            } else if (voxelAtPos != lastVoxel) {
                animal.OnVoxelEnter(voxelAtPos);
                animal.OnVoxelExit(lastVoxel);
            }

            lastVoxel = voxelAtPos;
        }
        oldPos = transform.position;
    }
}
