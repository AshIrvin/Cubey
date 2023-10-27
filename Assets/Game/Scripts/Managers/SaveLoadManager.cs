using System;
using System.Collections.Generic;
using BayatGames.SaveGamePro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;

[System.Serializable]
public class SaveLoadManager : MonoBehaviour
{
    public enum Awards
    {
        NoAward,
        OneStar,
        TwoStars,
        ThreeStars
    }
    
    public List<ChapterLevelData> showSaveData = new (6);
    public static List<ChapterLevelData> SaveStaticList;

    [SerializeField] private SaveMetaData saveMetaData;

    private static int lastChapterPlayed;
    private static int chapterLevelSaved;

    private int xmasStartMonth = 10;
    private int xmasEndMonth = 1;
    private bool useCloudSaves = false;

    public int chapterAmount = 6;
    public int levelAmount = 30;
    public bool deleteAllSaves;

    public static int ChapterAmount = 6;
    public static int LevelAmount = 30;
    public static bool GamePurchased = false;

    [SerializeField] private bool viewSavesInInspector;

    #region Getters

    public static int LastLevelPlayed
    {
        get => SaveStaticList[lastChapterPlayed].lastLevelPlayed;
        set => SaveStaticList[lastChapterPlayed].lastLevelPlayed = value;
    }

    public static int LastLevelUnlocked
    {
        get => SaveStaticList[lastChapterPlayed].lastLevelUnlocked;
        set => SaveStaticList[lastChapterPlayed].lastLevelUnlocked = value;
    }

    public static int LastChapterPlayed
    {
        get => SaveGame.Load<int>("LastChapterPlayed", 1);
        set
        {
            lastChapterPlayed = LastChapterPlayed;
            SaveGame.Save("LastChapterPlayed", value);
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

        if (SaveGame.Exists($"SaveChapters{0}.txt"))
        {
            SetupSaveClass();
            
            if (deleteAllSaves)
                ResetSaves();
            
            LoadSaves();            
        }
        else
        {
            Logger.Instance.ShowDebugLog("No save found");
            SetupSaveClass();
            FirstTimeUse();
            SaveGameInfo();
        }
        
        if (GamePurchased)
            UnlockedPurchasedChapters();

        UnlockSeasonalChapter();
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public void EnableCloudSaving(bool state)
    {
        // This should be a switch in the settings UI. On as default
    }

    private void SaveToCloud(string text, object save)
    {
        var data = new Dictionary<string, object> { { text, save } };
        
        Task task = CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    // TODO - Finish implementing cloud save/load
    private void LoadFromCloud()
    {
        Task task = CloudSaveService.Instance.Data.Player.LoadAllAsync();
    }

    private void FirstTimeUse()
    {
        if (SaveStaticList.Count == 0)
        {
            Logger.Instance.ShowDebugError("SaveStaticList not created. Old data?");
            return;
        }
        
        Logger.Instance.ShowDebugLog("Setting up 1st time use");
        SaveStaticList[0].chapterUnlocked = false;
        SaveStaticList[1].chapterUnlocked = true;
        SaveStaticList[2].chapterUnlocked = false;
        SaveStaticList[3].chapterUnlocked = false;
        
        for (int i = 0; i < chapterAmount; i++)
        {
            SaveStaticList[i].levels[0].levelUnlocked = true;
            SaveStaticList[i].lastLevelUnlocked = 0;
        }

        if (viewSavesInInspector)
        {
            showSaveData[0].chapterUnlocked = false;
            showSaveData[1].chapterUnlocked = true;
            showSaveData[2].chapterUnlocked = false;
            showSaveData[3].chapterUnlocked = false;
            
            for (int i = 0; i < chapterAmount; i++)
            {
                showSaveData[i].levels[0].levelUnlocked = true;
            }
        }
    }

    private void UnlockedPurchasedChapters()
    {
        showSaveData[4].chapterUnlocked = true;
        showSaveData[5].chapterUnlocked = true;
        
        UnlockChapter(4);
        UnlockChapter(5);
        SaveGameInfo();
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
                SaveStaticList[0].chapterUnlocked = true;

                if (viewSavesInInspector)
                { 
                    showSaveData[0].chapterUnlocked = true; 
                }

                Logger.Instance.ShowDebugLog("Xmas chapter unlocked");
            }
            else
            {
                Logger.Instance.ShowDebugLog("Not purchased. Xmas chapter not unlocked");
            }
        }
        else
        {
            SaveStaticList[0].chapterUnlocked = false;

            if (viewSavesInInspector)
            { 
                showSaveData[0].chapterUnlocked = false; 
            }

            Logger.Instance.ShowDebugLog("Xmas chapter locked. Month: " + DateTime.Now.Month);
        }
    }
    
    private void SetupSaveClass()
    {
        SaveStaticList = new List<ChapterLevelData>(chapterAmount);

        if (viewSavesInInspector) showSaveData = new List<ChapterLevelData>(chapterAmount);

        for (int i = 0; i < chapterAmount; i++)
        {
            if (viewSavesInInspector)
            {
                showSaveData.Add(new ChapterLevelData());
            }
            
            SaveStaticList.Add(new ChapterLevelData());
            
            if (viewSavesInInspector)
            {
                showSaveData[i].levels = new List<ChapterLevelData.LevelInfo>(levelAmount);
            }
            
            SaveStaticList[i].levels = new List<ChapterLevelData.LevelInfo>(levelAmount);
            
            for (int j = 0; j < levelAmount; j++)
            {
                if (viewSavesInInspector)
                {
                    showSaveData[i].levels.Add(new ChapterLevelData.LevelInfo());
                }
                
                SaveStaticList[i].levels.Add(new ChapterLevelData.LevelInfo());
            }   
        }
    }

    private void LoadSaves()
    {
        Logger.Instance.ShowDebugLog($"Found save, {(useCloudSaves ? "loading cloud saves." : "loading local saves.")}");

        if (useCloudSaves)
        {
            GetCloudSaves();
            return;
        }

        GetLocalSaves();
    }

    private void GetCloudSaves()
    {

    }

    private void GetLocalSaves()
    {
        for (int i = 0; i < chapterAmount; i++)
        {
            SaveStaticList[i] = SaveGame.Load<ChapterLevelData>($"SaveChapters{i}.txt");

            if (viewSavesInInspector)
            {
                showSaveData[i] = SaveGame.Load<ChapterLevelData>($"SaveChapters{i}.txt");
            }
        }

    }
    
    public static void SaveGameInfo()
    {
        Logger.Instance.ShowDebugLog("Saving game at... " + SaveGame.PersistentDataPath);

        for (int i = 0; i < 6; i++)
        {
            SaveGame.Save($"SaveChapters{i}.txt", SaveStaticList[i]);
        }
    }

    // TODO - Still needed?
/*    private void RefreshShowSaveData()
    {
        for (int i = 0; i < chapterAmount; i++)
        {
            showSaveData[i] = SaveGame.Load<ChapterLevelData>($"SaveChapters{i}.txt");
        }
    }*/

    // TODO - Still needed?
    public static void SaveChapterAndLevel(int n)
    {
        chapterLevelSaved = n;
    }
    
    // TODO - Still needed?
/*    public static void SaveLastChapterLevelPlayed(int n)
    {
        lastChapterLevelPlayed = n;
    }*/
    
    /// <summary>
    /// Pass chapter and level to get award. 1 = bronze. 3 = gold
    /// </summary>
    /// <param name="level"></param>
    public static int GetAwards(int level)
    {
        return SaveStaticList[lastChapterPlayed].levels[level].starsReceived;
    }

    public static void SetAward(int level, Awards awardType)
    { // check current award, so to not add too many!
        int currentAward = GetAwards(level);
        int remainingAward = (int)awardType - currentAward;
        
        if (remainingAward > 0)
        {
            SaveStaticList[lastChapterPlayed].allStars += remainingAward;
            SaveStaticList[lastChapterPlayed].levels[level].stars += remainingAward;
            SaveStaticList[lastChapterPlayed].levels[level].starsReceived += remainingAward;
        }
    }
    
    /// <summary>
    /// Only get level award
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static int GetLevelAward(int level)
    {
        return SaveStaticList[lastChapterPlayed].levels[level].stars;
    }

    /// <summary>
    /// Get all awards for the chapter
    /// </summary>
    /// <param name="chapter"></param>
    /// <param name="awardType"></param>
    /// <returns></returns>
    public static int GetChapterAward(int chapter, Awards awardType)
    {
        return SaveStaticList[chapter].allStars;
    }
    
    /// <summary>
    /// n = chapter to unlock. This required any more??
    /// </summary>
    /// <param name="n"></param>
    public static void UnlockChapter(int chapter)
    {
        if (chapter < ChapterAmount)
        {
            SaveStaticList[chapter].chapterUnlocked = true;
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
            SaveStaticList[lastChapterPlayed].levels[level].levelUnlocked = true;
            LastLevelUnlocked = level;
            // SaveGameInfo();
        }
    }
    
    public static bool GetChapterUnlocked(int chapter)
    {
        for (int i = 0; i < SaveStaticList.Count; i++)
        {
            if (SaveStaticList[chapter].chapterUnlocked)
            {
                return true;
            }
        }
        return false;
    }

    public static void ResetSaves()
    {
        if (SaveGame.Exists($"SaveChapters{0}.txt"))
            SaveGame.Delete($"SaveChapters{0}.txt");
        
        if (SaveGame.Exists("LastChapterPlayed"))
            SaveGame.Delete("LastChapterPlayed");

        if (PlayerPrefs.HasKey("levelsPlayed"))
            PlayerPrefs.DeleteKey("levelsPlayed");
        
        if (SaveGame.Exists("GamePurchased"))
            SaveGame.Delete("GamePurchased");

        LastLevelUnlocked = 0;
    }

    public static void SaveGamePurchased(bool state)
    {
        GamePurchased = state;
        SaveGame.Save("GamePurchased", state);
    }

    public static bool LoadGamePurchased()
    {
        GamePurchased = SaveGame.Load("GamePurchased", false);
        return GamePurchased;
    }

    public static void LevelTimeTaken(int chapter, int level, float time)
    {
        float t = SaveStaticList[chapter].levels[level].timeTaken;
        if (t == 0)
        {
            t = 1000;
        }
        
        if (time < t)
        {
            SaveStaticList[chapter].levels[level].timeTaken = time;
        }

        ChapterTimeTaken(chapter, level, time);
        
        Logger.Instance.ShowDebugLog("Level time taken: " + time + ", previous: " + t);
    }
    
    public static void ChapterTimeTaken(int chapter, int level, float time)
    {
        float totalChapterTime = 0;
        for (int i = 0; i < SaveStaticList[chapter].levels.Count; i++)
        {
            totalChapterTime += SaveStaticList[chapter].levels[i].timeTaken;
        }

        SaveStaticList[chapter].chapterTimeTaken = totalChapterTime; 
        
        Logger.Instance.ShowDebugLog("Total chapter time? " + totalChapterTime);
    }
}
