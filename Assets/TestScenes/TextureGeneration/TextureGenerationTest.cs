using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TextureGenerationTest : MonoBehaviour {
    Mesh mesh;


	// Use this for initialization
	void Start () {
        TextureGenerator textureGenerator = new TextureGenerator(512, 2);
        textureGenerator.GenerateTexture(testData(), 0);

        mesh = GetComponent<MeshFilter>().mesh = new Mesh();
        GenerateCube();

        Texture2D tex = new Texture2D(512, 512, TextureFormat.RGBAFloat, false);
        tex.SetPixels(textureGenerator.GetTexture(0));
        tex.Apply();

        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
    }
	
	// Update is called once per frame
	void Update () {
		
	}



    public Color[] testData() {
        Color[] data = new Color[512*512];
        for(int i = 0; i < data.Length; i++) {
            data[i] = new Color(
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f)
                );
        }

        return data;
    }


    public void GenerateCube() {
        BlockData[,,] blockdata = new BlockData[,,] {
            { { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) } },

            { { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.DIRT),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) } },

            { { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) },
            { new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR),new BlockData(BlockData.BlockType.AIR) } },
        };


        MeshData meshData = MeshDataGenerator.GenerateMeshData(blockdata);

        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.uv = meshData.uvs;
        mesh.RecalculateNormals();
    }
}
