using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelList))]
public class LevelListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var l = target as LevelList;

        if (!Application.isPlaying)
        {
            if (GUILayout.Button("Populate from Assets", GUILayout.Width(160)))
            {
                string pathName = l.pathName;
                l.UnityEditorPopulate(EditorUtil.GetAllAssets<LevelMetaData>(false, pathName));
                EditorUtil.ApplyChanges(l);
            }
        }
    }
}