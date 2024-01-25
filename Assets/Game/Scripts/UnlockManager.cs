using System;
using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    internal static void UnlockAllChapters()
    {
        for (int i = 1; i < 6; i++)
        {
            UnlockChapter(i);
        }

        CheckSeasonalChapter();
    }

    internal static void UnlockChapter(int chapter)
    {
        if (chapter < SaveLoadManager.SaveStaticList.Count)
        {
            SaveLoadManager.SaveStaticList[chapter].ChapterUnlocked = true;
        }
    }

    internal static void LockChapter(int chapter)
    {
        SaveLoadManager.SaveStaticList[chapter].ChapterUnlocked = false;
    }

    internal static void LockLevel(int chapter, int level)
    {
        SaveLoadManager.SaveStaticList[chapter].Levels[level].LevelUnlocked = false;
    }

    internal static void UnlockLevel(int chapter, int level)
    {
        var lastChapter = SaveLoadManager.SaveStaticList[chapter];

        if (level < lastChapter.Levels.Count && level >= LevelManager.LastLevelUnlocked)
        {
            lastChapter.Levels[level].LevelUnlocked = true;
            LevelManager.LastLevelUnlocked = level;
        }
    }

    internal static bool IsDateInWinter()
    {
        if (DateTime.Now.Month >= LevelManager.Instance.XmasStartMonth || // 11 >= 11 and <= 12
            DateTime.Now.Month <= LevelManager.Instance.XmasEndMonth) // 11 <= 1
        {
            return true;
        }

        return false;
    }

    internal static void CheckSeasonalChapter()
    {
        if (IsDateInWinter())
        {
            if (ShopManager.GamePurchased)
            {
                SaveLoadManager.SaveStaticList[0].ChapterUnlocked = true;

                Logger.Instance.ShowDebugLog("Xmas chapter unlocked");
            }
            else
            {
                Logger.Instance.ShowDebugLog("Not purchased. Xmas chapter not unlocked");
            }
        }
        else
        {
            SaveLoadManager.SaveStaticList[0].ChapterUnlocked = false;

            Logger.Instance.ShowDebugLog("Xmas chapter locked. Month: " + DateTime.Now.Month);
        }
    }

    internal static bool GetChapterUnlocked(int chapter)
    {
        return SaveLoadManager.SaveStaticList[chapter].ChapterUnlocked;
    }

    internal static bool GetLevelUnlocked(int chapter, int level)
    {
        return SaveLoadManager.SaveStaticList[chapter].Levels[level].LevelUnlocked;
    }
}
