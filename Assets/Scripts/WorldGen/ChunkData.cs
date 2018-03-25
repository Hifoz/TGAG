using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A structure containing data on a chunk
/// </summary>
public class ChunkData {
    public Vector3 pos;
    public GameObject chunkParent;
    public List<GameObject> terrainChunk = new List<GameObject>();
    public List<GameObject> waterChunk = new List<GameObject>();
    public GameObject[] trees;
    public Mesh[] treeColliders;

    private bool collidersEnabled = false;

    public ChunkData(Vector3 pos) {
        this.pos = pos;
    }

    public ChunkData() {
        
    }

    /// <summary>
    /// Enables colliders for objects in chunk
    /// </summary>
    public bool tryEnableColliders() {
        if (!collidersEnabled) {
            collidersEnabled = true;

            for (int i = 0; i < terrainChunk.Count; i++) {
                GameObject chunk = terrainChunk[i];
                MeshCollider collider = chunk.GetComponent<MeshCollider>();
                collider.enabled = true;
                collider.sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
            }

            for (int i = 0; i < waterChunk.Count; i++) {
                GameObject chunk = waterChunk[i];
                MeshCollider collider = chunk.GetComponent<MeshCollider>();
                collider.enabled = true;
                collider.sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
            }

            for (int i = 0; i < trees.Length; i++) {
                MeshCollider collider = trees[i].GetComponent<MeshCollider>();
                collider.enabled = true;
                collider.sharedMesh = treeColliders[i];
                treeColliders[i] = null;
            }
            return true;
        }
        return false;
    } 

    /// <summary>
    /// Disables colliders
    /// </summary>
    public void disableColliders() {
        collidersEnabled = false;

        for (int i = 0; i < terrainChunk.Count; i++) {
            GameObject chunk = terrainChunk[i];
            MeshCollider collider = chunk.GetComponent<MeshCollider>();
            collider.enabled = false;
        }

        for (int i = 0; i < waterChunk.Count; i++) {
            GameObject chunk = waterChunk[i];
            MeshCollider collider = chunk.GetComponent<MeshCollider>();
            collider.enabled = false;
        }

        for (int i = 0; i < trees.Length; i++) {
            MeshCollider collider = trees[i].GetComponent<MeshCollider>();
            collider.enabled = false;
        }
    }
}