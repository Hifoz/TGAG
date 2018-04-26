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
        //animalShowCase();
        //generateNoisePlane();
        //noiseCalcVisuals(new Vector3(0.5f, 0.4f));
        //noiseCalcVisuals(new Vector3(-0.045f, 0.4f));
        //noiseCalcVisuals(new Vector3(5.5f, 5.714f));
        generateSimplexGrid();
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

    private void generateNoisePlane() {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Texture2D tex = new Texture2D(1000, 1000);

        for(int i = 0; i < 1000; i++) {
            for (int j = 0; j < 1000; j++) {
                float noise = SimplexNoise.Simplex2D(new Vector3(i, j, 0), 0.1f);
                noise = (noise + 1) / 2;
                tex.SetPixel(i, j, new Color(noise, noise, noise));
            }
        }
        tex.Apply();

        obj.GetComponent<MeshRenderer>().material.mainTexture = tex;
    }

    float squaresToTriangles = (3f - Mathf.Sqrt(3f)) / 6f; //Converts points from square grid to triangel grid
    float trianglesToSquares = (Mathf.Sqrt(3f) - 1f) / 2f; //Converts points from triangle grid to square grid

    private void noiseCalcVisuals(Vector3 point, bool pointm = true) {
        if (pointm)
            pointMake(point, Color.blue);

        float skew = (point.x + point.y) * trianglesToSquares; //Transform the triangle grid to a cube grid
        float sx = point.x + skew;
        float sy = point.y + skew;

        int ix = Mathf.FloorToInt(sx);
        int iy = Mathf.FloorToInt(sy);

        List<Vector3> triangleSkew = new List<Vector3>();
        List<Vector3> triangleUnskew = new List<Vector3>();

        triangleUnskew.Add(Simplex2DPart(point, ix, iy));
        triangleUnskew.Add(Simplex2DPart(point, ix + 1, iy + 1));

        triangleSkew.Add(new Vector3(ix, iy));
        triangleSkew.Add(new Vector3(ix + 1, iy + 1));

        if (sx - ix >= sy - iy) { // Work out which triangle the point is inside
            triangleUnskew.Add(Simplex2DPart(point, ix + 1, iy));
            triangleSkew.Add(new Vector3(ix + 1, iy));
        } else {
            triangleUnskew.Add(Simplex2DPart(point, ix, iy + 1));
            triangleSkew.Add(new Vector3(ix, iy + 1));
        }

        StartCoroutine(drawTriangle(triangleSkew, Color.red));
        StartCoroutine(drawTriangle(triangleUnskew, Color.green));
    }

    private Vector3 Simplex2DPart(Vector3 point, int ix, int iy) {
        float unskew = (ix + iy) * squaresToTriangles;
        return new Vector3(ix - unskew, iy - unskew);
    }

    private IEnumerator drawTriangle(List<Vector3> triangle, Color color) {
        Debug.Log(color.ToString());
        foreach (var vert in triangle) {
            Debug.Log(vert);
        }

        while (true) {
            for (int i = 0; i < triangle.Count; i++) {
                Debug.DrawLine(triangle[i], triangle[(i + 1) % triangle.Count], color);
            }
            yield return 0;
        }
    }

    private void pointMake(Vector3 point, Color color) {
        var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        p.transform.position = point;
        p.transform.localScale = Vector3.one * 0.1f;
        p.GetComponent<MeshRenderer>().material.color = color;
    }

    private void generateSimplexGrid() {
        for (float x = 0; x < 5; x += 0.5f) {
            for (float y = 0; y < 5; y += 0.5f) {
                noiseCalcVisuals2(new Vector3(x, y), false);
            }
        }
    }

    private void noiseCalcVisuals2(Vector3 point, bool pointm = true) {
        if (pointm)
            pointMake(point, Color.blue);

        float skew = 0;
        float sx = point.x + skew;
        float sy = point.y + skew;

        int ix = Mathf.FloorToInt(sx);
        int iy = Mathf.FloorToInt(sy);

        List<Vector3> triangleSkew = new List<Vector3>();
        List<Vector3> triangleUnskew = new List<Vector3>();

        triangleUnskew.Add(Simplex2DPart2(point, ix, iy));
        triangleUnskew.Add(Simplex2DPart2(point, ix + 1, iy + 1));

        triangleSkew.Add(new Vector3(ix, iy));
        triangleSkew.Add(new Vector3(ix + 1, iy + 1));

        if (sx - ix >= sy - iy) { // Work out which triangle the point is inside
            triangleUnskew.Add(Simplex2DPart2(point, ix + 1, iy));
            triangleSkew.Add(new Vector3(ix + 1, iy));
        } else {
            triangleUnskew.Add(Simplex2DPart2(point, ix, iy + 1));
            triangleSkew.Add(new Vector3(ix, iy + 1));
        }

        StartCoroutine(drawTriangle(triangleSkew, Color.red));
        StartCoroutine(drawTriangle(triangleUnskew, Color.green));
    }

    private Vector3 Simplex2DPart2(Vector3 point, int ix, int iy) {
        float unskew = (ix + iy) * squaresToTriangles;
        return new Vector3(ix - unskew, iy - unskew);
    }
}
