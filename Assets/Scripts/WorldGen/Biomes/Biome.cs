using System;
using System.Collections.Generic;
using UnityEngine;


/*
 * WIP:
 * 
 * All variables are subject to change or move
 * 
 * Trees:
 *      Custom rules for tree generation based on biome? Currently the rules are stored in LSystemTreeGenerator.rules, and the rule to use is rng, one could have a set of rules per biome
 *      Tree related varaibles currently stored in the biome could also be changed.
 *      Could also affect the texture type, so different foliage types can have different leaves and tree textures?
 * 
 * Ideas for other biome modifiers:
 * - Skybox-changes?
 * - Different rules for CVDG.decideBlockType() based on chunk
 * 
 * 
 * 
 * 
 * 
 * IMPORTANT TO DO:
 *  - CVDT.findGroundLevel() currently only using first biome on position
 * 
 */

public class Biome {
    //General
    public readonly int snowHeight = 0;
    //2D noise settings
    public readonly float frequency2D = 0;
    public readonly float noiseExponent2D = 0;
    public readonly int octaves2D = 0;
    //3D noise settings
    public readonly float Structure3DRate = 0;
    public readonly float Unstructure3DRate = 0;
    public readonly float frequency3D = 0;
    //Foliage
    public readonly int maxTreesPerChunk = 0;
    public readonly float treeLineLength = 0;
    public readonly float treeVoxelSize = 0;
    public readonly float treeLeafThickness = 0;
    public readonly int grammarRecursionDepth = 0;
    private readonly float treeThickness;


    /// <summary>
    /// Generate a new biome based on a list of biomes.
    /// </summary>
    /// <param name="biomes">Each pair should contain a biomeand its weight</param>
    public Biome(List<Pair<Biome, float>> biomes) {
        // Weighted averaging of members:
        foreach(Pair<Biome, float> b in biomes) {
            snowHeight += (int)(b.first.snowHeight * b.second);
            frequency2D += b.first.frequency2D * b.second;
            noiseExponent2D += b.first.noiseExponent2D * b.second;
            octaves2D += (int)(b.first.octaves2D * b.second);
            Structure3DRate += b.first.Structure3DRate * b.second;
            Unstructure3DRate += b.first.Unstructure3DRate * b.second;
            frequency3D += b.first.frequency3D * b.second;

            // not averaging foliage settings because we don't change them per biome at the moment
        }

    }


    // I know this one looks horrible, but it is more compact than than making all members protected and adding public getters, as this is only for use in sub-classes :\
    /// <summary>
    /// A constructor taking in all the parameters
    /// </summary>
    protected Biome(int snowHeight, float frequency2D, int noiseExponent2D, int octaves2D, 
                 float Structure3DRate, float Unstructure3DRate, float frequency3D, 
                 int maxTreesPerChunk, float treeLineLength, float treeVoxelSize, float treeThickness, 
                 float treeLeafThickness, int grammarRecursionDepth) {
        this.snowHeight = snowHeight;
        this.frequency2D = frequency2D;
        this.noiseExponent2D = noiseExponent2D;
        this.octaves2D = octaves2D;
        this.Structure3DRate = Structure3DRate;
        this.Unstructure3DRate = Unstructure3DRate;
        this.frequency3D = frequency3D;
        this.maxTreesPerChunk = maxTreesPerChunk;
        this.treeLineLength = treeLineLength;
        this.treeVoxelSize = treeVoxelSize;
        this.treeThickness = treeThickness;
        this.treeLeafThickness = treeLeafThickness;
        this.grammarRecursionDepth = grammarRecursionDepth;
    }

    /// <summary>
    /// Loads a biome from a file
    /// </summary>
    /// <param name="filepath">path to file</param>
    public void loadFromFile(String filepath) {
        throw new NotImplementedException("Biome.loadFromFile(String filepath) not yet implemented.");
    }

}
