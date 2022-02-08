using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChapterLevelData
{
    public bool chapterUnlocked;
    public int lastLevelPlayed;
    public int lastLevelUnlocked;
    
    public int allBronze;
    public int allSilver;
    public int allGold;

    public List<LevelInfo> levels;

    [Serializable]
    public class LevelInfo
    {
        public bool levelUnlocked;
        
        public int awardsReceived;
        public int bronze;
        public int silver;
        public int gold;
    }
}
