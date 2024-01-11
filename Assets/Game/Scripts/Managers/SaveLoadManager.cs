using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[Serializable]
public class SaveLoadManager : MonoBehaviour
{
    public enum Awards
    {
        NoAward,
        OneStar,
        TwoStars,
        ThreeStars
    }

    [SerializeField] private SaveMetaData saveMetaData;
    
    public List<ChapterLevelData> showSaveData = new (6);

    public static List<ChapterLevelData> SaveStaticList;
    public static bool GamePurchased = false;
    public static bool useCloudSaving;
    private static int lastChapterPlayed;
    private static int chapterLevelSaved;

    public static readonly int ChapterAmount = 6;
    private static readonly int LevelAmount = 30;
    private readonly int xmasStartMonth = 10;
    private readonly int xmasEndMonth = 1;

    private readonly bool deleteAllSaves = false; // TODO - keep testing this till no errors. Follow

    #region Getters

    public static int LastLevelPlayed
    {
        get => SaveStaticList[lastChapterPlayed].LastLevelPlayed;
        set => SaveStaticList[lastChapterPlayed].LastLevelPlayed = value;
    }

    public static int LastLevelUnlocked
    {
        get => SaveStaticList[lastChapterPlayed].LastLevelUnlocked;
        set => SaveStaticList[lastChapterPlayed].LastLevelUnlocked = value;
    }

    public static int LastChapterPlayed
    {
        //get => SaveGame.Load("LastChapterPlayed", 1);
        get => 1;// SaveToFile.LoadFromJson("LastChapterPlayed", 1);
        set
        {
            lastChapterPlayed = LastChapterPlayed;
            //SaveGame.Save("LastChapterPlayed", value);
            //SaveToFile.SaveToJson("LastChapterPlayed", value);
        }
    }

    public static int ChapterLevelSaved 
    { 
        get => chapterLevelSaved;
        set => chapterLevelSaved = value;
    }

    #endregion Getters

    private void Awake()
    {
        LoadGamePurchased();

        if (deleteAllSaves)
            ResetSaves();

        SaveToFile.OnFirstTimeUse += CreateSaves;
        
        //if (GamePurchased)
            //UnlockedPurchasedChapters();

        //UnlockSeasonalChapter();
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
            //CreateSaves();

        }
    }

    private void UnlockedPurchasedChapters()
    {
        showSaveData[4].ChapterUnlocked = true;
        showSaveData[5].ChapterUnlocked = true;
        
        UnlockChapter(4);
        UnlockChapter(5);
        SaveGameData();
    }
    
    public void PurchasedAndUnlockFestiveChapter()
    {
        UnlockSeasonalChapter();
    }

    private void UnlockSeasonalChapter()
    {
        if (DateTime.Now.Month >= xmasStartMonth || // 11 >= 11 and <= 12
            DateTime.Now.Month <= xmasEndMonth) // 11 <= 1
        {
            if (LoadGamePurchased())
            {
                SaveStaticList[0].ChapterUnlocked = true;

                Logger.Instance.ShowDebugLog("Xmas chapter unlocked");
            }
            else
            {
                Logger.Instance.ShowDebugLog("Not purchased. Xmas chapter not unlocked");
            }
        }
        else
        {
            SaveStaticList[0].ChapterUnlocked = false;

            Logger.Instance.ShowDebugLog("Xmas chapter locked. Month: " + DateTime.Now.Month);
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
        
        if (useCloudSaving)
        {
            //await LoadFromCloud<string>($"SaveChapters{i}.txt");
        }     
    }

    public static async void SaveGameData()
    {
        SaveToFile.SaveChapterData(SaveStaticList);

        for (int i = 0; i < 6; i++)
        {
            //allChapterData.Add(SaveStaticList[i]);
            //SaveToFile.SaveChapterData(SaveStaticList[i]);

            if (useCloudSaving)
            {
                await CloudManager.SendToCloud($"SaveChapters{i}.txt", SaveStaticList[i]);
            }
        }
    }
    
    /// <summary>
    /// Pass chapter and level to get award. 1 = bronze. 3 = gold
    /// </summary>
    /// <param name="level"></param>
    public static int GetAwards(int level)
    {
        return SaveStaticList[lastChapterPlayed].Levels[level].StarsReceived;
    }

    public static void SetAward(int level, Awards awardType)
    { // check current award, so to not add too many!
        int currentAward = GetAwards(level);
        int remainingAward = (int)awardType - currentAward;
        
        if (remainingAward > 0)
        {
            SaveStaticList[lastChapterPlayed].AllStars += remainingAward;
            SaveStaticList[lastChapterPlayed].Levels[level].Stars += remainingAward;
            SaveStaticList[lastChapterPlayed].Levels[level].StarsReceived += remainingAward;
        }
    }
    
    /// <summary>
    /// Only get level award
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static int GetLevelAward(int level)
    {
        return SaveStaticList[lastChapterPlayed].Levels[level].Stars;
    }

    /// <summary>
    /// Get all awards for the chapter
    /// </summary>
    /// <param name="chapter"></param>
    /// <param name="awardType"></param>
    /// <returns></returns>
    public static int GetChapterAward(int chapter, Awards awardType)
    {
        return SaveStaticList[chapter].AllStars;
    }
    
    /// <summary>
    /// n = chapter to unlock. This required any more??
    /// </summary>
    /// <param name="n"></param>
    public static void UnlockChapter(int chapter)
    {
        if (chapter < ChapterAmount)
        {
            SaveStaticList[chapter].ChapterUnlocked = true;
        }
    }
    
    /// <summary>
    /// Checks the level that needs unlocked
    /// </summary>
    /// <param name="level"></param>
    public static void UnlockLevel(int level)
    {
        if (level < LevelAmount && level >= LastLevelUnlocked)
        {
            SaveStaticList[lastChapterPlayed].Levels[level].LevelUnlocked = true;
            LastLevelUnlocked = level;
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
        // Todo: update to new save file system

        File.Delete($"{Application.dataPath}/saveData.json");
        File.Delete($"{Application.dataPath}/saveData.json.meta");


        //if (SaveGame.Exists($"SaveChapters{0}.txt"))
        //    SaveGame.Delete($"SaveChapters{0}.txt");
        
        //if (SaveGame.Exists("LastChapterPlayed"))
        //    SaveGame.Delete("LastChapterPlayed");

        //if (PlayerPrefs.HasKey("levelsPlayed"))
        //    PlayerPrefs.DeleteKey("levelsPlayed");
        
        //if (SaveGame.Exists("GamePurchased"))
        //    SaveGame.Delete("GamePurchased");

        //LastLevelUnlocked = 0;
    }

    public static void SaveGamePurchased(bool state)
    {
        GamePurchased = state;
        //SaveGame.Save("GamePurchased", state);
        //SaveToFile.SaveToJson("GamePurchased", state);
    }

    public static bool LoadGamePurchased()
    {
        //GamePurchased = SaveGame.Load("GamePurchased", false);
        //GamePurchased = SaveToFile.LoadFromJson("GamePurchased", false);
        return GamePurchased;
    }

    public static void LevelTimeTaken(int chapter, int level, float time)
    {
        float t = SaveStaticList[chapter].Levels[level].TimeTaken;
        if (t == 0)
        {
            t = 1000;
        }
        
        if (time < t)
        {
            SaveStaticList[chapter].Levels[level].TimeTaken = time;
        }

        ChapterTimeTaken(chapter, level, time);
        
        Logger.Instance.ShowDebugLog("Level time taken: " + time + ", previous: " + t);
    }
    
    public static void ChapterTimeTaken(int chapter, int level, float time)
    {
        float totalChapterTime = 0;
        for (int i = 0; i < SaveStaticList[chapter].Levels.Count; i++)
        {
            totalChapterTime += SaveStaticList[chapter].Levels[i].TimeTaken;
        }

        SaveStaticList[chapter].ChapterTimeTaken = totalChapterTime; 
        
        Logger.Instance.ShowDebugLog("Total chapter time? " + totalChapterTime);
    }
}
