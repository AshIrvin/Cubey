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

        // TODO - if it's winter, check seasonal chapter
    }

    internal static void UnlockChapter(int chapter)
    {
        if (chapter < SaveLoadManager.SaveStaticList.Count)
        {
            SaveLoadManager.SaveStaticList[chapter].ChapterUnlocked = true;
        }
    }

    internal static void UnlockLevel(int level)
    {
        var lastChapter = SaveLoadManager.SaveStaticList[LevelManager.LastChapterPlayed];

        if (level < lastChapter.Levels.Count && level >= LevelManager.LastLevelUnlocked)
        {
            lastChapter.Levels[level].LevelUnlocked = true;
            LevelManager.LastLevelUnlocked = level;
        }
    }

    internal static void UnlockSeasonalChapter()
    {
        if (DateTime.Now.Month >= LevelManager.Instance.XmasStartMonth || // 11 >= 11 and <= 12
            DateTime.Now.Month <= LevelManager.Instance.XmasEndMonth) // 11 <= 1
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
}
