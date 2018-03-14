using UnityEngine;
using System.Collections;

public class WaterTest : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        RaycastHit hit;
        int layerMask = 1 << 4;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, 200f, layerMask)) {            
            Debug.DrawLine(transform.position, hit.point, Color.red);            
        }
        if (Physics.Raycast(new Ray(transform.position, Vector3.up), out hit, 200f, layerMask)) {
            Debug.DrawLine(transform.position, hit.point, Color.red);
        }
    }
}
