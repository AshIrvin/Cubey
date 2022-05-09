using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using Game.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class CameraManager : MonoBehaviour
{
    // [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BoolGlobalVariable gameLevel;
    [SerializeField] private ChapterList chapterList;
    [SerializeField] private SaveMetaData saveMetaData;
    
    
    // old
    [SerializeField] private Camera cam;
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
    
    
    
    public bool GameLevel
    {
        get => gameLevel.CurrentValue;
        set => gameLevel.CurrentValue = value;
    }

    public bool autoPanToCubey = true;
    
    private GameObject CubeyPlayer => gameManager.CubeyPlayer;


    private void Awake()
    {
        GameManager.LevelLoaded += EnteringLevel;
        MainMenuManager.onStart += PlayStartCameraSweep;
        MapManager.MapOpened += PanToLevelButton;
        // GameManager.LevelLoaded += PanToLevelCubey;
    }

    private void Start()
    {
        LoadAutoPanMode();
    }

    private void OnDestroy()
    {
        GameManager.LevelLoaded -= EnteringLevel;
        MainMenuManager.onStart -= PlayStartCameraSweep;
        MapManager.MapOpened -= PanToLevelButton;
        // GameManager.LevelLoaded -= PanToLevelCubey;
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
            if (panToCubey.IsPlaying() && Input.GetMouseButtonDown(0))
            {
                KillSequence();
            }
        }
        
        if (autoPanToCubey && !mapManager.enabled && gameManager.GameLevel && !panningFromExit) 
        {
            transform.position = Vector3.SmoothDamp(transform.position, CubeyPlayer.transform.position, ref velocity, defaultGameCamTime);
        }
    }

    public void KillSequence()
    {
        panToCubey.Kill();
        panningFromExit = false;
    }
    
    public void AutoPanToCubey(bool state)
    {
        autoPanToCubey = !state;
        PlayerPrefs.SetInt("autoPanToCubey", !state ? 1 : 0);
        Debug.Log("autoPanToCubey saving: " + !state);
    }

    private void LoadAutoPanMode()
    {
        autoPanToCubey = PlayerPrefs.GetInt("autoPanToCubey", 1) == 1;
        autoPanToCubeyButton.isOn = !autoPanToCubey;
    }
    
    private void PanToLevelCubey()
    {
        KillSequence();
        var cubeyPos = CubeyPlayer.transform.position;
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
        yield return new WaitWhile(() => gameManager.LevelMetaData == null);
        exitPos = gameManager.LevelMetaData.ExitPosition.transform.position;
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
