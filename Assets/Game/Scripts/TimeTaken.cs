using System;
using UnityEngine;

public class TimeTaken : MonoBehaviour
{
    public enum TimeAction
    {
        Start,
        Stop
    }

    private static float timeStarted = 0;
    private static float durationInLevel;

    internal static void TimeDuration(TimeAction timeAction, Action callback = null)
    {
        switch (timeAction)
        {
            case TimeAction.Start:
                Logger.Instance.ShowDebugLog("Time started");
                timeStarted = Time.time;
                break;
            case TimeAction.Stop:
                float duration = Mathf.Abs(timeStarted - Time.time);
                duration = Mathf.Round(duration * 100) / 100;
                LevelTimeTaken(LevelManager.LastChapterPlayed, LevelManager.LastLevelPlayed, duration);
                callback?.Invoke();
                break;
        }
    }

    internal static void HasTimerStarted(bool state)
    {
        if (state)
        {
            timeStarted = Time.time;
            return;
        }

        float duration = Mathf.Abs(timeStarted - Time.time);
        durationInLevel = Mathf.Round(duration * 100) / 100;
        LevelTimeTaken(LevelManager.LastChapterPlayed, LevelManager.LastLevelPlayed, durationInLevel);
    }

    private static void LevelTimeTaken(int chapter, int level, float time)
    {
        float t = SaveLoadManager.SaveStaticList[chapter].Levels[level].TimeTaken;

        if (t == 0)
        {
            t = 1000;
        }

        if (time < t)
        {
            SaveLoadManager.SetLevelTime(chapter, level, time);
        }

        ChapterTimeTaken(chapter);

        Logger.Instance.ShowDebugLog("Level time taken: " + time + ", previous: " + t);
    }

    private static void ChapterTimeTaken(int chapter)
    {
        float totalChapterTime = 0;
        for (int i = 0; i < SaveLoadManager.SaveStaticList[chapter].Levels.Count; i++)
        {
            totalChapterTime += SaveLoadManager.SaveStaticList[chapter].Levels[i].TimeTaken;
        }

        SaveLoadManager.SetChapterTime(chapter, totalChapterTime);

        Logger.Instance.ShowDebugLog("Total chapter time? " + totalChapterTime);
    }
}
