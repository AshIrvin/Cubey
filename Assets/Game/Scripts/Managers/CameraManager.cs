using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Game.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    // [SerializeField] private GameObject maps;
    [SerializeField] private GameObject cubeyOnMap;
    
    [SerializeField] private float speed = 0.1f;
    [SerializeField] private float camTime = 1f;
    [SerializeField] private float camToLevelButtonTime = 1.4f;
    [SerializeField] private float gameCamTime = 1f;//0.3f;
    [SerializeField] private float camMapDragTime = 0.3f;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    [SerializeField] private bool startFromExit;
    
    private Vector3 dragOrigin; //Where are we moving?
    private Vector3 buttonToStartFrom;
    private Vector3 buttonToEndAt;
    private float offsetFromButton = 0.1f;
    private float distanceFromStartButton;
    private float distanceFromEndButton;
    private bool playedIntroOnce;
    private bool reachedStartButton;
    private bool reachedEndButton;

    public bool panToLevel;
    public bool disableAutoPanMapCam;
    
    public bool GameLevel
    {
        get => gameLevel.CurrentValue;
        set => gameLevel.CurrentValue = value;
    }
    
    private GameObject CubeyPlayer => gameManager.CubeyPlayer;
    
    
    private void Awake()
    {
        gameLevel.OnValueChanged += EnteringLevel;
    }

    private void OnDestroy()
    {
        gameLevel.OnValueChanged -= EnteringLevel;
    }

    private void OnDisable()
    {
        // VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel, nextOpenLevel);
        nextOpenLevel = chapterList[SaveLoadManager.LastChapterPlayed].ChapterMapButtonList[SaveLoadManager.LastLevelUnlocked].transform.position;
        
    }

    private void Start()
    {
        panToLevel = false;
    }

    // todo fix this mess
    private void Update()
    {
        if (!playedIntroOnce && !mapManager.mapActive)
            StartCoroutine(PlayStartCameraSweep());

        if (mapManager.mapActive && !panToLevel && !reachedEndButton)
            StartCoroutine(PanToLevelButton());
        
        if (!gameManager.CamMovement || startFromExit) return;

        if (gameManager.GameLevel && CubeyPlayer != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, CubeyPlayer.transform.position, ref velocity,
                gameCamTime);
        }
        else if (!mapManager.enabled && !gameManager.GameLevel) // menu
        {
            transform.position = Vector3.SmoothDamp(transform.position, cubeyOnMap.transform.position, ref velocity, gameCamTime);
        }
        /*else
        {
            Debug.LogError("CameraManager can't find something...");
        }*/
    }

    private void EnteringLevel(bool enable)
    {
        if (!enable)
        {
            reachedStartButton = false;
            reachedEndButton = false;
            disableAutoPanMapCam = false;
            panToLevel = false;
            return;
        }
        
        if (startFromExit)
        {
            reachedStartButton = false;
            reachedEndButton = false;
            disableAutoPanMapCam = false;
            panToLevel = false;
            StartCoroutine(StartFromExit());
        }
    }

    private IEnumerator StartFromExit()
    {
        yield return new WaitWhile(() => gameManager.LevelMetaData == null);
        exitPos = gameManager.LevelMetaData.ExitPosition.transform.position;
        transform.position = exitPos;
        yield return new WaitForSeconds(0.5f);
        transform.position = Vector3.SmoothDamp(transform.position, CubeyPlayer.transform.position, ref velocity, gameCamTime);
        
        startFromExit = false;
    }
    
    private IEnumerator ChangeCamSpeed(float n)
    {
        yield return new WaitForSeconds(2);
        gameCamTime = n;
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

    private IEnumerator PlayStartCameraSweep()
    {
        transform.position = Vector3.SmoothDamp(transform.position, endCamPos, ref velocity, camTime);

        yield return new WaitForSeconds(3);

        playedIntroOnce = true;
    }

    [SerializeField] private Vector3 nextOpenLevel;
    
    private IEnumerator PanToLevelButton()
    {
        if (Input.GetMouseButtonDown(0))
        {
            panToLevel = true;
            disableAutoPanMapCam = true;
        }

        // TODO - SO
        var currentChapterList = chapterList[SaveLoadManager.LastChapterPlayed];
        var currentLevelNo = SaveLoadManager.LastLevelPlayed;
        var buttonPos = currentChapterList.ChapterMapButtonList[currentLevelNo].transform.position;

        if (currentLevelNo < currentChapterList.ChapterMapButtonList.Count && currentLevelNo != 0)
        {
            buttonToStartFrom = buttonPos;
            var moveToNewLevel = currentLevelNo < 29 ? currentLevelNo + 1 : currentLevelNo;
            buttonToEndAt = currentChapterList.ChapterMapButtonList[moveToNewLevel].transform.position;
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
        
        if (distanceFromStartButton > offsetFromButton && !reachedStartButton)
        {
            transform.position = buttonToStartFrom;
            reachedStartButton = true;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, buttonToStartFrom) < offsetFromButton);
        }

        if (distanceFromEndButton > offsetFromButton && !reachedEndButton)
        {
            var pos = Vector3.SmoothDamp(transform.position, buttonToEndAt, ref velocity, camToLevelButtonTime);
            transform.position = ClampCam(pos);
            yield return new WaitUntil(() => Vector3.Distance(transform.position, buttonToEndAt) < offsetFromButton);
            reachedEndButton = true;
        }
        
        buttonToStartFrom = Vector3.zero;
        buttonToEndAt = Vector3.zero;

        panToLevel = true;
    }
}
