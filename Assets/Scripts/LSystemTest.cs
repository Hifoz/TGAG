using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemTest : MonoBehaviour {

    List<LineSegment> tree;

	// Use this for initialization
	void Start () {
        tree = LSystemTreeGenerator.GenerateLSystemTree(transform.position);
	}
	
	// Update is called once per frame
	void Update () {
        renderTree();
	}

    private void renderTree() {
        foreach(var branch in tree) {
            Debug.DrawLine(branch.a, branch.b, Color.green);
        }
    }
}
