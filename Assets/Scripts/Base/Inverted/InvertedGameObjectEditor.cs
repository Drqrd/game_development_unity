using UnityEngine;
using UnityEditor;

namespace CustomEditors {
[CustomEditor(typeof(InvertedGameObject))]
public class InvertedMeshEditor : Editor
{
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InvertedGameObject script = (InvertedGameObject)target;
            if (GUILayout.Button("Generate")) {
                script.Generate();
            }
        }
}

}
