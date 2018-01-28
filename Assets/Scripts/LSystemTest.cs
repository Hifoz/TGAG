using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemTest : MonoBehaviour {

    List<List<LineSegment>> trees = new List<List<LineSegment>>();
    List<Vector3> locations = new List<Vector3>();

	// Use this for initialization
	void Start () {
        for (int i = 1; i < 100; i += 11) {
            for (int j = 1; j < 100; j += 11) {
                trees.Add(LSystemTreeGenerator.GenerateLSystemTree(new Vector3(i, 0, j)));
                locations.Add(new Vector3(i, 0, j));
            }
        }        
	}
	
	// Update is called once per frame
	void Update () {
        renderTree();
	}

    private void renderTree() {
        int i = 0;
        foreach (var tree in trees) {
            foreach (var branch in tree) {
                Debug.DrawLine(locations[i] + branch.a, locations[i] + branch.b, Color.green);             
            }
            i++;
        }
    }
}
