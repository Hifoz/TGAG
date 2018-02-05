using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class LandAnimal : MonoBehaviour {
    public Transform target;
    // Use this for initialization
    void Start() {
        AnimalSkeleton skeleton = new AnimalSkeleton();
        skeleton.generate(transform);
        GetComponent<SkinnedMeshRenderer>().sharedMesh = skeleton.createMesh();
        GetComponent<SkinnedMeshRenderer>().rootBone = transform;
        GetComponent<SkinnedMeshRenderer>().bones = skeleton.AallBones.ToArray();
    }

    // Update is called once per frame
    void Update() {

    }
}
