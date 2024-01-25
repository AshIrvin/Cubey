using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using UnityEngine.UI;
using System.Threading.Tasks;
using DG.Tweening;

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

    internal static Transform GameFolder;

    [Header("Scriptable Objects")]
    [SerializeField] private IntGlobalVariable pickupCountProperty;
    [SerializeField] private BoolGlobalVariable launchArc;
    [SerializeField] private BoolGlobalVariable stickyObject;
    
    [Header("Player")]
    [SerializeField] private GameObject cubeyPlayer;
    [SerializeField] private LeanForceRigidbodyCustom leanForceRb;
    
    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioSource gameMusic;

    #endregion Fields

    [Header("Platforms")]
    [SerializeField] private bool onBreakablePlatform;
    [SerializeField] private bool onMovingPlatform;

    [Header("Animation")]
    [SerializeField] private Animator starGold_anim;
    [SerializeField] private Animator starSilver_anim;
    [SerializeField] private Animator starBronze_anim;

    [Header("Other")]
    [SerializeField] private List<Image> starImages;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private GameObject deathWalls;
    [SerializeField] private GameObject exitRainbow;

    //private GameObject exitPrezzie;
    private ChapterList chapterList;
    
    private UiManager uiManager;
    private LevelManager levelManager;
    private AwardManager awardManager;
    private MainMenuManager mainMenuManager;
    private VisualEffects visualEffects;

    private GameState gameState;
    private bool jumpCounting;
    private float playerGooDrag = 40f;
    private float cubeyJumpMagValue = 0.5f;
    private int levelNo;
    private int chapterNo;

    private AwardManager.Awards award;
    private Vector3 flip;
    private GameObject prezzieClosed;
    private SpriteRenderer prezzieFlat;
    private BoxCollider prezzieCollision;

    #region Getters,Setters

    public bool CamMovement { get; set; }

    public int PickupCountProperty
    {
        get => pickupCountProperty.CurrentValue;
        set => pickupCountProperty.CurrentValue = value;
    }

    public int JumpCount
    {
        get => JumpsLeft;
        set
        {
            JumpsLeft = value;
            uiManager.JumpAmountText.text = JumpsLeft.ToString();
        }
    }

    public bool LaunchArc
    {
        get => launchArc.CurrentValue;
        set => launchArc.CurrentValue = value;
    }
    
    public GameObject GetLevelExit
    {
        get => exitRainbow;
    }

    public GameObject CubeyPlayer
    {
        get => cubeyPlayer;
        set => cubeyPlayer = value;
    }

    public GameState GameStateType => gameState;

    #endregion

    internal LevelMetaData LevelMetaData;
    private const int COUNTDOWN = 60;
    private const int DELAY_FAILED_SCREEN_IN_SECONDS = 4;
    internal const int JUMPS_TO_START = 10;
    internal int JumpsLeft;
    internal bool AllowPlayerMovement;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        chapterList = GlobalMetaData.Instance.ChapterList;
        visualEffects = VisualEffects.Instance;
        uiManager = UiManager.Instance;
        levelManager = LevelManager.Instance;
        awardManager = AwardManager.Instance;
        mainMenuManager = MainMenuManager.Instance;

        pickupCountProperty.OnValueChanged += CheckPickupCount;
        stickyObject.OnValueChanged += ToggleSticky;
        leanForceRb.onGround += PlayerAllowedJump;
        // FingerPos.allowedJump += PlayerAllowedJump;
        LevelManager.OnLevelLoad += OnLevelLoad;

        if (leanForceRb == null)
            leanForceRb = FindFirstObjectByType<LeanForceRigidbodyCustom>();

        if (GameFolder == null)
            GameFolder = GameObject.Find("Game").transform;
    }

    private void Start()
    {
        JumpsLeft = JUMPS_TO_START;
        uiManager.JumpAmountText.text = JumpsLeft.ToString();

        uiManager.SetGameLevelCanvases(false);
        SetupStarFinishImages();
    }



    private void GetLevelInfo()
    {
        levelNo = LevelManager.LastLevelPlayed;
        chapterNo = LevelManager.LastChapterPlayed;
        LevelMetaData = chapterList[LevelManager.LastChapterPlayed].LevelList[LevelManager.LastLevelPlayed];
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
            levelManager.LevelGameObject.transform.Find("StartPosition"))
        { 
            levelManager.LevelGameObject.transform.Find("StartPosition").gameObject.SetActive(false); 
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

        uiManager.MainMenu.SetActive(false);

        if (exitRainbow != null)
            exitRainbow.SetActive(false);

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

        uiManager.ShowPauseMenu(false);

        EnableCubeyLevelObject(true);
        UpdateLevelText(levelNo);

        JumpCount = JUMPS_TO_START;
        GetPickupsForLevel();
        ReParentExitSwirl(false);
        SetupExit();
        
        _ = UiManager.Instance.WaitForLevelMetaData();

        uiManager.PickupText();
        
        TimeTaken.TimeDuration(TimeTaken.TimeAction.Start);

        if (exitRainbow != null)
            PlayerFaceDirection(exitRainbow.transform.position.x < 0);

        SetGameState(GameState.Level);
    }

    internal void ToggleSticky(bool state)
    {
        if (!state)
        {
            playerRb.drag = 0;
            playerRb.angularDrag = 0;
            stickyObject.CurrentValue = false;
            playerRb.isKinematic = false;
            cubeyPlayer.transform.SetParent(GameFolder, true);
            return;
        }
        
        playerRb.drag = playerGooDrag;
        playerRb.angularDrag = playerGooDrag;
        playerRb.velocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
    }

    // TODO - needs moved?
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
        if (!jumpCounting && JumpsLeft > 0) return;

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
        await Task.Delay(DELAY_FAILED_SCREEN_IN_SECONDS * 1000);
        uiManager.ShowFailedScreen(true);
    }

    internal void ResetCubey()
    {
        playerRb.drag = 0;
        playerRb.angularDrag = 0;
        stickyObject.CurrentValue = false;
        playerRb.isKinematic = false;
        TimeTaken.TimeDuration(TimeTaken.TimeAction.Start);
    }

    // Comes from ExitManager
    internal void LoadEndScreen(bool won)
    {
        if (!won)
        {
            uiManager.ShowFailedScreen(true);
            return;
        }

        TimeTaken.TimeDuration(TimeTaken.TimeAction.Stop);
        audioManager.PlayAudio(audioManager.cubeyCelebration);
        ReParentExitSwirl(false);
        uiManager.ShowEndScreen(true);
            
        ShowStarsAchieved();
        SaveLoadManager.SaveGameData();
    }

    internal void EnableCubeyLevelObject(bool state)
    {
        if (!state)
        {
            playerRb.useGravity = false;
            cubeyPlayer.SetActive(false);
            return;
        }

        cubeyPlayer.SetActive(true);
        cubeyPlayer.transform.SetParent(GameFolder, true);
        SetPlayerParentAndPos();
        playerRb = cubeyPlayer.GetComponent<Rigidbody>();
        playerRb.freezeRotation = true;
        playerRb.velocity = new Vector3(0, 0, 0);
        playerRb.freezeRotation = false;
        playerRb.useGravity = true;

        playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
        
        if (leanForceRb == null)
            leanForceRb = cubeyPlayer.GetComponent<LeanForceRigidbodyCustom>();
    }

    private void UpdateLevelText(int n)
    {
        uiManager.LevelText.text = "Level " + (n+1);
    }

    private void CheckPickupCount(int na)
    {
        uiManager.PickupText();

        if (PickupCountProperty > 0) return;

        uiManager.DisablePickupGraphics();
        ShowExit();
        uiManager.ItemText.text = "Go to Exit";
    }

    private void GetPickupsForLevel()
    {
        GameObject pickupGroup = levelManager.LevelGameObject.transform.Find("Pickups").gameObject;

        var pickupCount = 0;
        for (int i = 0; i < pickupGroup.transform.childCount; i++)
        {
            pickupCount += 1;
            pickupGroup.transform.GetChild(i).gameObject.SetActive(true);
        }

        PickupCountProperty = pickupCount;

        uiManager.PickupText();
    }

    #region Exit setup

    private void ShowExit()
    {
        if (LevelManager.LastChapterPlayed == 0)
        {
            audioManager.PlayAudio(audioManager.cubeyExitOpen);
            ToggleXmasExit(false);
            return;
        }

        audioManager.PlayAudio(audioManager.cubeyExitOpen);

        if (exitRainbow == null)
            exitRainbow = FindExitRainbow();
            
        visualEffects.PlayEffect(visualEffects.peExitExplosion, exitRainbow.transform.position);
        AnimateExit();
    }

    private void AnimateExit()
    {
        Vector3 scaleDown = Vector3.zero;
        visualEffects.peExitSwirl.transform.DOScale(scaleDown, 1).OnComplete(ToggleExitObject);

        Vector3 scaleUp = Vector3.one;
        exitRainbow.SetActive(true);
        exitRainbow.transform.GetChild(0).gameObject.SetActive(true);
        exitRainbow.transform.localScale = Vector3.zero;
        exitRainbow.transform.DOScale(scaleUp, 1);
    }

    private void ToggleExitObject()
    {
        visualEffects.peExitSwirl.SetActive(false);
    }

    // TODO - remove all finds. Assign on load or in scriptable object or enum/switch
    private GameObject FindExitRainbow()
    {
        var levelParent = levelManager.LevelGameObject.transform;
        var length = levelParent.childCount;

        for (int i = 0; i < length; i++)
        {
            switch (levelParent.GetChild(i).name)
            {
                case "MovingExitPlatform":
                    return levelParent.GetChild(i).transform.Find("Exit").gameObject;
                case "Exit":
                    return levelParent.GetChild(i).gameObject;
                case "Spindle":
                    return levelParent.GetChild(i).GetChild(0).gameObject;
                case "XmasExit":
                    return levelParent.GetChild(i).transform.Find("Exit").gameObject;
                default:
                    Logger.Instance.ShowDebugError("Can't find the exit in: " + levelManager.LevelGameObject.name);
                    break;
            }
        }

        return null;
    }

    private void SetupExit()
    {
        exitRainbow = FindExitRainbow();

        if (chapterNo == 0)
        {
            SetupExitXmas();
            return;
        }

        SetupNormalExit();
    }

    private void SetupNormalExit()
    {
        visualEffects.peExitSwirl.SetActive(true);

        var pePos = exitRainbow.transform.position;
        pePos.y += 0.65f;
        visualEffects.peExitSwirl.transform.position = pePos;

        ReParentExitSwirl(true);

        exitRainbow.SetActive(false);
    }

    private void ReParentExitSwirl(bool state)
    {
        if (state)
        {
            visualEffects.peExitSwirl.transform.SetParent(exitRainbow.transform.parent);
            visualEffects.peExitSwirl.GetComponent<Animator>().enabled = true;
            return;
        }

        visualEffects.peExitSwirl.transform.SetParent(visualEffects.ParticleEffectsGroup.transform);
    }

    private void SetupExitXmas()
    {
        prezzieClosed = exitRainbow.transform.parent.Find("LargeXmasGift/Closed").gameObject;
        prezzieFlat = exitRainbow.transform.parent.Find("LargeXmasGift/Opened").GetComponent<SpriteRenderer>();
        prezzieCollision = exitRainbow.transform.parent.Find("LargeXmasGift/collision").GetComponent<BoxCollider>();

        ToggleXmasExit(true);
    }

    private void ToggleXmasExit(bool state)
    {
        prezzieClosed.SetActive(state);
        prezzieCollision.gameObject.SetActive(state);
        prezzieFlat.gameObject.SetActive(!state);

        var newPePos = prezzieFlat.transform.position;
        newPePos.y += 2f;

        if (prezzieFlat.gameObject.activeInHierarchy)
        {
            exitRainbow.SetActive(!state); // animate
            AnimateExit();
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peExitOpened, newPePos);
        }
    }

    #endregion Exit setup

    internal void PlayerAllowedJump(bool state)
    {
        AllowPlayerMovement = state;
        LaunchArc = state;
        leanForceRb.canJump = state;
    }

    private void SetPlayerParentAndPos()
    {
        cubeyPlayer.transform.SetParent(GameFolder.transform);

        var pos = LevelMetaData.LevelPrefab.transform.GetChild(1).transform.position;
        cubeyPlayer.transform.SetPositionAndRotation(pos, new Quaternion(0, 0, 0, 0));

        DisableStartPosition();
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

    private void UnlockNextLevel()
    {
        if (award > 0)
        {
            if (UnlockManager.GetLevelUnlocked(chapterNo, levelNo))
            {
                UnlockManager.UnlockLevel(chapterNo, LevelManager.LastLevelPlayed + 1);
                Debug.Log($"Unlocking chapter: {chapterNo}, level: {LevelManager.LastLevelPlayed + 1}");
            }
            else
            {
                Debug.LogError("ShowStarsAchieved. Can't find level unlock");
            }
        }
    }

    #region Stars Awarded

    private void ShowStarsAchieved()
    {
        award = AwardManager.Instance.StarsGiven();

        // TODO - Remove finds. Add to load or scriptable object
        uiManager.EndScreen.transform.Find("Buttons/Continue_button").gameObject.SetActive(award > 0);

        AwardManager.Instance.SetAwardForLevel(award);

        UnlockNextLevel();

        AssignStarsFromAward();
    }

    private void AssignStarsFromAward()
    {
        starImages[0].color = ColourManager.starDefault;
        starImages[1].color = ColourManager.starDefault;
        starImages[2].color = ColourManager.starDefault;

        starBronze_anim.StopPlayback();
        starSilver_anim.StopPlayback();
        starGold_anim.StopPlayback();

        float[] animSpeed = new float[3] { 1,1,1 };
        float[] normTime = new float[3] { 0,0,0 };

        switch (award)
        {
            case AwardManager.Awards.NoAward:
                uiManager.ModifyEndScreenInfoText(FinishedInfo.Failed);
                break;
            case AwardManager.Awards.OneStar:
                starImages[0].color = ColourManager.starBronze;
                animSpeed = new float[] { 1, 0, 0 };
                normTime = new float[] { 0.4f, 1, 1 };
                SetStars(normTime, animSpeed, FinishedInfo.Nearly);
                break;
            case AwardManager.Awards.TwoStars:
                starImages[0].color = ColourManager.starBronze;
                starImages[1].color = ColourManager.starSilver;
                normTime = new float [] { 0, -0.5f, 1 };
                SetStars(normTime, animSpeed, FinishedInfo.Nearly);
                break;
            case AwardManager.Awards.ThreeStars:
                starImages[0].color = ColourManager.starBronze;
                starImages[1].color = ColourManager.starSilver;
                starImages[2].color = ColourManager.starGold;
                SetStars(normTime, animSpeed, FinishedInfo.Completed);
                break;
        }
    }

    private void SetStars(float[] normTime, float[] animSpeed, FinishedInfo finishedInfo)
    {
        starBronze_anim.Play("StarBronze", 0, normTime[0]);
        starBronze_anim.speed = animSpeed[0];

        starSilver_anim.Play("StarSilver", 0, normTime[1]);
        starSilver_anim.speed = animSpeed[1];

        starGold_anim.Play("StarGold", 0, normTime[2]); // 0.4
        starGold_anim.speed = animSpeed[2];

        uiManager.ModifyEndScreenInfoText(finishedInfo);
    }

    #endregion Stars Awarded

    // TODO - is a loading image still needed?
    public async void LoadingScene(bool on)
    {
        if (uiManager.LoadingScreen == null) return;

        uiManager.LoadingScreen.SetActive(on);
        await Task.Delay(200);
        uiManager.LoadingScreen.SetActive(false);
    }

    internal void PlayerJumped()
    {
        audioManager.PlayAudio(audioManager.cubeyJump);

        if (!jumpCounting)
        {
            if (JumpsLeft != 0)
            {
                JumpsLeft--;
            }
        }
        else
        {
            JumpsLeft++;
        }

        JumpCount = JumpsLeft;

        _ = UiManager.Instance.WaitForLevelMetaData();
        CheckJumpCount();
    }

    internal void PlayerFaceDirection(bool right)
    {
        flip.x = -1;

        if (right)
            flip.x = 1;

        flip.y = 1;
        flip.z = 1;

        playerRb.gameObject.transform.localScale = flip;
    }

    #region App stuff

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

    //private void OnDestroy()
    //{
    //    pickupCountProperty.OnValueChanged -= CheckPickupCount;
    //    exitProperty.OnValueChanged -= LoadEndScreen;
    //    stickyObject.OnValueChanged -= ToggleSticky;
    //    leanForceRb.onGround -= PlayerAllowedJump;
    //}

    #endregion App stuff
}
