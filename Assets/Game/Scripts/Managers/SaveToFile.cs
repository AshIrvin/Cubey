using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public class SaveToFile : MonoBehaviour
{
    internal static Action OnFirstTimeUse;

    internal static void SetupJson()
    {
        var chapterData = new ChapterData
        {
            ChapterLevelsData = Enumerable.Range(0, 6)
                .Select(CreateChapterListData)
                .ToList()
        };

        SaveToJson(chapterData);

        OnFirstTimeUse?.Invoke();
        Debug.Log("SetupJson. Done");
    }

    private static ChapterLevelData CreateChapterListData(int n)
    {
        return new ChapterLevelData(n)
        {
            Levels = Enumerable.Repeat(new ChapterLevelData.LevelInfo(), 30).ToList()
        };
    }

    internal static void SaveChapterData(List<ChapterLevelData> chapterLevelData)
    {
        ChapterData chapterData = LoadFromJson();
        chapterData.ChapterLevelsData = chapterLevelData;
        
        SaveToJson(chapterData);
        Debug.Log("SaveChapterData. Done");
    }

    // save/append specific object to file
    private static void SaveToJson(ChapterData data)
    {
        var json = JsonUtility.ToJson(data);

        File.WriteAllText($"{ Application.dataPath}/saveData.json", json);

        Debug.Log("Save to Json: " + json.ToString());
    }

    // load file
    internal static ChapterData LoadFromJson()
    {
        var deserialised = DeserialiseData();

        _ = new ChapterData
        {
            ChapterLevelsData = new List<ChapterLevelData>(6)
        };

        ChapterData chapterData = deserialised;
        Debug.Log("LoadFromJson. Done.");
        return chapterData;
    }

    // gets full save file
    private static ChapterData DeserialiseData()
    {
        CheckFileExists();

        var json = File.ReadAllText($"{Application.dataPath}/saveData.json");
        var deserialised = JsonConvert.DeserializeObject<ChapterData>(json);

        Logger.Instance.ShowDebugLog("Deserialised data");

        return deserialised;
    }

    internal static bool CheckFileExists()
    {
        if (File.Exists($"{Application.dataPath}/saveData.json"))
            return true;

        return false;
    }
}
