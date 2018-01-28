using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class representing a line, this is used to represent trees
/// </summary>
public class LineSegment {
    public LineSegment(Vector3 a, Vector3 b) {
        this.a = a;
        this.b = b;
    }

    public LineSegment(Vector3 a, Vector3 b, LineSegment child) {
        this.a = a;
        this.b = b;
    }

    public Vector3 a;
    public Vector3 b;
}

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

    /// <summary>
    /// Class representing a tree.
    /// The size and bounds is used to fit the 
    /// BlockData[,,] pointmap to the tree.
    /// </summary>
    public class Tree {
        public List<LineSegment> tree;
        public Vector3 size;
        public Vector3 lowerBounds;
        public Vector3 upperBounds;
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
    private const char start = 'N';
    private static Dictionary<char, string> rules = new Dictionary<char, string>();
    private const float angle = 25f;

    private static Dictionary<Axis, Vector3> axis = new Dictionary<Axis, Vector3>();
    private const float boundingBoxModifier = 2.5f; //Constant that modifies the size of the bounding box for the tree

    /// <summary>
    /// Constructor, populates the dictionaries.
    /// </summary>
    static LSystemTreeGenerator() {
        //The rules that apply to the language
        //The '|' character delimits the different rules
        // for the given character.
        //Rules are chosen in a stochastic manner when more then one apply.
        rules.Add('N', "D[-ZND]+M+XD[-D+ZD]N" +
            "|YDN-Y" +
            "|ZDM+ZD-");
        rules.Add('M', "D[+N]-X");

        axis.Add(Axis.X, new Vector3(1, 0, 0));
        axis.Add(Axis.Y, new Vector3(0, 1, 0));
        axis.Add(Axis.Z, new Vector3(0, 0, 1));
    }

    /// <summary>
    /// Generates the MeshData for a tree
    /// </summary>
    /// <param name="pos">Position of the tree</param>
    /// <returns>Meshdata</returns>
    public static MeshData generateMeshData(Vector3 pos) {

        Tree tree = GenerateLSystemTree(pos);

        BlockData[,,] pointMap = new BlockData[Mathf.CeilToInt(tree.size.x), Mathf.CeilToInt(tree.size.y), Mathf.CeilToInt(tree.size.z)];
        //Debug.Log("(" + pointMap.GetLength(0) + "," + pointMap.GetLength(1) + "," + pointMap.GetLength(2) + ")");
        for (int x = 0; x < pointMap.GetLength(0); x++) {
            for (int y = 0; y < pointMap.GetLength(1); y++) {
                for (int z = 0; z < pointMap.GetLength(2); z++) {
                    pointMap[x, y, z] = new BlockData(calcBlockType(new Vector3(x + tree.lowerBounds.x, y, z + tree.lowerBounds.z), tree.tree));
                }
            }
        }
        return MeshDataGenerator.GenerateMeshData(pointMap, 0.2f, true);
    }
    
    /// <summary>
    /// Calculates the blocktype based on position and tree lines.
    /// </summary>
    /// <param name="pos">Position being investigated</param>
    /// <param name="tree">Tree lines</param>
    /// <returns>Blocktype for position</returns>
    private static BlockData.BlockType calcBlockType(Vector3 pos, List<LineSegment> tree) {
        foreach (var line in tree) {
            float dist = distance(pos, line);
            if (dist < ChunkConfig.treeThickness) {
                Debug.Log("TRUNK");
                return BlockData.BlockType.DIRT;
            }// else if (line.child == null && dist < 10f && leafPos(pos)) {
            //    return BlockData.BlockType.STONE;
            //}
        }
        return BlockData.BlockType.AIR;
    }

    /// <summary>
    /// Generates a Tree based on the grammar defined in this class.
    /// The tree is drawn by a turtle.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Tree GenerateLSystemTree(Vector3 pos) {
        //Initialize.
        Tree tree = new Tree();
        tree.tree = new List<LineSegment>(); ;
        tree.lowerBounds = new Vector3(99999, 0, 99999);
        tree.upperBounds = new Vector3(-99999, -99999, -99999);

        System.Random rng = new System.Random((int)(pos.x * 1849 + pos.y * 150 + pos.z * 4079));
        string word = recurseString(start.ToString(), 6, rng);

        Stack<Turtle> states = new Stack<Turtle>();
        Turtle turtle = new Turtle();
        turtle.heading = Vector3.up;
        turtle.pos = Vector3.zero;
        turtle.axis = Axis.X;
        turtle.lineLen = 5f;
        
        //Make the turtle proccess the word.
        foreach(char c in word) {
            switch (c) {
                case 'D':
                    tree.tree.Add(new LineSegment(turtle.pos, turtle.pos + turtle.heading * turtle.lineLen));
                    turtle.pos = turtle.pos + turtle.heading * turtle.lineLen;
                    tree.lowerBounds = updateLowerBounds(tree.lowerBounds, turtle.pos);
                    tree.upperBounds = updateUpperBounds(tree.upperBounds, turtle.pos);
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
                    turtle = states.Pop();
                    break;
            }
        }
        //Ready the result and return.
        tree.lowerBounds -= new Vector3(1, 0, 1) * ChunkConfig.treeThickness * boundingBoxModifier;
        tree.upperBounds += Vector3.one * ChunkConfig.treeThickness * boundingBoxModifier;
        tree.size = (tree.upperBounds - tree.lowerBounds);
        //Debug.Log(tree.size + "__" + tree.lowerBounds + "__" + tree.upperBounds);
        return tree;
    }

    /// <summary>
    /// Helper function for GenerateLSystemTree.
    /// </summary>
    /// <param name="bounds">The bounds to update</param>
    /// <param name="turtlePos">The position of the turtle</param>
    /// <returns>Updated bounds</returns>
    private static Vector3 updateLowerBounds(Vector3 bounds, Vector3 turtlePos) {
        if (turtlePos.x < bounds.x) {
            bounds.x = turtlePos.x;
        }
        if (turtlePos.z < bounds.z) {
            bounds.z = turtlePos.z;
        }
        return bounds;
    }

    /// <summary>
    /// Helper function for GenerateLSystemTree.
    /// </summary>
    /// <param name="bounds">The bounds to update</param>
    /// <param name="turtlePos">The position of the turtle</param>
    /// <returns>Updated bounds</returns>
    private static Vector3 updateUpperBounds(Vector3 bounds, Vector3 turtlePos) {
        if (turtlePos.x > bounds.x) {
            bounds.x = turtlePos.x;
        }
        if (turtlePos.y > bounds.y) {
            bounds.y = turtlePos.y;
        }
        if (turtlePos.z > bounds.z) {
            bounds.z = turtlePos.z;
        }
        return bounds;
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

    /// <summary>
    /// Computes the distance between a point and a line segment.
    /// Based on: http://geomalgorithms.com/a02-_lines.html
    /// </summary>
    /// <param name="P">Point</param>
    /// <param name="S">Line Segment</param>
    /// <returns>float distance</returns>
    private static float distance(Vector3 P, LineSegment S) {
        Vector3 v = S.b - S.a;
        Vector3 w = P - S.a;

        float c1 = Vector3.Dot(w, v);
        if (c1 <= 0)
            return Vector3.Distance(P, S.a);

        float c2 = Vector3.Dot(v, v);
        if (c2 <= c1)
            return Vector3.Distance(P, S.b);

        float b = c1 / c2;
        Vector3 Pb = S.a + b * v;
        return Vector3.Distance(P, Pb);
    }
}
