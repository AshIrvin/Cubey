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
            UnlockManager.LockChapter(i);

            for (int j = 0; j < SaveLoadManager.SaveStaticList[i].Levels.Count; j++)
            {
                UnlockManager.LockLevel(i, j);
            }

            UnlockManager.UnlockLevel(i, 0);
        }

        UnlockManager.UnlockChapter(1);
        Debug.Log("UnlockedFirstTimeLevels");
    }
}
