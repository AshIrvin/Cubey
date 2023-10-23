using System;
using System.Collections.Generic;
using System.Collections;
//using System.Diagnostics;
//using Game.Scripts;
//using IngameDebugConsole;
using UnityEngine;
using Lean.Touch;
//using UnityEditor;
using UnityEngine.SceneManagement;
//using UnityEngine.Serialization;
using UnityEngine.UI;
//using Debug = UnityEngine.Debug;
//using Random = System.Random;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    // TODO - class is too big. Needs split up. Game levels, UI, player?
    // Messy - once split up, order the properties, variables etc
    // Remove as many serialisedFields as possible

    private enum FinishedInfo
    {
        Failed,
        Nearly,
        Completed
    }

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
    [SerializeField] private AwardManager awardManager;

    [Header("Player")]
    [SerializeField] private GameObject cubeyPlayer;
    // [SerializeField] private GameObject cubeyPlayerPrefab;
    [SerializeField] private LeanForceRigidbodyCustom leanForceRb;
    
    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioSource gameMusic;

    public static Transform gameFolder;
    //private float timer;
    //private string levelName;

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
    
    [Header("UI Pickups")]
    [SerializeField] private List<Image> pickupUiImages;

    private bool camMovement;
    //private bool gameLevelEnabled;
    private float playerGooDrag = 40f;
    //private ParticleSystem pe;
    private float cubeyJumpMagValue = 0.5f;



    #region Getters,Setters

    public bool GameLevel
    {
        get
        {
            //gameLevelEnabled = gameLevel.CurrentValue;
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

    #endregion

    public LevelMetaData LevelMetaData => levelMetaData;
    public SaveMetaData SaveMetaData => saveMetaData;

    public bool playSingleLevel = false;

    [SerializeField] public bool allowPlayerMovement;
    [SerializeField] private bool jumpCounting;
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
    //private Vector3 cubeyPosition;
    public float cubeyMagnitude;
    private Vector3 flip;
    
    [SerializeField] private List<Image> starImages;
    [SerializeField] private List<string> finishedInfoText;
    [SerializeField] private List<string> nearlyFinishedInfoText;
    [SerializeField] private List<string> failedFinishedInfoText;

    private float timeStarted;
    private float durationInLevel;
    private int delayFailedScreenInSeconds = 4;

    public float cubeyJumpHeight = 2.6f;
    public bool useTimer;
    public Rigidbody playerRb;
    public static Action LevelLoaded;

    private void Awake()
    {
        gameLevel.OnValueChanged += LoadGameLevel;
        pickupCountProperty.OnValueChanged += CheckPickupCount;
        exitProperty.OnValueChanged += LoadEndScreen;
        stickyObject.OnValueChanged += ToggleSticky;
        leanForceRb.onGround += PlayerAllowedJump;
        // FingerPos.allowedJump += PlayerAllowedJump;
        
        if (leanForceRb == null)
            leanForceRb = FindObjectOfType<LeanForceRigidbodyCustom>();

        if (gameFolder == null)
            gameFolder = GameObject.Find("Game").transform;

        CheckScripts();
    }

    private void Start()
    {
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

    private void CheckScripts()
    {
        if (awardManager == null || visualEffects == null)
            Logger.Instance.ShowDebugError("Missing script");
    }

    private void GetLevelInfo()
    {
        levelNo = SaveLoadManager.LastLevelPlayed;
        chapterNo = SaveLoadManager.LastChapterPlayed;
        levelMetaData = chapterList[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed];
        
        //timer = levelMetaData.Timer;
        oneStar = levelMetaData.JumpsForBronze;
        twoStars = levelMetaData.JumpsForSilver;
        threeStars = levelMetaData.JumpsForGold;
        //levelName = levelMetaData.LevelName;
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
        PlayerAllowedJump(false);
        SetGameCanvases(false);
    }

    private void LoadGameLevel(bool state)
    {
        enabled = state;
        ResetCubeyPlayer(!state);
        LaunchArc = state;

        mainMenuManager.mainMenu.SetActive(!state);
        mainMenuManager.enabled = !state;

        if (exitObject != null)
            exitObject.SetActive(!state);

        SetGameCanvases(state);
        visualEffects.ParticleEffectsGo.SetActive(state);
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
        if (topUi == null) return;
        
        topUi.SetActive(state);
        PickupGraphic(SaveLoadManager.LastChapterPlayed);
    }

    private void DisableStartPosition()
    {
        if (mapManager.LevelGameObject != null &&
            mapManager.LevelGameObject.transform.GetChild(1).name.Contains("Start"))
        { 
            mapManager.LevelGameObject.transform.GetChild(1)?.GetChild(1)?.gameObject.SetActive(false); 
        }
        else if (mapManager.LevelGameObject.transform.GetChild(0).name.Contains("Start"))
        {
            mapManager.LevelGameObject.transform.GetChild(0)?.GetChild(1)?.gameObject.SetActive(false);
        }
        else
        {
            Logger.Instance.ShowDebugError("Can't find Start position");
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
        
        LevelLoaded?.Invoke();
    }

    // Todo - can this be an action? 

    // needs fixed - can jump at top of jump

    public void Update()
    {
        // SetPlayerJump();
    }

/*    private void SetPlayerJump()
    {
        if (playerRb.velocity.sqrMagnitude < cubeyJumpMagValue)
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
    }*/

    private void ChangeTextColour(Text text, Color color)
    {
        text.color = color;
    }

    // TODO - does this need to be coroutine?
    private IEnumerator UpdateAwardsNeeded()
    {
        yield return new WaitUntil(() => levelMetaData != null);

        if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForGold) // 10 - 8 < 3
        {
            ChangeTextColour(jumpAmountText, ColourManager.starGold);
        }
        else if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForSilver)
        {
            ChangeTextColour(jumpAmountText, ColourManager.starSilver);
        }
        else if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForBronze) // 9 - 9 <= 9
        {
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

    private bool IsJumpOverMagnitude()
    {
        if (playerRb.velocity.magnitude > cubeyJumpMagValue)
            return true;

        return false;
    }

    private void CheckJumpCount() 
    {
        if (jumpCounting && jumpLeft == 0 && /*leanForceRb.canJump &&*/ !onBreakablePlatform && !IsJumpOverMagnitude())
        {
            FailedScreen(true);
        }
        else if (jumpCounting && jumpLeft == 0 && /*leanForceRb.canJump &&*/ onBreakablePlatform)
        {
            //StartCoroutine(DelayFailedScreen());
            DelayAsyncFailedScreen();
        }
    }

/*    private IEnumerator DelayFailedScreen()
    {
        yield return new WaitForSeconds(4);
        FailedScreen(true);
    }*/

    private async void DelayAsyncFailedScreen()
    {
        await Task.Delay(delayFailedScreenInSeconds * 1000);
        FailedScreen(true);
    }


    // TODO - what's this for? Special level?
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
        playerRb.angularDrag = 0;
        stickyObject.CurrentValue = false;
        playerRb.isKinematic = false;
        HideScreens();
        enabled = false;
        // mapManager.enabled = true; // not needed
        mapManager.RestartLevel();
        TimeTaken(true);
    }

    // Used as failed screen button

    public void LoadMainMenu()
    {
        //StartCoroutine(LoadingScene(true));
        LoadingScene(true);

        var childCount = mapManager.LevelParent.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(mapManager.LevelParent.transform.GetChild(i).gameObject);
        }

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
            return;
        }

        TimeTaken(false);
        audioManager.PlayAudio(audioManager.cubeyCelebration);
        ReParentExitSwirl(false);
        EndScreen(true);
            
        ShowStarsAchieved();
        SaveLoadManager.SaveGameInfo();
    }

    private void ResetCubeyPlayer(bool state)
    {
        if (state)
        {
            playerRb.useGravity = false;
            cubeyPlayer.SetActive(false);
            return;
        }
        
        cubeyPlayer.SetActive(true);
        cubeyPlayer.transform.SetParent(gameFolder, true);
        SetPlayerParentAndPos();
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

        if (PickupCountProperty > 0) return;

        DisablePickupGraphics();
        OpenExit();
        itemText.text = "Go to Exit";
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
            // Logger.Instance.ShowDebugLog("<color:red>Setup pickups for level!</color>");
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
        if (SaveLoadManager.LastChapterPlayed == 0)
        {
            audioManager.PlayAudio(audioManager.cubeyExitOpen);
            SetupExitXmas(false, true);
            return;
        }

        audioManager.PlayAudio(audioManager.cubeyExitOpen);

        if (exitObject == null)
            exitObject = FindExit();
            
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peExitExplosion, exitObject.transform.position);
        VisualEffects.Instance.peExitSwirl.SetActive(false);

        exitObject.SetActive(true);
        exitObject.transform.GetChild(0).gameObject.SetActive(true);
    }

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
            // Logger.Instance.ShowDebugError("Can't find the exit in: " + mapManager.LevelGameObject.name);
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
        LaunchArc = state;
        leanForceRb.canJump = state;
    }

/*    IEnumerator DelayBeforeJump(bool state)
    {
        if(state)
        {
            yield return new WaitForSeconds(0.3f);
        }
        allowPlayerMovement = state;
        LaunchArc = state;
        leanForceRb.canJump = state;
    }*/

/*    public void PlayerVelocity(float n)
    {
        leanForceRb.velocityMultiplier = n;
    }*/

    private void SetPlayerParentAndPos()
    {
        cubeyPlayer.transform.SetParent(gameFolder.transform);

        var pos = levelMetaData.LevelPrefab.transform.GetChild(1).transform.position;
        DisableStartPosition();
        cubeyPlayer.gameObject.transform.SetPositionAndRotation(pos, new Quaternion(0, 0, 0, 0));
    }

    private SaveLoadManager.Awards StarsGiven()
    {
        var jumps = jumpsToStartWith - jumpLeft;

        if (jumps <= threeStars)
            return SaveLoadManager.Awards.ThreeStars;
        else if (jumps <= twoStars)
            return SaveLoadManager.Awards.TwoStars;
        else if (jumps <= oneStar)
            return SaveLoadManager.Awards.OneStar;

        return SaveLoadManager.Awards.NoAward;
    }

    private void SetupStarFinishImages()
    {
        if (starImages != null) 
            return;

        // TODO - Change this
        var sGrp = endScreen.transform.Find("StarsGrp");

        for (int i = 0; i < 3; i++)
        {
            starImages.Add(sGrp.transform.GetChild(i).GetComponent<Image>());
            starImages[i].color = ColourManager.starDefault;
        }
    }

    private void ShowStarsAchieved()
    {
        award = StarsGiven();
        
        // TODO - Change this
        endScreen.transform.Find("Buttons/Continue_button").gameObject.SetActive(award > 0);

        //SetAwardForLevel(award);
        AwardManager.instance.SetAwardForLevel(award);

        if (award > 0)
        {
            if (SaveLoadManager.SaveStaticList[chapterNo].levels[levelNo].levelUnlocked)
            {
                SaveLoadManager.UnlockLevel(SaveLoadManager.LastLevelPlayed + 1);
            }
            else
            {
                SaveLoadManager.SaveGameInfo();
            }
        }

        starImages[0].color = ColourManager.starDefault;
        starImages[1].color = ColourManager.starDefault;
        starImages[2].color = ColourManager.starDefault;
        
        starBronze_anim.StopPlayback();
        starSilver_anim.StopPlayback();
        starGold_anim.StopPlayback();

        SetStars();
    }

    private void SetStars()
    {
        switch (award)
        {
            case SaveLoadManager.Awards.NoAward:
                UpdateEndScreenInfoText(FinishedInfo.Failed);
                break;
            case SaveLoadManager.Awards.OneStar:
                // Logger.Instance.ShowDebugLog("Running bronze stars");
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
                // Logger.Instance.ShowDebugLog("Running silver stars");
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
                // Logger.Instance.ShowDebugLog("Running gold stars");
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

    public async void LoadingScene(bool on)
    {
        if (loadingScreen == null) return;

        loadingScreen.SetActive(on);
        //yield return new WaitForSeconds(0.2f);
        await Task.Delay(200);
        loadingScreen.SetActive(false);
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

    private void TimeTaken(bool state)
    {
        if (state)
        {
            timeStarted = Time.time;
            return;
        }

        float duration = Mathf.Abs(timeStarted - Time.time);
        durationInLevel = Mathf.Round(duration * 100) /100;
        SaveLoadManager.LevelTimeTaken(chapterNo, levelNo, durationInLevel);
    }

    private void EndScreen(bool state)
    {
        if (endScreen == null)
            Logger.Instance.ShowDebugError("Missing EndScreen object on GameManager script");
        
        endScreen.SetActive(state);

        Time.timeScale = state ? 0.1f : 1f;
    }

    public void FailedScreen(bool on)
    {
        if (failedScreen == null)
            Logger.Instance.ShowDebugError("Missing failedScreen object on GameManager script");

        failedScreen.SetActive(on);

        Time.timeScale = on ? 0f : 1f;
    }

    public void PlayerJumped()
    {
        audioManager.PlayAudio(audioManager.cubeyJump);

        if (!jumpCounting)
        {
            if (jumpLeft != 0)
            {
                jumpLeft--;
            }
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

    // TODO - no longer needed?
/*    public void HideGameObject(GameObject go)
    {
        StartCoroutine(HideObject(go));
    }*/

    private IEnumerator HideObject(GameObject go)
    {
        yield return new WaitForSeconds(3);
    }
    
}
