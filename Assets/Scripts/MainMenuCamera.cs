using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for the camera in the main menu.
/// </summary>
public class MainMenuCamera : MonoBehaviour {
    public Transform target;
    public float radius;
    public float height;
    public float speed;
    float angle = 0;
	// Update is called once per frame
	void Update () {
        angle += Time.deltaTime * speed;

        Vector3 targetPos = Vector3.zero;
        if (target != null) {
            targetPos = target.position;
        }

        transform.position = targetPos + new Vector3(Mathf.Sin(angle), height / radius, Mathf.Cos(angle)) * radius;
        transform.LookAt(targetPos);
	}
}
