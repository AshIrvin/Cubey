using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using UnityEditor;
using UnityEngine.Advertisements;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // TODO - a general refactor needed for this script
    /// <summary>
    /// 
    /// </summary>
    
    [SerializeField] private MapManager mapManager;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private SaveMetaData saveMetaData;
    [SerializeField] private ChapterList chapterList;
    [SerializeField] private GameObject menuEnvironmentParent;
    [SerializeField] private List<GameObject> menuEnvironments;
    [SerializeField] private GameObject mapsParent;
    [SerializeField] private List<GameObject> chapterMaps;
    
    public GameObject mainMenu;
    public LeanConstrainToBox leanConstrainToBox;
    
    [Header("Extra screens")]
    [SerializeField] private GameObject chapterFinishScreen;
    [SerializeField] private bool deleteLastChapterFinishScreenData;
    [SerializeField] private GameObject loadingScreen;
    
    // Old
    [Header("Scripts")]
    [SerializeField] private VisualEffects visualEffects;
    [SerializeField] private AudioManager audioManager;
    
    [Header("GameObjects")]
    [SerializeField] private GameObject chapterScreen;
    [SerializeField] private GameObject mainMenuUi;
    [SerializeField] private GameObject navButtons;
    [SerializeField] private GameObject screenDeleteSaveData;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Animator menuButtonAnim;

    [SerializeField] private Text startText;
    [SerializeField] private Text versionNo;

    private Color c1, c2, c2b, c3;

    private Vector3 pos1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 pos2 = new Vector3(1f, 1f, 1f);
    private Vector3 scale1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 scale2 = new Vector3(0.95f, 0.95f, 0.95f);

    [SerializeField] private LeanCameraZoomSmooth leanZoom;
    [SerializeField] private List<GameObject> chapterButtons;
    [SerializeField] private Color fadedButton = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] private Color unlockedButton = Color.white;
    
    public int chapterUnlockedTo;

    public static Action onStart;

    public bool NavButtons
    {
        get => navButtons.activeInHierarchy;
        set => navButtons.SetActive(value);
    }

    public bool BackButton
    {
        get => backButton.isActiveAndEnabled;
        set => backButton.gameObject.SetActive(value);
    }

    private void Awake()
    {
        MapManager.TimeDuration(true);
        SetNavButtons(false);
        LoadingScene(true);
        
        if (visualEffects == null) visualEffects = GetComponent<VisualEffects>();
        if (audioManager == null) audioManager = GetComponent<AudioManager>();
        if (mapManager == null) mapManager = GetComponent<MapManager>();
        if (leanConstrainToBox == null) leanConstrainToBox = cameraManager.gameObject.GetComponent<LeanConstrainToBox>();
        mapManager.enabled = false;
        
        if (chapterFinishScreen == null) chapterFinishScreen = GameObject.Find("ChapterFinishScreen");
        if (chapterButtons.Count == 0)
        {
            Debug.LogError("Assign chapter buttons to list!");
        }

        if (deleteLastChapterFinishScreenData)
        {
            PlayerPrefs.DeleteKey("chapterFinishScreenGold" + SaveLoadManager.LastChapterPlayed);
        }

        if (PlayerPrefs.HasKey("RefreshRate"))
        {
            SetRefreshRate(PlayerPrefs.GetInt("RefreshRate"));
        }
        MapManager.TimeDuration(false, "Menu");
    }

    private void Start()
    {
        versionNo.text = "v: " + Application.version;

        AddMenuEnvironments();
        SetColours();
        ButtonSizePong();
        SetMenuEnvironment(SaveLoadManager.LastChapterPlayed);
        chapterFinishScreen.SetActive(false);
        
        onStart.Invoke();
        LoadingScene(false);
    }

    private void OnEnable()
    {
        InitialiseAds.LoadLevel -= mapManager.LoadLevel;
        backButton.gameObject.SetActive(true);
    }

    private void SetRefreshRate(int n)
    {
        Application.targetFrameRate = n;
    }
    
    public void ChangeRefreshRate()
    {
        var refreshRateButton = EventSystem.current.currentSelectedGameObject.gameObject.
            transform.GetChild(0).GetComponent<Text>();

        if (Application.targetFrameRate == 120)
        {
            SetRefreshRate(60);
            PlayerPrefs.SetInt("RefreshRate", 60);
            refreshRateButton.text = "Normal mode (60hz)";
        }
        else
        {
            SetRefreshRate(120);
            PlayerPrefs.SetInt("RefreshRate", 120);
            refreshRateButton.text = "SuperSmooth mode (120hz)";
        }
    }

    public void LoadingScene(bool state)
    {
        loadingScreen.SetActive(state);
    }
    
    public void ToggleGameObject(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
    
    private void SetNavButtons(bool on)
    {
        navButtons.SetActive(on);
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
        for (int i = 0; i < chapterList.Count; i++)
        {
            menuEnvironmentParent = Instantiate(chapterList[SaveLoadManager.LastChapterPlayed].MenuEnvironment);
        }
    }

    private void AddMenuEnvironments()
    {
        for (int i = 0; i < chapterList.Count; i++)
        {
            var menu = Instantiate(chapterList[i].MenuEnvironment, menuEnvironmentParent.transform);
            menuEnvironments.Add(menu);
            menu.SetActive(false);
        }
    }

    private void SetMenuEnvironment(int n)
    {
        DisableMenuEnv();
        
        menuEnvironments[n].SetActive(true);
        leanZoom.Zoom = chapterList[n].MenuZoomLevel;
        
        // set purchased sign
        menuEnvironments[n].transform.Find("ThankYou")?.gameObject.SetActive(SaveLoadManager.GamePurchased);
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

    public void SetCollisionBox(string collisionName, BoxCollider col = null)
    {
        if (leanConstrainToBox.Target != null)
            leanConstrainToBox.Target.enabled = true;
        
        if (collisionName == "CollisionMenu")
        {
            leanConstrainToBox.Target = chapterList[SaveLoadManager.LastChapterPlayed].MenuEnvironment.transform.Find(collisionName).GetComponent<BoxCollider>();
        }
        else if (collisionName == "CollisionMap")
        {
            leanConstrainToBox.Target = chapterList[SaveLoadManager.LastChapterPlayed].ChapterMap.transform.Find(collisionName).GetComponent<BoxCollider>();
        }
        else if (collisionName == "LevelCollision")
        {
            leanConstrainToBox.Target = col;
            // leanConstrainToBox.Target = chapterList[SaveLoadManager.LastChapterPlayed].ChapterMap.transform.Find(collisionName).GetComponent<BoxCollider>();
        }
        else
        {
            if (leanConstrainToBox.Target != null)
            {
                leanConstrainToBox.Target.enabled = false;
                leanConstrainToBox.Target = null;
            }
        }
    }

    // Used in chapter menu buttons
    public void ShowMap(int n)
    {
        int chapter = -1;

        chapter = SaveLoadManager.LastChapterPlayed = n;
        
        SetMenuEnvironment(chapter);
        DisableMenuScreens();
        SetCollisionBox("CollisionMap");
        audioManager.AudioButtons.SetActive(false);
        leanZoom.enabled = true;
        SetNavButtons(true);
        DisableMenuEnv();

        mapManager.enabled = true;
        
        backButton.gameObject.SetActive(true);
        backButton.onClick.AddListener(() => LoadChapterScreen(true));
    }

    // disable chapters that aren't unlocked?
    private void CycleThroughUnlockedChapters()
    {
        int c = 0;

        for (int i = 0; i < chapterList.Count; i++)
        {
            if (SaveLoadManager.GetChapterUnlocked(i))
            {
                c = i;
                chapterButtons[c].GetComponent<Button>().interactable = true;
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
        audioManager.AudioButtons.SetActive(enable);
        
        leanZoom.enabled = false;
        backButton.onClick.AddListener(MainMenuScreen);
    }

    private void DisableMenuScreens()
    {
        chapterScreen.SetActive(false);
        mainMenuUi.SetActive(false);
        menuEnvironmentParent.SetActive(false);
    }

    private void MainMenuScreen()
    {
        mapManager.DisableMaps();
        SetMenuEnvironment(SaveLoadManager.LastChapterPlayed);
        SetCollisionBox("CollisionMenu");
        chapterFinishScreen.SetActive(false);
        chapterScreen.SetActive(false);
        SetNavButtons(false);
        mainMenuUi.SetActive(true);

        cameraManager.ResetCamPosition();
        leanZoom.enabled = true;
    }

    private void ButtonSizePong()
    {
        StartCoroutine(WaitForPong());
    }

    private IEnumerator WaitForPong()
    {
        yield return new WaitForSeconds(1);
        
        menuButtonAnim.SetBool("EnablePingPong", true);
    }
    
    public void TryChapterFinishScreen()
    {
        var goldFinishScreen = PlayerPrefs.GetInt("chapterFinishScreenGold" + SaveLoadManager.LastChapterPlayed , 0);
        
        if (SaveLoadManager.LastLevelPlayed == 29)
        {
            if (SaveLoadManager.GetLevelAward(29) >= 1) // if the last level has at least 1 star
            {
                if (goldFinishScreen == 0)
                {
                    PlayerPrefs.SetInt("chapterFinishScreenGold" + SaveLoadManager.LastChapterPlayed, 1);
                    chapterFinishScreen.GetComponent<ChapterComplete>().OpenPopup();
                }
            }
        }
    }

    public void EnableGoldAwardsButton(bool state)
    {
        if (chapterFinishScreen != null)
        {
            chapterFinishScreen.SetActive(state);
            chapterFinishScreen.GetComponent<ChapterComplete>().ShowCompleteScreen(state);
        }
    }
    
    public void ResetSaves()
    {
        screenDeleteSaveData.SetActive(false);
        
        SaveLoadManager.ResetSaves();

        SceneManager.LoadScene("CubeyGame");
    }
}
