using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class LandAnimal : MonoBehaviour {
    AnimalSkeleton skeleton;
    private float ikSpeed = 10;
    private float ikTolerance = 0.1f;

    public const float roamDistance = 20;
    private Vector3 roamCenter;

    Vector3 heading = Vector3.zero;
    public float turnSpeed = 50f;
    public float speed = 5f;
    public float levelSpeed = 30f;
    public float groundOffsetFactor = 0.7f;

    float timer = 0;
    private const float walkSpeed = 0.2f;

    private void Start() {
        Spawn(Vector3.up * 100);
    }

    // Update is called once per frame
    void Update() {
        if (skeleton != null) {
            move();
            levelSpine();
            //stayGrounded();
            walk();
            timer += Time.deltaTime;
        }
    }

    public void Spawn(Vector3 pos) {
        generate();
        transform.position = pos;
        roamCenter = pos;
        roamCenter.y = 0;

        heading = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        transform.LookAt(transform.position - heading);
    }

    private void generate() {
        foreach(Transform child in transform) {
            Destroy(child.gameObject);
        }
        transform.rotation = Quaternion.identity;
        skeleton = new AnimalSkeleton(transform);
        GetComponent<SkinnedMeshRenderer>().sharedMesh = skeleton.createMesh();
        GetComponent<SkinnedMeshRenderer>().rootBone = transform;
        GetComponent<SkinnedMeshRenderer>().bones = skeleton.getBones(AnimalSkeleton.BodyPart.ALL).ToArray();
    }

    private void levelSpine() {
        Transform spine = skeleton.getBones(AnimalSkeleton.BodyPart.SPINE)[0];
        RaycastHit hit1;
        RaycastHit hit2;
        Physics.Raycast(new Ray(spine.position + spine.forward * skeleton.spineLength / 2f, Vector3.down), out hit1);
        Physics.Raycast(new Ray(spine.position - spine.forward * skeleton.spineLength / 2f, Vector3.down), out hit2);
        Vector3 a = hit1.point - hit2.point;
        Vector3 b = spine.forward * skeleton.spineLength;

        float angle = Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
        Vector3 normal = Vector3.Cross(a, b);
        if (angle > 0.01f) {
            spine.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg * levelSpeed * Time.deltaTime, -normal) * spine.rotation;
        }
    }

    private void move() {
        float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), roamCenter);
        Vector3 toCenter = roamCenter - transform.position;
        toCenter.y = 0;
        if (dist > roamDistance && Vector3.Angle(toCenter, heading) > 90) {
            heading = -heading;
            heading = Quaternion.AngleAxis(80 * Random.Range(-1f, 1f), Vector3.up) * heading;
            transform.LookAt(transform.position - heading);
        }
        transform.position += heading * speed * Time.deltaTime;

        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit)) {
            transform.position = hit.point + Vector3.up * (skeleton.legLength) * groundOffsetFactor;
        } else {
            transform.position = new Vector3(0, -1000, 0);
        }        
    }

    private void stayGrounded() {
        List<Transform> rightLegs = skeleton.getBones(AnimalSkeleton.BodyPart.RIGHT_LEGS);
        List<Transform> leftLegs = skeleton.getBones(AnimalSkeleton.BodyPart.LEFT_LEGS);

        var right1 = rightLegs.GetRange(0, 3);
        var right2 = rightLegs.GetRange(3, 3);
        var left1 = leftLegs.GetRange(0, 3);
        var left2 = leftLegs.GetRange(3, 3);

        groundLeg(right1, -1);
        groundLeg(right2, -1);
        groundLeg(left1, 1);
        groundLeg(left2, 1);
    }

    private void walk() {
        List<Transform> rightLegs = skeleton.getBones(AnimalSkeleton.BodyPart.RIGHT_LEGS);
        List<Transform> leftLegs = skeleton.getBones(AnimalSkeleton.BodyPart.LEFT_LEGS);

        var right1 = rightLegs.GetRange(0, 3);
        var right2 = rightLegs.GetRange(3, 3);
        var left1 = leftLegs.GetRange(0, 3);
        var left2 = leftLegs.GetRange(3, 3);

        walkLeg(right1, -1, 0);
        walkLeg(right2, -1, Mathf.PI);
        walkLeg(left1, 1, Mathf.PI);
        walkLeg(left2, 1, 0);
    }

    private void groundLeg(List<Transform> leg, int sign) {
        Vector3 target = leg[0].position + sign * leg[0].right * skeleton.legLength / 2f;

        RaycastHit hit;
        if (Physics.Raycast(new Ray(target, Vector3.down), out hit)) {
            ccd(leg, hit.point);
        }
    }

    private void walkLeg(List<Transform> leg, int sign, float radOffset) {
        Vector3 target = leg[0].position + sign * leg[0].right * skeleton.legLength / 2f;
        target += heading * Mathf.Cos(timer + radOffset) * skeleton.legLength / 2f; 
        

        RaycastHit hit;
        if (Physics.Raycast(new Ray(target, Vector3.down), out hit)) {
            float heightOffset = (Mathf.Sin(timer + Mathf.PI + radOffset)) * skeleton.legLength / 2f;
            heightOffset = (heightOffset > 0) ? heightOffset : 0;

            target = hit.point;
            target.y += heightOffset;
            ccd(leg, target);
        }

    }

    /// <summary>
    /// Does one iteration of CCD
    /// </summary>
    /// <param name="limb">Limb to bend</param>
    /// <param name="target">Target to reach</param>
    /// <returns>Bool target reached</returns>
    private bool ccd(List<Transform> limb, Vector3 target) {
        Debug.DrawLine(target, target + Vector3.up * 10, Color.red);
        Transform[] arm = skeleton.getBones(AnimalSkeleton.BodyPart.RIGHT_LEGS).GetRange(0, 3).ToArray();
        Transform effector = limb[limb.Count - 1];
        float dist = Vector3.Distance(effector.position, target);

        if (dist > ikTolerance) {
            for (int i = limb.Count - 1; i >= 0; i--) {
                Transform bone = limb[i];

                Vector3 a = effector.position - bone.position;
                Vector3 b = target - bone.position;


                float angle = Mathf.Acos(Vector3.Dot(a, b) / (a.magnitude * b.magnitude));
                Vector3 normal = Vector3.Cross(a, b);
                if (angle > 0.01f) {
                    bone.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg * ikSpeed * Time.deltaTime, normal) * bone.rotation;
                }
            }
        } 
        
        return dist < ikTolerance;
    }
}
