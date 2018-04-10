using UnityEngine;
using System.Collections;

public enum VoxelRayCastTarget {
    SOLID,
    WATER
}

public class VoxelRayCastHit {
    public BlockData.BlockType type;
    public Vector3 blockPos;
    public Vector3 point;
    public float distance;
}

public static class VoxelPhysics {
    private static ChunkData[,] world;
    private static WorldGenManager worldGenManager;

    private static bool ready = false;

    public static bool Ready { get { return ready; } }

    public static void init(WorldGenManager wgm) {
        world = wgm.getChunkGrid();
        worldGenManager = wgm;
        ready = true;
    }

    public static void clear() {
        world = null;
        worldGenManager = null;
        ready = false;
    }

    public static BlockData.BlockType voxelAtPos(Vector3 worldPos) {
        Vector3Int index = worldGenManager.world2ChunkIndex(worldPos);
        if (!worldGenManager.checkBounds(index.x, index.y)) {
            return BlockData.BlockType.NONE;
        }
        ChunkData chunk = world[index.x, index.z];
        Vector3Int blockIndex = Utils.floorVectorToInt(worldPos - chunk.pos);
        return chunk.blockDataMap.get(blockIndex).blockType;
    } 

    public static bool isSolid(BlockData.BlockType type) {
        return !(type == BlockData.BlockType.WATER || type == BlockData.BlockType.NONE);
    }

    public static bool isWater(BlockData.BlockType type) {
        return type == BlockData.BlockType.WATER;
    }

    public static VoxelRayCastHit rayCast(Ray ray, float length, VoxelRayCastTarget target) {
        const float delta = 1f;
        for (float t = 0; t <= length; t += delta) {
            Vector3 sample = ray.origin + ray.direction * t;
            BlockData.BlockType currentBlock = voxelAtPos(sample);

            switch (target) {
                case VoxelRayCastTarget.SOLID:
                    if (isSolid(currentBlock)) {
                        return createVoxelRayCastHit(ray.origin, sample, currentBlock);
                    }
                    break;
                case VoxelRayCastTarget.WATER:
                    if (currentBlock == BlockData.BlockType.WATER) {
                        return createVoxelRayCastHit(ray.origin, sample, currentBlock);
                    }
                    break;
            }
        }
        return createVoxelRayCastHit(ray.origin, Vector3.down, BlockData.BlockType.NONE);
    }

    private static VoxelRayCastHit createVoxelRayCastHit(Vector3 origin, Vector3 sample, BlockData.BlockType type) {
        VoxelRayCastHit hit = new VoxelRayCastHit();
        hit.blockPos = Utils.floorVector(sample);
        hit.point = hit.blockPos + Vector3.up * 0.5f;
        hit.type = type;
        hit.distance = Vector3.Distance(origin, hit.blockPos);
        return hit;
    }
}
