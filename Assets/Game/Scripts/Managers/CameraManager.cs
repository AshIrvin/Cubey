using System.Collections;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    //[SerializeField] private BoolGlobalVariable gameLevel;
    //[SerializeField] private SaveMetaData saveMetaData;
    
    private ChapterList chapterList;

    // old
    [SerializeField] private Vector3 exitPos;
    [SerializeField] private Vector3 endCamPos;
    [SerializeField] private GameObject cubeyOnMap;
    
    [Header("Floats")]
    [SerializeField] private float speed = 0.1f;
    [SerializeField] private float camTime = 1f;
    [SerializeField] private float camToLevelButtonTime = 2.5f; // old 1.4
    [SerializeField] private float panToCubeyTime = 1f;//0.3f;
    // [SerializeField] private float camMapDragTime = 0.3f;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    private bool panningFromExit;

    private GameManager gameManager;
    private MapManager mapManager;
    private GlobalMetaData globalMetaData;
    //private Camera cam;

    private Vector3 dragOrigin; //Where are we moving?
    private Vector3 buttonToStartFrom;
    private Vector3 buttonToEndAt;
    private float offsetFromButton = 0.1f;
    private float distanceFromStartButton;
    private float distanceFromEndButton;
    private bool playedIntroOnce;
    private bool reachedStartButton;
    // private bool reachedEndButton;

    // public bool panToLevel;
    // public bool disableAutoPanMapCam;

    [SerializeField] private float delayCameraOnExitStart = 1f;
    private float gameCamTime2 = 2;
    private float defaultGameCamTime = 0.5f;
    private float camCubeyDistance = 0.3f;

    [SerializeField] private Vector3 nextOpenLevel;
    [SerializeField] private int panningToLevel;
    [SerializeField] private Toggle autoPanToCubeyButton;
    
    private Tween panToCubey;
    private GameObject cubeyPlayer;
    
    //public bool GameLevel
    //{
    //    get => gameLevel.CurrentValue;
    //    set => gameLevel.CurrentValue = value;
    //}

    public bool autoPanToCubey = true;
    


    private void Awake()
    {
        GameManager.LevelLoaded += EnteringLevel;
        MainMenuManager.onStart += PlayStartCameraSweep;
        MapManager.MapOpened += PanToLevelButton;

        chapterList = GlobalMetaData.Instance.ChapterList;
        gameManager = GameManager.Instance;
        mapManager = MapManager.Instance;
        globalMetaData = GlobalMetaData.Instance;

        //cam = Camera.main;
    }

    private void Start()
    {
        LoadAutoPanMode();

        GetCubeyPlayer();
    }

    private void OnDestroy()
    {
        GameManager.LevelLoaded -= EnteringLevel;
        MainMenuManager.onStart -= PlayStartCameraSweep;
        MapManager.MapOpened -= PanToLevelButton;
    }

    private void OnDisable()
    {
        // VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel, nextOpenLevel);
        nextOpenLevel = chapterList[SaveLoadManager.LastChapterPlayed].ChapterMapButtonList[SaveLoadManager.LastLevelUnlocked].transform.position;
    }

    private void Update()
    {
        if (panToCubey != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                KillSequence();
            }
        }

        SmoothMoveCamToCubey();
    }

    private void SmoothMoveCamToCubey()
    {
        if (gameManager == null || cubeyPlayer == null)
        {
            Logger.Instance.ShowDebugError("Missing GameManager or Cubey!");
        }

        //if (!autoPanToCubey || mapManager.isActiveAndEnabled) return;

        // TODO - this needs redone - could probably be an action subbed to level loading

        //if (autoPanToCubey && !mapManager.enabled && GlobalMetaData.Instance.GameLevel && !panningFromExit && !FingerPos.abovePlayer && !FingerPos.belowPlayer)
        //{
        //    transform.position = Vector3.SmoothDamp(transform.position, cubeyPlayer.transform.position, ref velocity, defaultGameCamTime);
        //}

        // Set what the camera does here, depending on game state
        switch (GameManager.Instance.GetGameState())
        {
            case GameManager.GameState.Menu:
                transform.position = Vector3.SmoothDamp(transform.position, cubeyOnMap.transform.position, ref velocity, defaultGameCamTime);
                break;
            case GameManager.GameState.Map:
                transform.position = Vector3.SmoothDamp(transform.position, cubeyOnMap.transform.position, ref velocity, defaultGameCamTime);
                break;
            case GameManager.GameState.Level:
                transform.position = Vector3.SmoothDamp(transform.position, cubeyPlayer.transform.position, ref velocity, defaultGameCamTime);
                break;
        }
    }

    private void GetCubeyPlayer()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;

        cubeyPlayer = gameManager.CubeyPlayer;
    }

    public void KillSequence()
    {
        if (panToCubey == null) return;
        panToCubey.Kill();
        panToCubey = null;
        panningFromExit = false;
    }
    
    public void AutoPanToCubey(bool state)
    {
        autoPanToCubey = !state;
        PlayerPrefs.SetInt("autoPanToCubey", !state ? 1 : 0);
        Logger.Instance.ShowDebugLog("autoPanToCubey saving: " + !state);
    }

    private void LoadAutoPanMode()
    {
        autoPanToCubey = PlayerPrefs.GetInt("autoPanToCubey", 1) == 1;
        autoPanToCubeyButton.isOn = !autoPanToCubey;
    }
    
    private void PanToLevelCubey()
    {
        KillSequence();
        var cubeyPos = cubeyPlayer.transform.position;
        panToCubey = DOTween.Sequence().Append(transform.DOMove(cubeyPos, panToCubeyTime).SetEase(Ease.InOutQuad));

        panToCubey.onComplete = () =>
        {
            panningFromExit = false;
        };
    }

    private void EnteringLevel()
    {
        StartCoroutine(StartFromExit());
    }

    private IEnumerator StartFromExit()
    {
        panningFromExit = true;
        panToCubeyTime = gameCamTime2;
        yield return new WaitWhile(() => globalMetaData.LevelMetaData == null);
        exitPos = globalMetaData.LevelMetaData.ExitPosition.transform.position;
        transform.position = exitPos;
        yield return new WaitForSeconds(delayCameraOnExitStart);

        PanToLevelCubey();
    }

    private IEnumerator ChangeCamSpeed(float n)
    {
        yield return new WaitForSeconds(2);
        panToCubeyTime = n;
    }

    public void ResetCamPosition()
    {
        transform.position = endCamPos;
    }

    private Vector3 ClampCam(Vector3 smoothPos)
    {
        var posX = Mathf.Clamp(smoothPos.x, -18, 18);
        var posY = Mathf.Clamp(smoothPos.y, -16, 8);
        smoothPos.x = posX;
        smoothPos.y = posY;

        return smoothPos;
    }

    private void PlayStartCameraSweep()
    {
        transform.DOMove(endCamPos, camTime);
    }

    private void PanToLevelButton()
    {
        KillSequence();
        
        var currentChapterList = chapterList[SaveLoadManager.LastChapterPlayed];
        var currentLevelNo = SaveLoadManager.LastLevelPlayed;
        var buttonPos = currentChapterList.ChapterMapButtonList[currentLevelNo].transform.position;

        if (currentLevelNo < currentChapterList.ChapterMapButtonList.Count && currentLevelNo != 0)
        {
            buttonToStartFrom = buttonPos;
            var moveToNewLevel = currentLevelNo < 29 ? currentLevelNo + 1 : currentLevelNo;
            buttonToEndAt = currentChapterList.ChapterMapButtonList[moveToNewLevel].transform.position;
            panningToLevel = SaveLoadManager.LastLevelUnlocked;
        }
        else
        {
            buttonToStartFrom = buttonPos;
            buttonToEndAt = buttonPos;
        }
        
        nextOpenLevel = currentChapterList.ChapterMapButtonList[SaveLoadManager.LastLevelUnlocked].transform.position;
        nextOpenLevel.z -= 0.5f;
        
        distanceFromStartButton = Vector3.Distance(transform.position, buttonToStartFrom);
        distanceFromEndButton = Vector3.Distance(transform.position, buttonToEndAt);
        
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel, nextOpenLevel);
        
        transform.position = buttonToStartFrom;
        reachedStartButton = true;

        panToCubey = DOTween.Sequence().Append(transform.DOMove(buttonToEndAt, camToLevelButtonTime).SetEase(Ease.InOutQuad));
        
        buttonToStartFrom = Vector3.zero;
        buttonToEndAt = Vector3.zero;
    }
}
