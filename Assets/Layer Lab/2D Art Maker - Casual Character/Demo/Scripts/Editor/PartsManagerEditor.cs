using UnityEditor;
using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
    [CustomEditor(typeof(PartsManager))]
    public class PartsManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var script = (PartsManager)target;
            if (GUILayout.Button("SetSkinData")) script.SetSkin();
        }
    }
}