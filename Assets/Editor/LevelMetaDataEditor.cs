using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelMetaData))]
public class LevelMetadataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var metadata = target as LevelMetaData;

        if (!Application.isPlaying)
        {
            if (GUILayout.Button("Get level info", GUILayout.Width(160)))
            {
                metadata.UnityEditorAssignData();
                EditorUtil.ApplyChanges(metadata);
            }
        }
    }
}