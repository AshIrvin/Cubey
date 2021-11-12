using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
    [SerializeField] private SaveMetaData saveMetaData;
    [SerializeField] private LevelMetaData levelMetaData;
    [SerializeField] private ChapterList chapterList;
    
    [SerializeField] private float timer;
    [SerializeField] private string levelName;
    
    [Header("Level Medal")]
    [SerializeField] private int gold;
    [SerializeField] private int silver;
    [SerializeField] private int bronze;
    
    [SerializeField] private int levelNo;
    [SerializeField] private int chapterNo;
    
    private void Awake()
    {
        levelNo = saveMetaData.LastLevelPlayed;
        chapterNo = saveMetaData.LastChapterPlayed;
        levelMetaData = chapterList[saveMetaData.LastChapterPlayed].LevelList[saveMetaData.LastLevelPlayed];
        
        timer = levelMetaData.Timer;
        bronze = levelMetaData.JumpsForBronze;
        silver = levelMetaData.JumpsForSilver;
        gold = levelMetaData.JumpsForGold;
        levelName = levelMetaData.LevelName;
    }
    
    
}
