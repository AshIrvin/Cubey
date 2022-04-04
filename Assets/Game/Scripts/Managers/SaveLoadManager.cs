using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using BayatGames.SaveGamePro;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
    
    private static int lastChapterLevelPlayed;
    private static int lastLevelUnlocked;
    private static int lastChapterPlayed;
    private static int chapterLevelSaved;
    private static int awardsReceived;

    public int chapterAmount = 6;
    public static int ChapterAmount = 6;
    public int levelAmount = 30;
    public static int LevelAmount = 30;
    public static bool GamePurchased = false;

    public bool deleteAllSaves;
    
    [SerializeField] private bool viewSavesInInspector;

    private int xmasStartMonth = 11;
    private int xmasEndMonth = 1;


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
            Debug.Log("No save found");
            SetupSaveClass();
            FirstTimeUse();
            SaveGameInfo();
        }
        
        if (GamePurchased)
            UnlockedPurchasedChapters();
    }

    private void FirstTimeUse()
    {
        Debug.Log("Setting up 1st time use");
        SaveStaticList[0].chapterUnlocked = false;
        SaveStaticList[1].chapterUnlocked = true;
        SaveStaticList[2].chapterUnlocked = true;
        SaveStaticList[3].chapterUnlocked = true;
        
        for (int i = 0; i < chapterAmount; i++)
        {
            SaveStaticList[i].levels[0].levelUnlocked = true;
            SaveStaticList[i].lastLevelUnlocked = 0;
        }

        if (viewSavesInInspector)
        {
            showSaveData[0].chapterUnlocked = false;
            showSaveData[1].chapterUnlocked = true;
            showSaveData[2].chapterUnlocked = true;
            showSaveData[3].chapterUnlocked = true;
            
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
    
    private void UnlockSeasonalChapter()
    {
        if (DateTime.Now.Month >= xmasStartMonth &&
            DateTime.Now.Month <= xmasEndMonth && LoadGamePurchased())
        {
            // TODO - show popup once that it's been unlocked for purchasers 
            SaveStaticList[0].chapterUnlocked = true;
            showSaveData[0].chapterUnlocked = true;
        }
        else
        {
            SaveStaticList[0].chapterUnlocked = false;
            showSaveData[0].chapterUnlocked = false;
        }
    }
    
    private void SetupSaveClass()
    {
        SaveStaticList = new List<ChapterLevelData>(chapterAmount);
        if (viewSavesInInspector) showSaveData = new List<ChapterLevelData>(chapterAmount);

        for (int i = 0; i < chapterAmount; i++)
        {
            if (viewSavesInInspector) 
                showSaveData.Add(new ChapterLevelData());
            
            SaveStaticList.Add(new ChapterLevelData());
            
            if (viewSavesInInspector) 
                showSaveData[i].levels = new List<ChapterLevelData.LevelInfo>(levelAmount);
            
            SaveStaticList[i].levels = new List<ChapterLevelData.LevelInfo>(levelAmount);
            
            for (int j = 0; j < levelAmount; j++)
            {
                if (viewSavesInInspector) 
                    showSaveData[i].levels.Add(new ChapterLevelData.LevelInfo());
                
                SaveStaticList[i].levels.Add(new ChapterLevelData.LevelInfo());
            }   
        }
    }

    private void LoadSaves()
    {
        Debug.Log("Found save, loading...");
        for (int i = 0; i < chapterAmount; i++)
        {
            SaveStaticList[i] = SaveGame.Load<ChapterLevelData>($"SaveChapters{i}.txt");
            showSaveData[i] = SaveGame.Load<ChapterLevelData>($"SaveChapters{i}.txt");
        }
    }
    
    public static void SaveGameInfo()
    {
        // Debug.Log("Saving game at... " + SaveGame.PersistentDataPath);

        for (int i = 0; i < 6; i++)
        {
            SaveGame.Save($"SaveChapters{i}.txt", SaveStaticList[i]);
        }
    }

    private void RefreshShowSaveData()
    {
        for (int i = 0; i < chapterAmount; i++)
        {
            showSaveData[i] = SaveGame.Load<ChapterLevelData>($"SaveChapters{i}.txt");
        }
    }

    public static void SaveChapterAndLevel(int n)
    {
        chapterLevelSaved = n;
    }
    
    public static void SaveLastChapterLevelPlayed(int n)
    {
        lastChapterLevelPlayed = n;
    }
    
    /// <summary>
    /// Pass chapter and level to get award. 1 = bronze. 3 = gold
    /// </summary>
    /// <param name="level"></param>
    public static int GetAwards(int level)
    {
        return SaveStaticList[lastChapterPlayed].levels[level].starsReceived;
    }

    public static void SetAward(int level, Awards awardType)
    {
        // check current award, so to not add too many!
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
        
        /*switch (awardType)
        {
            case Awards.NoAward:
                // no award
                break;
            case Awards.OneStar:
                return SaveStaticList[lastChapterPlayed].levels[level].bronze;
                break;
            case Awards.TwoStars:
                return SaveStaticList[lastChapterPlayed].levels[level].silver;
                break;
            case Awards.ThreeStars:
                return SaveStaticList[lastChapterPlayed].levels[level].stars;
                break;
        }
        return 0;*/
    }

    public static int GetChapterAward(int chapter, Awards awardType)
    {
        return SaveStaticList[chapter].allStars;
        
        /*switch (awardType)
        {
            case Awards.NoAward:
                // no award
                break;
            case Awards.OneStar:
                return SaveStaticList[chapter].allBronze;
                break;
            case Awards.TwoStars:
                return SaveStaticList[chapter].allSilver;
                break;
            case Awards.ThreeStars:
                return SaveStaticList[chapter].allStars;
                break;
        }
        return 0;*/
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
            SaveGameInfo();
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
        float t = SaveGame.Load("TimeTaken" + level, 0);
        if (time < t)
        {
            
            SaveGame.Save("TimeTaken" + level, time);
        }

        Debug.Log("Level time taken: " + time + ", previous time: " + t);
    }
    
    public static void ChapterTimeTaken(int chapter, float time)
    {
        SaveGame.Save("TimeTaken" + chapter, time);
    }
}
