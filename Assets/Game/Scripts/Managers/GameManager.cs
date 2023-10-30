using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Lean.Touch;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    // TODO - class is too big. Needs split up. Game levels, UI, player?
    // Messy - once split up, order the properties, variables etc
    // Remove as many serialisedFields as possible
    // This class shouldn't be disabled

    public static GameManager Instance;

    public enum FinishedInfo
    {
        Failed,
        Nearly,
        Completed
    }

    public enum GameState
    {
        Menu,
        Map,
        Level
    }

    #region Fields
    
    
    [Header("Scriptable Objects")]
    //[SerializeField] private BoolGlobalVariable gameLevel;
    [SerializeField] private IntGlobalVariable pickupCountProperty;
    [SerializeField] private BoolGlobalVariable exitProperty;
    [SerializeField] private BoolGlobalVariable launchArc;
    [SerializeField] private BoolGlobalVariable stickyObject;
    
    [Header("Scripts")]
    [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private VisualEffects visualEffects;

    [Header("Player")]
    [SerializeField] private GameObject cubeyPlayer;
    [SerializeField] private LeanForceRigidbodyCustom leanForceRb;
    
    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioSource gameMusic;

    #endregion Fields

    public static Transform gameFolder;

    [Header("Game Screens")]

    [SerializeField] private GameObject deathWalls;

    [Header("Level Exit")]
    private GameObject exitPrezzie;
    [SerializeField] private GameObject exitObject;
    
    [Header("UI Pickups")]
    [SerializeField] private List<Image> pickupUiImages;

    private ChapterList chapterList;
    private LevelMetaData levelMetaData;
    private MapManager mapManager;
    private UiManager uiManager;
    private LevelManager levelManager;
    private AwardManager awardManager;
    private GameState gameState;

    private bool camMovement;
    private float playerGooDrag = 40f;
    private float cubeyJumpMagValue = 0.5f;
    private int levelNo;
    private int chapterNo;

    #region Getters,Setters

    //public bool GameLevel
    //{
    //    get
    //    {
    //        return gameLevel.CurrentValue;
    //    }
    //    set => gameLevel.CurrentValue = value;
    //}

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
            uiManager.JumpAmountText.text = jumpLeft.ToString();
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

    public bool playSingleLevel = false;

    [SerializeField] public bool allowPlayerMovement;
    [SerializeField] private bool jumpCounting;
    [SerializeField] private bool onBreakablePlatform;
    [SerializeField] private bool onMovingPlatform;

    private readonly int jumpsToStartWith = 10;
    private readonly int countdown = 60;
    private SaveLoadManager.Awards award;
    private int jumpLeft;

    [Header("Animation")]
    [SerializeField] private Animator starGold_anim;
    [SerializeField] private Animator starSilver_anim;
    [SerializeField] private Animator starBronze_anim;

    [Header("Other")]
    public float cubeyMagnitude;
    private Vector3 flip;
    
    [SerializeField] private List<Image> starImages;

    private float timeStarted;
    private float durationInLevel;
    private int delayFailedScreenInSeconds = 4;



    public float cubeyJumpHeight = 2.6f;
    public bool useTimer;
    public Rigidbody playerRb;

    public static Action LevelLoaded;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        chapterList = GlobalMetaData.Instance.ChapterList;
        mapManager = MapManager.Instance;
        uiManager = UiManager.Instance;

        GlobalMetaData.Instance.GameLevel.OnValueChanged += LoadGameLevel;
        pickupCountProperty.OnValueChanged += CheckPickupCount;
        exitProperty.OnValueChanged += LoadEndScreen;
        stickyObject.OnValueChanged += ToggleSticky;
        leanForceRb.onGround += PlayerAllowedJump;
        // FingerPos.allowedJump += PlayerAllowedJump;

        levelManager = LevelManager.Instance;
        awardManager = AwardManager.Instance;

        if (leanForceRb == null)
            leanForceRb = FindFirstObjectByType<LeanForceRigidbodyCustom>();

        if (gameFolder == null)
            gameFolder = GameObject.Find("Game").transform;

        CheckScripts();
    }

    private void Start()
    {
        GlobalMetaData.Instance.HasGameLevelLoaded(false);
        visualEffects.peExitSwirl.SetActive(false);

        jumpLeft = jumpsToStartWith;
        uiManager.JumpAmountText.text = jumpLeft.ToString();

        SetGameCanvases(false);
        SetupStarFinishImages();
    }

    private void OnDestroy()
    {
        GlobalMetaData.Instance.GameLevel.OnValueChanged -= LoadGameLevel;
        pickupCountProperty.OnValueChanged -= CheckPickupCount;
        exitProperty.OnValueChanged -= LoadEndScreen;
        stickyObject.OnValueChanged -= ToggleSticky;
        leanForceRb.onGround -= PlayerAllowedJump;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public void SetGameState(GameState state)
    {
        gameState = state;
    }

    private void CheckScripts()
    {
        if (visualEffects == null)
            Logger.Instance.ShowDebugError("Missing visual effects script");
    }

    private void GetLevelInfo()
    {
        levelNo = SaveLoadManager.LastLevelPlayed;
        chapterNo = SaveLoadManager.LastChapterPlayed;
        levelMetaData = chapterList[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed];
        
        //timer = levelMetaData.Timer;
        //levelName = levelMetaData.LevelName;
    }

    //private void OnDisable()
    //{
    //    if (cubeyPlayer != null)
    //        cubeyPlayer.SetActive(false);

    //    PlayerAllowedJump(false);
    //    SetGameCanvases(false);
    //}

    private void SetGameCanvases(bool state)
    {
        TopUi(state);
        PauseMenu(false);
        EndScreen(false);
        FailedScreen(false);
    }

    private void TopUi(bool state)
    {
        if (uiManager.TopUi == null)
        {
            Logger.Instance.ShowDebugError("Missing UiManager!");
            return;
        }

        uiManager.TopUi.SetActive(state);
        PickupGraphic(SaveLoadManager.LastChapterPlayed);
    }

    private void DisableStartPosition()
    {
        if (levelManager.LevelGameObject != null &&
            levelManager.LevelGameObject.transform.GetChild(1).name.Contains("Start"))
        { 
            levelManager.LevelGameObject.transform.GetChild(1).GetChild(1).gameObject.SetActive(false); 
        }
        else if (levelManager.LevelGameObject.transform.GetChild(0).name.Contains("Start"))
        {
            levelManager.LevelGameObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            Logger.Instance.ShowDebugError("Can't find Start position");
        }
    }

    // this was OnEnable
    private void SubscribeToThisOnLevelLoad()
    {
        CamMovement = true;
        exitProperty.CurrentValue = false;
        GetLevelInfo();
        awardManager.GetLevelAwards();
        StartLevel();
    }

    // this comes from levelManager
    private void LoadGameLevel(bool state)
    {
        SubscribeToThisOnLevelLoad();
        //enabled = state;
        ResetCubeyPlayer(!state);
        LaunchArc = state;

        mainMenuManager.mainMenu.SetActive(!state);
        mainMenuManager.enabled = !state;

        if (exitObject != null)
            exitObject.SetActive(!state);

        SetGameCanvases(state);
        visualEffects.ParticleEffectsGo.SetActive(state);

        Logger.Instance.ShowDebugLog($"GameManager - loaded game level");
    }

    private void StartLevel()
    {
        if (cubeyPlayer == null)
            Logger.Instance.ShowDebugError("Can't find Cubey!");
        else
            flip = cubeyPlayer.transform.localScale;

        if (deathWalls != null)
            deathWalls.SetActive(false);
        
        RestartTimer();

        if (uiManager.PauseMenu != null)
            uiManager.PauseMenu.SetActive(false);

        
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

        SetGameState(GameState.Level);
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
            ChangeTextColour(uiManager.JumpAmountText, ColourManager.starGold);
        }
        else if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForSilver)
        {
            ChangeTextColour(uiManager.JumpAmountText, ColourManager.starSilver);
        }
        else if (jumpsToStartWith - jumpLeft <= levelMetaData.JumpsForBronze) // 9 - 9 <= 9
        {
            ChangeTextColour(uiManager.JumpAmountText, ColourManager.starBronze);
        }
        else
        {
            uiManager.AwardToGet.text = "Need bronze for next level";
            ChangeTextColour(uiManager.JumpAmountText, ColourManager.starDefault);
        }

        uiManager.OneStarTutorialText.text = levelMetaData.JumpsForBronze.ToString();
        uiManager.TwoStarTutorialText.text = levelMetaData.JumpsForSilver.ToString();
        uiManager.ThreeStarTutorialText.text = levelMetaData.JumpsForGold.ToString();

        uiManager.BronzePodium.text = levelMetaData.JumpsForBronze.ToString();
        uiManager.SilverPodium.text = levelMetaData.JumpsForSilver.ToString();
        uiManager.GoldPodium.text = levelMetaData.JumpsForGold.ToString();
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
        uiManager.HelpScreen.SetActive(on);
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
        if (!jumpCounting && jumpLeft > 0) return;

        if (!onBreakablePlatform && !IsJumpOverMagnitude())
        {
            FailedScreen(true);
        }
        else if (onBreakablePlatform)
        {
            DelayAsyncFailedScreen();
        }
    }

    private async void DelayAsyncFailedScreen()
    {
        await Task.Delay(delayFailedScreenInSeconds * 1000);
        FailedScreen(true);
    }

    // TODO - what's this for? Special level?
    private void RestartTimer()
    {
        //time = countdown;
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
        //enabled = false;        
        levelManager.RestartLevel();
        TimeTaken(true);
    }

    // Used as failed screen button
    public void LoadMainMenu()
    {
        LoadingScene(true);

        var childCount = levelManager.LevelParent.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            // TODO - this can be removed once all levels are instantiated on load
            Destroy(levelManager.LevelParent.transform.GetChild(i).gameObject);
        }

        HideScreens();
        SceneManager.LoadScene("CubeyGame");
    }

    // loads from exiting or timer ending
    private void LoadEndScreen(bool won)
    {
        if (!won)
        {
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
        //if (state)
        //{
        //    playerRb.useGravity = false;
        //    cubeyPlayer.SetActive(false);
        //    return;
        //}
        
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
        uiManager.LevelText.text = "Level " + (n+1);
    }

    private void CheckPickupCount(int count)
    {
        PickupText();

        if (PickupCountProperty > 0) return;

        DisablePickupGraphics();
        OpenExit();
        uiManager.ItemText.text = "Go to Exit";
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
        if (uiManager.ItemText != null)
            uiManager.ItemText.text = PickupCountProperty + " X";
    }

    private void CountSweetsForLevel()
    {
        GameObject pickupGroup = levelManager.LevelGameObject.transform.Find("Pickups").gameObject;

        if (pickupGroup == null)
        {
            // Logger.Instance.ShowDebugLog("<color:red>Setup pickups for level!</color>");
            return;
        }

        var pickupCount = 0;
        for (int i = 0; i < pickupGroup.transform.childCount; i++)
        {
            pickupCount += 1;
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
            
        visualEffects.PlayEffect(visualEffects.peExitExplosion, exitObject.transform.position);
        visualEffects.peExitSwirl.SetActive(false);

        exitObject.SetActive(true);
        exitObject.transform.GetChild(0).gameObject.SetActive(true);
    }

    // TODO - remove all finds. Assign on load or in scriptable object
    private GameObject FindExit()
    {
        if (levelManager.LevelGameObject.transform.GetChild(0).name.Contains("MovingExitPlatform"))
        {
            return levelManager.LevelGameObject.transform.GetChild(0).transform.Find("Exit").gameObject;
        }
        else if (levelManager.LevelGameObject.transform.GetChild(0).name.Contains("Exit")) // default position
        {
            return levelManager.LevelGameObject.transform.GetChild(0).gameObject;
        }
        else if (levelManager.LevelGameObject.transform.Find("Exit")) 
        {
            return levelManager.LevelGameObject.transform.Find("Exit").gameObject;
        }
        else if (levelManager.LevelGameObject.transform.Find("Spindle"))
        {
            return levelManager.LevelGameObject.transform.Find("Spindle").GetChild(0).gameObject;
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

        if (jumps <= AwardManager.Instance.ThreeStars)
            return SaveLoadManager.Awards.ThreeStars;
        else if (jumps <= AwardManager.Instance.TwoStars)
            return SaveLoadManager.Awards.TwoStars;
        else if (jumps <= AwardManager.Instance.OneStar)
            return SaveLoadManager.Awards.OneStar;

        return SaveLoadManager.Awards.NoAward;
    }

    private void SetupStarFinishImages()
    {
        if (starImages != null) 
            return;

        // TODO - Remove finds. Add to load or scriptable object
        var sGrp = uiManager.EndScreen.transform.Find("StarsGrp");

        for (int i = 0; i < 3; i++)
        {
            starImages.Add(sGrp.transform.GetChild(i).GetComponent<Image>());
            starImages[i].color = ColourManager.starDefault;
        }
    }

    private void ShowStarsAchieved()
    {
        award = StarsGiven();

        // TODO - Remove finds. Add to load or scriptable object
        uiManager.EndScreen.transform.Find("Buttons/Continue_button").gameObject.SetActive(award > 0);

        AwardManager.Instance.SetAwardForLevel(award);

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
                uiManager.ModifyEndScreenInfoText(FinishedInfo.Failed);
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

                uiManager.ModifyEndScreenInfoText(FinishedInfo.Nearly);
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

                uiManager.ModifyEndScreenInfoText(FinishedInfo.Nearly);
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

                uiManager.ModifyEndScreenInfoText(FinishedInfo.Completed);
                break;
        }
    }



    // TODO - is a loading image still needed?
    public async void LoadingScene(bool on)
    {
        if (uiManager.LoadingScreen == null) return;

        uiManager.LoadingScreen.SetActive(on);
        await Task.Delay(200);
        uiManager.LoadingScreen.SetActive(false);
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
        
        if (uiManager.PauseMenu != null)
        {
            uiManager.PauseMenu.SetActive(state);
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
        if (uiManager.EndScreen == null)
            Logger.Instance.ShowDebugError("Missing EndScreen object on GameManager script");

        uiManager.EndScreen.SetActive(state);

        Time.timeScale = state ? 0.1f : 1f;
    }

    public void FailedScreen(bool on)
    {
        if (uiManager.FailedScreen == null)
            Logger.Instance.ShowDebugError("Missing failedScreen object on GameManager script");

        uiManager.FailedScreen.SetActive(on);

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
}
