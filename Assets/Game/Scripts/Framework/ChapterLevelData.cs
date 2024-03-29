using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChapterLevelData
{
    /// <summary>
    /// Object for creating chapters and the levels assigned to them
    /// </summary>
    
    public bool chapterUnlocked;
    public int lastLevelPlayed;
    public int lastLevelUnlocked;
    
    public int allBronze;
    public int allSilver;
    public int allStars;
    public float chapterTimeTaken;
    
    public List<LevelInfo> levels;

    [Serializable]
    public class LevelInfo
    {
        public bool levelUnlocked;
        
        public int starsReceived;
        public int bronze;
        public int silver;
        public int stars;
        public float timeTaken;
        public int jumpsUsed;
    }
}
