using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTree : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MeshData md = TreeGenerator.generateMeshData(transform.position);
        GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(md);
	}

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            MeshData md = TreeGenerator.generateMeshData(transform.position);
            GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(md);
        }
    }
}
