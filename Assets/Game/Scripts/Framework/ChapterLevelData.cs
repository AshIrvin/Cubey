using System;
using System.Collections.Generic;

[Serializable]
public class ChapterLevelData
{
    public int ChapterNumber;
    public bool ChapterUnlocked;
    public int LastLevelPlayed;
    public int LastLevelUnlocked;
    public int AllBronze;
    public int AllSilver;
    public int AllStars;
    public float ChapterTimeTaken;
    
    public List<LevelInfo> Levels;

    public ChapterLevelData(int number)
    {
        ChapterNumber = number;
    }

    [Serializable]
    public class LevelInfo
    {
        public bool LevelUnlocked;
        public int StarsReceived = 0;
        public int Bronze = 0;
        public int Silver;
        public int Stars;
        public float TimeTaken;
        public int JumpsUsed;
    }
}

public class ChapterData
{
    public List<ChapterLevelData> ChapterLevelsData;
    public bool GamePurchased = false;
}