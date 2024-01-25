using UnityEngine;

public class AwardManager : MonoBehaviour
{
    public enum Awards
    {
        NoAward,
        OneStar,
        TwoStars,
        ThreeStars
    }

    public static AwardManager Instance;

    private LevelMetaData levelMetaData;

    internal int ThreeStars { private set; get; }
    internal int TwoStars { private set; get; }
    internal int OneStar { private set; get; }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    internal static int GetChapterAward(int chapter)
    {
        return SaveLoadManager.SaveStaticList[chapter].AllStars;
    }

    internal Awards StarsGiven()
    {
        var jumps = GameManager.JUMPS_TO_START - GameManager.Instance.JumpsLeft;

        if (jumps <= ThreeStars)
            return Awards.ThreeStars;
        else if (jumps <= TwoStars)
            return Awards.TwoStars;
        else if (jumps <= OneStar)
            return Awards.OneStar;

        return Awards.NoAward;
    }

    internal void GetLevelAwards()
    {
        levelMetaData = GlobalMetaData.Instance.LevelMetaData;

        OneStar = levelMetaData.JumpsForBronze;
        TwoStars = levelMetaData.JumpsForSilver;
        ThreeStars = levelMetaData.JumpsForGold;
    }

    internal void SetAwardForLevel(Awards award)
    {
        SetStarAward(LevelManager.LastLevelPlayed, award);
    }

    private void SetStarAward(int level, Awards award)
    {
        if (GetLevelAward(level) < (int)award)
            SetAward(level, award);
    }

    internal static int GetLevelAward(int level)
    {
        return SaveLoadManager.SaveStaticList[LevelManager.LastChapterPlayed].Levels[level].Stars;
    }

    public static int GetAwards(int level)
    {
        return SaveLoadManager.SaveStaticList[LevelManager.LastChapterPlayed].Levels[level].StarsReceived;
    }

    public static void SetAward(int level, AwardManager.Awards awardType)
    { // check current award, so to not add too many!
        int currentAward = GetAwards(level);
        int remainingAward = (int)awardType - currentAward;

        if (remainingAward > 0)
        {
            var lastChapter = LevelManager.LastChapterPlayed;
            SaveLoadManager.SaveStaticList[lastChapter].AllStars += remainingAward;
            SaveLoadManager.SaveStaticList[lastChapter].Levels[level].Stars += remainingAward;
            SaveLoadManager.SaveStaticList[lastChapter].Levels[level].StarsReceived += remainingAward;
        }
    }
}
