using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TextureGenerationTest))]
[ExecuteInEditMode]
public class TextureGenerationTestEditor : Editor {

    /*public override void OnInspectorGUI() {
        if (GUILayout.Button("Regenerate"))
            ((TextureGenerationTest)target).Generate();
    }*/
}
#endif