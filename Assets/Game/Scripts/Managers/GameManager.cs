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
    
    [Header("Scriptable Objects")]
    [SerializeField] private BoolGlobalVariable gameLevel;
    [SerializeField] private IntGlobalVariable pickupCountProperty;
    [SerializeField] private BoolGlobalVariable exitProperty;
    [SerializeField] private BoolGlobalVariable launchArc;
    [SerializeField] private BoolGlobalVariable stickyObject;
    
    [Header("Scripts")]
    [SerializeField] private MapManager mapManager;
    [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private VisualEffects visualEffects;

    [Header("Player")]
    [SerializeField] private GameObject cubeyPlayer;
    [SerializeField] private GameObject cubeyPlayerPrefab;
    [SerializeField] private LeanForceRigidbodyCustom leanForceRb;
    
    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioSource levelMusic;

    public static Transform gameFolder;
    private float timer;
    private string levelName;

    private int gold;
    private int silver;
    private int bronze;

    private int levelNo;
    private int chapterNo;

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

    [Header("UI Text")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text itemText;
    [SerializeField] private Text jumpText;
    [SerializeField] private Text jumpAmountText;
    [SerializeField] private Text awardToGet;
    
    [Header("Level Exit")]
    private GameObject exitPrezzie;
    [SerializeField] private GameObject exitObject;
    [SerializeField] public float cubeyJumpHeight = 2.6f;
    [SerializeField] public bool useTimer;
    [SerializeField] private bool camMovement;
    [SerializeField] private bool gameLevelEnabled;
    [SerializeField] private float playerGooDrag = 35f;

    private ParticleSystem pe;
    public Rigidbody playerRb;

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
        get => jumpLeft;
        set
        {
            jumpLeft = value;
            jumpAmountText.text = jumpLeft.ToString();
        }
    }

    public bool LaunchArc
    {
        get => launchArc;
        set => launchArc.CurrentValue = value;
    }
    
    public GameObject GetLevelExit
    {
        get => exitObject;
    }

    public GameObject CubeyPlayer
    {
        get => cubeyPlayer;
        set => cubeyPlayer = value;
    }

    public LevelMetaData LevelMetaData => levelMetaData;
    public SaveMetaData SaveMetaData => saveMetaData;

    public bool playSingleLevel = false;

    // [SerializeField] private GameObject xagonBg;
    // [SerializeField] private GameObject treeRight;
    // [SerializeField] private GameObject levelsGrp;

    [SerializeField] public bool allowPlayerMovement;
    // [SerializeField] private bool allowFlight;
    [SerializeField] private bool jumpCountIncreases;
    [SerializeField] private bool xagon;
    [SerializeField] private bool onBreakablePlatform;
    [SerializeField] private bool onMovingPlatform;
    [SerializeField] private bool forceJump;

    [Header("ints")]
    private int jumpLeft;
    private int jumpsToStartWith = 10;
    private int time;
    private int countdown = 60;

    [SerializeField] private int award;

    [Header("Animation")]
    [SerializeField] private Animator starGold_anim;
    [SerializeField] private Animator starSilver_anim;
    [SerializeField] private Animator starBronze_anim;

    [Header("Star Colours")]
    private Color starGold = new Color(0.95f, 0.95f, 0, 1);
    private Color starSilver = new Color(1, 0.86f, 0, 1);
    private Color starBronze = new Color(1, 0.5f, 0, 1);
    private Color starDefault = new Color(1, 1, 1, 0.3f);

    [Header("Other")]
    private Vector3 cubeyPosition;
    public float cubeyMagnitude;
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

        if (gameFolder == null)
            gameFolder = GameObject.Find("Game").transform;
        
        GameLevel = false;

        visualEffects.peExitSwirl.SetActive(false);

        jumpLeft = jumpsToStartWith;
        jumpAmountText.text = jumpLeft.ToString();

        SetGameCanvases(false);
    }

    private void OnDestroy()
    {
        gameLevel.OnValueChanged -= LoadGameLevel;
        pickupCountProperty.OnValueChanged -= CheckPickupCount;
        exitProperty.OnValueChanged -= LoadEndScreen;
        stickyObject.OnValueChanged -= ToggleSticky;
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
    }

    private void OnDisable()
    {
        if (cubeyPlayer != null)
            cubeyPlayer.SetActive(false);
        leanForceRb.canJump = false;
        SetGameCanvases(false);
        starGold_anim.Play("mapStarGoldBounce");
    }
    
    private void LoadGameLevel(bool enable)
    {
        enabled = enable;
        ResetCubeyPlayer(!enable);
        launchArc.CurrentValue = enable;

        mainMenuManager.mainMenu.SetActive(!enable);
        mainMenuManager.enabled = !enable;
        exitObject.SetActive(!enable);

        SetGameCanvases(enable);
        visualEffects.ParticleEffectsGo.SetActive(enable);
    }

    private void SetGameCanvases(bool enable)
    {
        if (topUi != null)
            topUi.SetActive(enable);
        PauseGame(false);
        EndScreen(false);
        FailedScreen(false);
    }

    private void DisableStartPosition()
    {
        if (mapManager.LevelGameObject.transform.GetChild(1).name.Contains("Start"))
            mapManager.LevelGameObject.transform.GetChild(1)?.GetChild(1)?.gameObject.SetActive(false);
        else if (mapManager.LevelGameObject.transform.GetChild(0).name.Contains("Start"))
        {
            mapManager.LevelGameObject.transform.GetChild(0)?.GetChild(1)?.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Can't find Start position");
        }
    }

    private void StartLevel()
    {
        // visualEffects.player = cubeyPlayer;

        // timer = GetComponent<Timer>();
        if (deathWalls != null)
            deathWalls.SetActive(false);
        
        // ResetCubeyPlayer(false);
        RestartTimer();

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        
        // EndScreen(false);
        // FailedScreen(false);
        
        if (cubeyPlayer != null)
            flip = cubeyPlayer.transform.localScale;
        else
            Debug.LogError("Can't find Cubey!!");
        
        DisableStartPosition();
        
        UpdateLevelText(levelNo);

        //blowingLeaves.Stop();

        if (chapterNo == 0)
        {
            pickupName = " Sweets";
            // setup game music
            levelMusic = GameObject.Find("XmasMusic").GetComponent<AudioSource>();
            audioManager.levelMusic = levelMusic;
        }
        else
        {
            pickupName = " Pickups";
            levelMusic = GameObject.Find("LevelMusic").GetComponent<AudioSource>();
            audioManager.levelMusic = levelMusic;
        }

        JumpCount = 10;
        CountSweetsForLevel();
        ReParentExitSwirl(false);
        SetupExit();
        StartCoroutine(UpdateAwardsNeeded());
        
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
            cubeyMagnitude = playerRb.velocity.magnitude;

        // CheckJumpCount();

        if (isPlayerCubeNotNull)
            cubeyPosition = cubeyPlayer.transform.position;
    }

    private IEnumerator UpdateAwardsNeeded()
    {
        yield return new WaitUntil(() => levelMetaData != null);
        
        if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForGold) // 10 - 8 < 3
        {
            awardToGet.text = levelMetaData.JumpsForGold + " jumps for gold";
        }
        else if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForSilver)
        {
            awardToGet.text = levelMetaData.JumpsForSilver + " jumps for silver";
        }
        else if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForBronze) // 9 - 9 <= 9
        {
            awardToGet.text = levelMetaData.JumpsForBronze + " jumps for bronze";
        }
        else
        {
            awardToGet.text = "Need bronze for next level";
        }
    }
    
    public void ToggleSticky(bool on)
    {
        if (!on)
        {
            playerRb.drag = 0;
            stickyObject.CurrentValue = false;
            playerRb.isKinematic = false;
            cubeyPlayer.transform.SetParent(gameFolder, true);
            return;
        }
        
        playerRb.drag = playerGooDrag;
        playerRb.velocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
        PlayerAllowedJump(true);
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
        if (jumpCountIncreases && jumpLeft == 0 && leanForceRb.canJump && !onBreakablePlatform && !CheckJumpMagnitude())
        {
            FailedScreen(true);
        }
        else if (jumpCountIncreases && jumpLeft == 0 && leanForceRb.canJump && onBreakablePlatform)
        {
            StartCoroutine(DelayFailedScreen());
        }
        
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
    }

    public void RestartLevel()
    {
        playerRb.drag = 0;
        stickyObject.CurrentValue = false;
        playerRb.isKinematic = false;
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
        }
        else
        {
            // Todo change end text to COMPLETED? 
            // play audio
            audioManager.PlayAudio(audioManager.cubeyCelebtration);
            ReParentExitSwirl(false);
            EndScreen(true);
            ShowStarsAchieved();
        }
    }
    
    private void ResetCubeyPlayer(bool disable)
    {
        Debug.Log("Reseting Cubey");
        
        if (disable)
        {
            playerRb.useGravity = false;
            cubeyPlayer.SetActive(false);
            return;
        }
        
        if (cubeyPlayer == null && cubeyPlayerPrefab != null)
        {
            cubeyPlayer = Instantiate(cubeyPlayerPrefab);
            playerRb = cubeyPlayer.GetComponent<Rigidbody>();
        }
        
        cubeyPlayer.SetActive(true);
        cubeyPlayer.transform.SetParent(gameFolder, true);
        // ReParentExitSwirl(false);
        GetPlayerSpawn();
        playerRb = cubeyPlayer.gameObject.GetComponent<Rigidbody>();
        playerRb.freezeRotation = true;
        playerRb.velocity = new Vector3(0, 0, 0);
        playerRb.freezeRotation = false;
        playerRb.useGravity = true;

        playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
        if (leanForceRb == null)
            leanForceRb = cubeyPlayer.GetComponent<LeanForceRigidbodyCustom>();
        else
            leanForceRb.canJump = true;

        RestartTimer();
    }

    private void UpdateLevelText(int n)
    {
        levelText.text = "Level " + (n+1);
    }

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

    // Need to get actual in game object.
    // 1st child in level for the exit, or find it under the spindle
    private GameObject FindExit()
    {
        if (mapManager.LevelGameObject.transform.GetChild(0).name.Contains("MovingExitPlatform"))
        {
            return mapManager.LevelGameObject.transform.GetChild(0).transform.Find("Exit").gameObject;
        }
        else if (mapManager.LevelGameObject.transform.GetChild(0).name.Contains("Exit")) // default position
        {
            return mapManager.LevelGameObject.transform.GetChild(0).gameObject;
        }
        else if (mapManager.LevelGameObject.transform.Find("Exit")) 
        {
            return mapManager.LevelGameObject.transform.Find("Exit").gameObject;
        }
        else if (mapManager.LevelGameObject.transform.Find("Spindle"))
        {
            return mapManager.LevelGameObject.transform.Find("Spindle").GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("Can't find the exit in: " + mapManager.LevelGameObject.name);
            return null;
        }
    }
    
    private void SetupExit()
    {
        exitObject = FindExit();
        
        visualEffects.peExitSwirl.SetActive(true);
        
        var pePos = exitObject.transform.position;
        pePos.y += 0.65f;
        visualEffects.peExitSwirl.transform.position = pePos;

        if (exitObject.transform.parent.name.Contains("Spindle") || exitObject.transform.parent.name.Contains("MovingExitPlatform"))
        {
            ReParentExitSwirl(true);
        }
        
        exitObject.SetActive(false);
    }

    private void ReParentExitSwirl(bool move)
    {
        if (move)
        {
            visualEffects.peExitSwirl.transform.SetParent(exitObject.transform.parent);
        }
        else
        {
            visualEffects.peExitSwirl.transform.SetParent(visualEffects.ParticleEffectsGo.transform);
        }
    }

    /// <summary>
    /// Find the exit in the xmas levels
    /// </summary>
    /// <param name="closed">Present hiding exit</param>
    /// <param name="open">Flattened present with exit open</param>
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
        if (pauseMenu != null)
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
        cubeyPlayer.transform.SetParent(gameFolder.transform);

        var pos = levelMetaData.LevelPrefab.transform.GetChild(1).transform.position;
        DisableStartPosition();
        cubeyPlayer.gameObject.transform.SetPositionAndRotation(pos, new Quaternion(0, 0, 0, 0));
    }

    private int StarsGiven()
    {
        var jumps = jumpsToStartWith - jumpLeft;

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
        
        if (n == 1 && chapterList[saveMetaData.LastChapterPlayed].AwardsBronze < 3)
            chapterList[saveMetaData.LastChapterPlayed].AwardsBronze += 1;
        else if (n == 2 && chapterList[saveMetaData.LastChapterPlayed].AwardsSilver < 3)
        {
            chapterList[saveMetaData.LastChapterPlayed].AwardsBronze += 1;
            chapterList[saveMetaData.LastChapterPlayed].AwardsSilver += 1;
        }
        else if (n == 3 && chapterList[saveMetaData.LastChapterPlayed].AwardsGold < 3)
        {
            chapterList[saveMetaData.LastChapterPlayed].AwardsBronze += 1;
            chapterList[saveMetaData.LastChapterPlayed].AwardsSilver += 1;
            chapterList[saveMetaData.LastChapterPlayed].AwardsGold += 1;
        }
        
        EditorUtility.SetDirty(levelMetaData);
        EditorUtility.SetDirty(chapterList[saveMetaData.LastChapterPlayed]);
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

    IEnumerator LoadingScene(bool on)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(on);
            yield return new WaitForSeconds(0.2f);
            loadingScreen.SetActive(false);
        }
    }

    // Todo 2 pause menus?
    private void PauseMenu(bool on)
    {
        audioManager.MuteAudio(audioManager.levelMusic, on);
        pauseMenu.SetActive(on);
        Time.timeScale = on ? 0f : 1f;
    }

    private void EndScreen(bool on)
    {
        if (endScreen != null)
            endScreen.SetActive(on);
        Time.timeScale = on ? 0.1f : 1f;
    }

    public void FailedScreen(bool on)
    {
        failedScreen.SetActive(on);
        Time.timeScale = on ? 0f : 1f;
    }

    public void PlayerJumped()
    {
        audioManager.PlayAudio(audioManager.cubeyJump);

        if (!jumpCountIncreases)
        {
            if (jumpLeft == 0)
                jumpLeft = 0;
            else
                jumpLeft--;
        }
        else
        {
            jumpLeft++;
        }

        // jumpAmountText.text = jumpCount.ToString();
        JumpCount = jumpLeft;
        
        StartCoroutine(UpdateAwardsNeeded());
        CheckJumpCount();
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

    /*private void Stats_Load()
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
    }*/

    public void HideGameObject(GameObject go)
    {
        StartCoroutine(HideObject(go));
    }

    private IEnumerator HideObject(GameObject go)
    {
        yield return new WaitForSeconds(3);
    }
    
}
