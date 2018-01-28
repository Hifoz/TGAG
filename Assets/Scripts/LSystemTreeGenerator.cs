using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


public static class LSystemTreeGenerator {

    private enum Axis { X, Y, Z };

    private struct Turtle {
        public Vector3 heading;
        public Vector3 pos;
        public Axis axis;
    }

    private static char[] language = new char[] {
        'N', //Nothing
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
    private const float angle = 25f;// Mathf.PI / 6f;

    private static Dictionary<Axis, Vector3> axis = new Dictionary<Axis, Vector3>();

    static LSystemTreeGenerator() {
        rules.Add('N', "D[-XND]+XD+XD[-D+D]N");

        axis.Add(Axis.X, new Vector3(1, 0, 0));
        axis.Add(Axis.Y, new Vector3(0, 1, 0));
        axis.Add(Axis.Z, new Vector3(0, 0, 1));
    }

    public static List<LineSegment> GenerateLSystemTree(Vector3 pos) {
        List<LineSegment> tree = new List<LineSegment>();
        //tree.Add(new LineSegment(new Vector3(0, 0, 0), new Vector3(0, 10, 5)));
        //tree.Add(new LineSegment(new Vector3(0, 10, 5), new Vector3(10, 15, 5)));
        //tree.Add(new LineSegment(new Vector3(0, 10, 5), new Vector3(15, 15, 5)));

        string word = recurseString(start.ToString(), 4);
        Stack<Turtle> states = new Stack<Turtle>();
        Turtle turtle = new Turtle();
        turtle.heading = Vector3.up;
        turtle.pos = Vector3.zero;
        turtle.axis = Axis.X;
        
        foreach(char c in word) {
            switch (c) {
                case 'D':
                    tree.Add(new LineSegment(turtle.pos, turtle.pos + turtle.heading));
                    turtle.pos = turtle.pos + turtle.heading;
                    break;
                case 'X':
                    turtle.axis = Axis.X;
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
            
        return tree;
    }

    private static string recurseString(string input, int depth) {
        if (depth == 0) {
            return input;
        }

        string output = "";
        foreach (char c in input) {
            if (rules.ContainsKey(c)) {
                output += rules[c];
            } else {
                output += c;
            }
        }
        return recurseString(output, depth - 1);
    }
}
