using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using LineFunctions;

namespace CustomEditors {
    [CustomEditor(typeof(LineFunction))]
    public class LineFunctionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LineFunction script = (LineFunction)target;
            if (GUILayout.Button("Test Value")) {
                if (Application.isPlaying) {
                    script.TestValue();
                }
                else {
                    Debug.LogWarning("Line Function (Script): Application must be in play mode to test!");
                }
            }
        }
    }
}
