using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[Serializable]
public class SaveLoadManager : MonoBehaviour
{
    [SerializeField] private SaveMetaData saveMetaData;
    
    public static List<ChapterLevelData> SaveStaticList;
    public static bool useCloudSaving;    

    private static int chapterLevelSaved;
    private const bool DELETE_ALL_SAVES = false; // TODO - keep testing this till no errors. Follow

    #region Getters

    public static int ChapterLevelSaved 
    { 
        get => chapterLevelSaved;
        set => chapterLevelSaved = value;
    }

    #endregion Getters

    private void Awake()
    {
        if (DELETE_ALL_SAVES)
            ResetSaves();

        SaveToFile.OnFirstTimeUse += CreateSaves;
    }

    private void Start()
    {
        if (SaveToFile.CheckFileExists())
        {
            Logger.Instance.ShowDebugLog("Existing player, loading save");

            LoadSaves();
        }
        else
        {
            Logger.Instance.ShowDebugLog("No save found");
            SaveToFile.SetupJson();
        }
    }

    private void CreateSaves()
    {
        SaveStaticList ??= new List<ChapterLevelData>();

        var list = SaveToFile.LoadFromJson().ChapterLevelsData;

        foreach (var item in list)
        {
            SaveStaticList.Add(item);
        }

        Debug.Log("CreateSaves. : " + SaveStaticList.Count);
    }

    private async void LoadSaves()
    {
        Logger.Instance.ShowDebugLog($"LoadSaves. Found save, {(useCloudSaving ? "loading cloud saves." : "loading local saves.")}");

        SaveStaticList = SaveToFile.LoadFromJson().ChapterLevelsData;
        ShopManager.SetGamePurchased(SaveToFile.LoadFromJson().GamePurchased);

        UnlockManager.CheckSeasonalChapter();

        if (!useCloudSaving) return;
        
        await CloudManager.LoadFromCloud<string>($"{Application.dataPath}/saveData.json");    
    }

    public static async void SaveGameData()
    {
        SaveToFile.SaveChapterData(SaveStaticList);

        if (!useCloudSaving) return;

        await CloudManager.SendToCloud($"{Application.dataPath}/saveData.json", SaveStaticList);
    }

    public static void ResetSaves()
    {
        File.Delete($"{Application.dataPath}/saveData.json");
        File.Delete($"{Application.dataPath}/saveData.json.meta");

        PlayerPrefs.DeleteAll();
    }

    internal static void SetLevelTime(int chapter, int level, float time)
    {
        SaveStaticList[chapter].Levels[level].TimeTaken = time;
    }

    internal static void SetChapterTime(int chapter, float time)
    {
        SaveStaticList[chapter].ChapterTimeTaken = time;
    }
}
