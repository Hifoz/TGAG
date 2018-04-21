using UnityEngine;

/// <summary>
/// Target type for a raycast
/// </summary>
public enum VoxelRayCastTarget {
    SOLID,
    NON_SOLID,
    WATER
}

/// <summary>
/// Result of a VoxelPhysics.rayCast
/// </summary>
public class VoxelRayCastHit {
    public BlockData.BlockType type;
    public Vector3 blockPos;
    public Vector3 point;
    public float distance;
}

/// <summary>
/// Class containing voxel physics logic
/// </summary>
public static class VoxelPhysics {
    private static ChunkData[,] world;
    private static WorldGenManager worldGenManager;

    private static bool ready = false;

    public static bool Ready { get { return ready; } }

    /// <summary>
    /// Inits the voxelPhysics
    /// </summary>
    /// <param name="wgm">world gen manager</param>
    public static void init(WorldGenManager wgm) {
        world = wgm.getChunkGrid();
        worldGenManager = wgm;
        ready = true;
    }

    /// <summary>
    /// Clears the VoxelPhysics
    /// </summary>
    public static void clear() {
        world = null;
        worldGenManager = null;
        ready = false;
    }

    /// <summary>
    /// Returns the voxel at the given position
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public static BlockData.BlockType voxelAtPos(Vector3 worldPos) {
        if (worldGenManager == null)
            return BlockData.BlockType.NONE;
        Vector3Int index = worldGenManager.world2ChunkIndex(worldPos);
        if (!worldGenManager.checkBounds(index.x, index.z)) {
            return BlockData.BlockType.NONE;
        }
        ChunkData chunk = world[index.x, index.z];
        if (chunk == null) {
            return BlockData.BlockType.NONE;
        }
        Vector3Int blockIndex = Utils.floorVectorToInt(worldPos - chunk.pos);
        return chunk.blockDataMap.get(blockIndex).blockType;
    } 

    /// <summary>
    /// Checks if the block is a solid
    /// </summary>
    /// <param name="type">block to check</param>
    /// <returns>true/false</returns>
    public static bool isSolid(BlockData.BlockType type) {
        return !(type == BlockData.BlockType.WATER || type == BlockData.BlockType.NONE || type == BlockData.BlockType.WIND);
    }

    /// <summary>
    /// Checks if the block is water
    /// </summary>
    /// <param name="type">Type of block</param>
    /// <returns>true/false</returns>
    public static bool isWater(BlockData.BlockType type) {
        return type == BlockData.BlockType.WATER;
    }

    /// <summary>
    /// Checks if the block is wind
    /// </summary>
    /// <param name="type">Type of block</param>
    /// <returns>true/false</returns>
    public static bool isWind(BlockData.BlockType type) {
        return type == BlockData.BlockType.WIND;
    }

    /// <summary>
    /// Raycasts against the voxel terrain (Not trees and wind)
    /// </summary>
    /// <param name="ray">Ray to cast</param>
    /// <param name="length">Length of cast</param>
    /// <param name="target">Target type for cast</param>
    /// <returns>Data about the hit for the raycast</returns>
    public static VoxelRayCastHit rayCast(Ray ray, float length, VoxelRayCastTarget target, float start = 0f) {
        const float delta = 1f;
        for (float t = start; t <= length; t += delta) {
            Vector3 sample = ray.origin + ray.direction * t;
            BlockData.BlockType currentBlock = voxelAtPos(sample);

            switch (target) {
                case VoxelRayCastTarget.SOLID:
                    if (isSolid(currentBlock)) {
                        return createVoxelRayCastHit(ray, sample, currentBlock);
                    }
                    break;
                case VoxelRayCastTarget.NON_SOLID:
                    if (!isSolid(currentBlock)) {
                        return createVoxelRayCastHit(ray, sample, currentBlock);
                    }
                    break;
                case VoxelRayCastTarget.WATER:
                    if (isWater(currentBlock)) {
                        return createVoxelRayCastHit(ray, sample, currentBlock);
                    }
                    break;
            }
        }
        return createVoxelRayCastHit(ray, Vector3.down, BlockData.BlockType.NONE);
    }
    
    /// <summary>
    /// Creates a VoxelRayCastHit
    /// </summary>
    /// <param name="ray">Ray to use</param>
    /// <param name="sample">Position that ended the raycast</param>
    /// <param name="type">Type of block that was hit</param>
    /// <returns>result</returns>
    private static VoxelRayCastHit createVoxelRayCastHit(Ray ray, Vector3 sample, BlockData.BlockType type) {
        VoxelRayCastHit hit = new VoxelRayCastHit();
        hit.blockPos = Utils.floorVector(sample);
        hit.distance = Vector3.Distance(ray.origin, hit.blockPos) - 0.5f;
        hit.point = ray.origin + ray.direction * hit.distance;
        hit.type = type;        
        return hit;
    }

    /// <summary>
    /// Finds the surface point from the provided origin.
    /// It either raycast up at empty space if origin is inside a solid
    /// or raycasts down at solids if origin is outside a solid
    /// </summary>
    /// <param name="origin">point to find surface for</param>
    /// <returns>hit for surface point</returns>
    public static VoxelRayCastHit findSurfacePoint(Vector3 origin) {
        VoxelRayCastHit hit;
        if (!isSolid(voxelAtPos(origin))) {
            hit = rayCast(new Ray(origin, Vector3.down), 100f, VoxelRayCastTarget.SOLID);
        } else {
            hit = rayCast(new Ray(origin, Vector3.up), 100f, VoxelRayCastTarget.NON_SOLID);
        }
        return hit;
    }
}
