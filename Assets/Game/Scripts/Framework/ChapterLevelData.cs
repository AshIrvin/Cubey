using System;
using System.Collections.Generic;

[System.Serializable]
public class ChapterLevelData
{   
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
