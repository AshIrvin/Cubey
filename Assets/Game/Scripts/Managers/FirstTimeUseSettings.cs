using UnityEngine;

public class FirstTimeUseSettings : MonoBehaviour
{
    private void Awake()
    {
        // TODO - there could be an execution order issue
        SaveToFile.OnFirstTimeUse += UnlockedFirstTimeLevels;
    }

    // Could be used to reset the game too
    private void UnlockedFirstTimeLevels()
    {
        var length = SaveLoadManager.SaveStaticList.Count;

        for (int i = 0; i < length; i++)
        {
            SaveLoadManager.SaveStaticList[i].ChapterUnlocked = false;
        }

        SaveLoadManager.SaveStaticList[1].ChapterUnlocked = true;
        SaveLoadManager.SaveStaticList[1].Levels[0].LevelUnlocked = true;
    }
}
