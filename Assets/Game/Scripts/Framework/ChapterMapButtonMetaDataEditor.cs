using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChapterMetaData))]
public class ChapterMapButtonMetaDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var metadata = target as ChapterMetaData;

        if (!Application.isPlaying)
        {
            if (GUILayout.Button("Assign Map Buttons to List", GUILayout.Width(200)))
            {
                metadata.UnityEditorAssignData();
                EditorUtility.SetDirty(metadata);
            }
            if (GUILayout.Button("Assign Images to Buttons", GUILayout.Width(200)))
            {
                metadata.UnityEditorAutoAssignSprite();
                EditorUtility.SetDirty(metadata);
            }            
            if (GUILayout.Button("Assign Numbers to Buttons", GUILayout.Width(200)))
            {
                metadata.UnityEditorAssignLevelNumbers();
                EditorUtility.SetDirty(metadata);
            }            
            if (GUILayout.Button("Assign Data to Level metadata", GUILayout.Width(200)))
            {
                metadata.UnityEditorAssignInfoToLevelMetaData();
                EditorUtility.SetDirty(metadata);
            }
            if (GUILayout.Button("Assign Path to Level metadata", GUILayout.Width(200)))
            {
                metadata.UnityEditorAssignPathToLevelMetaData();
                EditorUtility.SetDirty(metadata);
            }
        }
    }
}