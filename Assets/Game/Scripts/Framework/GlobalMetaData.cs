using UnityEngine;

[DefaultExecutionOrder(0)]
public class GlobalMetaData : MonoBehaviour
{
    public static GlobalMetaData Instance;

    public ChapterList ChapterList;
    private LevelMetaData levelMetaData;

    public LevelMetaData LevelMetaData => levelMetaData;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void AssignLevelMetaData()
    {
        levelMetaData = ChapterList[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed];
    }
}
