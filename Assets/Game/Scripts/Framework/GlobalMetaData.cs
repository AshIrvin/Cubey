using UnityEngine;

[DefaultExecutionOrder(0)]
public class GlobalMetaData : MonoBehaviour
{
    public static GlobalMetaData Instance;

    private LevelMetaData levelMetaData;

    public ChapterList ChapterList;
    public LevelMetaData LevelMetaData => levelMetaData;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void AssignLevelMetaData()
    {
        levelMetaData = ChapterList[LevelManager.LastChapterPlayed].LevelList[LevelManager.LastLevelPlayed];
    }
}
