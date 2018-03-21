using System;
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
 * - Skybox
 * - decideBlockType()
 * - Trees
 * 
 */

/// <summary>
/// Base class for biomes
/// </summary>
public class Biome {
    //General
    public readonly int minGroundHeight = 0;
    public readonly int maxGroundHeight = 0;
    public readonly int snowHeight = 0;
    //2D noise settings
    public readonly float frequency2D = 0;
    public readonly float noiseExponent2D = 0;
    public readonly int octaves2D = 0;
    //3D noise settings
    public readonly float structure3DRate = 0;
    public readonly float unstructure3DRate = 0;
    public readonly float frequency3D = 0;
    //Foliage
    public readonly int maxTreesPerChunk = 0;
    public readonly float treeLineLength = 0;
    public readonly float treeVoxelSize = 0;
    public readonly float treeLeafThickness = 0;
    public readonly int grammarRecursionDepth = 0;
    public readonly float treeThickness = 0;


    // I know this one looks horrible, but it is more compact than than making all members protected and adding public getters, as this is only for use in sub-classes :\
    /// <summary>
    /// A constructor taking in all the parameters. Used by sub-classes as readonly variables can only be set in the constructor of the class it is stored in.
    /// </summary>
    protected Biome(int minGroundHeight, int maxGroundHeight, int snowHeight, float frequency2D, 
                    float noiseExponent2D, int octaves2D, float structure3DRate, float unstructure3DRate, 
                    float frequency3D, int maxTreesPerChunk, float treeLineLength, float treeVoxelSize, 
                    float treeLeafThickness, int grammarRecursionDepth, float treeThickness) {
        this.minGroundHeight = minGroundHeight;
        this.maxGroundHeight = maxGroundHeight;
        this.snowHeight = snowHeight;
        this.frequency2D = frequency2D;
        this.noiseExponent2D = noiseExponent2D;
        this.octaves2D = octaves2D;
        this.structure3DRate = structure3DRate;
        this.unstructure3DRate = unstructure3DRate;
        this.frequency3D = frequency3D;
        this.maxTreesPerChunk = maxTreesPerChunk;
        this.treeLineLength = treeLineLength;
        this.treeVoxelSize = treeVoxelSize;
        this.treeLeafThickness = treeLeafThickness;
        this.grammarRecursionDepth = grammarRecursionDepth;
        this.treeThickness = treeThickness;
    }


    /// <summary>
    /// Loads a biome from a file
    /// </summary>
    /// <param name="filepath">path to file</param>
    public void loadFromFile(String filepath) {
        throw new NotImplementedException("Biome.loadFromFile(String filepath) not yet implemented.");
    }

}
