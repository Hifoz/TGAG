using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// A custom editor window used for changing WorldGen settings at runtime.
/// </summary>
public class WorldGenEditor : EditorWindow {

    private static int seed = 1337;
    private static int chunkSize = 10;
    private static int chunkCount = 11;
    private static int chunkHeight = 50; // Chunk height must not exceed (5376/(chunkSize^2))
    private static float frequency = 0.02f;
    private static float noiseExponent;
    private static int octaves = 1;

    private static ChunkManager chunkManager;

    [MenuItem("TGAG/WorldGenEditor")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(WorldGenEditor));        
        copyFromConfig();
    }

    void OnGUI() {
        seed = EditorGUILayout.IntField("Seed", seed);
        chunkSize = EditorGUILayout.IntField("Chunk Size", chunkSize);
        chunkCount = EditorGUILayout.IntField("Chunk Count", chunkCount);
        chunkHeight = EditorGUILayout.IntField("Chunk Height", chunkHeight);
        frequency = EditorGUILayout.FloatField("Frequency", frequency);
        noiseExponent = EditorGUILayout.FloatField("Noise Exponent", noiseExponent);
        octaves = EditorGUILayout.IntSlider("Octaves", octaves, 0, 10);

        if (chunkManager == null) {
            GUILayout.Label("No ChunkManager found!", EditorStyles.boldLabel);
        } else {
            GUILayout.Label("ChunkManager found!", EditorStyles.boldLabel);
        }

        if (GUILayout.Button("Find ChunkManager")) {
            chunkManager = FindObjectOfType<ChunkManager>();
        }

        if (GUILayout.Button("Apply")) {
            apply();
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
        frequency = ChunkConfig.frequency;
        noiseExponent = ChunkConfig.noiseExponent; 
        octaves = ChunkConfig.octaves;
    }

    /// <summary>
    /// Applies the WorldGen settings provided by the user.
    /// </summary>
    private void apply() {
        Debug.Log("Applying settings!");
        if (chunkManager != null) {
            chunkManager.clear();
        }

        ChunkConfig.seed = seed;
        ChunkConfig.chunkSize = chunkSize;
        ChunkConfig.chunkCount = chunkCount;
        ChunkConfig.chunkHeight = chunkHeight;
        ChunkConfig.frequency = frequency;
        ChunkConfig.noiseExponent = noiseExponent;
        ChunkConfig.octaves = octaves;

        if (chunkManager != null) {
            chunkManager.init();
        }
    }    
}
