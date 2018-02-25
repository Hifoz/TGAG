using System;
using System.Collections.Generic;

public class BlockDataMap {
    public BlockData[] blockData;

    private int sizeX;
    private int sizeY;
    private int sizeZ;


    public BlockDataMap(int sizeX, int sizeY, int sizeZ) {
        blockData = new BlockData[sizeX * sizeY * sizeZ];
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.sizeZ = sizeZ;
    }


    public int get1dIndex(int x, int y, int z) {
        return x + (y + z * sizeY) * sizeX;
    }

    public int GetLength(int i = -1) {
        switch (i) {
            case 0: return sizeX;
            case 1: return sizeY;
            case 2: return sizeZ;
            default: return sizeX * sizeY * sizeZ;
        }
    }

}