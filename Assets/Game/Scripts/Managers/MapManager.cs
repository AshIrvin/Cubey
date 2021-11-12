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
        EnableMap(saveMetaData.LastChapterPlayed);
        mapActive = true;
        SetCubeyMapPosition(false);
        
    }

    private void SetCubeyMapPosition(bool reset)
    {
        cubeyOnMap.SetActive(!reset);
        
        var chapter = allChapters[saveMetaData.LastChapterPlayed];
        var currentLevelNo = chapter.LastLevelPlayed;
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
    }
    
    public void DisableMaps()
    {
        for (int i = 0; i < chapterMaps.Count; i++)
            chapterMaps[i].SetActive(false);
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
        LevelGameObject = Instantiate(allChapters[saveMetaData.LastChapterPlayed].LevelList[saveMetaData.LastLevelPlayed].LevelPrefab, levelParent.transform);
        levelGameObject.SetActive(true);
        EnableGameManager = true;
        enabled = false;
    }
    
    private void LoadLevel(int levelNumber)
    {
        CheckLevelsPlayed(); 
        
        var mapCollider = allChapters[saveMetaData.LastChapterPlayed].ChapterMap.GetComponentInChildren<BoxCollider>();
        if (mainMenuManager.leanConstrainToBox != null)
            mainMenuManager.leanConstrainToBox.Target = mapCollider;
        else
            Debug.LogError("Can't find mainMenuManager.leanConstrainToBox! mainMenuManager ok?:" + mainMenuManager);
        
        // if (audioManager != null && audioManager.allowSounds)
        //     audioManager.PlayMusic(audioManager.menuStartLevel);

        var l = levelNumber.ToString();

        if (l.Length > 2)
        {
            var levelString = l[1].ToString() + l[2].ToString();
            saveMetaData.LastLevelPlayed = allChapters[saveMetaData.LastChapterPlayed].LastLevelPlayed = int.Parse(levelString);
        } else
        {
            saveMetaData.LastLevelPlayed = allChapters[saveMetaData.LastChapterPlayed].LastLevelPlayed = levelNumber;
        }
        
        EditorUtility.SetDirty(saveMetaData);
        EditorUtility.SetDirty(allChapters[saveMetaData.LastChapterPlayed]);
        
        // EditorUtil.ApplyChanges(saveMetaData);
        // EditorUtil.ApplyChanges(allChapters[saveMetaData.LastChapterPlayed]);
            
        LevelGameObject = Instantiate(allChapters[saveMetaData.LastChapterPlayed].LevelList[saveMetaData.LastLevelPlayed].LevelPrefab, levelParent.transform);
        saveMetaData.LevelLoaded = levelGameObject.name;
        levelGameObject.SetActive(true);
        EnableGameManager = true;
        enabled = false;
    }

    public void QuitToMap()
    {
        DestroyLevels();
        Time.timeScale = 1;
        enabled = true;
        EnableGameManager = false;
        
        EnableMap(saveMetaData.LastChapterPlayed);
    }

    private void DestroyLevels()
    {
        for (int i = 0; i < LevelParent.transform.childCount; i++)
        {
            Destroy(LevelParent.transform.GetChild(i).gameObject);
        }
    }

    #region Button Level Locking

    /*private void UnlockAllLevels(int n)
    {
        levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
            
        saveMetaData.LastChapterPlayed = allChapters.Count;
        saveMetaData.LastLevelPlayed = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList.Count;
            
        CheckLevelUnlocks();
        SetUnlockGold(n);

        SetCubeyMapPosition(false);
    }*/
    
    private void SetUnlockGold(int count)
    {
        for (int i = 0; i < count+1; i++)
        {
            AssignAwards(3);
        }
    }
    
    private void AssignAwards(int s)
    {
        if (s == 1)
            allChapters[saveMetaData.LastChapterPlayed].AwardsBronze += 1;
        else if (s == 2)
            allChapters[saveMetaData.LastChapterPlayed].AwardsSilver += 1;
        else if (s == 3)
            allChapters[saveMetaData.LastChapterPlayed].AwardsGold += 1;
        
        EditorUtility.SetDirty(allChapters[saveMetaData.LastChapterPlayed]);
    }
    
    // Check which levels are unlocked inside the chapter
    private void CycleButtonLocks()
    {
        bool levelUnlocked = false;
        var lastChapter = allChapters[saveMetaData.LastChapterPlayed];
        // Debug.Log("Cycling through unlocks for chapter: " + lastChapter);

        var buttons = lastChapter.InGameMapButtonList;
        levelButtons = buttons;
            
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i].GetComponent<Button>();
            var screenShot = button.transform.Find("Mask").transform.Find("Screenshot").gameObject;
            
            button.interactable = i == 0;
            
            if (i > 0)
            {
                if (lastChapter.LevelList[i - 1].AwardsReceived > 0)
                {
                    Debug.Log($"AwardsReceived for level {i}/({i-1}): " + lastChapter.LevelList[i - 1].AwardsReceived);
                    button.interactable = true;
                }
                else
                {
                    button.interactable = false;

                    if (lastChapter.LastLevelUnlocked < i && !levelUnlocked)
                    {
                        levelUnlocked = true;
                        lastChapter.LastLevelUnlocked = i-1;
                        Debug.Log($"level {i}/({i-1}) unlocked. Metadata save: {lastChapter.LastLevelUnlocked}");
                        EditorUtility.SetDirty(allChapters[saveMetaData.LastChapterPlayed]);
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



    [Header("Star Colours")]
    private Color starGold = new Color(0.95f, 0.95f, 0, 1);
    private Color starSilver = new Color(1, 0.86f, 0, 1);
    private Color starBronze = new Color(1, 0.5f, 0, 1);
    private Color starDefault = new Color(1, 1, 1, 0.3f);
    private GameObject mapPickup;
    private Vector3 lerpPos1 = new Vector3(0.9f, 0.9f, 0.9f);
    private Vector3 lerpPos2 = new Vector3(1f, 1f, 1f);
    
    // Set stars for each level button
    private void SetStarsForEachLevel()
    {
        var level = allChapters[saveMetaData.LastChapterPlayed].LastLevelPlayed;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            var b = levelButtons[i].GetComponent<Button>();
            // var sGrp = levelButtons[i].transform.Find("StarsGrp");
            var sGrp = levelButtons[i].transform.GetChild(4);
            var scoreForLevel = allChapters[saveMetaData.LastChapterPlayed].LevelList[i].AwardsReceived;
            var starImages = new List<SpriteRenderer>();
            
            // Todo get pickup from save
            /*if (saveMetaData.LastChapterPlayed == 0)
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

            
            // CheckAwards(scoreForLevel);

            

            for (int j = 0; j < 3; j++)
            {
                starImages.Add(sGrp.transform.GetChild(j).GetComponent<SpriteRenderer>());
                starImages[j].color = starDefault;
            }

            switch (scoreForLevel)
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
                        starImages[2].transform.localScale = Vector3.Lerp(lerpPos1, lerpPos2, Mathf.PingPong(Time.time, 1));
                    break;
                default:
                    break;
            }
        }
    }
    
    
    #endregion
}
