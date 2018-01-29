using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TextureGenerationTest : MonoBehaviour {
    Mesh mesh;


    /// <summary>
    /// Used to test the texture generator.
    /// </summary>
    public void Generate() {
        // Create a texturearray with some textures
        TextureManager textureManager = new TextureManager(512, (int)BlockData.BlockType.COUNT * 3);
        string sharedPath = "Textures/temp/";
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        textureManager.loadTextureFromFile(sharedPath + "temp_dirt");
        textureManager.loadTextureFromFile(sharedPath + "temp_stone");
        textureManager.loadTextureFromFile(sharedPath + "temp_stone");
        textureManager.loadTextureFromFile(sharedPath + "temp_stone");
        textureManager.loadTextureFromFile(sharedPath + "temp_sand");
        textureManager.loadTextureFromFile(sharedPath + "temp_sand");
        textureManager.loadTextureFromFile(sharedPath + "temp_sand");
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_top");
        textureManager.loadTextureFromFile(sharedPath + "temp_grass_side");
        textureManager.skipIndex();
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_top");
        textureManager.loadTextureFromFile(sharedPath + "temp_snow_side");
        textureManager.skipIndex();

        // Create the mesh
        mesh = GetComponent<MeshFilter>().sharedMesh = new Mesh();
        GenerateTestCubes();

        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TexArr", textureManager.getTextureArray());
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
            data[i] = new Color(pV, pV, pV, 1);
        }

        return data;
    }


    /// <summary>
    /// Generates cubes for testing texture generation.
    /// </summary>
    public void GenerateTestCubes() {
        BlockData[,,] blockdata = new BlockData[,,] {
            { { new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE) },
            { new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE) },
            { new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE) } },

            { { new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.DIRT),new BlockData(BlockData.BlockType.NONE) },
            { new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE) },
            { new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.DIRT, BlockData.BlockType.GRASS),new BlockData(BlockData.BlockType.NONE) } },

            { { new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE) },
            { new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE) },
            { new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE),new BlockData(BlockData.BlockType.NONE) } },
        };


        MeshData meshData = MeshDataGenerator.GenerateMeshData(blockdata);

        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.uv = meshData.uvs;
        mesh.colors = meshData.colors;
        mesh.RecalculateNormals();
    }
}
