using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// A custom editor window used for changing WorldGen settings at runtime.
/// </summary>
public class WorldGenEditor : EditorWindow {

    public static float voxelSize = 1;
    public static int chunkSize = 10;
    public static int chunkCount = 11;
    public static int chunkHeight = 50; // Chunk height must not exceed (5376/(chunkSize^2))
    public static float frequency = 0.02f;

    private static ChunkManager chunkManager;

    [MenuItem("TGAG/WorldGenEditor")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(WorldGenEditor));        
        copyFromConfig();
    }

    void OnGUI() {
        voxelSize = EditorGUILayout.FloatField("Voxel Size (Not used, might remove)", voxelSize);
        chunkSize = EditorGUILayout.IntField("Chunk Size", chunkSize);
        chunkCount = EditorGUILayout.IntField("Chunk Count", chunkCount);
        chunkHeight = EditorGUILayout.IntField("Chunk Height", chunkHeight);
        frequency = EditorGUILayout.FloatField("Frequency", frequency);

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
        voxelSize = ChunkConfig.voxelSize;
        chunkSize = ChunkConfig.chunkSize;
        chunkCount = ChunkConfig.chunkCount;
        chunkHeight = ChunkConfig.chunkHeight;
        frequency = ChunkConfig.frequency;
    }

    /// <summary>
    /// Applies the WorldGen settings provided by the user.
    /// </summary>
    private void apply() {
        Debug.Log("Applying settings!");
        if (chunkManager != null) {
            chunkManager.clear();
        }

        ChunkConfig.voxelSize = voxelSize;
        ChunkConfig.chunkSize = chunkSize;
        ChunkConfig.chunkCount = chunkCount;
        ChunkConfig.chunkHeight = chunkHeight;
        ChunkConfig.frequency = frequency;

        if (chunkManager != null) {
            chunkManager.init();
        }
    }    
}
