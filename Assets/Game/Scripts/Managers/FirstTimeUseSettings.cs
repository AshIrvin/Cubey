using UnityEngine;

public class FirstTimeUseSettings : MonoBehaviour
{
    private void Awake()
    {
        // TODO - there could be an execution order issue
        SaveToFile.OnFirstTimeUse += UnlockedFirstTimeLevels;
    }

    private void UnlockedFirstTimeLevels()
    {
        var chapterLength = SaveLoadManager.SaveStaticList.Count;

        for (int i = 0; i < chapterLength; i++)
        {
            SaveLoadManager.SaveStaticList[i].ChapterUnlocked = false;

            for (int j = 0; j < SaveLoadManager.SaveStaticList[i].Levels.Count; j++)
            {
                SaveLoadManager.SaveStaticList[i].Levels[j].LevelUnlocked = false;
            }
        }

        SaveLoadManager.SaveStaticList[1].ChapterUnlocked = true;
        SaveLoadManager.SaveStaticList[1].Levels[0].LevelUnlocked = true;
    }
}
