using UnityEngine;
using System.Collections;

public class VoxelCollider : MonoBehaviour {
    private Vector3 oldPos;
    private Rigidbody rb;
    private Animal animal;

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
                transform.position = oldPos;
                rb.velocity = Vector3.zero;
                animal.SendMessage("OnCollsiionEnter", null);
            }
        }
        oldPos = transform.position;
    }
}
