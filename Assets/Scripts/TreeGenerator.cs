using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator {
    const int size = 50;
    const int height = 20;
    public static MeshData generateMeshData() {
        BlockData[,,] pointMap = new BlockData[size,height,size];
        for(int x = 0; x < pointMap.GetLength(0); x++) {
            for (int y = 0; y < pointMap.GetLength(1); y++) {
                for (int z = 0; z < pointMap.GetLength(2); z++) {
                    if (Vector3.Distance(new Vector3(x, 0, z), new Vector3(size/2, 0, size/2)) < size / 4) {
                        pointMap[x, y, z] = new BlockData(BlockData.BlockType.DIRT);
                    } else {
                        pointMap[x, y, z] = new BlockData(BlockData.BlockType.AIR);
                    }
                }
            }
        }
        return MeshDataGenerator.GenerateMeshData(pointMap, 0.1f, true);
    }
}
