using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTree : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MeshData md = TreeGenerator.generateMeshData();
        GetComponent<MeshFilter>().mesh = MeshDataGenerator.applyMeshData(md);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
