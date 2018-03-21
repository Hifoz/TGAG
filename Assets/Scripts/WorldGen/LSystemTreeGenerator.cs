using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class generates MeshData for meshes of trees using L-System algorithms.
/// </summary>
public static class LSystemTreeGenerator {

    //Helper classes/enum
    /// <summary>
    /// Enum representing an axis, used by turtle to draw
    /// </summary>
    private enum Axis { X, Y, Z };

    /// <summary>
    /// Struct representing a turtle, turtles draw the trees
    /// </summary>
    private struct Turtle {
        public Vector3 heading;
        public Vector3 pos;
        public Axis axis;
        public float lineLen;
    }

    //Defenition of language (the array is not used in the code)
    private static char[] language = new char[] {
        'N', //Variable
        'M', //Second Variable
        'D', //Draw
        'X', //X axis
        'Y', //Y axis
        'Z', //Z axis
        '+', //Postive rotation
        '-', //Negative rotation
        '[', //Push to stack
        ']'  //Pop from stack
    };
    private const string start = "DDN";
    private static Dictionary<char, string> rules = new Dictionary<char, string>();
    private const float angle = 25f;

    private static Dictionary<Axis, Vector3> axis = new Dictionary<Axis, Vector3>();
    private const float boundingBoxModifier = 2.0f; //Constant that modifies the size of the bounding box for the tree

    /// <summary>
    /// Constructor, populates the dictionaries.
    /// </summary>
    static LSystemTreeGenerator() {
        //The rules that apply to the language
        //The '|' character delimits the different rules
        // for the given character.
        //Rules are chosen in a stochastic manner when more then one apply.
        rules.Add('N', "[-ZND]+M+XD[-D+XD]N" +
            "|-YDN-Y" +
            "|ZDDM+ZD-");
        rules.Add('M', "D[+N]-X");
        //rules.Add('N', "ND"); Generates straight trees

        axis.Add(Axis.X, new Vector3(1, 0, 0));
        axis.Add(Axis.Y, new Vector3(0, 1, 0));
        axis.Add(Axis.Z, new Vector3(0, 0, 1));
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  __  __           _           __  _____      _       _   __  __                _____          _      	//
    // |  \/  |         | |         / / |  __ \    (_)     | | |  \/  |              / ____|        | |     	//
    // | \  / | ___  ___| |__      / /  | |__) |__  _ _ __ | |_| \  / | __ _ _ __   | |     ___   __| | ___ 	//
    // | |\/| |/ _ \/ __| '_ \    / /   |  ___/ _ \| | '_ \| __| |\/| |/ _` | '_ \  | |    / _ \ / _` |/ _ \	//
    // | |  | |  __/\__ \ | | |  / /    | |  | (_) | | | | | |_| |  | | (_| | |_) | | |___| (_) | (_| |  __/	//
    // |_|  |_|\___||___/_| |_| /_/     |_|   \___/|_|_| |_|\__|_|  |_|\__,_| .__/   \_____\___/ \__,_|\___|	//
    //                                                                      | |                             	//
    //                                                                      |_|                             	//
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Generates the MeshData for a tree
    /// </summary>
    /// <param name="pos">Position of the tree</param>
    /// <returns>Meshdata</returns>
    public static MeshData[] generateMeshData(Vector3 pos) {

        List<LineSegment> tree = GenerateLSystemTree(pos);

        float modifier = ((ChunkConfig.treeThickness < ChunkConfig.treeLeafThickness) ? ChunkConfig.treeLeafThickness : ChunkConfig.treeThickness) * boundingBoxModifier;
        LineStructureBounds bounds = new LineStructureBounds(tree, LineStructureType.TREE, modifier);

        BlockDataMap pointMap = new BlockDataMap(bounds.sizeI.x, bounds.sizeI.y, bounds.sizeI.z);
        BlockDataMap pointMapTrunk = new BlockDataMap(bounds.sizeI.x, bounds.sizeI.y, bounds.sizeI.z);
        //Debug.Log("(" + pointMap.GetLength(0) + "," + pointMap.GetLength(1) + "," + pointMap.GetLength(2) + ")");
        for (int x = 0; x < pointMap.GetLength(0); x++) {
            for (int y = 0; y < pointMap.GetLength(1); y++) {
                for (int z = 0; z < pointMap.GetLength(2); z++) {
                    int i = pointMap.index1D(x, y, z);
                    Vector3 samplePos = new Vector3(x, y, z) + bounds.lowerBounds;
                    samplePos = Utils.floorVector(samplePos);
                    pointMap.mapdata[i] = new BlockData(calcBlockType(samplePos, tree), BlockData.BlockType.NONE);
                    pointMapTrunk.mapdata[i] = pointMap.mapdata[i];
                    if (pointMap.mapdata[i].blockType == BlockData.BlockType.LEAF) {
                        pointMapTrunk.mapdata[i] = new BlockData(BlockData.BlockType.NONE, BlockData.BlockType.NONE);
                    }
                }
            }
        }
        MeshData[] meshData = new MeshData[2];
        meshData[0] = MeshDataGenerator.GenerateMeshData(pointMap, ChunkConfig.treeVoxelSize, -Utils.floorVector(bounds.lowerBounds))[0];
        meshData[1] = MeshDataGenerator.GenerateMeshData(pointMapTrunk, ChunkConfig.treeVoxelSize, -Utils.floorVector(bounds.lowerBounds))[0];
        return meshData;
    }
    
    /// <summary>
    /// Calculates the blocktype based on position and tree lines.
    /// </summary>
    /// <param name="pos">Position being investigated</param>
    /// <param name="tree">Tree lines</param>
    /// <returns>Blocktype for position</returns>
    private static BlockData.BlockType calcBlockType(Vector3 pos, List<LineSegment> tree) {
        for (int i = 0; i < tree.Count; i++) {
            float dist = tree[i].distance(pos);
            if (dist < ChunkConfig.treeThickness) {
                return BlockData.BlockType.WOOD;
            } else if (tree[i].endLine && dist < ChunkConfig.treeLeafThickness && leafPos(pos)) {
                return BlockData.BlockType.LEAF;
            }
        }
        return BlockData.BlockType.NONE;
    }

    /// <summary>
    /// Is this a position for a leaf?
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private static bool leafPos(Vector3 pos) {
        pos += Vector3.one * 1000; // remove offset
        Vector3Int p = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
        bool evenPos = (p.x % 2 == 0 && p.y % 2 == 0 && p.z % 2 == 0);
        bool oddPos = (p.x % 2 == 1 && p.y % 2 == 1 && p.z % 2 == 1);
        return evenPos || oddPos;
    }

    //////////////////////////////////////////////////////////////////////////////
    //  _          _____           _                    _____          _      	//
    // | |        / ____|         | |                  / ____|        | |     	//
    // | |  _____| (___  _   _ ___| |_ ___ _ __ ___   | |     ___   __| | ___ 	//
    // | | |______\___ \| | | / __| __/ _ \ '_ ` _ \  | |    / _ \ / _` |/ _ \	//
    // | |____    ____) | |_| \__ \ ||  __/ | | | | | | |___| (_) | (_| |  __/	//
    // |______|  |_____/ \__, |___/\__\___|_| |_| |_|  \_____\___/ \__,_|\___|	//
    //                    __/ |                                               	//
    //                   |___/                                                	//
    //////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Generates a Tree based on the grammar defined in this class.
    /// The tree is drawn by a turtle.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static List<LineSegment> GenerateLSystemTree(Vector3 pos) {
        //Initialize.
        List<LineSegment> tree = new List<LineSegment>(); ;

        System.Random rng = new System.Random(NoiseUtils.Vector2Seed(pos));
        string word = recurseString(start.ToString(), ChunkConfig.grammarRecursionDepth, rng);

        Stack<Turtle> states = new Stack<Turtle>();
        Turtle turtle = new Turtle();
        turtle.heading = Vector3.up;
        turtle.pos = Vector3.zero;
        turtle.axis = Axis.X;
        turtle.lineLen = ChunkConfig.treeLineLength;
        
        //Make the turtle proccess the word.
        foreach(char c in word) {
            switch (c) {
                case 'D':
                    tree.Add(new LineSegment(turtle.pos, turtle.pos + turtle.heading * turtle.lineLen));
                    turtle.pos = turtle.pos + turtle.heading * turtle.lineLen;
                    break;
                case 'X':
                    turtle.axis = Axis.X;
                    break;
                case 'Y':
                    turtle.axis = Axis.Y;
                    break;
                case 'Z':
                    turtle.axis = Axis.Z;
                    break;
                case '+':
                    turtle.heading = Quaternion.AngleAxis(angle, axis[turtle.axis]) * turtle.heading;
                    break;
                case '-':
                    turtle.heading = Quaternion.AngleAxis(-angle, axis[turtle.axis]) * turtle.heading;
                    break;
                case '[':
                    states.Push(turtle);
                    break;
                case ']':
                    LineSegment line = tree[tree.Count - 1]; //When the turtle pops, the branch is complete.
                    line.endLine = true;
                    tree[tree.Count - 1] = line;
                    turtle = states.Pop();
                    break;
            }
        }
        LineSegment lastLine = tree[tree.Count - 1]; //When the turtle pops, the branch is complete.
        lastLine.endLine = true;
        tree[tree.Count - 1] = lastLine;        
        return tree;
    }   

    /// <summary>
    /// Recurses the string and applies the rules of the grammar defined by this class.
    /// </summary>
    /// <param name="input">Input word</param>
    /// <param name="depth">The recursive depth</param>
    /// <param name="rng">A random number generator</param>
    /// <returns>Word after applied rules</returns>
    private static string recurseString(string input, int depth, System.Random rng) {
        if (depth == 0) {
            return input;
        }

        string output = "";
        foreach (char c in input) {
            if (rules.ContainsKey(c)) {
                string[] rule = rules[c].Split('|');
                output += rule[(int)(rng.NextDouble() * rule.Length)];
            } else {
                output += c;
            }
        }
        return recurseString(output, depth - 1, rng);
    }
}
