using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// A custom editor window used for changing WorldGen settings at runtime.
/// </summary>
public class WorldGenEditor : EditorWindow {

    private static int seed = 1337;
    private static int chunkSize = 20;
    private static int chunkCount = 20;
    private static int chunkHeight = 100; // Chunk height must not exceed (5376/(chunkSize^2))
    private static float frequency2D = 0.01f;
    private static float noiseExponent2D = 2;
    private static int octaves2D = 2;
    private static float Structure3DRate = 0.75f;
    private static float Unstructure3DRate = 0.85f;
    private static float frequency3D = 0.0075f;

    private static ChunkManager chunkManager;

    [MenuItem("TGAG/WorldGenEditor")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(WorldGenEditor));        
        copyFromConfig();
    }

    void OnGUI() {
        GUILayout.Label("Chunk settings");
        seed = EditorGUILayout.IntField("Seed", seed);
        chunkSize = EditorGUILayout.IntField("Chunk Size", chunkSize);
        chunkCount = EditorGUILayout.IntField("Chunk Count", chunkCount);
        chunkHeight = EditorGUILayout.IntField("Chunk Height", chunkHeight);
        GUILayout.Label("2D noise settings");
        frequency2D = EditorGUILayout.FloatField("Frequency 2D", frequency2D);
        noiseExponent2D = EditorGUILayout.FloatField("Noise Exponent 2D", noiseExponent2D);
        octaves2D = EditorGUILayout.IntSlider("Octaves 2D", octaves2D, 0, 10);
        GUILayout.Label("3D noise settings");
        Structure3DRate = EditorGUILayout.FloatField("Structure 3D Rate", Structure3DRate);
        Unstructure3DRate = EditorGUILayout.FloatField("Unstructure 3D Rate", Unstructure3DRate);
        frequency3D = EditorGUILayout.FloatField("Frequency 3D", frequency3D);

        if (chunkManager == null) {
            GUILayout.Label("No ChunkManager found!", EditorStyles.boldLabel);
        } else {
            GUILayout.Label("ChunkManager found!", EditorStyles.boldLabel);
        }

        if (GUILayout.Button("Find ChunkManager")) {
            chunkManager = FindObjectOfType<ChunkManager>();
        }

        if (GUILayout.Button("Apply")) {
            if (chunkManager != null) {
                apply();
            }
        }
    }

    /// <summary>
    /// Copies settings from ChunkConfig to the editor window.
    /// </summary>
    private static void copyFromConfig() {
        seed = ChunkConfig.seed;
        chunkSize = ChunkConfig.chunkSize;
        chunkCount = ChunkConfig.chunkCount;
        chunkHeight = ChunkConfig.chunkHeight;
        frequency2D = ChunkConfig.frequency2D;
        noiseExponent2D = ChunkConfig.noiseExponent2D; 
        octaves2D = ChunkConfig.octaves2D;
        Structure3DRate = ChunkConfig.Structure3DRate;
        Unstructure3DRate = ChunkConfig.Unstructure3DRate;
        frequency3D = ChunkConfig.frequency3D;
    }

    /// <summary>
    /// Applies the WorldGen settings provided by the user.
    /// </summary>
    private void apply() {
        Debug.Log("Applying settings!");

        chunkManager.clear();

        ChunkConfig.seed = seed;
        ChunkConfig.chunkSize = chunkSize;
        ChunkConfig.chunkCount = chunkCount;
        ChunkConfig.chunkHeight = chunkHeight;
        ChunkConfig.frequency2D = frequency2D;
        ChunkConfig.noiseExponent2D = noiseExponent2D;
        ChunkConfig.octaves2D = octaves2D;
        ChunkConfig.Structure3DRate = Structure3DRate;
        ChunkConfig.Unstructure3DRate = Unstructure3DRate;
        ChunkConfig.frequency3D = frequency3D;

        chunkManager.init();
    }    
}
