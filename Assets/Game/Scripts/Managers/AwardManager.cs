using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwardManager : MonoBehaviour
{
    public static AwardManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void SetAwardForLevel(SaveLoadManager.Awards award)
    {
        int chapter = SaveLoadManager.LastChapterPlayed;
        int level = SaveLoadManager.LastLevelPlayed;

        SetStarAward(level, award);
    }

    public void SetStarAward(int level, SaveLoadManager.Awards award)
    {
        var levelAward = SaveLoadManager.GetLevelAward(level);

        if (levelAward < (int)award)
            SaveLoadManager.SetAward(level, award);
    }


}
