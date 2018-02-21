using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LandAnimalNPC))]
public class LandAnimalEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        LandAnimalNPC myScript = (LandAnimalNPC)target;
        if (GUILayout.Button("Reset Joints")) {
            myScript.resetJoints();
        }
    }
}
