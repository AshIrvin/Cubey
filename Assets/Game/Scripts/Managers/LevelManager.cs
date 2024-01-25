using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; set; }

    [SerializeField] private GameObject levelParent;
    [SerializeField] private GameObject levelGameObject;
    [SerializeField] private InitialiseAds initialiseAds;
    [SerializeField] private bool deleteLevelsPlayed;
    [SerializeField] private List<ChapterLevels> chapterLevelsList;

    public static Action OnLevelLoad;
    public static Action<int> OnLevelCompleted;
    public static Action OnLevelFailed;

    internal static readonly int ChapterAmount = 6;
    internal static readonly int LevelAmount = 30;

    internal readonly int XmasStartMonth = 10;
    internal readonly int XmasEndMonth = 1;

    private BoxCollider levelCollision;
    private int levelToLoad;
    private int demoLevelsPlayed;
    private readonly int maxDemoLevel = 10;

    #region Getters

    public int LevelsPlayed => demoLevelsPlayed;
    public int MaxDemoLevel => maxDemoLevel;

    public GameObject LevelGameObject
    {
        get => levelGameObject;
        set => levelGameObject = value;
    }

    public GameObject LevelParent
    {
        get
        {
            if (levelParent == null)
            { // if forgotten to assign in the inspector...
                levelParent = GameObject.Find("Game/LevelParent");
            }

            return levelParent;
        }
    }

    internal static int LastChapterPlayed
    {
        get
        {
            return PlayerPrefs.GetInt("LastChapterPlayed", 1);
        }
        set
        {
            PlayerPrefs.SetInt("LastChapterPlayed", value);
        }
    }

    public static int LastLevelPlayed
    {
        get => SaveLoadManager.SaveStaticList[LastChapterPlayed].LastLevelPlayed;
        set => SaveLoadManager.SaveStaticList[LastChapterPlayed].LastLevelPlayed = value;
    }

    public static int LastLevelUnlocked
    {
        get => SaveLoadManager.SaveStaticList[LastChapterPlayed].LastLevelUnlocked;
        set => SaveLoadManager.SaveStaticList[LastChapterPlayed].LastLevelUnlocked = value;
    }

    #endregion Getters

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        CheckDemoLevelsPlayed();

        SpawnAllLevels();

        UiManager.OnLevelButtonPressed += CheckLevelsPlayed;
    }

    public void SetLevelToLoad(int levelNo)
    {
        levelToLoad = levelNo;
    }

    private void CheckDemoLevelsPlayed()
    {
        if (deleteLevelsPlayed)
        {
            demoLevelsPlayed = 0;
            PlayerPrefs.SetInt("demoLevelsPlayed", 0);
        }
        else
        {
            demoLevelsPlayed = PlayerPrefs.GetInt("demoLevelsPlayed", 0);
        }
    }

    private void GetDemoLevelWithAds(int n)
    {
        if (PlayerPrefs.HasKey("demoLevelsPlayed"))
        {
            demoLevelsPlayed = PlayerPrefs.GetInt("demoLevelsPlayed");
            demoLevelsPlayed += 1;
            PlayerPrefs.SetInt("demoLevelsPlayed", demoLevelsPlayed);

            if (demoLevelsPlayed >= AdSettings.Instance.LevelsBeforeAd)
            {
                SetAmountLevelsPlayed(0);
                InitialiseAds.LoadAd?.Invoke();
                Logger.Instance.ShowDebugLog("Ad 1. Invoking ad...");
            }

            LoadLevelNumber(n);
            return;
        }

        SetAmountLevelsPlayed(1);
        LoadLevelNumber(n);
    }

    private void SetAmountLevelsPlayed(int n)
    {
        demoLevelsPlayed = n;
        PlayerPrefs.SetInt("demoLevelsPlayed", n);
    }

    private void CheckLevelsPlayed(int n)
    {
        if (ShopManager.GamePurchased)
        {
            LoadLevelNumber(n);
            return;
        }

        levelToLoad = n;

        GetDemoLevelWithAds(n);
    }

    public void PrepareToLoadLevelFromAd()
    {
        MainMenuManager.Instance.SetNavButtons(false);

        Logger.Instance.ShowDebugLog("Ad 5 - finished. Loading level: " + levelToLoad);

        AdSettings.Instance.EnableAdBackgroundBlocker(false);

        LoadLevelNumber(levelToLoad);
    }

    public void LoadLevelNumber(int levelNumber)
    {
        var levelNo = levelNumber.ToString();

        if (levelNo.Length > 2)
        {
            var levelString = levelNo[1].ToString() + levelNo[2].ToString();
            LastLevelPlayed = int.Parse(levelString);
        }
        else
        {
            LastLevelPlayed = levelNumber;
        }

        GlobalMetaData.Instance.AssignLevelMetaData();

        levelGameObject = GetLevel(levelNumber);
        // TODO - remove this find
        levelCollision = levelGameObject.transform.Find("Environment/LevelCollision").GetComponent<BoxCollider>();

        levelGameObject.SetActive(true);
        AdSettings.Instance.EnableAdBackgroundBlocker(false);
        initialiseAds.DestroyTopBannerAd();

        Logger.Instance.ShowDebugLog($"Game level {levelNumber++} loaded");

        MainMenuManager.Instance.SetCollisionBox(MainMenuManager.CollisionBox.Level, levelCollision);
        GameManager.Instance.SetGameState(GameManager.GameState.Level);

        OnLevelLoad?.Invoke();
    }

    private void SpawnAllLevels()
    {
        var list = GlobalMetaData.Instance.ChapterList;
        var count = list.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject chapterGroup = new GameObject(list[i].name);
            chapterGroup.transform.SetParent(LevelParent.transform);

            var levelList = list[i].LevelList;
            chapterLevelsList.Add(new ChapterLevels(chapterGroup, new List<GameObject>()));

            AddLevelToList(chapterGroup, levelList, i);
        }
    }

    private void AddLevelToList(GameObject chapterGroup, LevelList levelList, int i)
    {
        for (int j = 0; j < levelList.Count; j++)
        {
            if (levelList[j] != null)
            {
                var levelPrefab = Instantiate(levelList[j].LevelPrefab, chapterGroup.transform);

                chapterLevelsList[i].LevelList.Add(levelPrefab);

                levelPrefab.SetActive(false);
            }
        }
    }

    private GameObject GetLevel(int levelNo)
    {
        var chapter = GlobalMetaData.Instance.ChapterList[LastChapterPlayed];
        var prefabLevel = chapter.LevelList[levelNo].LevelPrefab;

        foreach (var Chapters in chapterLevelsList)
        {
            foreach (var level in Chapters.LevelList)
            {
                if (Chapters.Chapter.name.Contains(chapter.name) && level.name.Contains(prefabLevel.name))
                {
                    level.SetActive(true);
                    return level;
                }
            }
        }

        return null;
    }

    [Serializable]
    public class ChapterLevels
    {
        public GameObject Chapter;
        public List<GameObject> LevelList;

        public ChapterLevels(GameObject chapter, List<GameObject> levelList)
        {
            Chapter = chapter;
            LevelList = levelList;
        }
    }
}