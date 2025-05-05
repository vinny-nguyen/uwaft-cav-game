using UnityEditor;
using UnityEngine;

namespace NodeMap.EditorTools
{
    /// <summary>
    /// Custom editor for the NodePlacerTool
    /// </summary>
    [CustomEditor(typeof(NodePlacerTool))]
    public class NodePlacerToolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw default fields (references)

            NodePlacerTool tool = (NodePlacerTool)target;

            GUILayout.Space(10);

            if (GUILayout.Button("Reposition Node Markers", GUILayout.Height(40)))
            {
                tool.RepositionNodeMarkers();
            }
        }
    }
}
