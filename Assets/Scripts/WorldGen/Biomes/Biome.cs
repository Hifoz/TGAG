using System;
using System.Collections.Generic;
using UnityEngine;


/*
 * WIP:
 * 
 * 
 * All variables are subject to change
 * 
 * Trees:
 *      Custom rules for tree generation based on biome? (Currently the rules are stored in LSystemTreeGenerator.rules, and the rule to use is rng)
 *      Tree related varaibles currently stored in the biome could also be changed.
 *      Could also affect the texture type, so different foliage types can have different leaves and tree textures?
 * 
 * 
 * Ideas for other biome modifiers:
 * - Skybox-changes?
 * - Different rules for CVDG.decideBlockType() based on chunk
 * - 
 * 
 * 
 * 
 */

public class Biome {
    //General
    public int snowHeight;
    //2D noise settings
    public float frequency2D;
    public float noiseExponent2D;
    public int octaves2D;
    //3D noise settings
    public float Structure3DRate;
    public float Unstructure3DRate;
    public float frequency3D;
    //Foliage
    public int maxTreesPerChunk;
    public float treeLineLength;
    public float treeVoxelSize;
    public float treeThickness;
    public float treeLeafThickness;
    public int grammarRecursionDepth;


    /// <summary>
    /// Loads a biome from a file
    /// </summary>
    /// <param name="filepath">path to file</param>
    public void loadFromFile(String filepath) {
        throw new NotImplementedException("Biome.loadFromFile(String filepath) not yet implemented.");
    }
}
