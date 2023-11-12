using System.Collections;
using DG.Tweening;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    private ChapterList chapterList;

    [SerializeField] private Vector3 endCamPos;
    [SerializeField] private GameObject cubeyOnMap;
    
    [Header("Floats")]
    [SerializeField] private float speed = 0.1f;
    [SerializeField] private float camTime = 1f;
    [SerializeField] private float camToLevelButtonTime = 2.5f; // old 1.4
    [SerializeField] private float panToCubeyTime = 2f;//0.3f;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    [SerializeField] private float delayCameraOnExitStart = 1f;
    [SerializeField] private Vector3 nextOpenLevel;
    [SerializeField] private int panningToLevel;
    [SerializeField] private Toggle autoPanToCubeyButton;
    [SerializeField] private float camCubeyOffset = 0.1f;

    private GameManager gameManager;
    //private MapManager mapManager;
    private GlobalMetaData globalMetaData;

    private bool panningFromExit;
    private Vector3 dragOrigin; //Where are we moving?
    private Vector3 buttonToStartFrom;
    private Vector3 buttonToEndAt;
    private float offsetFromButton = 0.1f;
    private float distanceFromStartButton;
    private float distanceFromEndButton;
    private bool playedIntroOnce;
    private bool reachedStartButton;

    //private float gameCamTime2 = 2;
    private float defaultGameCamTime = 0.5f;
    private float camCubeyDistance = 0.3f;
    
    private Tween panToCubey;
    private GameObject cubeyPlayer;

    public bool autoPanToCubey = true;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        LevelManager.OnLevelLoad += EnteringLevel;
        MainMenuManager.OnMainMenuLoad += OnMainMenuLoad;
        MapManager.OnMapLoad += PanToLevelButton;
        //LeanTouch.OnFingerUp += ReturnPanToCubey;
        //LeanTouch.OnFingerDown += CamFollowFinger;

        chapterList = GlobalMetaData.Instance.ChapterList;
        gameManager = GameManager.Instance;
        //mapManager = MapManager.Instance;
        globalMetaData = GlobalMetaData.Instance;
    }

    private void Start()
    {
        LoadAutoPanMode();

        GetCubeyPlayer();
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
    }

    private void LateUpdate()
    {
        SmoothMoveCamToCubey();
    }

    private void SmoothMoveCamToCubey()
    {
        switch (GameManager.Instance.GameStateType)
        {
            case GameManager.GameState.Level:
                if (!autoPanToCubey || panningFromExit || FingerPos.abovePlayer || FingerPos.belowPlayer) return;
                
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

    private void EnteringLevel()
    {
        StartCoroutine(PanFromExit());
    }

    private IEnumerator PanFromExit()
    {
        panningFromExit = true;
        yield return new WaitWhile(() => globalMetaData.LevelMetaData == null);
        transform.position = globalMetaData.LevelMetaData.ExitPosition.transform.position;
        yield return new WaitForSeconds(delayCameraOnExitStart);

        ExitPanToCubey();
    }

    private void ExitPanToCubey()
    {
        KillSequence();
        var cubeyPos = cubeyPlayer.transform.position;
        panToCubey = DOTween.Sequence().Append(transform.DOMove(cubeyPos, panToCubeyTime).SetEase(Ease.InOutQuad));

        panToCubey.onComplete = () =>
        {
            panningFromExit = false;
        };
    }

    private void ReturnPanToCubey(LeanFinger finger)
    {
        if (gameManager.GameStateType != GameManager.GameState.Level) return;

        KillSequence();
        var cubeyPos = cubeyPlayer.transform.position;
        panToCubey = DOTween.Sequence().Append(transform.DOMove(cubeyPos, panToCubeyTime).SetEase(Ease.InOutQuad));
    }

    private void CamFollowFinger(LeanFinger finger)
    {
        KillSequence();
        var cubeyPos = cubeyPlayer.transform.position;
        panToCubey = DOTween.Sequence().Append(transform.DOMove(cubeyPos, panToCubeyTime).SetEase(Ease.InOutQuad));
    }

    private void PanToMenuCubey()
    {
        KillSequence();
        var cubeyPos = cubeyPlayer.transform.position;
        panToCubey = DOTween.Sequence().Append(transform.DOMove(cubeyPos, panToCubeyTime).SetEase(Ease.InOutQuad));

        panToCubey.onComplete = () =>
        {
            panningFromExit = false;
        };
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

    private void OnMainMenuLoad()
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
        
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peMapLevelStars, nextOpenLevel);
        
        transform.position = buttonToStartFrom;
        reachedStartButton = true;

        panToCubey = DOTween.Sequence().Append(transform.DOMove(buttonToEndAt, camToLevelButtonTime).SetEase(Ease.InOutQuad));
        
        buttonToStartFrom = Vector3.zero;
        buttonToEndAt = Vector3.zero;
    }
}
