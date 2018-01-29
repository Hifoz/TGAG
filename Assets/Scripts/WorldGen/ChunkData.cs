using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A structure containing data on a chunk
/// </summary>
public class ChunkData {
    public Vector3 pos;
    public GameObject chunk;
    public GameObject[] trees;

    public ChunkData(Vector3 pos) {
        this.pos = pos;
    }
}