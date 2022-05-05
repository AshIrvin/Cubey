using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Game.Scripts;
using IngameDebugConsole;
using UnityEngine;
using Lean.Touch;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = System.Random;

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
    // [SerializeField] private GameObject cubeyPlayerPrefab;
    [SerializeField] private LeanForceRigidbodyCustom leanForceRb;
    
    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioSource gameMusic;

    public static Transform gameFolder;
    private float timer;
    private string levelName;

    private int threeStars;
    private int twoStars;
    private int oneStar;

    private int levelNo;
    private int chapterNo;

    [Header("Game Screens")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject failedScreen;
    [SerializeField] private GameObject helpScreen;
    [SerializeField] private GameObject topUi;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject deathWalls;

    [Header("UI Text")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text itemText;
    [SerializeField] private Text jumpAmountText;
    [SerializeField] private Text jumpText;
    [SerializeField] private Text awardToGet;
    [SerializeField] private Text bronzePodium;
    [SerializeField] private Text silverPodium;
    [SerializeField] private Text goldPodium;
    [SerializeField] private Text oneStarTutorialText;
    [SerializeField] private Text twoStarTutorialText;
    [SerializeField] private Text threeStarTutorialText;
    [SerializeField] private Text endScreenInfo;
    
    [Header("Level Exit")]
    private GameObject exitPrezzie;
    [SerializeField] private GameObject exitObject;
    
    public float cubeyJumpHeight = 2.6f;
    public bool useTimer;
    
    private bool camMovement;
    private bool gameLevelEnabled;
    private float playerGooDrag = 35f;
    private ParticleSystem pe;
    private float cubeyJumpMagValue = 0.5f;
    public Rigidbody playerRb;

    [Header("UI Pickups")]
    [SerializeField] private List<Image> pickupUiImages;
    
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
        get => launchArc.CurrentValue;
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

    [SerializeField] public bool allowPlayerMovement;
    [SerializeField] private bool jumpCountIncreases;
    [SerializeField] private bool onBreakablePlatform;
    [SerializeField] private bool onMovingPlatform;

    [Header("ints")]
    private int jumpLeft;
    private int jumpsToStartWith = 10;
    private int time;
    private int countdown = 60;

    private SaveLoadManager.Awards award;

    [Header("Animation")]
    [SerializeField] private Animator starGold_anim;
    [SerializeField] private Animator starSilver_anim;
    [SerializeField] private Animator starBronze_anim;

    [Header("Other")]
    private Vector3 cubeyPosition;
    public float cubeyMagnitude;
    private Vector3 flip;
    // private bool isPlayerRbNotNull;
    // private bool isPlayerCubeNotNull;


    private void Awake()
    {
        gameLevel.OnValueChanged += LoadGameLevel;
        pickupCountProperty.OnValueChanged += CheckPickupCount;
        exitProperty.OnValueChanged += LoadEndScreen;
        stickyObject.OnValueChanged += ToggleSticky;
        leanForceRb.onGround += PlayerAllowedJump;
        
        if (leanForceRb == null)
            leanForceRb = FindObjectOfType<LeanForceRigidbodyCustom>();

        if (gameFolder == null)
            gameFolder = GameObject.Find("Game").transform;
        
        GameLevel = false;

        visualEffects.peExitSwirl.SetActive(false);

        jumpLeft = jumpsToStartWith;
        jumpAmountText.text = jumpLeft.ToString();

        SetGameCanvases(false);
        SetupStarFinishImages();
    }

    private void OnDestroy()
    {
        gameLevel.OnValueChanged -= LoadGameLevel;
        pickupCountProperty.OnValueChanged -= CheckPickupCount;
        exitProperty.OnValueChanged -= LoadEndScreen;
        stickyObject.OnValueChanged -= ToggleSticky;
        leanForceRb.onGround -= PlayerAllowedJump;
    }
    
    private void GetLevelInfo()
    {
        levelNo = SaveLoadManager.LastLevelPlayed;
        chapterNo = SaveLoadManager.LastChapterPlayed;
        levelMetaData = chapterList[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed];
        
        timer = levelMetaData.Timer;
        oneStar = levelMetaData.JumpsForBronze;
        twoStars = levelMetaData.JumpsForSilver;
        threeStars = levelMetaData.JumpsForGold;
        levelName = levelMetaData.LevelName;
        
        // Debug.Log($"Level {levelNo}s info received.");
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
        // Debug.Log("GameManager disabled");
        if (cubeyPlayer != null)
            cubeyPlayer.SetActive(false);
        PlayerAllowedJump(false);
        SetGameCanvases(false);
        starGold_anim.Play("mapStarGoldBounce");
    }
    
    private void LoadGameLevel(bool enable)
    {
        enabled = enable;
        ResetCubeyPlayer(!enable);
        LaunchArc = enable;

        mainMenuManager.mainMenu.SetActive(!enable);
        mainMenuManager.enabled = !enable;
        if (exitObject != null)
        {
            exitObject.SetActive(!enable);
        }

        SetGameCanvases(enable);
        visualEffects.ParticleEffectsGo.SetActive(enable);
    }

    private void SetGameCanvases(bool state)
    {
        TopUi(state);
        PauseMenu(false);
        EndScreen(false);
        FailedScreen(false);
    }

    private void TopUi(bool state)
    {
        if (topUi != null)
        {
            topUi.SetActive(state);
            PickupGraphic(SaveLoadManager.LastChapterPlayed);
        }
    }

    private void DisableStartPosition()
    {
        if (mapManager.LevelGameObject != null && 
            mapManager.LevelGameObject.transform.GetChild(1).name.Contains("Start"))
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
        if (deathWalls != null)
            deathWalls.SetActive(false);
        
        RestartTimer();

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        
        if (cubeyPlayer != null)
            flip = cubeyPlayer.transform.localScale;
        // else
        //     Debug.LogError("Can't find Cubey!!");
        
        DisableStartPosition();
        UpdateLevelText(levelNo);

        JumpCount = 10;
        CountSweetsForLevel();
        ReParentExitSwirl(false);
        SetupExit();
        StartCoroutine(UpdateAwardsNeeded());

        PickupText();
        mapManager.enabled = false;
        TimeTaken(true);

        PlayerFaceDirection(exitObject.transform.position.x < 0);
    }

    public void Update()
    {
        if (playerRb.velocity.magnitude < cubeyJumpMagValue)
        {
            PlayerAllowedJump(true);
        }
        else
        {
            if (stickyObject.CurrentValue)
            {
                return;
            }
            PlayerAllowedJump(false);
        }
    }

    private void ChangeTextColour(Text text, Color color)
    {
        text.color = color;
    }
    
    private IEnumerator UpdateAwardsNeeded()
    {
        yield return new WaitUntil(() => levelMetaData != null);
        
        if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForGold) // 10 - 8 < 3
        {
            // awardToGet.text = levelMetaData.JumpsForGold + " jumps for gold";
            ChangeTextColour(jumpAmountText, ColourManager.starGold);
        }
        else if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForSilver)
        {
            // awardToGet.text = levelMetaData.JumpsForSilver + " jumps for silver";
            ChangeTextColour(jumpAmountText, ColourManager.starSilver);
        }
        else if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForBronze) // 9 - 9 <= 9
        {
            // awardToGet.text = levelMetaData.JumpsForBronze + " jumps for bronze";
            ChangeTextColour(jumpAmountText, ColourManager.starBronze);
        }
        else
        {
            awardToGet.text = "Need bronze for next level";
            ChangeTextColour(jumpAmountText, ColourManager.starDefault);
        }

        oneStarTutorialText.text = levelMetaData.JumpsForBronze.ToString();
        twoStarTutorialText.text = levelMetaData.JumpsForSilver.ToString();
        threeStarTutorialText.text = levelMetaData.JumpsForGold.ToString();

        bronzePodium.text = levelMetaData.JumpsForBronze.ToString();
        silverPodium.text = levelMetaData.JumpsForSilver.ToString();
        goldPodium.text = levelMetaData.JumpsForGold.ToString();
    }
    
    public void ToggleSticky(bool state)
    {
        if (!state)
        {
            playerRb.drag = 0;
            playerRb.angularDrag = 0;
            stickyObject.CurrentValue = false;
            playerRb.isKinematic = false;
            cubeyPlayer.transform.SetParent(gameFolder, true);
            return;
        }
        
        playerRb.drag = playerGooDrag;
        playerRb.angularDrag = playerGooDrag;
        playerRb.velocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
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
        if (playerRb.velocity.magnitude > cubeyJumpMagValue)
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
        TimeTaken(true);
    }

    // Used as failed screen button
    public void LoadMainMenu()
    {
        for (int i = 0; i < mapManager.LevelParent.transform.childCount; i++)
        {
            Destroy(mapManager.LevelParent.transform.GetChild(i).gameObject);
        }
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
            // Time hits 0 - did not finish. Time??
            FailedScreen(true);
        }
        else
        {
            TimeTaken(false);
            // Todo change end text to COMPLETED? 
            audioManager.PlayAudio(audioManager.cubeyCelebration);
            ReParentExitSwirl(false);
            EndScreen(true);
            
            ShowStarsAchieved();
        }
    }
    
    private void ResetCubeyPlayer(bool disable)
    {
        if (disable)
        {
            playerRb.useGravity = false;
            cubeyPlayer.SetActive(false);
            return;
        }
        
        cubeyPlayer.SetActive(true);
        cubeyPlayer.transform.SetParent(gameFolder, true);
        GetPlayerSpawn();
        playerRb = cubeyPlayer.gameObject.GetComponent<Rigidbody>();
        playerRb.freezeRotation = true;
        playerRb.velocity = new Vector3(0, 0, 0);
        playerRb.freezeRotation = false;
        playerRb.useGravity = true;

        playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
        if (leanForceRb == null)
            leanForceRb = cubeyPlayer.GetComponent<LeanForceRigidbodyCustom>();

        RestartTimer();
    }

    private void UpdateLevelText(int n)
    {
        levelText.text = "Level " + (n+1);
    }

    private void CheckPickupCount(int count)
    {
        PickupText();
        
        if (PickupCountProperty <= 0)
        {
            DisablePickupGraphics();
            OpenExit();
            itemText.text = "Go to Exit";
        }
    }


    private void DisablePickupGraphics()
    {
        foreach (var image in pickupUiImages)
        {
            image.gameObject.SetActive(false);
        }
    }
    
    private void PickupGraphic(int n)
    {
        DisablePickupGraphics();
        
        pickupUiImages[n].gameObject.SetActive(true);
    }
    
    private void PickupText()
    {
        if (itemText != null)
            itemText.text = PickupCountProperty + " X";
    }

    private void CountSweetsForLevel()
    {
        GameObject pickupGroup = mapManager.LevelGameObject.transform.Find("Pickups").gameObject;

        if (pickupGroup == null)
        {
            // Debug.Log("<color:red>Setup pickups for level!</color>");
            return;
        }

        // PickupCountProperty = 0;
        var pickupCount = 0;
        for (int i = 0; i < pickupGroup.transform.childCount; i++)
        {
            pickupCount += 1;
            // PickupCountProperty++;
        }

        PickupCountProperty = pickupCount;

        PickupText();
    }

    private void OpenExit()
    {
        // count sweets in level
        if (SaveLoadManager.LastChapterPlayed == 0)
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
            // Debug.Log("Open Exit");
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
            // Debug.LogError("Can't find the exit in: " + mapManager.LevelGameObject.name);
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

        ReParentExitSwirl(true);
        
        exitObject.SetActive(false);
    }

    private void ReParentExitSwirl(bool move)
    {
        if (move)
        {
            visualEffects.peExitSwirl.transform.SetParent(exitObject.transform.parent);
            visualEffects.peExitSwirl.GetComponent<Animator>().enabled = true;
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

    public void PlayerAllowedJump(bool state)
    {
        allowPlayerMovement = state;
        leanForceRb.canJump = state;
        LaunchArc = state;
        // Debug.Log("allowed jump: " + state);
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

    private SaveLoadManager.Awards StarsGiven()
    {
        var jumps = jumpsToStartWith - jumpLeft;

        // print("jumps: " + jumps + ", jumpsToStartWith: " + jumpsToStartWith);

        if (jumps <= threeStars)
            return SaveLoadManager.Awards.ThreeStars;
        else if (jumps <= twoStars)
            return SaveLoadManager.Awards.TwoStars;
        else if (jumps <= oneStar)
            return SaveLoadManager.Awards.OneStar;

        return SaveLoadManager.Awards.NoAward;
    }
    
    private void SetAwardForLevel(SaveLoadManager.Awards award)
    {
        int chapter = SaveLoadManager.LastChapterPlayed;
        int level = SaveLoadManager.LastLevelPlayed;
        
        SetStarAward(level, award);
    }
    
    private void SetStarAward(int level, SaveLoadManager.Awards award)
    {
        var levelAward = SaveLoadManager.GetLevelAward(level);
        
        if (levelAward < (int)award)
            SaveLoadManager.SetAward(level, award);
    }
    
    [SerializeField] private List<Image> starImages;
    
    private void SetupStarFinishImages()
    {
        if (starImages != null) 
            return;
        
        var sGrp = endScreen.transform.Find("StarsGrp");

        for (int i = 0; i < 3; i++)
        {
            starImages.Add(sGrp.transform.GetChild(i).GetComponent<Image>());
            starImages[i].color = ColourManager.starDefault;
        }
    }
    
    // Shows stars on finished screen
    private void ShowStarsAchieved()
    {
        award = StarsGiven();
        
        endScreen.transform.Find("Buttons/Continue_button").gameObject.SetActive(award > 0);

        SetAwardForLevel(award);

        if (award > 0 && SaveLoadManager.SaveStaticList[chapterNo].levels[levelNo].levelUnlocked)
        {
            SaveLoadManager.UnlockLevel(SaveLoadManager.LastLevelPlayed + 1);
        }

        starImages[0].color = ColourManager.starDefault;
        starImages[1].color = ColourManager.starDefault;
        starImages[2].color = ColourManager.starDefault;
        
        starBronze_anim.StopPlayback();
        starSilver_anim.StopPlayback();
        starGold_anim.StopPlayback();
        
        switch (award)
        {
            case SaveLoadManager.Awards.NoAward:
                UpdateEndScreenInfoText(FinishedInfo.Failed);
                break;
            case SaveLoadManager.Awards.OneStar:
                // Debug.Log("Running bronze stars");
                starImages[0].color = ColourManager.starBronze;
                starImages[1].color = ColourManager.starDefault;
                starImages[2].color = ColourManager.starDefault;

                starBronze_anim.Play("StarBronze", 0, 0.4f);
                starBronze_anim.speed = 1;

                starSilver_anim.Play("StarSilver", 0, 1);
                starSilver_anim.speed = 0;

                starGold_anim.Play("StarGold", 0, 1);
                starGold_anim.speed = 0;

                UpdateEndScreenInfoText(FinishedInfo.Nearly);
                break;
            case SaveLoadManager.Awards.TwoStars:
                // Debug.Log("Running silver stars");
                starImages[0].color = ColourManager.starBronze;
                starImages[1].color = ColourManager.starSilver;
                starImages[2].color = ColourManager.starDefault;

                starBronze_anim.Play("StarBronze", 0, 0);
                starBronze_anim.speed = 1;

                starSilver_anim.Play("StarSilver", 0, -0.5f);
                starSilver_anim.speed = 1;

                starGold_anim.StopPlayback();
                starGold_anim.Play("StarGold", 0, 1);
                starGold_anim.speed = 1;
                
                UpdateEndScreenInfoText(FinishedInfo.Nearly);
                break;
            case SaveLoadManager.Awards.ThreeStars:
                // Debug.Log("Running gold stars");
                starImages[0].color = ColourManager.starBronze;
                starImages[1].color = ColourManager.starSilver;
                starImages[2].color = ColourManager.starGold;

                starBronze_anim.Play("StarBronze", 0, 0);
                starBronze_anim.speed = 1;
                
                starSilver_anim.Play("StarSilver", 0, 0f);
                starSilver_anim.speed = 1;
                
                starGold_anim.Play("StarGold", 0, 0); // 0.4
                starGold_anim.speed = 1;
                
                UpdateEndScreenInfoText(FinishedInfo.Completed);
                break;
        }
    }

    private enum FinishedInfo
    {
        Failed,
        Nearly,
        Completed
    }
    
    [SerializeField] private List<string> finishedInfoText;
    [SerializeField] private List<string> nearlyFinishedInfoText;
    [SerializeField] private List<string> failedFinishedInfoText;

    private void UpdateEndScreenInfoText(FinishedInfo info)
    {
        switch (info)
        {
            case FinishedInfo.Failed:
                endScreenInfo.text = failedFinishedInfoText[UnityEngine.Random.Range(0, failedFinishedInfoText.Count)];
                break;
            case FinishedInfo.Nearly:
                endScreenInfo.text = nearlyFinishedInfoText[UnityEngine.Random.Range(0, nearlyFinishedInfoText.Count)];
                break;
            case FinishedInfo.Completed:
                endScreenInfo.text = finishedInfoText[UnityEngine.Random.Range(0, finishedInfoText.Count)];
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

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && enabled)
        {
            PauseMenu(true);
        }
    }
    
    public void PauseMenu(bool state)
    {
        LaunchArc = !state;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(state);
        }
        
        Time.timeScale = state ? 0f : 1f;
        
        if (audioManager.allowMusic)
        {
            audioManager.MuteAudio(audioManager.gameMusic, state);
        }
    }
    
    private float timeStarted;
    private float durationInLevel;
    
    private void TimeTaken(bool start)
    {
        if (start)
        {
            timeStarted = Time.time;
        }
        else
        {
            float duration = Mathf.Abs(timeStarted - Time.time);
            durationInLevel = Mathf.Round(duration * 100) /100;
            SaveLoadManager.LevelTimeTaken(chapterNo, levelNo, durationInLevel);
        }
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

        JumpCount = jumpLeft;
        
        StartCoroutine(UpdateAwardsNeeded());
        CheckJumpCount();
    }

    public void PlayerFaceDirection(bool right)
    {
        flip.x = -1;

        if (right)
            flip.x = 1;

        flip.y = 1;
        flip.z = 1;

        playerRb.gameObject.transform.localScale = flip;
    }

    public void HideGameObject(GameObject go)
    {
        StartCoroutine(HideObject(go));
    }

    private IEnumerator HideObject(GameObject go)
    {
        yield return new WaitForSeconds(3);
    }
    
}
