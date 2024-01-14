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
                .ToList(),
            GamePurchased = false
        };

        SaveToJson(chapterData);

        OnFirstTimeUse?.Invoke();
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
        chapterData.GamePurchased = ShopManager.GamePurchased;
        
        SaveToJson(chapterData);
    }

    private static void SaveToJson(ChapterData data)
    {
        var json = JsonUtility.ToJson(data);

        File.WriteAllText($"{ Application.dataPath}/saveData.json", json);
    }

    internal static ChapterData LoadFromJson()
    {
        var deserialised = DeserialiseData();

        _ = new ChapterData
        {
            ChapterLevelsData = new List<ChapterLevelData>(6),
        };

        ChapterData chapterData = deserialised;
        return chapterData;
    }

    private static ChapterData DeserialiseData()
    {
        CheckFileExists();

        var json = File.ReadAllText($"{Application.dataPath}/saveData.json");
        var deserialised = JsonConvert.DeserializeObject<ChapterData>(json);

        return deserialised;
    }

    internal static bool CheckFileExists()
    {
        if (File.Exists($"{Application.dataPath}/saveData.json"))
            return true;

        return false;
    }
}
