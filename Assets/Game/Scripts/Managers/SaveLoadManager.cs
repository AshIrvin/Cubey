using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BayatGames.SaveGamePro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;


public class SaveLoadManager : MonoBehaviour
{

    [SerializeField] private SaveMetaData saveMetaData;
    [SerializeField] private ChapterList allChapters;
    // [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private GameObject levelParent;

    [SerializeField] public List<GameObject> levelButtons;


    // old
    // [SerializeField] private PlayerLevelStats levelStats;
    [SerializeField] private AudioManager audioManager;

    [Header("Levels")]
    [SerializeField]
    private int levelsPlayed;

    [Header("Buttons")]
    // [SerializeField] private List<GameObject> chapterButtons = new List<GameObject>();
    [SerializeField] private Button buttonGold;

    [Header("Lists")]
    [SerializeField] private GameObject[] stars;
    [SerializeField] private Image[] lock_img;
    // [SerializeField] private List<int> chapterLevelSaved;

    [Header("Map")]
    [SerializeField] private GameObject chapterMap;

    [SerializeField] private GameObject starGrp;
    [SerializeField] private GameObject levelSweet, cubeyOnMap, levelButterfly, levelPickup;

    [Header("Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject screenGoldAward;
    [SerializeField] private GameObject screenChapterComplete;

    [Header("Lock Buttons for Editor")]
    [SerializeField] private GameObject buttonLockAll, buttonUnlockAll;

    [Header("Buttons")]


    [Header("Star Colours")]
    [SerializeField] private Vector3 lastLevelButtonPos;
    [SerializeField] private Vector3 nextLevel;

    [Header("Star Colours")]
    [SerializeField] private Color starGold = new Color(0.95f, 0.95f, 0, 1);
    [SerializeField] private Color starSilver = new Color(1, 0.86f, 0, 1);
    [SerializeField] private Color starBronze = new Color(1, 0.5f, 0, 1); //
    [SerializeField] private Color starDefault = new Color(1, 1, 1, 0.3f);

    [SerializeField] private Vector3 cubeyOffset = new Vector3(0, -1.8f, 0);

    [Header("ints")]
    [SerializeField] private int gold;
    [SerializeField] private int silver, bronze;
    [SerializeField] public bool goldScreenPlayed;


    private int totalLevels;
    private Vector3 s1 = new Vector3(0.9f, 0.9f, 0.9f);
    private Vector3 s2 = new Vector3(1f, 1f, 1f);
    private Animator starAnim;
    private GameObject lockImg;
    private GameObject mapPickup;

    private void Awake()
    {
        // mainMenuManager = GetComponent<MainMenuManager>();
    }

    private void Start()
    {
        screenChapterComplete.SetActive(false);

        Time.timeScale = 1f;

        if (loadingScreen.activeInHierarchy)
            LoadingScreen(false);

#if UNITY_EDITOR
        buttonLockAll.SetActive(true);
        buttonUnlockAll.SetActive(true);
#elif !UNITY_EDITOR
        buttonLockAll.SetActive(false);
        buttonUnlockAll.SetActive(false);
#endif
        
        if (PlayerPrefs.HasKey("goldScreenPlayed"))
        {
            var g = PlayerPrefs.GetInt("goldScreenPlayed");
            if (g == 1)
                goldScreenPlayed = true;
        }

        
        // unlocks chapter button?
        /*for (int i = 0; i < chapterButtons.Count; i++)
        {
            if (PlayerPrefs.HasKey(i + "level"))
                chapterLevelSaved.Add(PlayerPrefs.GetInt(i + "level"));
        }*/
    }

    /*private void AssignMapButtons()
    {
        var buttonList = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
        
        for (int i = 0; i < buttonList.Count; i++)
        {
            buttonList[i].GetComponent<Button>().onClick.AddListener(GetLevelNoToLoad);
        }
    }*/

    public void LoadChapterComplete()
    {
        CloseWindow(screenChapterComplete);
    }

    private void CloseWindow(GameObject go)
    {
        go.SetActive(!go.activeInHierarchy);
    }

    private void CheckLastLevelVisited()
    {
        var newLevel = saveMetaData.LastLevelPlayed;
        var levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;

        newLevel++;

        // set Cubey to last level played
        // Todo set up level buttons for chapter entered
        lastLevelButtonPos = levelButtons[newLevel-1].transform.position;
        
        if (newLevel < levelButtons.Count)
            nextLevel = levelButtons[newLevel].transform.position;

        SetCubeyPosition(lastLevelButtonPos);
    }

    // comes from level buttons on map
    public void GetLevelNoToLoad()
    {
        var levelButtonClicked = EventSystem.current.currentSelectedGameObject.gameObject.transform.Find("LevelText_no").GetComponent<Text>().text.ToString();
        var chapter = saveMetaData.LastChapterPlayed;

        if (int.TryParse(levelButtonClicked, out var levelToLoad))
        {
            levelToLoad += chapter * 100;
        }

        print("Get Level No To Load: " + levelToLoad);

        // 3 digits
        LoadLevel(levelToLoad);
    }

    private void AssignPickup(int n)
    {
        if (levelPickup == null) Debug.Log("<color=red>No pickup set</color> for chapter " + saveMetaData.LastChapterPlayed);
        
        if (n == 0)
        {
            levelPickup = levelSweet;
            levelPickup = allChapters[saveMetaData.LastChapterPlayed].PickupIcon;
        } else
        {
            levelPickup = levelButterfly;
        }
        print("level pickup set to: " + n);
    }

    // Assign as 3rd event to chapter buttons
    public void AssignLevelButtonsToList()
    {
        // levelButtons.Clear();
        levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
        Debug.Log($"chapter {saveMetaData.LastChapterPlayed} buttons being set");

        for (int i = 0; i < allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList.Count; i++)
        {
            allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList[i].GetComponentInChildren<Text>()
                .text = (i+1).ToString();
            
            // levelButtons[i].GetComponent<Button>().onClick.AddListener(() => GetLevelNoToLoad());
            var n = i;
            // Debug.Log("level button: " + levelButtons[i].GetComponent<Button>().name);
            if (levelButtons[i].GetComponent<Button>() != null)
                levelButtons[i].GetComponent<Button>().onClick.AddListener(delegate {LoadLevel(n); });
            else
                Debug.Log("can't find button");         
            
        }
        
        Debug.Log($"Assigning {levelButtons.Count} level buttons");
        
        

        // AssignMapButtons();
        // GetLevel();
        // CheckLevelUnlocks();

        // SetStarsToMapButtons();

        // before this, level buttons needs added to a list
        // SetStarsForEachLevel();
    }

    // placement of stars
    private void SetStarsToMapButtons()
    {
        var t = new Vector3(0, -180f, 0);
        var s = new Vector3(1.3f, 1.3f, 1.3f);
        var lvlSweet = new Vector3(-100, 179, 0);

        if(saveMetaData.LastChapterPlayed == 0)
            AssignPickup(0);
        else
            AssignPickup(1);


        print("Assigning stars to map buttons");

        // var levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
        
        foreach (var starred in levelButtons)
        {
            var found = (starred.transform.Find("StarsGrp(Clone)") != null ||
                     starred.transform.Find("LevelSweet(Clone)") != null);

            if (found) continue;
            var starGameObject = Instantiate(starGrp, starred.transform);
            var sweetGameObject = Instantiate(levelPickup, starred.transform);

            starGameObject.transform.localPosition = t;
            starGameObject.transform.localScale = s;

            sweetGameObject.transform.localPosition = lvlSweet;

        }
        
        if (cubeyOnMap == null)
            cubeyOnMap = GameObject.Find("CubeyOnMap");

    }

    // Set stars for each level button
    private void SetStarsForEachLevel()
    {
        var level = saveMetaData.LastLevelPlayed;
        // var levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
        
        // storyButtonEnd.interactable = false;
        // storyButtonEnd.gameObject.SetActive(false);

        for (int i = 0; i < levelButtons.Count; i++)
        {
            var b = levelButtons[i].GetComponent<Button>();
            var sGrp = levelButtons[i].transform.Find("StarsGrp(Clone)");

            // Todo get pickup from save
            if (saveMetaData.LastChapterPlayed == 0)
            {
                mapPickup = levelButtons[i].transform.Find("LevelSweet(Clone)").gameObject;
            }
            else
                mapPickup = levelButtons[i].transform.Find("LevelButterfly(Clone)").gameObject;

            if (b.interactable && i < level)
            {
                mapPickup.SetActive(false);
            }
            else
            {
                mapPickup.SetActive(true);
            }

            // load level stats
            // levelStats.LoadLevelStats(i);

            // set score with stars
            // var scoreForLevel = levelStats.score;
            // CheckAwards(scoreForLevel);

            var starImages = new List<SpriteRenderer>();

            for (var j = 0; j < 3; j++)
            {
                starImages.Add(sGrp.transform.GetChild(j).GetComponent<SpriteRenderer>());
                starImages[j].color = starDefault;
            }

            /*switch (scoreForLevel)
            {
                case 1:
                    starImages[0].color = starBronze;
                    break;
                case 2:
                    starImages[0].color = starBronze;
                    starImages[1].color = starSilver;
                    break;
                case 3:
                    starImages[0].color = starBronze;
                    starImages[1].color = starSilver;
                    starImages[2].color = starGold;
                    if (level == i)
                        starImages[2].transform.localScale = Vector3.Lerp(s1, s2, Mathf.PingPong(Time.time, 1));
                    break;
                default:
                    break;
            }*/
        }

        // Todo delete all story stuff
        // if (MainMenuManager.Instance.chapter == 0)
        /*if (saveMetaData.LastChapterPlayed == 0)
        {
            if (levelButtons[20].GetComponent<Button>().interactable)
            {
                storyButtonEnd.gameObject.SetActive(true);
                storyButtonEnd.interactable = true;
            }
        }*/

        CheckLastLevelVisited();
    }

    public void EnableBonusLevel(Button button)
    {
        // saved after end story
        if (PlayerPrefs.HasKey("goldScreenPlayed"))
        {
            var g = PlayerPrefs.GetInt("goldScreenPlayed");
            if (g == 1)
                goldScreenPlayed = true;
        }

        if (gold >= totalLevels - 2 && !goldScreenPlayed)
        {
            screenGoldAward.SetActive(true);
            button.interactable = true;
            goldScreenPlayed = true;
            lastLevelButtonPos = buttonGold.transform.position;
            SetCubeyPosition(lastLevelButtonPos);

            SetButtonColour(button);

            PlayerPrefs.SetInt("goldScreenPlayed", 1);
        } else if (gold >= totalLevels - 2 && goldScreenPlayed)
        {
            button.interactable = true;
            SetButtonColour(button);

            lastLevelButtonPos = buttonGold.transform.position;
            SetCubeyPosition(lastLevelButtonPos);
        }
        else
        {
            button.interactable = false;
            screenGoldAward.SetActive(false);
            CheckLastLevelVisited();
        }

    }

    private void SetButtonColour(Button button)
    {
        Image image = button.GetComponent<Image>();
        var screenShot = button.transform.Find("Mask").transform.Find("Screenshot").gameObject;
        Image screen = screenShot.GetComponent<Image>();
        var colour = screen.color;
        colour = Color.white;
        screen.color = colour;
    }

    public void SetCubeyPosition(Vector3 pos)
    {
        // print("Cubey set to: " + pos + ", level: " + level);

        cubeyOffset = new Vector3(1.2f, -1.6f, 0);

        cubeyOnMap.transform.position = pos + cubeyOffset;
    }

    private void CheckAwards(int s)
    {
        if (s == 1)
            bronze += 1;
        else if (s == 2)
            silver += 1;
        else if (s == 3)
            gold += 1;
    }

    private void LoadingScreen(bool on)
    {
        // TODO switch off chapter buttons
        // MainMenuManager.Instance.chapterButtons.SetActive(false);
        
        if (loadingScreen != null)
            loadingScreen.SetActive(on);
    }

    /// <summary>
    /// Checks when to display ads
    /// </summary>
    private void CheckLevelsPlayed()
    {
        if (PlayerPrefs.HasKey("levelsPlayed"))
        {
            var n = PlayerPrefs.GetInt("levelsPlayed");
            levelsPlayed += n;
            levelsPlayed++;
            PlayerPrefs.SetInt("levelsPlayed", levelsPlayed);

            if (n == 5)
            {
                PlayerPrefs.SetInt("levelsPlayed", 0);
                
                // TODO Fix Ads
                // MainMenuManager.Instance.ShowAd();
            }
        }
        else
            PlayerPrefs.SetInt("levelsPlayed", 1);

        //MainMenuManager.Instance.CheckAdLoad();
    }

    // Comes from map buttons
    private void LoadLevel(int levelNumber)
    {
        // LoadingScreen(true);

        // for Ads
        CheckLevelsPlayed(); 

        if (audioManager != null && audioManager.allowSounds)
            audioManager.PlayMusic(audioManager.menuStartLevel);

        var l = levelNumber.ToString();

        if (l.Length > 2)
        {
            var levelString = l[1].ToString() + l[2].ToString();
            saveMetaData.LastLevelPlayed = int.Parse(levelString);
        } else
        {
            saveMetaData.LastLevelPlayed = levelNumber;
        }

        // Todo. switch off menu
        // GetComponent<MainMenuManager>().MainMenu(false);
        
        // load level 
        GameObject level = Instantiate(allChapters[saveMetaData.LastChapterPlayed].LevelList[saveMetaData.LastLevelPlayed].LevelPrefab);
        level.SetActive(true);
    }

    // Get level from save
    private void GetLevel()
    {
        var level = saveMetaData.LastLevelPlayed;
        levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
        
        if (levelButtons.Count > 0)
        {
            nextLevel = levelButtons[level > 0 ? level-1 : 0].transform.position;
            
            lastLevelButtonPos = levelButtons[level].transform.position;
        }
        else
        {
            print("<color=red>No map level buttons found</color>");
        }

        print("2. get level: " + level);
    }

    // Todo is this needed? Get Level In Chapter
    public int GetLevelInChapter(int level)
    {
        int singleLevel = 0;
        print("get level in chapter: " + level);
        if (level >= 0)
        {
            // convert back to single level form
            string s = level.ToString("000");
            if (s.Length == 3)
            {
                string lvl = s[1] + s[2].ToString();
                singleLevel = int.Parse(lvl);
            }
            else if (s.Length == 4)
            {
                string lvl = s[2] + s[3].ToString();
                singleLevel = int.Parse(lvl);
            }
            else
            {
                var c = saveMetaData.LastChapterPlayed;
                s = c + level.ToString("000");

                string lvl = s[1] + s[2].ToString();
                singleLevel = int.Parse(lvl);
            }
            print("return single level: " + singleLevel);
        }
        //else
        //    singleLevel = 00;

        return singleLevel;
    }

    public void UnlockNLevels(int n)
    {
        var levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
        
        saveMetaData.LastChapterPlayed = allChapters.Count;
        saveMetaData.LastLevelPlayed = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList.Count;
        
        CheckLevelUnlocks();
        SetUnlockGold(n);

        SetCubeyPosition(levelButtons[n].transform.position);
    }

    public void UnlockAllLevels()
    {
        var levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
        // level = levelButtons.Count; // 3
        // print("unlock All levels: " + level);
        // PlayerPrefs.SetInt("level", level);

        // print("levels unlocked: " + level);

        CheckLevelUnlocks();
        //CheckChapterUnlocks();
        SetUnlockGold(levelButtons.Count);
    }

    private void SetUnlockGold(int count)
    {
        for (int i = 0; i < count+1; i++)
        {
            CheckAwards(3);
        }
    }

    // comes from Lock all levels chapter button
    public void LockAllLevels()
    {
        SaveGame.Clear();
        gold = 0;

        saveMetaData.LastChapterPlayed = 0;
        saveMetaData.LastLevelPlayed = 0;

        CheckLevelUnlocks();

        // reset cubey to 1st level
        if (levelButtons.Count > 0)
            SetCubeyPosition(levelButtons[0].transform.position);
    }

    private void CheckChapterUnlocks()
    {
        // CycleButtonLocks(chapterButtons);
    }

    private void CheckLevelUnlocks() 
    {
        var levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
        CycleButtonLocks(levelButtons);
    }

    // Check which levels are unlocked inside the chapter
    private void CycleButtonLocks(List<GameObject> buttons)
    {
        // var levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
        
        // get correct saved level
        // GetLevel();

        // print("Unlock to latest Saved level: " + level);

        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i].GetComponent<Button>();

            lockImg = button.gameObject.transform.Find("Lock_image").gameObject;

            var screenShot = button.transform.Find("Mask").transform.Find("Screenshot").gameObject;

            lockImg.gameObject.SetActive(false);

            // check against saved level and unlock the next level
            button.interactable = i < (saveMetaData.LastLevelPlayed + 1);

            // save last level unlocked?
            if (button.interactable)
                saveMetaData.LastLevelPlayed += 1;

            totalLevels = levelButtons.Count - 1;

            if (lock_img.Length > 0)
            {
                Image img = lock_img[i].GetComponent<Image>();
            }

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
    }
}
