using System;
using System.Collections.Generic;
using System.Collections;
using Game.Scripts;
using IngameDebugConsole;
using UnityEngine;
using Lean.Touch;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Metadata")]    
    [SerializeField] private SaveMetaData saveMetaData;
    [SerializeField] private LevelMetaData levelMetaData;
    [SerializeField] private ChapterList chapterList;
    
    [SerializeField] private BoolGlobalVariable gameLevel;
    [SerializeField] private IntGlobalVariable pickupCountProperty;
    [SerializeField] private BoolGlobalVariable exitProperty;
    [SerializeField] private BoolGlobalVariable launchArc;
    [SerializeField] private BoolGlobalVariable stickyObject;
    
    [Header("Scripts")]
    [SerializeField] private MapManager mapManager;
    [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private VisualEffects visualEffects;

    
    
    private float timer;
    private string levelName;

    private int gold;
    private int silver;
    private int bronze;

    private int levelNo;
    private int chapterNo;
    [Header("Player")]
    [SerializeField] private GameObject cubeyPlayer;
    [SerializeField] private GameObject cubeyPlayerPrefab;
    [SerializeField] private LeanForceRigidbodyCustom leanForceRb;
    
    [Header("Game Screens")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject failedScreen;
    [SerializeField] private GameObject helpScreen;
    [SerializeField] private GameObject topUi;
    // [SerializeField] private GameObject levelOpen;
    [SerializeField] private GameObject loadingScreen;
    // [SerializeField] private GameObject environment;
    
    [SerializeField] private GameObject deathWalls;

    [Header("Text")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text itemText;
    [SerializeField] private Text jumpText;
    [SerializeField] private Text jumpAmountText;
    
    [Header("Level Exit")]
    private GameObject exitPrezzie;
    [SerializeField] private GameObject exitObject;
    
    [SerializeField] public float cubeyJumpHeight = 2.6f;
    [SerializeField] public bool useTimer;

    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioSource levelMusic;
    [SerializeField] private bool camMovement;

    private ParticleSystem pe;
    
    public Rigidbody playerRb;
    
    [SerializeField] private bool gameLevelEnabled;
    
    public bool GameLevel
    {
        get
        {
            gameLevelEnabled = gameLevel.CurrentValue;
            return gameLevel.CurrentValue;
        }
        set => gameLevel.CurrentValue = value;
    }

    public bool CamMovement
    {
        get => camMovement;
        set => camMovement = value;
    }

    public int PickupCountProperty
    {
        get => pickupCountProperty.CurrentValue;
        set => pickupCountProperty.CurrentValue = value;
    }

    public int JumpCount
    {
        get => jumpCount;
        set
        {
            jumpCount = value;
            jumpAmountText.text = jumpCount.ToString();
        }
    }

    public bool LaunchArc
    {
        get => launchArc;
        set => launchArc.CurrentValue = value;
    }

    public LevelMetaData LevelMetaData => levelMetaData;
    public SaveMetaData SaveMetaData => saveMetaData;

    public bool StickyObject
    {
        get => stickyObject;
        set => stickyObject.CurrentValue = value;
    }

    public bool playSingleLevel = false;

    // [SerializeField] private GameObject xagonBg;
    // [SerializeField] private GameObject treeRight;
    // [SerializeField] private GameObject levelsGrp;


    // [SerializeField] public bool playerStuck;
    [SerializeField] public bool allowMovement;
    // [SerializeField] private bool allowFlight;
    [SerializeField] private bool jumpCountIncreases;
    [SerializeField] private bool xagon;
    [SerializeField] private bool onBreakablePlatform;
    [SerializeField] private bool onMovingPlatform;
    [SerializeField] private bool forceJump;


    // [SerializeField] private Text jumpsText;
    // [SerializeField] private Text youLasted;
    // [SerializeField] private Text bestTime;

    [Header("ints")]
    // private int chapterAndLevelNo;
    // private int savedLevel;
    // private int itemCount;
    private int jumpCount;
    private int jumpsToStartWith = 10;
    private int time;
    private int countdown = 60;

    // [SerializeField] private int pickupsLeft;
    // [SerializeField] private int maxSweets;
    [SerializeField] private int award;

    [Header("Animation")]
    [SerializeField] private Animator starGold_anim;
    [SerializeField] private Animator starSilver_anim;
    [SerializeField] private Animator starBronze_anim;

    [Header("Star Colours")]
    [SerializeField] private Color starGold = new Color(0.95f, 0.95f, 0, 1);
    [SerializeField] private Color starSilver = new Color(1, 0.86f, 0, 1);
    [SerializeField] private Color starBronze = new Color(1, 0.5f, 0, 1);
    [SerializeField] private Color starDefault = new Color(1, 1, 1, 0.3f);

    [Header("Other")]
    [SerializeField] private Vector3 cubeyPosition;
    
    public float CubeyMagnitude;

    private string pickupName = " Pickups";
    private int stat_Jumps;
    private Vector3 flip;
    
    private bool isPlayerRbNotNull;
    private bool isPlayerCubeNotNull;


    
    private void Awake()
    {
        gameLevel.OnValueChanged += LoadGameLevel;
        pickupCountProperty.OnValueChanged += CheckPickupCount;
        exitProperty.OnValueChanged += LoadEndScreen;
        stickyObject.OnValueChanged += ToggleSticky;

        if (leanForceRb == null)
            leanForceRb = FindObjectOfType<LeanForceRigidbodyCustom>();
        
        GameLevel = false;

        visualEffects.peExitSwirl.SetActive(false);

        jumpCount = jumpsToStartWith;
        jumpAmountText.text = jumpCount.ToString();

        SetGameCanvases(false);
    }
    
    private void OnDestroy()
    {
        gameLevel.OnValueChanged -= LoadGameLevel;
        pickupCountProperty.OnValueChanged -= CheckPickupCount;
        exitProperty.OnValueChanged -= LoadEndScreen;
        // launchArc.onValueChanged -= EnableLaunchArc;
    }
    
    private void GetLevelInfo()
    {
        levelNo = saveMetaData.LastLevelPlayed;
        chapterNo = saveMetaData.LastChapterPlayed;
        levelMetaData = chapterList[saveMetaData.LastChapterPlayed].LevelList[saveMetaData.LastLevelPlayed];
        
        timer = levelMetaData.Timer;
        bronze = levelMetaData.JumpsForBronze;
        silver = levelMetaData.JumpsForSilver;
        gold = levelMetaData.JumpsForGold;
        levelName = levelMetaData.LevelName;
        Debug.Log($"Level {levelNo}s info received.");
    }

    private void OnEnable()
    {
        CamMovement = true;
        exitProperty.CurrentValue = false;
        GetLevelInfo();
        StartLevel();
        StartCoroutine(LoadingScene(false));
    }

    private void OnDisable()
    {
        // cubeyPlayer.transform.SetParent(transform);
        cubeyPlayer.SetActive(false);
        leanForceRb.canJump = false;
        SetGameCanvases(false);
        // mapManager.enabled = true;
        
        starGold_anim.Play("mapStarGoldBounce");
    }

    // private void EnableLaunchArc(bool on)
    // {
    //     
    // }
    
    private void LoadGameLevel(bool enable)
    {
        enabled = enable;
        launchArc.CurrentValue = enable;
        
        mainMenuManager.mainMenu.SetActive(!enable);
        mainMenuManager.enabled = !enable;
        exitObject.SetActive(!enable);

        ResetCubeyPlayer(!enable);
        SetGameCanvases(enable);
        visualEffects.ParticleEffectsGo = enable;
    }

    private void SetGameCanvases(bool enable)
    {
        topUi.SetActive(enable);
        PauseGame(false);
        EndScreen(false);
        FailedScreen(false);
    }

    private void DisableStartPosition()
    {
        // levelMetaData.LevelPrefab.transform.GetChild(1);
        // find PlacementCube, disable
        mapManager.LevelGameObject.transform.GetChild(1)?.GetChild(1)?.gameObject.SetActive(false);
    }

    private void StartLevel()
    {
        Debug.Log("Starting level");
        
        visualEffects.player = cubeyPlayer;

        // timer = GetComponent<Timer>();
        if (deathWalls != null)
            deathWalls.SetActive(false);
        
        // ResetCubeyPlayer(false);
        RestartTimer();

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        
        // EndScreen(false);
        // FailedScreen(false);
        
        flip = cubeyPlayer.transform.localScale;
        DisableStartPosition();
        
        UpdateLevelText(levelNo);

        //blowingLeaves.Stop();

        /*if (chapterNo == 0)
        {
            pickupName = " Sweets";
            // setup game music
            // levelMusic = GameObject.Find("XmasMusic").GetComponent<AudioSource>();
            // audioManager.levelMusic = levelMusic;
        }
        else
        {
            pickupName = " Pickups";
            // levelMusic = GameObject.Find("LevelMusic").GetComponent<AudioSource>();
            // audioManager.levelMusic = levelMusic;
        }*/

        JumpCount = 10;
        CountSweetsForLevel();
        SetupExit();
        
        if (audioManager != null)
            audioManager.menuMusic = null;

        if (itemText != null)
            itemText.text = PickupCountProperty + pickupName + " left";
        
        mapManager.enabled = false;
    }

    public void Update()
    {
        if (leanForceRb.canJump && CheckJumpMagnitude() && !forceJump)
        {
            PlayerAllowedJump(false);
        }

        if (onMovingPlatform && playerRb.velocity.magnitude < 1f)
        {
            PlayerAllowedJump(true);
        }
        else if (playerRb.velocity.magnitude < 0.1f)
            PlayerAllowedJump(true);

        if (isPlayerRbNotNull)
            CubeyMagnitude = playerRb.velocity.magnitude;

        CheckJumpCount();

        if (isPlayerCubeNotNull)
            cubeyPosition = cubeyPlayer.transform.position;
    }

    private void ToggleSticky(bool on)
    {
        if (!on)
        {
            // return everything to normal?
            playerRb.drag = 0;
            return;
        }
        // StickyObject = true;
        
        /*
        playerRb.useGravity = false;
        playerRb.velocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
        playerRb.isKinematic = true;
        PlayerVelocity(0);
        PlayerAllowedJump(true);
        */
        
        // slow down gravity or turn on drag?
        // playerRb.useGravity = false;
        playerRb.drag = 10;
        playerRb.velocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
        playerRb.isKinematic = true;
        PlayerVelocity(0);
        PlayerAllowedJump(true);
        
        /*playerStuck = on;
        
        playerRb.useGravity = !on;
        playerRb.velocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
        playerRb.isKinematic = on;
        PlayerVelocity(0);
        PlayerAllowedJump(on);*/
        
        playerRb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        
        
        Debug.Log("Toggle sticky " + on);
    }
    
    public void LoadHelpScreen(bool on)
    {
        helpScreen.SetActive(on);
    }

    public void ToggleScreen(GameObject screen)
    {
        screen.SetActive(!screen.activeSelf);
    }

    private bool CheckJumpMagnitude()
    {
        if (playerRb.velocity.magnitude > 0.1f)
            return true;
        return false;
    }


    private void CheckJumpCount() 
    {
        // checks when jumpcount has reached 0 and not on a breakable platform
        if (jumpCountIncreases && jumpCount == 0 && leanForceRb.canJump && !onBreakablePlatform && !CheckJumpMagnitude())
        {
            FailedScreen(true);
        }
        else if (jumpCountIncreases && jumpCount == 0 && leanForceRb.canJump && onBreakablePlatform)
            StartCoroutine(DelayFailedScreen());
        
    }

    private IEnumerator DelayFailedScreen()
    {
        yield return new WaitForSeconds(4);
        FailedScreen(true);
    }

    private void RestartTimer()
    {
        time = countdown;
        // var startTimer = GetComponent<Timer>();
    }

    private void HideScreens()
    {
        PauseMenu(false);
        FailedScreen(false);
        EndScreen(false);
        
        // UiManager.Instance.Tutorial(false, "", "");
    }

    public void RestartLevel()
    {
        StartCoroutine(LoadingScene(true));
        HideScreens();
        enabled = false;
        mapManager.enabled = true;
        mapManager.RestartLevel();
        
    }

    // Used as failed screen button
    public void LoadMainMenu()
    {
        GameLevel = false;
        StartCoroutine(LoadingScene(true));
        HideScreens();
        SceneManager.LoadScene("CubeyGame");
    }

    // loads from exiting or timer ending
    private void LoadEndScreen(bool won)
    {
        if (!won)
        {
            // Time hits 0 - did not finish
            FailedScreen(true);
            /*if (allowFlight)
            {
                // youLasted.text = "You lasted\n" + GetStopwatchCount() + " seconds!";
                // load continue button
                // UiManager.continueButton.SetActive(true);
            }*/
        }
        else
        {
            // change end text to COMPLETED 
            // PE_Stars();
            // play audio
            // audioManager.PlayAudio(audioManager.cubeyCelebtration);

            EndScreen(true);
            ShowStarsAchieved();
        }
    }

    /*private int GetStopwatchCount()
    {
        // var stopwatch = GetComponent<Timer>().stopwatch;
        return (int)stopwatch;
    }*/

    private void PE_Stars()
    {
        pe.Play();
    }

    private void ResetCubeyPlayer(bool disable)
    {
        if (disable)
        {
            playerRb.useGravity = false;
            cubeyPlayer.SetActive(false);
            return;
        }
        
        if (cubeyPlayer == null && cubeyPlayerPrefab != null)
        {
            GameObject cubey = Instantiate(cubeyPlayerPrefab);
            playerRb = cubey.GetComponent<Rigidbody>();
        }
        
        cubeyPlayer.SetActive(true);
        GetPlayerSpawn();
        playerRb = cubeyPlayer.gameObject.GetComponent<Rigidbody>();
        playerRb.freezeRotation = true;
        playerRb.velocity = new Vector3(0, 0, 0);
        playerRb.freezeRotation = false;
        playerRb.useGravity = true;

        playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;

        leanForceRb.canJump = true;

        /*if (allowFlight)
        {
            playerRb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
            /*for(int i = 0; i < ScrollingHorizontalLevel.Instance.spawnedPlatforms.Count; i++)
            {
                Destroy(ScrollingHorizontalLevel.Instance.spawnedPlatforms[i].gameObject);
            }
            ScrollingHorizontalLevel.Instance.spawnedPlatforms.Clear();#1#
        }*/

        RestartTimer();
    }

    private void UpdateLevelText(int n)
    {
        levelText.text = "Level " + n;
    }

    /*public void UpdateLevelString(string n)
    {
        levelText.text = n;
        print("l: " + n);
    }*/

    private void CheckPickupCount(int count)
    {
        itemText.text = PickupCountProperty + pickupName + " left";
        
        if (PickupCountProperty <= 0)
        {
            OpenExit();
            itemText.text = "Go to Exit";
        }
    }

    private void CountSweetsForLevel()
    {
        PickupCountProperty = 0;
        GameObject pickupGroup = mapManager.LevelGameObject.transform.Find("Pickups").gameObject;

        if (pickupGroup == null)
        {
            Debug.Log("<color:red>Setup pickups for level!</color>");
            return;
        }
        
        for (int i = 0; i < pickupGroup.transform.childCount; i++)
        {
            PickupCountProperty++;
        }

        if (!useTimer)
        {
            if (PickupCountProperty == 1)
            {
                itemText.text = PickupCountProperty + pickupName + " \nleft";
            } else
                itemText.text = PickupCountProperty + pickupName + " \nleft";
        } else
        {
            itemText.text = PickupCountProperty + pickupName + " \ncollected";
        }
    }

    private void OpenExit()
    {
        // count sweets in level
        if (saveMetaData.LastChapterPlayed == 0)
        {
            audioManager.PlayAudio(audioManager.cubeyExitOpen);
            SetupExitXmas(false, true);
        }
        else
        {
            audioManager.PlayAudio(audioManager.cubeyExitOpen);

            if (exitObject == null)
                exitObject = FindExit();
            
            // play pe explosion
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peExitExplosion, exitObject.transform.position);
            VisualEffects.Instance.peExitSwirl.SetActive(false);

            // enable and scale up exit to full size
            exitObject.SetActive(true);
            exitObject.transform.GetChild(0).gameObject.SetActive(true);
            Debug.Log("Open Exit");
        }
    }

    // Need to get actual in game object
    private GameObject FindExit()
    {
        return mapManager.LevelGameObject.transform.GetChild(0).name.Contains("Exit")
            ? mapManager.LevelGameObject.transform.GetChild(0).gameObject
            : mapManager.LevelGameObject.transform.Find("Exit").gameObject;
    }
    
    private void SetupExit()
    {
        exitObject = FindExit();
        
        visualEffects.peExitSwirl.SetActive(true);
        
        var pePos = exitObject.transform.position;
        pePos.y += 0.65f;
        visualEffects.peExitSwirl.transform.position = pePos;
        
        exitObject.SetActive(false);
    }

    private void SetupExitXmas(bool closed, bool open)
    {
        exitPrezzie = FindExit();

        var prezzieClosed = exitPrezzie.transform.GetChild(0).gameObject;
        var prezzieFlat = exitPrezzie.transform.GetChild(1).GetComponent<SpriteRenderer>();
        var prezzieCollision = exitPrezzie.transform.GetChild(2).GetComponent<BoxCollider>();

        prezzieClosed.gameObject.SetActive(closed);
        prezzieCollision.enabled = closed;

        prezzieFlat.enabled = open;

        var newPePos = prezzieFlat.transform.position;
        newPePos.y += 2f;

        /*if (prezzieFlat.enabled)
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peExitOpened, newPePos);*/
    }

    public void PauseGame(bool enable)
    {
        pauseMenu.SetActive(enable);
        Time.timeScale = enable ? 0 : 1;
    }
    
    /*private void HideAllLevels()
    {
        /*if (ChaptersAndLevels.levelsList.Count > 0)
        {
            foreach (GameObject go in ChaptersAndLevels.levelsList)
                go.SetActive(false);
        }#1#
    }*/

    public void PlayerAllowedJump(bool jump)
    {
        leanForceRb.canJump = jump;
    }

    public void PlayerVelocity(float n)
    {
        leanForceRb.velocityMultiplier = n;
    }

    private void GetPlayerSpawn()
    {
        cubeyPlayer.transform.SetParent(null);

        var pos = levelMetaData.LevelPrefab.transform.GetChild(1).transform.position;
        DisableStartPosition();
        // levelMetaData.LevelPrefab.transform.GetChild(1)
        cubeyPlayer.gameObject.transform.SetPositionAndRotation(pos, new Quaternion(0, 0, 0, 0));
    }

    /// <summary>
    /// LOAD MAP AFTER EACH LEVEL
    /// </summary>
    /*private void LoadMapInMenu()
    {
        // Set map depending on chapterNothat player is on
        // PlayerPrefs.SetInt("loadMap", ChaptersAndLevels.chapterNo);

        // saveMetaData.ReturnToMap = true;
        saveMetaData.LastLevelPlayed = levelNo;
        // PlayerPrefs.SetInt("lastLevel", levelNo);

        // SceneManager.LoadScene("Main_Menu");
    }*/

    private int StarsGiven()
    {
        
        var jumps = jumpsToStartWith - jumpCount;

        print("jumps: " + jumps + ", jumpsToStartWith: " + jumpsToStartWith);

        if (jumps <= gold)
            return 3;
        else if (jumps <= silver)
            return 2;
        else if (jumps <= bronze)
            return 1;
            

        return 0;
    }

    private void SetAwardForLevel(int n)
    {
        if (n > levelMetaData.AwardsReceived)
            levelMetaData.AwardsReceived = n;
        
        EditorUtility.SetDirty(levelMetaData);
    }

    // Shows stars on finished screen
    private void ShowStarsAchieved()
    {
        award = StarsGiven();

        var sGrp = endScreen.transform.Find("StarsGrp");
        List<Image> starImages = new List<Image>();

        for (int i = 0; i < 3; i++)
        {
            starImages.Add(sGrp.transform.GetChild(i).GetComponent<Image>());
            starImages[i].color = starDefault;
        }

        SetAwardForLevel(award);

        switch (award)
        {
            case 1:
                starImages[0].color = starBronze;

                starBronze_anim.Play("StarBronze", 0);
                starBronze_anim.speed = 1;

                starSilver_anim.Play("StarSilver", 0, 0f);
                starSilver_anim.speed = 0;

                starGold_anim.Play("StarGold", 0, 0.4f);
                starGold_anim.speed = 0;

                starImages[2].color = starDefault;
                break;
            case 2:
                starImages[0].color = starBronze;
                starImages[1].color = starSilver;

                starBronze_anim.Play("StarBronze", 0);
                starBronze_anim.speed = 0;

                starSilver_anim.Play("StarSilver", 0, 0f);
                starSilver_anim.speed = 1;

                starGold_anim.Play("StarGold", 0, 0.4f);
                starGold_anim.speed = 0;
                starImages[2].color = starDefault;
                break;
            case 3:
                starImages[0].color = starBronze;
                starImages[1].color = starSilver;
                starImages[2].color = starGold;
                starGold_anim.Play("StarGold", 0, 0.4f);
                starBronze_anim.speed = 0;
                starSilver_anim.speed = 0;
                starGold_anim.speed = 1;
                break;
        }
    }

    private void SaveLevelStats()
    {
        /*levelNo--;
        var time_ = timer.countdown;
        var star = StarsGiven();
        var items = GetFriendsFound();
        var jumped = jumpCount;
        var restarted = 0;
        var score = award;
        print("award: " + award);

        levelStats.SaveLevel(levelNo, time_, star, items, jumped, restarted, score);*/
    }

    IEnumerator LoadingScene(bool on)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(on);
            yield return new WaitForSeconds(0.2f);
            loadingScreen.SetActive(false);
        }
    }

    private void SaveLevel()
    {
        /*var chapterLevel = int.Parse(ChaptersAndLevels.chapterNo.ToString("0") + levelNo.ToString("00"));

        // is it more than the savedLevel?
        // is the savedLevel showing up as another chapter?
        // need to get savedLevel for this chapter
        if (levelNo > GetSavedLevel())
            PlayerPrefs.SetInt(ChaptersAndLevels.chapterNo + "level", chapterLevel);

        print("SaveLevel: Chapter+Level: " + chapterLevel);*/
    }

    // When leaving game level
    private int GetSavedLevel()
    {
        // saving correct 3 digit number
        /*if (PlayerPrefs.HasKey(ChaptersAndLevels.chapterNo + "level"))
        {
            var chapterLevel = PlayerPrefs.GetInt(ChaptersAndLevels.chapterNo + "level"); // returns 3
            print("1. chapter and level saved previously: " + savedLevel);
            
            //savedLevel = chapterLevel;
            savedLevel = saveMetaData.LastLevelPlayed; // returns 2 digits

            print("2. level saved previously: " + savedLevel);

            return savedLevel;
        }
        else
        {
            print("no saved level found");
        }*/
        return 0;
    }

    private void PauseMenu(bool on)
    {
        //audioManager.MuteAudio(audioManager.levelMusic, on);
        pauseMenu.SetActive(on);
        Time.timeScale = on ? 0f : 1f;
    }

    private void EndScreen(bool on)
    {
        endScreen.SetActive(on);
        Time.timeScale = on ? 0.1f : 1f;
    }

    private void FailedScreen(bool on)
    {
        failedScreen.SetActive(on);
        Time.timeScale = on ? 0f : 1f;
    }

    public void PlayerJumped()
    {
        audioManager.PlayAudio(audioManager.cubeyJump);

        if (!jumpCountIncreases)
        {
            if (jumpCount == 0)
                jumpCount = 0;
            else
                jumpCount--;
        }
        else
        {
            jumpCount++;
        }

        // jumpAmountText.text = jumpCount.ToString();
        JumpCount = jumpCount;
    }
    
    

    /*public void AddJump()
    {
        jumpCount++;
        jumpText.text = jumpCount.ToString();
    }*/

    public void PlayerFaceDirection(bool right)
    {
        flip.x = -1;

        if (right)
            flip.x = 1;

        flip.y = 1;
        flip.z = 1;

        playerRb.gameObject.transform.localScale = flip;
    }

    private void Stats_Load()
    {
        if (PlayerPrefs.HasKey("stat_Jumps"))
        {
            stat_Jumps = PlayerPrefs.GetInt("stat_Jumps");
        }
    }

    private void Stats_Save()
    {
        if (jumpCount > stat_Jumps)
        {
            PlayerPrefs.SetInt("stat_Jumps", stat_Jumps);
        }
    }

    public void HideGameObject(GameObject go)
    {
        StartCoroutine(HideObject(go));
    }

    private IEnumerator HideObject(GameObject go)
    {
        yield return new WaitForSeconds(3);
    }
    
    private void BasicLevelSetup(int c, int n)
    {
        // set exit up
        if (ChaptersAndLevels.levelsList[n].transform.Find("Exit"))
        {
            // exitObject = ChaptersAndLevels.levelsList[n].transform.Find("Exit").transform.GetChild(0).gameObject;
        }
        else if (ChaptersAndLevels.levelsList[n].transform.Find("MovingExitPlatform"))
        {
            // exitObject = ChaptersAndLevels.levelsList[n].transform.Find("MovingExitPlatform").Find("Exit").transform.GetChild(0).gameObject;
            // visualEffects.peExitSwirl.transform.parent = ChaptersAndLevels.levelsList[n].transform.Find("MovingExitPlatform").transform;
        }
        else
        {
            print("missing exit");
        }

        // enable exit swirl
        visualEffects.peExitSwirl.SetActive(true);
        // var pePos = exitObject.transform.position;
        // pePos.y += 0.65f;
        // visualEffects.peExitSwirl.transform.position = pePos;

        // exitObject.SetActive(false);
    }
}
