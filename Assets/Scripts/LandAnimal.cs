using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class LandAnimal : MonoBehaviour {
    public Transform target;
    AnimalSkeleton skeleton;

    Transform effector, bone;

    // Use this for initialization
    void Start() {
        skeleton = new AnimalSkeleton();
        skeleton.generate(transform);
        GetComponent<SkinnedMeshRenderer>().sharedMesh = skeleton.createMesh();
        GetComponent<SkinnedMeshRenderer>().rootBone = transform;
        GetComponent<SkinnedMeshRenderer>().bones = skeleton.AallBones.ToArray();
        StartCoroutine(ccd());
    }

    // Update is called once per frame
    void Update() {
        Debug.DrawLine(bone.position, effector.position, Color.red);
        Debug.DrawLine(bone.position, target.position, Color.green);
    }

    IEnumerator ccd() {
        Transform[] arm = skeleton.ArightLegs.GetRange(0, 3).ToArray();
        effector = arm[2];
        while (true) {
            Debug.Log(target.position);
            if (Vector3.Distance(effector.position, target.position) > 0.5f) {
                for (int i = 0; i < arm.Length - 1; i++) {
                    bone = arm[i];
                   
                    Vector3 a = effector.position - bone.position;
                    Vector3 b = target.position - bone.position;


                    float angle = Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
                    Vector3 normal = Vector3.Cross(a, b);
                    Debug.Log(angle);
                    if (angle > 0.01f) {
                        //bone.Rotate(new Vector3(0, 0, -angle * Mathf.Rad2Deg));
                        bone.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg * 0.5f, normal) * bone.rotation;
                    }
                    yield return new WaitForSeconds(0.2f);
                }
            }
            yield return 0;
        }
    }
}
