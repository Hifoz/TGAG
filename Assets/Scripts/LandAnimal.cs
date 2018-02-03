using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LandAnimal : MonoBehaviour {

    // Use this for initialization
    void Start() {
        AnimalSkeleton skeleton = new AnimalSkeleton();
        skeleton.generate();
        GetComponent<MeshFilter>().mesh = AnimalSkeleton.createMesh(skeleton);
    }

    // Update is called once per frame
    void Update() {

    }
}
