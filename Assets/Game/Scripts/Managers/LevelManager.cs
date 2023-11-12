using System;
using System.Collections.Generic;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; set; }

    [SerializeField] private GameObject levelParent;
    [SerializeField] private GameObject levelGameObject;
    [SerializeField] private InitialiseAds initialiseAds;
    [SerializeField] private bool deleteLevelsPlayed;
    [SerializeField] private List<ChapterLevels> chapterLevelsList;

    private BoxCollider levelCollision;
    private int levelToLoad;
    private int levelsPlayed;
    private readonly int maxDemoLevel = 10;

    public static Action OnLevelLoad;

    #region Getters

    public int LevelsPlayed => levelsPlayed;
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

    #endregion Getters

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        CheckForLevelsPlayed();

        SpawnAllLevels();
    }

    public void SetLevelToLoad(int levelNo)
    {
        levelToLoad = levelNo;
    }

    private void CheckForLevelsPlayed()
    {
        if (deleteLevelsPlayed)
        {
            levelsPlayed = 0;
            PlayerPrefs.SetInt("levelsPlayed", 0);
        }
        else
        {
            levelsPlayed = PlayerPrefs.GetInt("levelsPlayed", 0);
        }
    }

    private void GetDemoLevelWithAds(int n)
    {
        if (PlayerPrefs.HasKey("levelsPlayed"))
        {
            levelsPlayed = PlayerPrefs.GetInt("levelsPlayed");
            levelsPlayed += 1;
            PlayerPrefs.SetInt("levelsPlayed", levelsPlayed);

            if (levelsPlayed >= AdSettings.Instance.LevelsBeforeAd)
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
        levelsPlayed = n;
        PlayerPrefs.SetInt("levelsPlayed", n);
    }

    // Attached to Level buttons on map
    public void GetLevelNoToLoad()
    {
        var levelButtonClicked = EventSystem.current.currentSelectedGameObject.gameObject.transform.Find("LevelText_no").GetComponent<Text>().text.ToString();
        int.TryParse(levelButtonClicked, out int n);
        n -= 1;
        CheckLevelsPlayed(n);
    }

    private void CheckLevelsPlayed(int n)
    {
        if (SaveLoadManager.GamePurchased)
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
            SaveLoadManager.LastLevelPlayed = int.Parse(levelString);
        }
        else
        {
            SaveLoadManager.LastLevelPlayed = levelNumber;
        }

        GlobalMetaData.Instance.AssignLevelMetaData();

        levelGameObject = GetLevel(levelNumber);
        // TODO - remove this find
        levelCollision = levelGameObject.transform.Find("Environment").Find("LevelCollision").GetComponent<BoxCollider>();

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
        var chapter = GlobalMetaData.Instance.ChapterList[SaveLoadManager.LastChapterPlayed];
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


