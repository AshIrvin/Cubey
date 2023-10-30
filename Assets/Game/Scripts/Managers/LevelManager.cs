using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; set; }

    //[SerializeField] private BoolGlobalVariable gameLevel;
    [SerializeField] private GameObject levelParent;
    [SerializeField] private GameObject levelGameObject;
    [SerializeField] private InitialiseAds initialiseAds;
    [SerializeField] private bool deleteLevelsPlayed;

    public int levelsPlayed;
    public int maxDemoLevel = 10;

    private BoxCollider levelCollision;
    private int levelToLoad;

    #region Getters

    //public bool GameLevel
    //{
    //    set => gameLevel.CurrentValue = value;
    //}

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

    private void OnEnable()
    {
        InitialiseAds.LoadLevel += LoadLevel;
    }

    private void OnDisable()
    {
        InitialiseAds.LoadLevel -= LoadLevel;
    }

    private void OnApplicationQuit()
    {
        
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

    private void GetDemoLevelWithAds(int n)
    {
        if (PlayerPrefs.HasKey("levelsPlayed"))
        {
            var played = PlayerPrefs.GetInt("levelsPlayed");
            levelsPlayed = played;
            levelsPlayed += 1;
            PlayerPrefs.SetInt("levelsPlayed", levelsPlayed);

            if (played >= AdSettings.Instance.LevelsBeforeAd)
            {
                PlayerPrefs.SetInt("levelsPlayed", 0);
                levelsPlayed = 0;
                Logger.Instance.ShowDebugLog("Ad 1. Invoking ad...");
                AdSettings.Instance.LoadAd?.Invoke();
            }
            else
            {
                LoadLevelNumber(n);
            }
        }
        else
        {
            levelsPlayed = 1;
            PlayerPrefs.SetInt("levelsPlayed", 1);
            LoadLevelNumber(n);
        }
    }

    // Attached to Level buttons on map
    public void GetLevelNoToLoad()
    {
        var levelButtonClicked = EventSystem.current.currentSelectedGameObject.gameObject.transform.Find("LevelText_no").GetComponent<Text>().text.ToString();
        int.TryParse(levelButtonClicked, out int n);
        n -= 1;
        CheckLevelsPlayed(n);
    }

    public void RestartLevel()
    {
        Logger.Instance.ShowDebugLog("Restarting Level");

        GlobalMetaData.Instance.HasGameLevelLoaded(false);
        Destroy(levelGameObject);

        if (LevelParent.transform.childCount > 0)
        {
            for (int i = 0; i < LevelParent.transform.childCount; i++)
            {
                Destroy(LevelParent.transform.GetChild(i).gameObject);
            }
        }

        // TODO - change from instantiation to enabling/disabling
        LevelGameObject = Instantiate(GlobalMetaData.Instance.ChapterList[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed].LevelPrefab, LevelParent.transform);
        levelGameObject.SetActive(true);
        GlobalMetaData.Instance.HasGameLevelLoaded(true);
        enabled = false;
    }

    public void LoadLevel()
    {
        MainMenuManager.Instance.SetNavButtons(false);

        Logger.Instance.ShowDebugLog("Ad 5 - finished. Loading level: " + levelToLoad);

        AdSettings.Instance.EnableAdBackgroundBlocker(false);

        //initialiseAds.enabled = false;
        LoadLevelNumber(levelToLoad);
    }

    public void LoadLevelNumber(int levelNumber)
    {
        var l = levelNumber.ToString();

        if (l.Length > 2)
        {
            var levelString = l[1].ToString() + l[2].ToString();
            SaveLoadManager.LastLevelPlayed = int.Parse(levelString);
        }
        else
        {
            SaveLoadManager.LastLevelPlayed = levelNumber;
        }

        GlobalMetaData.Instance.AssignLevelMetaData();

        // TODO - Remove the Finds - add to chapter scriptable object
        if (LevelGameObject == null)
        {
            // TODO - change from instantiation to enabling/disabling
            LevelGameObject = Instantiate(GlobalMetaData.Instance.ChapterList[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed].LevelPrefab, LevelParent.transform);
            levelCollision = levelGameObject.transform.Find("Environment").Find("LevelCollision").GetComponent<BoxCollider>();
        }

        levelGameObject.SetActive(true);
        AdSettings.Instance.EnableAdBackgroundBlocker(false);
        initialiseAds.DestroyTopBannerAd();

        GlobalMetaData.Instance.HasGameLevelLoaded(true);
        Logger.Instance.ShowDebugLog($"Game level {levelNumber++} loaded");

        enabled = false;
        MainMenuManager.Instance.SetCollisionBox(MainMenuManager.CollisionBox.Level, levelCollision);
        GameManager.Instance.SetGameState(GameManager.GameState.Level);
    }

    public void DestroyLevels()
    {
        for (int i = 0; i < LevelParent.transform.childCount; i++)
        {
            Destroy(LevelParent.transform.GetChild(i).gameObject);
        }
    }
}
