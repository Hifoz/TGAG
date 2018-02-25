using System;
using System.Collections.Generic;

/// <summary>
/// Container for a blockmap
/// </summary>
public class BlockDataMap {
    public BlockData[] mapdata;

    private int sizeX;
    private int sizeY;
    private int sizeZ;

    /// <summary>
    /// Constructs a new blockdata map
    /// </summary>
    /// <param name="sizeX">number of blocks in X-dimension</param>
    /// <param name="sizeY">number of blocks in Y-dimension</param>
    /// <param name="sizeZ">number of blocks in Z-dimension</param>
    public BlockDataMap(int sizeX, int sizeY, int sizeZ) {
        mapdata = new BlockData[sizeX * sizeY * sizeZ];
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.sizeZ = sizeZ;
    }

    /// <summary>
    /// Convert a 3D index to a 1D index
    /// </summary>
    /// <param name="x">x of 3d index</param>
    /// <param name="y">y of 3d index</param>
    /// <param name="z">z of 3d index</param>
    /// <returns>1d index</returns>
    public int index1D(int x, int y, int z) {
        return x + (y + z * sizeY) * sizeX;
    }

    /// <summary>
    /// Gets the length of the array, either the full length, or the length of x, y, or z
    /// </summary>
    /// <param name="i">index of dimension to get length of.
    /// 0=x, 1=y, 2=z. Any other value (or empty) will give full length of array.
    /// </param>
    /// <returns>the length of the array, either the full length, or the length of x, y, or z</returns>
    public int GetLength(int i = -1) {
        switch (i) {
            case 0: return sizeX;
            case 1: return sizeY;
            case 2: return sizeZ;
            default: return sizeX * sizeY * sizeZ;
        }
    }

}