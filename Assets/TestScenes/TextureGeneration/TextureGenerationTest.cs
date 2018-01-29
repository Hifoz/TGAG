using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TextureGenerationTest : MonoBehaviour {
    Mesh mesh;


	// Use this for initialization
	void Start () {
        Generate();
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public void Generate() {
        // Create a texturearray with some textures
        TextureGenerator textureGenerator = new TextureGenerator(512, 3, "tex2dArr.asset");
        textureGenerator.generateTexture(testData(), 0);
        textureGenerator.generateTexture(testData(200, 0.01f), 0);
        textureGenerator.generateTexture(testData(600, 0.02f), 0);
        string subpath = textureGenerator.buildAsset();

        // Create the mesh
        mesh = GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GenerateCube();

        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("Tex", Resources.Load<Texture2DArray>("Textures/" + subpath));
    }


    /// <summary>
    /// Used to generate testData for the texture generator
    /// </summary>
    /// <param name="seed"></param>
    /// <param name="freq"></param>
    /// <returns></returns>
    public Color[] testData(float seed = 0f, float freq = 0.005f) {
        Color[] data = new Color[512*512];
        for(int i = 0; i < data.Length; i++) {
            float pV = SimplexNoise.Simplex2D(new Vector3((i/512)+seed,(i%512)+seed), freq);
            data[i] = new Color(pV, pV, pV);
        }

        return data;
    }


    /// <summary>
    /// Generates a single cube surrounded by air
    /// </summary>
    public void GenerateCube() {
        BlockData[,,] blockdata = new BlockData[,,] {
            { { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) } },

            { { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.SAND),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.DIRT),new BlockData(BlockData.BlockType.AIR) } },

            { { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) } },
        };


        MeshData meshData = MeshDataGenerator.GenerateMeshData(blockdata);

        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.uv = meshData.uvs;
        mesh.colors = meshData.colors;
        mesh.RecalculateNormals();
    }
}
