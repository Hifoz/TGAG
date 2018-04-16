using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ThesisFigures : MonoBehaviour {

    // Use this for initialization
    void Start() {
        generate1Voxel();
    }

    // Update is called once per frame
    void Update() {

    }

    private void generate1Voxel() {
        BlockDataMap map = new BlockDataMap(5, 5, 5);
        map.mapdata[map.index1D(3, 3, 3)] = new BlockData(BlockData.BlockType.DIRT, BlockData.BlockType.GRASS);
        MeshData data = MeshDataGenerator.GenerateMeshData(map)[0];
        MeshDataGenerator.applyMeshData(GetComponent<MeshFilter>(), data);
    }
}
