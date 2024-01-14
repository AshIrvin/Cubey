using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[Serializable]
public class SaveLoadManager : MonoBehaviour
{
    [SerializeField] private SaveMetaData saveMetaData;
    
    public List<ChapterLevelData> showSaveData = new (6);

    public static List<ChapterLevelData> SaveStaticList;
    public static bool useCloudSaving;    

    //internal static bool GamePurchased = false;

    private static int chapterLevelSaved;
    private readonly bool deleteAllSaves = false; // TODO - keep testing this till no errors. Follow

    #region Getters

    public static int ChapterLevelSaved 
    { 
        get => chapterLevelSaved;
        set => chapterLevelSaved = value;
    }

    #endregion Getters

    private void Awake()
    {
        //GetGamePurchased();

        if (deleteAllSaves)
            ResetSaves();

        SaveToFile.OnFirstTimeUse += CreateSaves;
        
        //if (GamePurchased)
        //    UnlockManager.UnlockAllChapters();

        //UnlockManager.UnlockSeasonalChapter();
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

    private void LoadSaves()
    {
        Logger.Instance.ShowDebugLog($"LoadSaves. Found save, {(useCloudSaving ? "loading cloud saves." : "loading local saves.")}");

        SaveStaticList = SaveToFile.LoadFromJson().ChapterLevelsData;
        ShopManager.GamePurchased = SaveToFile.LoadFromJson().GamePurchased;

        if (useCloudSaving)
        {
            //await LoadFromCloud<string>($"SaveChapters{i}.txt");
        }     
    }

    public static async void SaveGameData()
    {
        SaveToFile.SaveChapterData(SaveStaticList);

        if (!useCloudSaving) return;

        for (int i = 0; i < 6; i++)
        {
            await CloudManager.SendToCloud($"SaveChapters{i}.txt", SaveStaticList[i]);
        }
    }

    public static bool GetChapterUnlocked(int chapter)
    {
        for (int i = 0; i < SaveStaticList.Count; i++)
        {
            if (SaveStaticList[chapter].ChapterUnlocked)
            {
                return true;
            }
        }
        return false;
    }

    public static void ResetSaves()
    {
        File.Delete($"{Application.dataPath}/saveData.json");
        File.Delete($"{Application.dataPath}/saveData.json.meta");

        //LastLevelUnlocked = 0;
    }
}
