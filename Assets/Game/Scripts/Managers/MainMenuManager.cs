using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using UnityEditor;
using UnityEngine.Advertisements;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private CameraManager cameraManager;
    
    [SerializeField] private SaveMetaData saveMetaData;
    [SerializeField] private ChapterList allChapters;
    // [SerializeField] private BoolGlobalVariable gameUiProperty;
    
    [SerializeField] private GameObject menuEnvironmentParent;
    [SerializeField] private List<GameObject> menuEnvironments;
    [SerializeField] private GameObject mapsParent;
    [SerializeField] private List<GameObject> chapterMaps;
    
    public GameObject mainMenu;
    public LeanConstrainToBox leanConstrainToBox;
    
    [Header("Extra screens")]
    [SerializeField] private GameObject chapterFinishScreen;

    // Old
    [Header("Scripts")]
    [SerializeField] private VisualEffects visualEffects;
    [SerializeField] private AudioManager audioManager;
    
    [Header("GameObjects")]
    // [SerializeField] private GameObject[] chapter_maps;
    [SerializeField] private GameObject chapterScreen;
    [SerializeField] private GameObject mainMenuUi;
    
    [SerializeField] private GameObject navButtons;
    [SerializeField] private GameObject screenDeleteSaveData;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button startButton;

    /*
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem peLvlStars;

    [SerializeField] private ParticleSystem peSnowThrow1;
    [SerializeField] private ParticleSystem peSnowThrow2;
    */

    // [Header("Other")]
    // [SerializeField] public int chapter;

    [SerializeField] private Text startText;
    [SerializeField] private Text versionNo;
    // [SerializeField] private Text goldWon;

    // [SerializeField] public bool mapActive;
    // [SerializeField] private bool[] chapterComplete = new bool[5]; // todo this used or using the new ChapterUnlocked bool

    private Color c1, c2, c2b, c3;

    /*[Header("StartButton Movement")]*/
    private Vector3 pos1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 pos2 = new Vector3(1f, 1f, 1f);
    private Vector3 scale1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 scale2 = new Vector3(0.95f, 0.95f, 0.95f);

    [SerializeField]
    private LeanCameraZoomSmooth leanZoom;

    private Animator anim;
    public int chapterUnlockedTo;
    
    /*private bool NavButtons
    {
        get => navButtons.activeInHierarchy;
        set => navButtons.SetActive(value);
    }*/

    /*public bool GameUiProperty
    {
        set => gameUiProperty.currentValue = value;
    }*/

    private void Awake()
    {
        SetNavButtons(false);
        
        if (visualEffects == null) visualEffects = GetComponent<VisualEffects>();
        if (audioManager == null) audioManager = GetComponent<AudioManager>();
        if (mapManager == null) mapManager = GetComponent<MapManager>();
        if (leanConstrainToBox == null) leanConstrainToBox = cameraManager.gameObject.GetComponent<LeanConstrainToBox>();
        mapManager.enabled = false;
        
        // find gameobjects
        if (chapterFinishScreen == null) chapterFinishScreen = GameObject.Find("ChapterFinishScreen");
        if (chapterButtons.Count == 0)
        {
            Debug.LogError("Assign chapter buttons to list!");
        }
    }

    private void SetNavButtons(bool on)
    {
        navButtons.SetActive(on);
    }

    // Start is called before the first frame update
    void Start()
    {
        versionNo.text = "v: " + Application.version;

        AddMenuEnvironments();
        SetColours();
        anim = startButton.GetComponent<Animator>();
        ButtonSizePong();
        SetMenuEnvironment(saveMetaData.LastChapterPlayed);
        chapterFinishScreen.SetActive(false);
    }

    private void SetColours()
    {
        c1 = new Color(0, 0, 0, 0); // clear
        c2 = new Color(0.1f, 0.3f, 0.7f, 1); // blue
        c2b = new Color(0.2f, 0.4f, 0.9f, 1); // blue
        c3 = new Color(1, 1, 1, 1); // white
    }
    
    private void UpdateMenuEnvironments()
    {
        for (int i = 0; i < allChapters.Count; i++)
        {
            menuEnvironmentParent = Instantiate(allChapters[saveMetaData.LastChapterPlayed].MenuEnvironment);
        }
    }
    
    private void AddMenuEnvironments()
    {
        for (int i = 0; i < allChapters.Count; i++)
        {
            var menu = Instantiate(allChapters[i].MenuEnvironment, menuEnvironmentParent.transform);
            menuEnvironments.Add(menu);
            menu.SetActive(false);
        }
    }

    private void SetMenuEnvironment(int n)
    {
        DisableMenuEnv();
        
        menuEnvironments[n].SetActive(true);
        leanZoom.Zoom = allChapters[n].MenuZoomLevel;
    }

    private void DisableMenuEnv()
    {
        for (int i = 0; i < menuEnvironments.Count; i++)
        {
            menuEnvironments[i].SetActive(false);
        }
    }

    private void OnDestroy()
    {
        Destroy(menuEnvironmentParent);
    }

    public void ButtonClose(GameObject go)
    {
        go.SetActive(!go.activeInHierarchy);
    }

    private void EnableMap(bool on)
    {
        /*if (on)
        {
            PlayEffect(peSnowThrow1, peSnowThrow1.transform.position, true);
            PlayEffect(peSnowThrow2, peSnowThrow2.transform.position, true);
        } else
        {
            CancelPe(peSnowThrow1);
            CancelPe(peSnowThrow2);
        }*/
    }

    /*private void LoadMap(int chapter)
    {
        DisableMaps();
        chapterMaps[chapter]?.SetActive(true);
    }*/

    public void SetCollisionBox(string collisionName)
    {
        if (leanConstrainToBox.Target != null)
            leanConstrainToBox.Target.enabled = true;
        
        if (collisionName == "CollisionMenu")
        {
            leanConstrainToBox.Target = allChapters[saveMetaData.LastChapterPlayed].MenuEnvironment.transform.Find(collisionName).GetComponent<BoxCollider>();
        }
        else if (collisionName == "CollisionMap")
        {
            leanConstrainToBox.Target = allChapters[saveMetaData.LastChapterPlayed].ChapterMap.transform.Find(collisionName).GetComponent<BoxCollider>();
        }
        else
        {
            if (leanConstrainToBox.Target != null)
            {
                leanConstrainToBox.Target.enabled = false;
                leanConstrainToBox.Target = null;
            }
            Debug.Log("No Collision box found!");
        }
    }
    
    // Used in chapter menu buttons
    public void ShowMap(int n)
    {
        var chapter = saveMetaData.LastChapterPlayed = n;

        EditorUtility.SetDirty(saveMetaData);
        
        SetMenuEnvironment(chapter);
        DisableMenuScreens();

        // leanConstrainToBox.Target = allChapters[saveMetaData.LastChapterPlayed].ChapterMap.transform.Find("CollisionMap").GetComponent<BoxCollider>();
        SetCollisionBox("CollisionMap");
        
        leanZoom.enabled = true;
        SetNavButtons(true);
        DisableMenuEnv();

        mapManager.enabled = true;
        
        backButton.onClick.AddListener(() => LoadChapterScreen(true));
    }

    [SerializeField] private List<GameObject> chapterButtons;

    [SerializeField] private Color fadedButton = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] private Color unlockedButton = Color.white;
    
    // disable chapters that aren't unlocked?
    private void CycleThroughUnlockedChapters()
    {
        int c = 0;

        for (int i = 0; i < allChapters.Count; i++)
        {
            if (allChapters[i].ChapterUnlocked)
            {
                c++;
                chapterButtons[i].GetComponent<Button>().interactable = true;
                var image = chapterButtons[i].transform.GetChild(0).GetChild(0).GetComponent<Image>();
                image.color = unlockedButton;
            }
            else
            {
                chapterButtons[i].GetComponent<Button>().interactable = false;
                var image = chapterButtons[i].transform.GetChild(0).GetChild(0).GetComponent<Image>();
                image.color = fadedButton;
            }
        }
        
        Debug.Log("Chapter " + c + " is unlocked");
        chapterUnlockedTo = c;
    }
    
    // for going back to chapter screen using back button on map and Start ui button
    public void LoadChapterScreen(bool enable)
    {
        CycleThroughUnlockedChapters();
        
        chapterScreen.SetActive(enable);
        SetNavButtons(enable);
        mainMenuUi.SetActive(!enable);
        menuEnvironmentParent.SetActive(true);
        mapManager.enabled = false;
        
        cameraManager.panToLevel = false;
        cameraManager.disableAutoPanMapCam = false;
        
        leanZoom.enabled = false;
        backButton.onClick.AddListener(MainMenuScreen);
    }

    private void DisableMenuScreens()
    {
        chapterScreen.SetActive(false);
        mainMenuUi.SetActive(false);
        menuEnvironmentParent.SetActive(false);
    }

    public void BackButton()
    {
        cameraManager.panToLevel = false;
        cameraManager.disableAutoPanMapCam = false;
    }

    private void MainMenuScreen()
    {
        mapManager.DisableMaps();
        SetMenuEnvironment(saveMetaData.LastChapterPlayed);
        SetCollisionBox("CollisionMenu");
        // LoadChapterScreen(false); // todo Loop???
        chapterScreen.SetActive(false);
        SetNavButtons(false);
        mainMenuUi.SetActive(true);
        
        cameraManager.ResetCamPosition();
        leanZoom.enabled = true;
    }

    private void FadeInStartButton()
    {
        startText.color = Color.Lerp(c2, c2b, Mathf.PingPong(Time.time, 1));
    }

    private void ButtonSizePong()
    {
        StartCoroutine(WaitForPong());
    }

    private IEnumerator WaitForPong()
    {
        yield return new WaitForSeconds(2.5f);
        anim.SetBool("StartButton_anim", false);
        anim.SetBool("EnablePingPong", true);
    }
    
    public void TryChapterFinishScreen()
    {
        var bronze = PlayerPrefs.GetInt("chapterFinishScreenBronze", 0);
        var silver = PlayerPrefs.GetInt("chapterFinishScreenSilver", 0);
        var gold = PlayerPrefs.GetInt("chapterFinishScreenGold", 0);
        
        // if all levels have been unlocked or at least bronze won, check for how many awards
        if (allChapters[saveMetaData.LastChapterPlayed].AwardsBronze > 29 && bronze == 0)
        {
            chapterFinishScreen.SetActive(true);
            if (saveMetaData.LastChapterPlayed < allChapters.Count)
            {
                allChapters[saveMetaData.LastChapterPlayed + 1].ChapterUnlocked = true;
                EditorUtility.SetDirty(allChapters[saveMetaData.LastChapterPlayed]);
                // popup for next chapter?
            }
            else
            {
                Debug.LogError("Anomaly detected with last chapter / allChapters");
            }
            PlayerPrefs.SetInt("chapterFinishScreenBronze", 1);
        }
        else if (allChapters[saveMetaData.LastChapterPlayed].AwardsSilver > 29 && silver == 0)
        {
            chapterFinishScreen.SetActive(true);
            
            PlayerPrefs.SetInt("chapterFinishScreenSilver", 1);
        }
        else if (allChapters[saveMetaData.LastChapterPlayed].AwardsGold > 29 && gold == 0)
        {
            chapterFinishScreen.SetActive(true);

            PlayerPrefs.SetInt("chapterFinishScreenGold", 1);
        }
    }

    public void EnableGoldAwardsButton(bool state)
    {
        chapterFinishScreen.SetActive(true);
        // chapterFinishScreen.GetComponent<ChapterComplete>().goldStarButton.SetActive(state);
    }
    
    public void ResetSaves()
    {
        screenDeleteSaveData.SetActive(false);

        for (int i = 0; i < allChapters.Count; i++)
        {
            allChapters[i].AwardsBronze = 0;
            allChapters[i].AwardsSilver = 0;
            allChapters[i].AwardsGold = 0;
            for (int j = 0; j < allChapters[i].LevelList.Count; j++)
            {
                allChapters[i].LevelList[j].AwardsReceived = 0;
            }
        }

        MainMenuScreen();
    }
}
