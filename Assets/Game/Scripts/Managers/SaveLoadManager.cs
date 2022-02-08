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
    // Save
        // Chapter and Level number
        // Last level unlocked
        // Last level played
        // Last Chapter played
    // Awards
        // Received for each level
        // Total for each chapter
    
    // Load
        // Chapter and level number
    // Awards
        // what award for each level
        // How many for the chapter
    //
    
    public enum Awards
    {
        NoAward,
        Bronze,
        Silver,
        Gold
    }
    
    public List<ChapterLevelData> showSaveData = new (6);
    public static List<ChapterLevelData> SaveStaticList;
    
    private static int lastChapterLevelPlayed;
    // private static int lastLevelPlayed;
    private static int lastLevelUnlocked;
    private static int lastChapterPlayed;
    private static int chapterLevelSaved;
    private static int awardsReceived;

    [SerializeField] private bool viewSavesInInspector;
    
    /// <summary>
    /// Returns a 3 digit chapter and level number. eg: 203, 415 etc
    /// </summary>
    public static int LastChapterLevelPlayed => lastChapterLevelPlayed;

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
        if (SaveGame.Exists($"SaveChapters{0}.txt"))
        {
            SetupSaveClass();
            LoadSaves();            
        }
        else
        {
            Debug.Log("No save found");
            SetupSaveClass();
            FirstTimeUse();
            SaveGameInfo();
        }
    }

    public int chapterAmount = 6;
    public static int ChapterAmount = 6;
    public int levelAmount = 30;
    public static int LevelAmount = 30;

    private void FirstTimeUse()
    {
        Debug.Log("Setting up 1st time use");
        SaveStaticList[0].chapterUnlocked = false;
        SaveStaticList[1].chapterUnlocked = true;
        
        for (int i = 0; i < chapterAmount; i++)
        {
            SaveStaticList[i].levels[0].levelUnlocked = true;
            SaveStaticList[i].lastLevelUnlocked = 0;
        }

        if (viewSavesInInspector)
        {
            showSaveData[0].chapterUnlocked = false;
            showSaveData[1].chapterUnlocked = true;
            
            for (int i = 0; i < chapterAmount; i++)
            {
                showSaveData[i].levels[0].levelUnlocked = true;
            }
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
        Debug.Log("Saving game at... " + SaveGame.PersistentDataPath);

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
        return SaveStaticList[lastChapterPlayed].levels[level].awardsReceived;
    }

    public static void SetAward(int level, Awards awardType)
    {
        // if (SaveStaticList[lastChapterPlayed].levels[level].awardsReceived < 3)
           // SaveStaticList[lastChapterPlayed].levels[level].awardsReceived += 1;

        switch (awardType)
        {
            case Awards.NoAward:
                SaveStaticList[lastChapterPlayed].allBronze += 1;
                SaveStaticList[lastChapterPlayed].levels[level].bronze += 1;
                SaveStaticList[lastChapterPlayed].levels[level].awardsReceived = 1;
                break;
            case Awards.Silver:
                SaveStaticList[lastChapterPlayed].allSilver += 1;
                SaveStaticList[lastChapterPlayed].levels[level].silver += 1;
                SaveStaticList[lastChapterPlayed].levels[level].awardsReceived = 2;
                break;
            case Awards.Gold:
                SaveStaticList[lastChapterPlayed].allGold += 1;
                SaveStaticList[lastChapterPlayed].levels[level].gold += 1;
                SaveStaticList[lastChapterPlayed].levels[level].awardsReceived = 3;
                break;
        }
    }
    
    /// <summary>
    /// Only get level award
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static int GetLevelAward(int level, Awards awardType)
    {
        switch (awardType)
        {
            case Awards.NoAward:
                // no award
                break;
            case Awards.Bronze:
                return SaveStaticList[lastChapterPlayed].levels[level].bronze;
                break;
            case Awards.Silver:
                return SaveStaticList[lastChapterPlayed].levels[level].silver;
                break;
            case Awards.Gold:
                return SaveStaticList[lastChapterPlayed].levels[level].gold;
                break;
        }
        return 0;
    }

    public static int GetChapterAward(int chapter, Awards awardType)
    {
        switch (awardType)
        {
            case Awards.NoAward:
                // no award
                break;
            case Awards.Bronze:
                return SaveStaticList[chapter].allBronze;
                break;
            case Awards.Silver:
                return SaveStaticList[chapter].allSilver;
                break;
            case Awards.Gold:
                return SaveStaticList[chapter].allGold;
                break;
        }
        return 0;
    }
    
    /// <summary>
    /// n = chapter to unlock
    /// </summary>
    /// <param name="n"></param>
    public static void UnlockChapter(int chapter)
    {
        if (chapter < ChapterAmount)
        {
            SaveStaticList[chapter].chapterUnlocked = true;
            Debug.Log("Unlocking chapter: " + chapter);
            SaveGameInfo();
        }
    }
    
    /// <summary>
    /// Checks the level that needs unlocked
    /// </summary>
    /// <param name="level"></param>
    public static void UnlockLevel(int level)
    {
        if (level < LevelAmount)
        {
            SaveStaticList[lastChapterPlayed].levels[level].levelUnlocked = true;
            Debug.Log("Unlocking chapter level: " + lastChapterPlayed + "." + level);
            SaveGameInfo();
        }
    }
    
    public static bool GetChapterUnlocked(int chapter)
    {
        for (int i = 0; i < SaveStaticList.Count; i++)
        {
            if (SaveStaticList[chapter].chapterUnlocked)
            {
                Debug.Log("chapter unlocked: " + chapter);
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

        LastLevelUnlocked = 0;
    }
}
