using UnityEngine;

public class AwardManager : MonoBehaviour
{
    public static AwardManager Instance;

    private LevelMetaData levelMetaData;

    private int threeStars;
    private int twoStars;
    private int oneStar;

    public int ThreeStars => threeStars;
    public int TwoStars => twoStars;
    public int OneStar => oneStar;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void GetLevelAwards()
    {
        levelMetaData = GlobalMetaData.Instance.LevelMetaData;

        oneStar = levelMetaData.JumpsForBronze;
        twoStars = levelMetaData.JumpsForSilver;
        threeStars = levelMetaData.JumpsForGold;
    }

    public void SetAwardForLevel(SaveLoadManager.Awards award)
    {
        SetStarAward(SaveLoadManager.LastLevelPlayed, award);
    }

    private void SetStarAward(int level, SaveLoadManager.Awards award)
    {
        if (SaveLoadManager.GetLevelAward(level) < (int)award)
            SaveLoadManager.SetAward(level, award);
    }
}
