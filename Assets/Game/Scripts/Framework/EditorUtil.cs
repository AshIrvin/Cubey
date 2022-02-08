using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public static class EditorUtil
{
#if UNITY_EDITOR
    public static List<T> GetAllAssets<T>(bool includeBundleAssets, string pathName) where T : ScriptableObject
    {
        string[] guidList = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)), new[] {pathName});
        List<T> list = new List<T>(guidList.Length);

        foreach (string guid in guidList)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (includeBundleAssets || AssetDatabase.GetImplicitAssetBundleName(path) == "")
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset != null)
                {
                    list.Add(asset);
                }
            }
        }

        return list.OrderBy((x) => x.name).ToList();
    }

    public static void ApplyChanges(ScriptableObject obj)
    {
        EditorUtility.SetDirty(obj);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static T[] GetAssetsAtPath<T>(string path) where T : Object
    {
        List<T> returnList = new List<T>();

        //get the contents of the folder's full path (excluding any meta files) sorted alphabetically
        IEnumerable<string> fullpaths = Directory.GetFiles(path).Where(x => !x.EndsWith(".meta")).OrderBy(s => s);

        //loop through the folder contents
        foreach (string fullpath in fullpaths)
        {
            //determine a path starting with Assets
            string assetPath = fullpath.Replace(Application.dataPath, "Assets");

            //load the asset at this relative path
            Object obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            //and add it to the list if it is of type T
            if (obj is T)
            {
                returnList.Add(obj as T);
            }
        }

        return returnList.OrderBy(x => x.name).ToArray();
    }
#endif
}
    

