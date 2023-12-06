using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Lean.Touch;
using UnityEngine.UI;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
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
    [SerializeField] private IntGlobalVariable pickupCountProperty;
    [SerializeField] private BoolGlobalVariable launchArc;
    [SerializeField] private BoolGlobalVariable stickyObject;
    
    [Header("Scripts")]
    [SerializeField] private MainMenuManager mainMenuManager;
    private VisualEffects visualEffects;

    [Header("Player")]
    [SerializeField] private GameObject cubeyPlayer;
    [SerializeField] private LeanForceRigidbodyCustom leanForceRb;
    
    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioSource gameMusic;

    #endregion Fields

    public static Transform gameFolder;

    [SerializeField] private GameObject deathWalls;

    [Header("Level Exit")]
    [SerializeField] private GameObject exitObject;

    private GameObject exitPrezzie;
    private ChapterList chapterList;
    private LevelMetaData levelMetaData;
    //private MapManager mapManager;
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

    public GameState GameStateType => gameState;

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


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        chapterList = GlobalMetaData.Instance.ChapterList;
        visualEffects = VisualEffects.Instance;
        uiManager = UiManager.Instance;

        pickupCountProperty.OnValueChanged += CheckPickupCount;
        stickyObject.OnValueChanged += ToggleSticky;
        leanForceRb.onGround += PlayerAllowedJump;
        // FingerPos.allowedJump += PlayerAllowedJump;
        LevelManager.OnLevelLoad += OnLevelLoad;

        levelManager = LevelManager.Instance;
        awardManager = AwardManager.Instance;

        if (leanForceRb == null)
            leanForceRb = FindFirstObjectByType<LeanForceRigidbodyCustom>();

        if (gameFolder == null)
            gameFolder = GameObject.Find("Game").transform;
    }

    private void Start()
    {
        visualEffects.peExitSwirl.SetActive(false);

        jumpLeft = jumpsToStartWith;
        uiManager.JumpAmountText.text = jumpLeft.ToString();

        uiManager.SetGameLevelCanvases(false);
        SetupStarFinishImages();
    }

    //private void OnDestroy()
    //{
    //    pickupCountProperty.OnValueChanged -= CheckPickupCount;
    //    exitProperty.OnValueChanged -= LoadEndScreen;
    //    stickyObject.OnValueChanged -= ToggleSticky;
    //    leanForceRb.onGround -= PlayerAllowedJump;
    //}

    private void GetLevelInfo()
    {
        levelNo = SaveLoadManager.LastLevelPlayed;
        chapterNo = SaveLoadManager.LastChapterPlayed;
        levelMetaData = chapterList[SaveLoadManager.LastChapterPlayed].LevelList[SaveLoadManager.LastLevelPlayed];
        
        //timer = levelMetaData.Timer;
        //levelName = levelMetaData.LevelName;
    }

    public void QuitingLevel()
    {
        if (cubeyPlayer != null)
            cubeyPlayer.SetActive(false);

        PlayerAllowedJump(false);
        uiManager.SetGameLevelCanvases(false);
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

    private void OnLevelLoad()
    {
        SetObjectStates();
        GetLevelInfo();
        awardManager.GetLevelAwards();
        StartLevel();
    }

    public void SetGameState(GameState state)
    {
        gameState = state;
    }

    private void SetObjectStates()
    {
        LaunchArc = true;

        mainMenuManager.mainMenu.SetActive(false);

        if (exitObject != null)
            exitObject.SetActive(false);

        uiManager.SetGameLevelCanvases(true);
        visualEffects.ParticleEffectsGroup.SetActive(true);
        CamMovement = true;
        //exitProperty.CurrentValue = false;
    }

    private void StartLevel()
    {
        if (cubeyPlayer == null)
        {
            Logger.Instance.ShowDebugError("Can't find Cubey!");
            return;
        }
        
        flip = cubeyPlayer.transform.localScale;

        if (deathWalls != null)
            deathWalls.SetActive(false);
        
        RestartTimer();

        uiManager.ShowPauseMenu(false);

        EnableCubeyLevelObject(true);
        DisableStartPosition();
        UpdateLevelText(levelNo);

        JumpCount = 10;
        CountSweetsForLevel();
        ReParentExitSwirl(false);
        SetupExit();
        StartCoroutine(UpdateAwardsNeeded());

        uiManager.PickupText();
        
        TimeTaken(true);

        PlayerFaceDirection(exitObject.transform.position.x < 0);

        SetGameState(GameState.Level);
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
            uiManager.ShowFailedScreen(true);
        }
        else if (onBreakablePlatform)
        {
            DelayAsyncFailedScreen();
        }
    }

    private async void DelayAsyncFailedScreen()
    {
        await Task.Delay(delayFailedScreenInSeconds * 1000);
        uiManager.ShowFailedScreen(true);
    }

    // TODO - what's this for? Special level?
    private void RestartTimer()
    {
        //time = countdown;
    }

    public void ResetCubey()
    {
        playerRb.drag = 0;
        playerRb.angularDrag = 0;
        stickyObject.CurrentValue = false;
        playerRb.isKinematic = false;
        TimeTaken(true);
    }

    // TODO - move this to UiManager
    public void LoadEndScreen(bool won)
    {
        if (!won)
        {
            uiManager.ShowFailedScreen(true);
            return;
        }

        TimeTaken(false);
        audioManager.PlayAudio(audioManager.cubeyCelebration);
        ReParentExitSwirl(false);
        uiManager.ShowEndScreen(true);
            
        ShowStarsAchieved();
        SaveLoadManager.SaveGameInfo();
    }

    public void EnableCubeyLevelObject(bool state)
    {
        if (!state)
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
        uiManager.LevelText.text = "Level " + (n+1);
    }

    private void CheckPickupCount(int count)
    {
        uiManager.PickupText();

        if (PickupCountProperty > 0) return;

        uiManager.DisablePickupGraphics();
        OpenExit();
        uiManager.ItemText.text = "Go to Exit";
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

        uiManager.PickupText();
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

    private void ReParentExitSwirl(bool state)
    {
        if (state)
        {
            visualEffects.peExitSwirl.transform.SetParent(exitObject.transform.parent);
            visualEffects.peExitSwirl.GetComponent<Animator>().enabled = true;
            return;
        }

        visualEffects.peExitSwirl.transform.SetParent(visualEffects.ParticleEffectsGroup.transform);
        
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
        if (!hasFocus && gameState == GameState.Level)
        {
            print($"OnApplicationFocus. gameState: {gameState}");
            uiManager.ShowPauseMenu(true);
            return;
        }

        // TODO - create pause image with no buttons
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
