using UnityEngine;
using System.Collections;

public class WaterTest : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        RaycastHit hit;
        int layerMask = 1 << 4 | 1 << 8;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, 200f, layerMask)) {
            if (hit.transform.gameObject.layer == 4) {
                Debug.DrawLine(transform.position, hit.point, Color.red);
            } else {
                Debug.DrawLine(transform.position, hit.point, Color.green);
            }
        }
        if (Physics.Raycast(new Ray(transform.position, Vector3.up), out hit, 200f, layerMask)) {
            if (hit.transform.gameObject.layer == 4) {
                Debug.DrawLine(transform.position, hit.point, Color.red);
            } else {
                Debug.DrawLine(transform.position, hit.point, Color.green);
            }
        }
    }
}
