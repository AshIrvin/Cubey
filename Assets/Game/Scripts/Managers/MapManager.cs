using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class MapManager : MonoBehaviour
{
    // TODO - the class is for managing maps only
    // Everything else belongs in another class

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

    #region Privates

    private MainMenuManager mainMenuManager;
    private AdSettings adSettings;
    private List<GameObject> levelButtons;    
    private Vector3 lerpPos1 = new Vector3(0.9f, 0.9f, 0.9f);
    private Vector3 lerpPos2 = new Vector3(1f, 1f, 1f);
    private ChapterList chapterList;
    private List<SpriteRenderer> starImages = new(3);

    #endregion Privates

    public bool mapActive;
    public static Action MapOpened;
    public GameObject CubeyOnMap => cubeyOnMap;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        chapterList = GlobalMetaData.Instance.ChapterList;
        mainMenuManager = MainMenuManager.Instance;
        adSettings = AdSettings.Instance;

        AddChapterMaps();

        adSettings.EnableAdBackgroundBlocker(false);
        shopButton.SetActive(true);
    }

    private void OnEnable()
    {
        LevelManager.Instance.SetLevelToLoad(0);
        GameManager.Instance.SetGameState(GameManager.GameState.Map);

        if (mainMenuManager == null)
        {
            mainMenuManager = MainMenuManager.Instance;
        }

        EnableMap(SaveLoadManager.LastChapterPlayed);
        
        mapActive = true;
        SetCubeyMapPosition(false);
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel);

        MapOpened?.Invoke();
    }

    private void OnDisable()
    {
        adSettings.EnableAdBackgroundBlocker(false);
        DisableMaps();
        mapActive = false;
        SetCubeyMapPosition(true);
        VisualEffects.Instance.StopEffect(VisualEffects.Instance.peNewLevel);
    }

    private void SetCubeyMapPosition(bool state)
    {
        if (cubeyOnMap == null)
            Logger.Instance.ShowDebugError("Can't find Cubey for the map!");
        else
            cubeyOnMap.SetActive(!state);
        
        var chapter = chapterList[SaveLoadManager.LastChapterPlayed];
        var currentLevelNo = SaveLoadManager.LastLevelPlayed;
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

            // TODO - change from instantiation to enabling/disabling
            var map = Instantiate(chapterList[i].ChapterMap, mapsParent.transform);
            chapterMaps.Add(map);

            // TODO - Get rid of find objects. Once instantiation is removed, this should be fixed
            AssignMapButtons(map, i);

            map.SetActive(false);
        }
    }

    private void AssignMapButtons(GameObject map, int i)
    {
        var mapButtons = map.transform.Find("Canvas_Map/Map_buttons").gameObject;

        for (int j = 0; j < mapButtons.transform.childCount; j++)
        {
            if (mapButtons.transform.GetChild(j).name.Contains("Leveln"))
            {
                GameObject levelButton = mapButtons.transform.GetChild(j).gameObject;
                chapterList[i].InGameMapButtonList.Add(levelButton);
                levelButton.GetComponent<Button>().onClick.AddListener(LevelManager.Instance.GetLevelNoToLoad);
            }
        }
    }

    private void EnableMap(int chapter)
    {
        DisableMaps();
        chapterMaps[chapter].SetActive(true);
        SaveLoadManager.LastChapterPlayed = chapter;
        CycleButtonLocks();
        
        mainMenuManager.EnableGoldAwardsButton(true);
        mainMenuManager.TryChapterFinishScreen();
        
        if(!SaveLoadManager.GamePurchased && LevelManager.Instance.levelsPlayed >= AdSettings.Instance.LevelsBeforeAd)
        {
            // TODO - enable ads. Shouldn't it already be enabled?
            // initialiseAds.enabled = true;
            // PrepareAd?.Invoke();
        }

        GameManager.Instance.SetGameState(GameManager.GameState.Map);
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

    #region Used in UI buttons

    // Attached to End Screen continue button
    public void QuitToMap()
    {
        InitialiseAds.LoadLevel -= LevelManager.Instance.LoadLevel;

        GameObject cubey = GameObject.FindWithTag("Player").transform.gameObject;
        cubey.transform.SetParent(null, true);
        VisualEffects.Instance.peExitSwirl.transform.SetParent(VisualEffects.Instance.ParticleEffectsGo.transform, true);

        //if (!SaveLoadManager.GamePurchased)
        //{
        //    initialiseAds.LoadTopBannerAd();
        //}
        
        GlobalMetaData.Instance.HasGameLevelLoaded(false);
        Time.timeScale = 1;
        enabled = true;
        LevelManager.Instance.DestroyLevels();
        mainMenuManager.NavButtons = true;
        mainMenuManager.SetCollisionBox(MainMenuManager.CollisionBox.Map);
        
        // TODO: check if the last level was chapter 1, level 5 that completes the tutorial
        if (SaveLoadManager.LastChapterPlayed == 1 && SaveLoadManager.GetLevelAward(4) > 1
                                                   && PlayerPrefs.GetInt("tutorialFinished", 0) == 0)
        {
            PlayerPrefs.SetInt("tutorialFinished", 1);
            SaveLoadManager.UnlockChapter(2);
            SaveLoadManager.UnlockChapter(3);
            SaveLoadManager.SaveGameInfo();
            tutoralCompleteGo.SetActive(true);
        }
    }

    #endregion

    #region Button Level Locking

    // Check which levels are unlocked inside the chapter
    private void CycleButtonLocks()
    {
        var lastChapter = chapterList[SaveLoadManager.LastChapterPlayed];

        var buttons = lastChapter.InGameMapButtonList;
        levelButtons = buttons;
            
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i].GetComponent<Button>();
            var screenShot = button.transform.Find("Mask").GetChild(0).gameObject;
            
            // set all level 1 buttons unlocked - needed?
            button.interactable = i == 0;

            CheckGamePurchased(button, i);
            
            Image screen = screenShot.GetComponent<Image>();
            var colour = screen.color;
            colour = Color.white;
            screen.color = colour;

            if (!button.interactable)
            {
                colour.a = 0.3f;
            } else
            {
                colour.a = 1f;
            }
            screen.color = colour;
        }

        SetStarsForEachLevel();
        
        SetButtonToShopButton(lastChapter.InGameMapButtonList[10].GetComponent<Button>());
    }

    private void CheckGamePurchased(Button button, int i)
    {
        int maxLevels = SaveLoadManager.GamePurchased ? 30 : LevelManager.Instance.maxDemoLevel;

        if (i > 0 && i < maxLevels)
        {
            if (SaveLoadManager.SaveStaticList[SaveLoadManager.LastChapterPlayed].levels[i].levelUnlocked)
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
            }
        }
    }

    private void SetButtonToShopButton(Button button)
    {
        if (!ShopManager.GamePurchased)
        {
            var pos = button.transform.position;
            var b = button.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => EnableShopMenu());
            b.interactable = true;
            b.image = shopButton.GetComponent<Image>();
            GameObject level10 = b.transform.GetChild(2).GetChild(0).gameObject;
            level10.GetComponent<Image>().sprite = b.image.sprite;
            level10.GetComponent<Image>().color = Color.white;
        }
    }

    private void EnableShopMenu()
    {
        mainMenuManager.ToggleGameObject(shopMenu);
        mainMenuManager.BackButton = false;
    }
    
    
    
    // Set stars for each level button
    private void SetStarsForEachLevel()
    {
        var level = SaveLoadManager.LastLevelPlayed;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            var b = levelButtons[i].GetComponent<Button>();
            var sGrp = levelButtons[i].transform.GetChild(4);

            var awardForLevel = SaveLoadManager.GetAwards(i);

            starImages.Clear();
            
            for (int j = 0; j < 3; j++)
            {
                starImages.Add(sGrp.transform.GetChild(j).GetComponent<SpriteRenderer>());
                starImages[j].color = ColourManager.starDefault;
            }

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
                    if (level == i)
                    {
                        starImages[2].transform.localScale =
                            Vector3.Lerp(lerpPos1, lerpPos2, Mathf.PingPong(Time.time, 1));
                    }
                    break;
                default:
                    break;
            }
        }
    }
    
    #endregion

    private void OnDestroy()
    {
        InitialiseAds.LoadLevel -= LevelManager.Instance.LoadLevel;
    }
}
