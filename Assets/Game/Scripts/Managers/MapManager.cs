using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    #region Fields

    [Header("GameObjects")]
    [SerializeField] private GameObject mapsParent; 
    [SerializeField] private GameObject cubeyOnMap;
    [SerializeField] private GameObject shopButton;
    [SerializeField] private List<GameObject> chapterMaps;
    [SerializeField] private GameObject tutoralCompleteGo;
    [SerializeField] private GameObject shopMenu;

    #endregion Fields

    #region Private

    private MainMenuManager mainMenuManager;
    private AdSettings adSettings;
    private ChapterList chapterList;
    private VisualEffects visualEffects;
    private GameManager gameManager;
    //private ShopManager shopManager;

    private List<GameObject> levelButtons;    
    private Vector3 lerpPos1 = new Vector3(0.9f, 0.9f, 0.9f);
    private Vector3 lerpPos2 = new Vector3(1f, 1f, 1f);
    private List<SpriteRenderer> starImages = new(3);
    [SerializeField]private Sprite originalLevel10Image;

    #endregion Private

    #region Public

    internal static Action OnMapLoad;
    internal GameObject CubeyOnMap => cubeyOnMap;
    internal static Action OnQuitToMap;
    
    #endregion Public

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        LevelManager.OnLevelLoad += OnLevelLoad;
        UiManager.OnGamePurchased += ResetLevelShopButton;
        UiManager.OnDemoMode += DemoMode;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        visualEffects = VisualEffects.Instance;
        mainMenuManager = MainMenuManager.Instance;
        chapterList = GlobalMetaData.Instance.ChapterList;
        mainMenuManager = MainMenuManager.Instance;
        adSettings = AdSettings.Instance;
        //shopManager = ShopManager.Instance;

        adSettings.EnableAdBackgroundBlocker(false);
        shopButton.SetActive(true);

        AddChapterMaps();
    }

    public void LoadMapScreen()
    {
        gameManager.SetGameState(GameManager.GameState.Map);

        EnableMap(LevelManager.LastChapterPlayed);
        
        SetCubeyMapPosition(false);

        OnMapLoad?.Invoke();
    }

    private void OnLevelLoad()
    {
        adSettings.EnableAdBackgroundBlocker(false);
        DisableMaps();
        SetCubeyMapPosition(true);
    }

    private void SetCubeyMapPosition(bool state)
    {
        if (cubeyOnMap == null)
        {
            Logger.Instance.ShowDebugError("Can't find Cubey for the map!");
            return;
        }

        cubeyOnMap.SetActive(!state);
        
        var chapter = chapterList[LevelManager.LastChapterPlayed];
        var currentLevelNo = LevelManager.LastLevelPlayed;
        var pos = chapter.ChapterMapButtonList[currentLevelNo].transform.position;
        
        pos.x -= 1.1f;
        pos.y -= 1.5f;
        cubeyOnMap.transform.position = state ? Vector3.zero : pos;
    }

    private void AddChapterMaps()
    {
        chapterMaps.Clear();

        for (int i = 0; i < chapterList.Count; i++)
        {
            chapterList[i].InGameMapButtonList.Clear();

            var map = Instantiate(chapterList[i].ChapterMap, mapsParent.transform);
            chapterMaps.Add(map);

            AssignMapButtons(map, i);

            map.SetActive(false);
        }
    }

    private void AssignMapButtons(GameObject map, int i)
    {
        var mapButtons = map.transform.Find("Canvas_Map/Map_buttons").gameObject;
        var length = mapButtons.transform.childCount;

        for (int j = 0; j < length; j++)
        {
            if (mapButtons.transform.GetChild(j).name.Contains("Leveln"))
            {
                GameObject levelButton = mapButtons.transform.GetChild(j).gameObject;
                chapterList[i].InGameMapButtonList.Add(levelButton);
                levelButton.GetComponent<Button>().onClick.AddListener(UiManager.Instance.GetLevelNoToLoad);
            }
        }
    }

    private void EnableMap(int chapter)
    {
        DisableMaps();
        chapterMaps[chapter].SetActive(true);
        LevelManager.LastChapterPlayed = chapter;
        CycleButtonLocks();
        
        mainMenuManager.EnableGoldAwardsButton(true);
        mainMenuManager.TryChapterFinishScreen();
        
/*        if(!ShopManager.GamePurchased && LevelManager.Instance.LevelsPlayed >= AdSettings.Instance.LevelsBeforeAd)
        {
            // TODO - enable ads. Shouldn't it already be enabled?
            // initialiseAds.enabled = true;
            // PrepareAd?.Invoke();
        }*/

        gameManager.SetGameState(GameManager.GameState.Map);
        gameManager.QuitingLevel();
    }

    public void DisableMaps()
    {
        for (int i = 0; i < chapterMaps.Count; i++)
        {
            if (chapterMaps[i] != null)
                chapterMaps[i].SetActive(false);
        }

        mainMenuManager.EnableGoldAwardsButton(false);
    }

    public void QuitToMap()
    {
        UiManager.Instance.MainMenu.SetActive(true);
        
        gameManager.SetGameState(GameManager.GameState.Map);

        visualEffects.peExitSwirl.transform.SetParent(VisualEffects.Instance.ParticleEffectsGroup.transform, true);
        visualEffects.peExitSwirl.SetActive(false);

        // TODO - move all timescales together
        Time.timeScale = 1;

        LoadMapScreen();
        mainMenuManager.NavButtons = true;
        mainMenuManager.SetCollisionBox(MainMenuManager.CollisionBox.Map);

        IsTutorialComplete();
    }

    private void IsTutorialComplete()
    {
        // TODO: check if the last level was chapter 1, level 5 that completes the tutorial
        if (LevelManager.LastChapterPlayed == 1 &&
            AwardManager.GetLevelAward(4) > 1 &&
            PlayerPrefs.GetInt("tutorialFinished", 0) == 0)
        {
            PlayerPrefs.SetInt("tutorialFinished", 1);
            UnlockManager.UnlockChapter(2);
            UnlockManager.UnlockChapter(3);
            SaveLoadManager.SaveGameData();
            mainMenuManager.ShowMenuBackButton(false);
            tutoralCompleteGo.SetActive(true);
        }
    }

    #region Button Level Locking

    // Check which levels are unlocked inside the chapter
    private void CycleButtonLocks()
    {
        var lastChapter = chapterList[LevelManager.LastChapterPlayed];

        var buttons = lastChapter.InGameMapButtonList;
        levelButtons = buttons;
            
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i].GetComponent<Button>();
            var screenShot = button.transform.Find("Mask/Screenshot").GetComponent<Image>();
            
            // set all level 1 buttons unlocked - needed?
            //button.interactable = i == 0;

            CheckLevelUnlocks(button, i);
            
            var colour = Color.white;
            screenShot.color = colour;

            colour.a = button.interactable ? 1 : 0.3f;

            screenShot.color = colour;
        }

        SetStarsForEachLevel();
        
        //SetShopToButton();

        Debug.Log("CycleButtonLocks in chapter " + lastChapter.ChapterNumber);
    }

    private void CheckLevelUnlocks(Button button, int i)
    {
        // Tutorial finishing should still pop up. This is only for unlocked levels, which all should be unlocked

        int maxLevels = LevelManager.LevelAmount; //ShopManager.GamePurchased ? 30 : LevelManager.Instance.MaxDemoLevel;

        if (i >= 0 && i < maxLevels)
        {
            button.interactable = SaveLoadManager.SaveStaticList[LevelManager.LastChapterPlayed].Levels[i].LevelUnlocked;
            return;
        }

        button.interactable = false;
    }

    private void DemoMode()
    {
        ShopManager.SetGamePurchased(false);

        CycleButtonLocks();

        mainMenuManager.ToggleThankYouSign();
        mainMenuManager.CycleThroughUnlockedChapters();
    }

    private void SetShopToButton()
    {
        var button = chapterList[LevelManager.LastChapterPlayed].InGameMapButtonList[LevelManager.Instance.MaxDemoLevel].GetComponent<Button>();
        originalLevel10Image = button.transform.Find("Mask/Screenshot").GetComponent<Image>().sprite;

        if (ShopManager.GamePurchased) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => EnableShopMenu());
        button.interactable = true;

        Image level10 = button.transform.Find("Mask/Screenshot").GetComponent<Image>();
        level10.sprite = shopButton.GetComponent<Image>().sprite;
        level10.color = Color.white;
    }

    private void ResetLevelShopButton()
    {
        var button = chapterList[LevelManager.LastChapterPlayed].InGameMapButtonList[LevelManager.Instance.MaxDemoLevel].GetComponent<Button>();

        GameObject level10 = button.transform.Find("Mask/Screenshot").gameObject;
        var image = level10.GetComponent<Image>();
        image.sprite = originalLevel10Image;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => UiManager.Instance.GetLevelNoToLoad());

        button.interactable = SaveLoadManager.SaveStaticList[LevelManager.LastChapterPlayed].Levels[LevelManager.Instance.MaxDemoLevel].LevelUnlocked;
        var colour = image.color;
        colour.a = button.interactable ? 1 : 0.3f;
        image.color = colour;
    }

    private void EnableShopMenu()
    {
        mainMenuManager.ToggleGameObject(shopMenu);
        mainMenuManager.ShowMenuBackButton(false);
    }
    
    private void SetStarsForEachLevel()
    {
        var levelNumber = LevelManager.LastLevelPlayed;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            var sGrp = levelButtons[i].transform.GetChild(4);
            var awardForLevel = AwardManager.GetAwards(i);

            starImages.Clear();
            
            for (int j = 0; j < 3; j++)
            {
                starImages.Add(sGrp.transform.GetChild(j).GetComponent<SpriteRenderer>());
                starImages[j].color = ColourManager.starDefault;
            }

            AssignStarsToLevelButton(awardForLevel, levelNumber, i);
        }
    }

    private void AssignStarsToLevelButton(int awardForLevel, int levelNumber, int i)
    {
        switch (awardForLevel)
        {
            case 1:
                starImages[0].color = ColourManager.starBronze;
                break;
            case 2:
                starImages[0].color = ColourManager.starBronze;
                starImages[1].color = ColourManager.starSilver;
                break;
            case 3:
                starImages[0].color = ColourManager.starBronze;
                starImages[1].color = ColourManager.starSilver;
                starImages[2].color = ColourManager.starGold;
                if (levelNumber == i)
                {
                    starImages[2].transform.localScale =
                        Vector3.Lerp(lerpPos1, lerpPos2, Mathf.PingPong(Time.time, 1));
                }
                break;
            default:
                break;
        }
    }
    
    #endregion

    private void OnDestroy()
    {
        //InitialiseAds.LoadLevel -= LevelManager.Instance.PrepareToLoadLevel;
    }
}
