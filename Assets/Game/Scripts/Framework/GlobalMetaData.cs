using UnityEngine;

[DefaultExecutionOrder(0)]
public class GlobalMetaData : MonoBehaviour
{
    public static GlobalMetaData Instance;

    [SerializeField] private BoolGlobalVariable gameLevel;
    private LevelMetaData levelMetaData;

    public ChapterList ChapterList;

    public LevelMetaData LevelMetaData => levelMetaData;
    public BoolGlobalVariable GameLevel => gameLevel;
    

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void AssignLevelMetaData()
    {
        levelMetaData = ChapterList[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed];
    }

    public void HasGameLevelLoaded(bool state)
    {
        gameLevel.CurrentValue = state;
    }
}
