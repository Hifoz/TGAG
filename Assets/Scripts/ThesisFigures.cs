using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ThesisFigures : MonoBehaviour {
    public GameObject la;
    public GameObject aa;
    public GameObject wa;

    // Use this for initialization
    void Start() {
        //generate1Voxel();
        //voxelLine();
        animalShowCase();
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

    private void voxelLine() {
        float scale = 1;
        float voxelSize = 0.5f;
        LineSegment line = new LineSegment(Vector3.zero, new Vector3(5, 5, 0), 1);
        var lines = new List<LineSegment>() { line };
        LineStructureBounds bounds = new LineStructureBounds(lines, LineStructureType.ANIMAL, 10, (voxelSize * scale));

        BlockDataMap pointMap = new BlockDataMap(bounds.sizeI.x, bounds.sizeI.y, bounds.sizeI.z);
        for (int x = 0; x < pointMap.GetLength(0); x++) {
            for (int y = 0; y < pointMap.GetLength(1); y++) {
                for (int z = 0; z < pointMap.GetLength(2); z++) {
                    int i = pointMap.index1D(x, y, z);
                    Vector3 samplePos = new Vector3(x, y, z) * (voxelSize * scale) + bounds.lowerBounds;
                    pointMap.mapdata[i] = new BlockData(calcBlockType(lines, samplePos, scale), BlockData.BlockType.NONE);
                }
            }
        }
        MeshData meshData = new MeshData();
        meshData = MeshDataGenerator.GenerateMeshData(pointMap, (voxelSize * scale), -(bounds.lowerBounds / (voxelSize * scale)), MeshDataGenerator.MeshDataType.ANIMAL, 4984)[0];
        MeshDataGenerator.applyMeshData(GetComponent<MeshFilter>(), meshData);
    }

    /// <summary>
    /// Calculates the blocktype of the position
    /// </summary>
    /// <param name="pos">Position to examine</param>
    /// <returns>Blocktype</returns>
    private BlockData.BlockType calcBlockType(List<LineSegment> skeleton, Vector3 pos, float scale) {
        for (int i = 0; i < skeleton.Count; i++) {
            float dist = skeleton[i].distance(pos);
            if (dist < (skeleton[i].radius)) {
                return BlockData.BlockType.ANIMAL;
            }
        }
        return BlockData.BlockType.NONE;
    }

    private void animalShowCase() {
        generateAnimal(Instantiate(la, Vector3.zero, Quaternion.identity));
        generateAnimal(Instantiate(aa, Vector3.zero + Vector3.right * 20, Quaternion.identity));
        generateAnimal(Instantiate(wa, Vector3.zero + Vector3.right * 40, Quaternion.identity));
    }

    private void generateAnimal(GameObject animalObj) {
        Animal animal = animalObj.GetComponent<Animal>();
        animal.enabled = true;
        AnimalSkeleton skeleton = AnimalUtils.createAnimalSkeleton(animalObj, animal.GetType());
        skeleton.generateInThread();
        animal.setSkeleton(skeleton);
        AnimalUtils.addAnimalBrainPlayer(animal);
        animal.enabled = false;
    }
}
