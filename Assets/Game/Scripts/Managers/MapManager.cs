using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [SerializeField] private SaveMetaData saveMetaData;
    [SerializeField] private ChapterList allChapters;
    [SerializeField] private BoolGlobalVariable enableGameManager;
    [SerializeField] private MainMenuManager mainMenuManager;

    [SerializeField] private int levelsPlayed;
    
    [SerializeField] private List<GameObject> chapterMaps;
    
    [SerializeField] private GameObject mapsParent;
    [SerializeField] public bool mapActive;
    [SerializeField] private GameObject levelParent;
    [SerializeField] private GameObject cubeyOnMap;
    
    [SerializeField] private GameObject levelGameObject;
    
    private List<GameObject> levelButtons;


    private GameObject mapPickup;
    private Vector3 lerpPos1 = new Vector3(0.9f, 0.9f, 0.9f);
    private Vector3 lerpPos2 = new Vector3(1f, 1f, 1f);
    
    private bool EnableGameManager
    {
        set => enableGameManager.CurrentValue = value;
    }

    public GameObject LevelGameObject
    {
        get => levelGameObject;
        set => levelGameObject = value;
    }

    public GameObject LevelParent => levelParent;
    public GameObject CubeyOnMap => cubeyOnMap;

    
    private void Awake()
    {
        AddChapterMaps();
    }

    private void OnEnable()
    {
        EnableMap(SaveLoadManager.LastChapterPlayed);
        mapActive = true;
        SetCubeyMapPosition(false);
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel);
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
        
        CycleButtonLocks();
        
        mainMenuManager.EnableGoldAwardsButton(true);
        mainMenuManager.TryChapterFinishScreen();
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
    
    // comes from level buttons on map
    public void GetLevelNoToLoad()
    {
        var levelButtonClicked = EventSystem.current.currentSelectedGameObject.gameObject.transform.Find("LevelText_no").GetComponent<Text>().text.ToString();
        int.TryParse(levelButtonClicked, out int n);
        n -= 1;
        LoadLevel(n);
    }

    public void RestartLevel()
    {
        Debug.Log("Restarting Level");
        EnableGameManager = false;
        Destroy(levelGameObject);    
        LevelGameObject = Instantiate(allChapters[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed].LevelPrefab, levelParent.transform);
        levelGameObject.SetActive(true);
        EnableGameManager = true;
        enabled = false;
        
    }
    
    private void LoadLevel(int levelNumber)
    {
        CheckLevelsPlayed(); 
        
        mainMenuManager.SetCollisionBox("CollisionMap");
        
        // if (audioManager != null && audioManager.allowSounds)
        //     audioManager.PlayMusic(audioManager.menuStartLevel);

        var l = levelNumber.ToString();

        if (l.Length > 2)
        {
            var levelString = l[1].ToString() + l[2].ToString();
            SaveLoadManager.LastLevelPlayed = int.Parse(levelString);
        } else
        {
            SaveLoadManager.LastLevelPlayed = levelNumber;
        }
            
        LevelGameObject = Instantiate(allChapters[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed].LevelPrefab, levelParent.transform);
        // TODO - SO - NOT USED?
        // saveMetaData.LevelLoaded = levelGameObject.name;
        levelGameObject.SetActive(true);
        EnableGameManager = true;
        enabled = false;
        mainMenuManager.SetCollisionBox(null);
    }

    public void QuitToMap()
    {
        GameObject cubey = GameObject.FindWithTag("Player").transform.gameObject;
        cubey.transform.SetParent(null, true);
        VisualEffects.Instance.peExitSwirl.transform.SetParent(VisualEffects.Instance.ParticleEffectsGo.transform, true);
        EnableGameManager = false;
        Time.timeScale = 1;
        enabled = true;
        DestroyLevels();
        EnableMap(SaveLoadManager.LastChapterPlayed);
        // mainMenuManager.TryChapterFinishScreen();
    }

    private void DestroyLevels()
    {
        
        for (int i = 0; i < LevelParent.transform.childCount; i++)
        {
            Destroy(LevelParent.transform.GetChild(i).gameObject);
        }
    }

    #region Button Level Locking

    // No longer applicable
    /*public void UnlockAllLevels(bool revert)
    {
        // revert to previous level/chapter state
        if (revert)
        {
            SaveLoadManager.LastLevelPlayed = PlayerPrefs.GetInt("lastLevelPlayed");
            SaveLoadManager.LastChapterPlayed = PlayerPrefs.GetInt("lastChapterPlayed");
            // todo need to cycle through all the chapters and set each last level unlocked for each
            int levelBeforeUnlock = PlayerPrefs.GetInt("levelBeforeUnlock");
            mainMenuManager.chapterUnlockedTo = PlayerPrefs.GetInt("chapterBeforeUnlock");
            return;
        }
        
        PlayerPrefs.SetInt("lastLevelPlayed", allChapters[SaveLoadManager.LastChapterPlayed].LastLevelPlayed);
        PlayerPrefs.SetInt("lastChapterPlayed", SaveLoadManager.LastChapterPlayed);
        PlayerPrefs.SetInt("levelBeforeUnlock", allChapters[SaveLoadManager.LastChapterPlayed].LastLevelUnlocked);
        PlayerPrefs.SetInt("chapterBeforeUnlock", mainMenuManager.chapterUnlockedTo);
        
        levelButtons = allChapters[SaveLoadManager.LastChapterPlayed].ChapterMapButtonList;
            
        SaveLoadManager.LastChapterPlayed = allChapters.Count;
        SaveLoadManager.LastLevelPlayed = allChapters[SaveLoadManager.LastChapterPlayed].ChapterMapButtonList.Count;
            
        SetCubeyMapPosition(false);
    }*/
    
    /*private void SetUnlockGold(int count)
    {
        for (int i = 0; i < count+1; i++)
        {
            AssignAwards(3);
        }
    }
    
    private void AssignAwards(int s)
    {
        if (s == 1)
            allChapters[SaveLoadManager.LastChapterPlayed].AwardsBronze += 1;
        else if (s == 2)
            allChapters[SaveLoadManager.LastChapterPlayed].AwardsSilver += 1;
        else if (s == 3)
            allChapters[SaveLoadManager.LastChapterPlayed].AwardsGold += 1;
        
        EditorUtility.SetDirty(allChapters[SaveLoadManager.LastChapterPlayed]);
    }*/
    
    // Check which levels are unlocked inside the chapter
    private void CycleButtonLocks()
    {
        bool levelUnlocked = false;
        var lastChapter = allChapters[SaveLoadManager.LastChapterPlayed];

        var buttons = lastChapter.InGameMapButtonList;
        levelButtons = buttons;
            
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i].GetComponent<Button>();
            var screenShot = button.transform.Find("Mask").GetChild(0).gameObject;
            
            // set all level 1 buttons unlocked - needed?
            button.interactable = i == 0;
            
            if (i > 0)
            {
                if (SaveLoadManager.SaveStaticList[SaveLoadManager.LastChapterPlayed].levels[i].levelUnlocked)
                {
                    button.interactable = true;
                }
                else
                {
                    button.interactable = false;

                    if (SaveLoadManager.LastLevelUnlocked < i && !levelUnlocked)
                    {
                        levelUnlocked = true;
                        SaveLoadManager.LastLevelUnlocked = i-1;
                    }
                }
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

        SetStarsForEachLevel();
    }
    
    List<SpriteRenderer> starImages = new (3);
    
    // Set stars for each level button
    private void SetStarsForEachLevel()
    {
        // var level = allChapters[SaveLoadManager.LastChapterPlayed].LastLevelPlayed;
        var level = SaveLoadManager.LastLevelPlayed;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            var b = levelButtons[i].GetComponent<Button>();
            var sGrp = levelButtons[i].transform.GetChild(4);
            // var scoreForLevel = allChapters[SaveLoadManager.LastChapterPlayed].LevelList[i].AwardsReceived;

            var awardForLevel = SaveLoadManager.GetAwards(i);
            // var starImages = new List<SpriteRenderer>();
            starImages.Clear();
            
            // Todo get pickup from save
            /*if (SaveLoadManager.LastChapterPlayed == 0)
            {
                mapPickup = levelButtons[i].transform.Find("LevelSweet(Clone)").gameObject;
            }
            else
                mapPickup = levelButtons[i].transform.Find("LevelButterfly(Clone)").gameObject;*/

            /*if (b.interactable && i < level)
            {
                mapPickup.SetActive(false);
            }
            else
            {
                mapPickup.SetActive(true);
            }*/
            
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
}
