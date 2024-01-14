using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    public enum CollisionBox
    {
        Menu,
        Map,
        Level
    }

    #region Fields

    [Header("Scripts")]
    
    private CameraManager cameraManager;
    private GameManager gameManager;
    private AudioManager audioManager;

    [Header("Maps/Menu")]
    [SerializeField] private GameObject menuEnvironmentParent;
    [SerializeField] private List<GameObject> menuEnvironments;
    [SerializeField] private GameObject mapsParent;
    [SerializeField] private List<GameObject> chapterMaps;
    
    [Header("Extra screens")]
    [SerializeField] private GameObject chapterFinishScreen;
    [SerializeField] private bool deleteLastChapterFinishScreenData;
    [SerializeField] private GameObject loadingScreen;
    
    [Header("GameObjects")]
    [SerializeField] private GameObject chapterScreen;
    [SerializeField] private GameObject mainMenuUi;
    [SerializeField] private GameObject navButtons;
    [SerializeField] private GameObject screenDeleteSaveData;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Animator menuButtonAnim;
    [SerializeField] private GameObject shopButton;

    [SerializeField] private Text startText;
    [SerializeField] private Text versionNo;

    [SerializeField] private LeanCameraZoomSmooth leanZoom;
    [SerializeField] private List<GameObject> chapterButtons;
    [SerializeField] private Color fadedButton = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] private Color unlockedButton = Color.white;

    #endregion Fields

    #region Private variables

    private Color c1, c2, c2b, c3;
    private Vector3 pos1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 pos2 = new Vector3(1f, 1f, 1f);
    private Vector3 scale1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 scale2 = new Vector3(0.95f, 0.95f, 0.95f);
    private ChapterList chapterList;
    private MapManager mapManager;

    #endregion Private variables

    #region Getters

    public bool NavButtons
    {
        get => navButtons.activeInHierarchy;
        set => navButtons.SetActive(value);
    }

    public bool BackButton
    {
        get => backButton.isActiveAndEnabled;
        private set => backButton.gameObject.SetActive(value);
    }

    #endregion Getters

    public static Action OnMainMenuLoad;

    public int chapterUnlockedTo;
    public LeanConstrainToBox leanConstrainToBox;
    public GameObject mainMenu;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        SetNavButtons(false);
        //LoadingScene(true);

        DeleteFinishScreenData();

        SetRefreshRate(PlayerPrefs.GetInt("RefreshRate"));

        UiManager.DeleteSaves += ResetSaves;
        UiManager.OnChapterButtonPressed += ShowMap;
        UiManager.OnLoadChapterScreen += EnableChapterScreen;
    }

    private void OnEnable()
    {
        backButton.gameObject.SetActive(true);
    }

    private void Start()
    {
        mapManager = MapManager.Instance;
        gameManager = GameManager.Instance;
        cameraManager = CameraManager.Instance;
        audioManager = AudioManager.Instance;

        CheckForErrors();

        versionNo.text = "v: " + Application.version;
        chapterList = GlobalMetaData.Instance.ChapterList;

        AddMenuEnvironments();
        SetColours();
        LoadMainMenu();
    }

    public void LoadMainMenu()
    {
        mainMenu.SetActive(true);
        menuEnvironmentParent.SetActive(true);
        mainMenuUi.SetActive(true);
        ShowMenuBackButton(false);

        ButtonSizePong();
        ChangeMenuEnvironment(LevelManager.LastChapterPlayed);
        chapterFinishScreen.SetActive(false);

        OnMainMenuLoad?.Invoke();
        
        shopButton.SetActive(true);
        
        gameManager.SetGameState(GameManager.GameState.Menu);
    }

    public void ShowMenuBackButton(bool state)
    {
        BackButton = state;
    }

    private void DeleteFinishScreenData()
    {
        if (deleteLastChapterFinishScreenData)
        {
            PlayerPrefs.DeleteKey("chapterFinishScreenGold" + LevelManager.LastChapterPlayed);
        }
    }

    public void SetNavButtons(bool state)
    {
        NavButtons = state;
    }

    private void CheckForErrors()
    {
        if (chapterButtons.Count == 0)
            Logger.Instance.ShowDebugError("Assign chapter buttons to list!");

        if (leanConstrainToBox == null) leanConstrainToBox = cameraManager.gameObject.GetComponent<LeanConstrainToBox>();
        if (chapterFinishScreen == null) chapterFinishScreen = GameObject.Find("ChapterFinishScreen");
    }

    private void SetRefreshRate(int n)
    {
        if (PlayerPrefs.HasKey("RefreshRate"))
        {
            Application.targetFrameRate = n;
        }
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

    // TODO - loading scene required?
    public void LoadingScene(bool state)
    {
        loadingScreen.SetActive(state);
    }
    
    public void ToggleGameObject(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    private void SetColours()
    {
        c1 = new Color(0, 0, 0, 0); // clear
        c2 = new Color(0.1f, 0.3f, 0.7f, 1); // blue
        c2b = new Color(0.2f, 0.4f, 0.9f, 1); // blue
        c3 = new Color(1, 1, 1, 1); // white
    }

    private void AddMenuEnvironments()
    {
        for (int i = 0; i < chapterList.Count; i++)
        {
            var menu = Instantiate(chapterList[i].MenuEnvironment, menuEnvironmentParent.transform);

            if (chapterList[i].MenuCollision == null)
                chapterList[i].MenuCollision = chapterList[i].MenuEnvironment.transform.Find("CollisionMenu").GetComponent<BoxCollider>();

            if (chapterList[i].MapCollision == null)
                chapterList[i].MapCollision = chapterList[i].ChapterMap.transform.Find("CollisionMap").GetComponent<BoxCollider>();

            if (chapterList[i].MapCollision == null)
                print("Can't find: " + chapterList[i].ChapterMap.name);

            menuEnvironments.Add(menu);
            menu.SetActive(false);
        }
    }

    private void ChangeMenuEnvironment(int chapterNo)
    {
        DisableMenuEnv();
        
        menuEnvironments[chapterNo].SetActive(true);

        leanZoom.Zoom = chapterList[chapterNo].MenuZoomLevel;

        ToggleThankYouSign();
    }

    internal void ToggleThankYouSign()
    {
        menuEnvironments[LevelManager.LastChapterPlayed].transform.Find("ThankYou").gameObject.SetActive(ShopManager.GamePurchased);
    }

    private void DisableMenuEnv()
    {
        for (int i = 0; i < menuEnvironments.Count; i++)
        {
            menuEnvironments[i].SetActive(false);
        }
    }

    public void ButtonClose(GameObject go)
    {
        go.SetActive(!go.activeInHierarchy);
    }

    public void SetCollisionBox(CollisionBox collisionBox, BoxCollider col = null)
    {
        if (leanConstrainToBox.Target != null)
            leanConstrainToBox.Target.enabled = true;

        switch (collisionBox)
        {
            case CollisionBox.Menu:
                leanConstrainToBox.Target = chapterList[LevelManager.LastChapterPlayed].MenuCollision.GetComponent<BoxCollider>();
                break;
            case CollisionBox.Map:
                leanConstrainToBox.Target = chapterList[LevelManager.LastChapterPlayed].MapCollision.GetComponent<BoxCollider>();
                break;
            case CollisionBox.Level:
                leanConstrainToBox.Target = col;
                break;
            default:
                if (leanConstrainToBox.Target != null)
                {
                    leanConstrainToBox.Target.enabled = false;
                    leanConstrainToBox.Target = null;
                }
                break;
        }
    }

    private void ShowMap(int n)
    {
        int chapter = LevelManager.LastChapterPlayed = n;
        
        ChangeMenuEnvironment(chapter);
        DisableMenuScreens();
        SetCollisionBox(CollisionBox.Map);
        audioManager.AudioButtons.SetActive(false);
        leanZoom.enabled = true;
        SetNavButtons(true);
        DisableMenuEnv();

        MapManager.Instance.LoadMapScreen();

        BackButton = true;
        backButton.onClick.AddListener(() => EnableChapterScreen(true));
    }

    private void EnableChapterScreen(bool enable)
    {
        CycleThroughUnlockedChapters();
        
        chapterScreen.SetActive(enable);
        SetNavButtons(enable);
        mainMenuUi.SetActive(!enable);
        menuEnvironmentParent.SetActive(true);
        audioManager.AudioButtons.SetActive(enable);
        
        leanZoom.enabled = false;
        BackButton = true;
        backButton.onClick.AddListener(MainMenuScreen);
    }

    // disable chapters that aren't unlocked?
    internal void CycleThroughUnlockedChapters()
    {
        for (int i = 3; i < chapterList.Count; i++)
        {
            bool unlocked = ShopManager.GamePurchased ? SaveLoadManager.GetChapterUnlocked(i) : false;
            chapterButtons[i].GetComponent<Button>().interactable = unlocked;
            var image = chapterButtons[i].transform.Find("Mask/Screenshot").GetComponent<Image>();
            image.color = unlocked ? unlockedButton : fadedButton;
        }
    }

    private void DisableMenuScreens()
    {
        chapterScreen.SetActive(false);
        mainMenuUi.SetActive(false);
        menuEnvironmentParent.SetActive(false);
    }

    // TODO - is this and LoadMainMenu required? line133
    private void MainMenuScreen()
    {
        mapManager.DisableMaps();
        ChangeMenuEnvironment(LevelManager.LastChapterPlayed);
        SetCollisionBox(CollisionBox.Menu);
        chapterFinishScreen.SetActive(false);
        chapterScreen.SetActive(false);
        SetNavButtons(false);
        mainMenuUi.SetActive(true);
        OnMainMenuLoad?.Invoke();
        cameraManager.ResetCamPosition();
        leanZoom.enabled = true;
    }

    private void ButtonSizePong()
    {
        Pong();
    }

    private async void Pong()
    {
        await System.Threading.Tasks.Task.Delay(1000);
        menuButtonAnim.SetBool("EnablePingPong", true);
    }
    
    public void TryChapterFinishScreen()
    {
        var goldFinishScreen = PlayerPrefs.GetInt("chapterFinishScreenGold" + LevelManager.LastChapterPlayed , 0);
        
        if (LevelManager.LastLevelPlayed == 29 &&
            AwardManager.GetLevelAward(29) >= 1 &&
            goldFinishScreen == 0)
        {
            PlayerPrefs.SetInt("chapterFinishScreenGold" + LevelManager.LastChapterPlayed, 1);
            chapterFinishScreen.GetComponent<ChapterComplete>().OpenPopup();
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
