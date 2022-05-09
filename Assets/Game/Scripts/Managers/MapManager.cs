using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private SaveMetaData saveMetaData;
    [SerializeField] private ChapterList allChapters;
    [SerializeField] private BoolGlobalVariable gameLevel;
    [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private InitialiseAds initialiseAds;
    [SerializeField] private int levelsPlayed;
    public bool deleteLevelsPlayed;
    
    [Header("GameObjects")]
    [SerializeField] private GameObject mapsParent;
    [SerializeField] private GameObject levelParent;
    [SerializeField] private GameObject cubeyOnMap;
    [SerializeField] private GameObject levelGameObject;
    [SerializeField] private GameObject bgAdBlocker;
    [SerializeField] private GameObject shopButton;
    [SerializeField] private List<GameObject> chapterMaps;
    [SerializeField] private int manyLevelsBeforeAds = 5;
    
    private List<GameObject> levelButtons;
    private int levelToLoad;
    private GameObject mapPickup;
    private Vector3 lerpPos1 = new Vector3(0.9f, 0.9f, 0.9f);
    private Vector3 lerpPos2 = new Vector3(1f, 1f, 1f);

    public bool mapActive;
    
    public static Action LoadAd;
    public static Action PrepareAd;
    public static Action MapOpened;
    
    private bool GameLevel
    {
        set => gameLevel.CurrentValue = value;
    }

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
            {
                Debug.Log("Finding LevelParent...");
                levelParent = GameObject.Find("Game/LevelParent");
            }
            return levelParent;
        }
    }

    public GameObject CubeyOnMap => cubeyOnMap;

    public bool GamePurchased
    {
        get => SaveLoadManager.GamePurchased;
        set => SaveLoadManager.SaveGamePurchased(value);
    }

    private void Awake()
    {
        AddChapterMaps();

        InitialiseAds.LoadLevel += LoadLevel;

        if (deleteLevelsPlayed)
        {
            levelsPlayed = 0;
            PlayerPrefs.SetInt("levelsPlayed", 0);
        }
        else
        {
            levelsPlayed = PlayerPrefs.GetInt("levelsPlayed", 0);
        }
        
        bgAdBlocker?.SetActive(false);

        shopButton.SetActive(true);
    }

    private void OnEnable()
    {
        levelToLoad = 0;
        EnableMap(SaveLoadManager.LastChapterPlayed);
        mapActive = true;
        SetCubeyMapPosition(false);
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel);

        MapOpened.Invoke();
    }

    /// <summary>
    /// Sets the position of Cubey on the map
    /// </summary>
    /// <param name="reset">Disables Cubey</param>
    private void SetCubeyMapPosition(bool reset)
    {
        cubeyOnMap.SetActive(!reset);
        
        var chapter = allChapters[SaveLoadManager.LastChapterPlayed];
        var currentLevelNo = SaveLoadManager.LastLevelPlayed;
        var pos = chapter.ChapterMapButtonList[currentLevelNo].transform.position;
        
        pos.x -= 1.1f;
        pos.y -= 1.5f;
        cubeyOnMap.transform.position = reset ? Vector3.zero : pos;
    }

    private void OnDisable()
    {
        InitialiseAds.LoadLevel -= LoadLevel;
        bgAdBlocker.SetActive(false);
        DisableMaps();
        mapActive = false;
        SetCubeyMapPosition(true);
        VisualEffects.Instance.StopEffect(VisualEffects.Instance.peNewLevel);
    }

    private void AddChapterMaps()
    {
        chapterMaps.Clear();
        
        for (int i = 0; i < allChapters.Count; i++)
        {
            allChapters[i].InGameMapButtonList.Clear();
            var map = Instantiate(allChapters[i].ChapterMap, mapsParent.transform);
            chapterMaps.Add(map);
            
            var mapButtons = map.transform.Find("Canvas_Map").Find("Map_buttons").gameObject;
            for (int j = 0; j < mapButtons.transform.childCount; j++)
            {
                if (mapButtons.transform.GetChild(j).name.Contains("Leveln"))
                {
                    allChapters[i].InGameMapButtonList.Add(mapButtons.transform.GetChild(j).gameObject);
                    var button = mapButtons.transform.GetChild(j).GetComponent<Button>();
                    string levelNumber = (j+1).ToString();
                    button.transform.GetChild(1).GetComponent<Text>().text = levelNumber;
                    if (allChapters[i].LevelList[j].LevelSprite != null)
                        button.transform.Find("Mask/Screenshot").GetComponent<Image>().sprite = allChapters[i].LevelList[j].LevelSprite;
                    button.onClick.AddListener(GetLevelNoToLoad);
                }
            }
            
            map.SetActive(false);
        }
    }

    private void EnableMap(int chapter)
    {
        DisableMaps();
        chapterMaps[chapter]?.SetActive(true);
        SaveLoadManager.LastChapterPlayed = chapter;
        CycleButtonLocks();
        
        mainMenuManager.EnableGoldAwardsButton(true);
        mainMenuManager.TryChapterFinishScreen();
        
        if(!SaveLoadManager.GamePurchased)
        {
            PrepareGoogleAds();
        }
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

    /// <summary>
    /// Checks when to display ADs
    /// </summary>
    private void CheckLevelsPlayed(int n)
    {
        if (SaveLoadManager.GamePurchased)
        {
            LoadLevel(n);
            return;
        }
        
        levelToLoad = n;
        if (PlayerPrefs.HasKey("levelsPlayed"))
        {
            var played = PlayerPrefs.GetInt("levelsPlayed");
            levelsPlayed = played;
            levelsPlayed += 1;
            PlayerPrefs.SetInt("levelsPlayed", levelsPlayed);

            if (played >= manyLevelsBeforeAds)
            {
                // shopButton.SetActive(false);
                mainMenuManager.NavButtons = false;
                PlayerPrefs.SetInt("levelsPlayed", 0);
                levelsPlayed = 0;
                // bgAdBlocker.SetActive(true);
                Debug.Log("Ad 1. Invoking ad...");
                LoadAd?.Invoke();
                // LoadLevel(n);
            }
            else
            {
                LoadLevel(n);
            }
        }
        else
        {
            levelsPlayed = 1;
            PlayerPrefs.SetInt("levelsPlayed", 1);
            LoadLevel(n);
        }
    }

    // comes from level buttons on map

    public void GetLevelNoToLoad()
    {
        var levelButtonClicked = EventSystem.current.currentSelectedGameObject.gameObject.transform.Find("LevelText_no").GetComponent<Text>().text.ToString();
        int.TryParse(levelButtonClicked, out int n);
        n -= 1;
        CheckLevelsPlayed(n);
    }

    public void RestartLevel()
    {
        Debug.Log("Restarting Level");
        GameLevel = false;
        Destroy(levelGameObject);
        if (LevelParent.transform.childCount > 0)
        {
            for (int i = 0; i < LevelParent.transform.childCount; i++)
            {
                Destroy(LevelParent.transform.GetChild(i).gameObject);
            }
        }
        
        LevelGameObject = Instantiate(allChapters[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed].LevelPrefab, LevelParent.transform);
        levelGameObject.SetActive(true);
        GameLevel = true;
        enabled = false;
    }

    private void LoadLevel()
    {
        Debug.Log("Ad 5 - finished. Loading level: " + levelToLoad);
        if (bgAdBlocker == null)
        {
            bgAdBlocker = GameObject.Find("GoogleAds/Canvas/BlockBg").gameObject;
            bgAdBlocker.SetActive(false);
        }
        else
        {
            bgAdBlocker.SetActive(false);
        }
        
        LoadLevel(levelToLoad);
    }

    private void PrepareGoogleAds()
    {
        Debug.Log("Preparing ad...");
        PrepareAd?.Invoke();
    }

    private BoxCollider levelCollision;

    private void LoadLevel(int levelNumber)
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
        
        if (LevelGameObject == null)
        {
            LevelGameObject = Instantiate( allChapters[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed].LevelPrefab, LevelParent.transform);
            levelCollision = levelGameObject.transform.Find("Environment").Find("LevelCollision")?.GetComponent<BoxCollider>();
        }
        
        levelGameObject?.SetActive(true);
        bgAdBlocker.SetActive(false);
        GameLevel = true;
        enabled = false;
        mainMenuManager?.SetCollisionBox("LevelCollision", levelCollision);
    }

    // Comes from end screen continue button
    public void QuitToMap()
    {
        GameObject cubey = GameObject.FindWithTag("Player").transform.gameObject;
        cubey.transform.SetParent(null, true);
        VisualEffects.Instance.peExitSwirl.transform.SetParent(VisualEffects.Instance.ParticleEffectsGo.transform, true);
        GameLevel = false;
        Time.timeScale = 1;
        enabled = true;
        DestroyLevels();
        mainMenuManager.NavButtons = true;
        mainMenuManager?.SetCollisionBox("CollisionMap");
    }

    private void DestroyLevels()
    {
        for (int i = 0; i < LevelParent.transform.childCount; i++)
        {
            Destroy(LevelParent.transform.GetChild(i).gameObject);
        }
    }

    #region Button Level Locking

    private int maxDemoLevel = 10;

    public void PurchaseGameButton()
    {
        GamePurchased = true;
        // SceneManager.LoadScene("CubeyGame");
        Debug.Log("Game Purchased: " + GamePurchased);
    }

    public void DemoMode()
    {
        GamePurchased = false;
        Debug.Log("Demo mode. Purchased: " + GamePurchased);
    }
    
    // Check which levels are unlocked inside the chapter
    private void CycleButtonLocks()
    {
        var lastChapter = allChapters[SaveLoadManager.LastChapterPlayed];

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
        int maxLevels = SaveLoadManager.GamePurchased ? 30 : maxDemoLevel;

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

    [SerializeField] private GameObject shopMenu;
    
    private void SetButtonToShopButton(Button button)
    {
        // move the shop button over level 10
        if (!GamePurchased)
        {
            var pos = button.transform.position;
            var b = button.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => mainMenuManager.ToggleGameObject(shopMenu));
            b.interactable = true;
            b.image = shopButton.GetComponent<Image>();
            GameObject level10 = b.transform.GetChild(2).GetChild(0).gameObject;
            level10.GetComponent<Image>().sprite = b.image.sprite;
            level10.GetComponent<Image>().color = Color.white;
        }
    }
    
    List<SpriteRenderer> starImages = new (3);
    
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
                        starImages[2].transform.localScale = Vector3.Lerp(lerpPos1, lerpPos2, Mathf.PingPong(Time.time, 1));
                    break;
                default:
                    break;
            }
        }
    }
    
    #endregion

    private void OnDestroy()
    {
        InitialiseAds.LoadLevel -= LoadLevel;
    }
}
