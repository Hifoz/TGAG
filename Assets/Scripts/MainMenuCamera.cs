using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for the camera in the main menu.
/// </summary>
public class MainMenuCamera : MonoBehaviour {
    public float radius;
    public float height;
    public float speed;
    float angle = 0;
	// Update is called once per frame
	void Update () {
        angle += Time.deltaTime * speed;

        transform.position = new Vector3(Mathf.Sin(angle), height / radius, Mathf.Cos(angle)) * radius;
        transform.LookAt(Vector3.zero);
	}
}
