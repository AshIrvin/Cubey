using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using UnityEditor;
using UnityEngine.Advertisements;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private CameraManager cameraManager;
    
    [SerializeField] private SaveMetaData saveMetaData;
    [SerializeField] private ChapterList allChapters;
    // [SerializeField] private BoolGlobalVariable gameUiProperty;
    
    [SerializeField] private GameObject menuEnvironmentParent;
    [SerializeField] private List<GameObject> menuEnvironments;
    [SerializeField] private GameObject mapsParent;
    [SerializeField] private List<GameObject> chapterMaps;
    
    public GameObject mainMenu;
    public LeanConstrainToBox leanConstrainToBox;
    
    
    
    // Old
    [Header("Scripts")]
    [SerializeField] private VisualEffects visualEffects;
    [SerializeField] private AudioManager audioManager;
    
    [Header("GameObjects")]
    // [SerializeField] private GameObject[] chapter_maps;
    [SerializeField] private GameObject chapterScreen;
    [SerializeField] private GameObject mainMenuUi;
    
    [SerializeField] private GameObject navButtons;
    [SerializeField] private GameObject screenDeleteSaveData;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button startButton;

    /*
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem peLvlStars;

    [SerializeField] private ParticleSystem peSnowThrow1;
    [SerializeField] private ParticleSystem peSnowThrow2;
    */

    // [Header("Other")]
    // [SerializeField] public int chapter;

    [SerializeField] private Text startText;
    [SerializeField] private Text versionNo;
    [SerializeField] private Text goldWon;

    // [SerializeField] public bool mapActive;
    [SerializeField] private bool[] chapterComplete = new bool[5];

    private Color c1, c2, c2b, c3;

    /*[Header("StartButton Movement")]*/
    private Vector3 pos1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 pos2 = new Vector3(1f, 1f, 1f);
    private Vector3 scale1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 scale2 = new Vector3(0.95f, 0.95f, 0.95f);

    // [SerializeField]
    // private GameObject cam;
    
    [SerializeField]
    private LeanCameraZoomSmooth leanZoom;

    private Animator anim;

    
    
    /*private bool NavButtons
    {
        get => navButtons.activeInHierarchy;
        set => navButtons.SetActive(value);
    }*/

    /*public bool GameUiProperty
    {
        set => gameUiProperty.currentValue = value;
    }*/

    private void Awake()
    {
        // gameUiProperty.onValueChanged += SetNavButtons;

        SetNavButtons(false);
        
        if (visualEffects == null) visualEffects = GetComponent<VisualEffects>();
        if (audioManager == null) audioManager = GetComponent<AudioManager>();
        if (mapManager == null) mapManager = GetComponent<MapManager>();
        if (leanConstrainToBox == null) leanConstrainToBox = cameraManager.gameObject.GetComponent<LeanConstrainToBox>();
        mapManager.enabled = false;
    }

    private void SetNavButtons(bool on)
    {
        Debug.Log("set nav buttons: " + on);
        navButtons.SetActive(on);
    }

    // Start is called before the first frame update
    void Start()
    {
        // if (cam == null)
        //     cam = Gam//GameObject.Find("Camera").gameObject;
        // leanZoom = cam.GetComponent<LeanCameraZoomSmooth>();

        /*if (!mapManager.mapActive)
            leanZoom.enabled = false;
        else
            leanZoom.enabled = true;*/

        versionNo.text = "v: " + Application.version;

        AddMenuEnvironments();

        SetColours();

        anim = startButton.GetComponent<Animator>();
        ButtonSizePong();

        SetMenuEnvironment(saveMetaData.LastChapterPlayed);
        
        
    }

    private void SetColours()
    {
        c1 = new Color(0, 0, 0, 0); // clear
        c2 = new Color(0.1f, 0.3f, 0.7f, 1); // blue
        c2b = new Color(0.2f, 0.4f, 0.9f, 1); // blue
        c3 = new Color(1, 1, 1, 1); // white
    }
    
    private void UpdateMenuEnvironments()
    {
        for (int i = 0; i < allChapters.Count; i++)
        {
            menuEnvironmentParent = Instantiate(allChapters[saveMetaData.LastChapterPlayed].MenuEnvironment);
        }
    }
    
    private void AddMenuEnvironments()
    {
        for (int i = 0; i < allChapters.Count; i++)
        {
            var menu = Instantiate(allChapters[i].MenuEnvironment, menuEnvironmentParent.transform);
            menuEnvironments.Add(menu);
            menu.SetActive(false);
        }
    }

    private void SetMenuEnvironment(int n)
    {
        DisableMenuEnv();
        
        menuEnvironments[n].SetActive(true);
    }

    private void DisableMenuEnv()
    {
        for (int i = 0; i < menuEnvironments.Count; i++)
        {
            menuEnvironments[i].SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // gameUiProperty.onValueChanged -= SetNavButtons;
        Destroy(menuEnvironmentParent);
    }

    public void ButtonClose(GameObject go)
    {
        go.SetActive(false);
    }

    private void EnableMap(bool on)
    {
        /*if (on)
        {
            PlayEffect(peSnowThrow1, peSnowThrow1.transform.position, true);
            PlayEffect(peSnowThrow2, peSnowThrow2.transform.position, true);
        } else
        {
            CancelPe(peSnowThrow1);
            CancelPe(peSnowThrow2);
        }*/
    }

    /*private void LoadMap(int chapter)
    {
        DisableMaps();
        chapterMaps[chapter]?.SetActive(true);
    }*/
    
    
    
    public void ShowMap(int n)
    {
        var chapter = saveMetaData.LastChapterPlayed = n;
        // leanConstrainToBox.Target = null;

        var mapCollider = allChapters[chapter].ChapterMap.GetComponentInChildren<BoxCollider>();
        leanConstrainToBox.Target = mapCollider;
        
        // EditorUtil.ApplyChanges(saveMetaData);
        EditorUtility.SetDirty(saveMetaData);
        
        SetMenuEnvironment(chapter);
        DisableMenuScreens();

        leanZoom.enabled = true;
        // navButtons.SetActive(true);
        SetNavButtons(true);
        
        DisableMenuEnv();

        mapManager.enabled = true;
        
        backButton.onClick.AddListener(() => LoadChapterScreen(true));
    }

     /*private void SetupButtonsForMap()
     {
         levelButtons = allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList;
         // Debug.Log($"chapter {saveMetaData.LastChapterPlayed} buttons being set");

         for (int i = 0; i < allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList.Count; i++)
         {
             allChapters[saveMetaData.LastChapterPlayed].ChapterMapButtonList[i].GetComponentInChildren<Text>()
                 .text = (i+1).ToString();
         
             var n = i;
             if (levelButtons[i].GetComponent<Button>() != null)
                 levelButtons[i].GetComponent<Button>().onClick.AddListener(delegate {LoadLevel(n); });
             else
                 Debug.Log("can't find button " + i);         
         
         }
         Debug.Log($"Assigning {levelButtons.Count} level buttons");

         // AssignMapButtons();
         // GetLevel();
         // CheckLevelUnlocks();
         // SetStarsToMapButtons();
         // before this, level buttons needs added to a list
         // SetStarsForEachLevel();
     
     }*/

    /*public void MainMenu(bool enable)
    {
        mainMenu.SetActive(enable);
    }*/
    
    public void LoadChapterScreen(bool enable)
    {
        leanConstrainToBox.Target = allChapters[saveMetaData.LastChapterPlayed].ChapterMap.GetComponentInChildren<BoxCollider>();
        
        chapterScreen.SetActive(enable);
        SetNavButtons(enable);
        mainMenuUi.SetActive(!enable);
        menuEnvironmentParent.SetActive(true);
        mapManager.enabled = false;
        
        cameraManager.panToLevel = false;
        cameraManager.disableAutoPanMapCam = false;
        
        leanZoom.enabled = false;
        backButton.onClick.AddListener(MainMenuScreen);
    }

    /*public void DisableMaps()
    {
        for (int i = 0; i < chapterMaps.Count; i++)
            chapterMaps[i].SetActive(false);
    }*/

    private void DisableMenuScreens()
    {
        chapterScreen.SetActive(false);
        mainMenuUi.SetActive(false);
        menuEnvironmentParent.SetActive(false);
    }

    public void BackButton()
    {
        cameraManager.panToLevel = false;
        cameraManager.disableAutoPanMapCam = false;
        
        /*if (mapManager.mapActive)
        {
            cameraManager.panToLevel = false;
            cameraManager.disableAutoPanMapCam = false;
            
            // backButton.GetComponent<Button>().onClick.AddListener(delegate { ChapterSelectScreen(true); });
        }
        else
        {
            cameraManager.panToLevel = false;
            cameraManager.disableAutoPanMapCam = false;
            // backButton.GetComponent<Button>().onClick.AddListener(delegate { MainMenuScreen(); });
        }*/
    }

    private void MainMenuScreen()
    {
        mapManager.DisableMaps();
        SetMenuEnvironment(saveMetaData.LastChapterPlayed);
        LoadChapterScreen(false);
        cameraManager.ResetCamPosition();
    }

    private void FadeInStartButton()
    {
        startText.color = Color.Lerp(c2, c2b, Mathf.PingPong(Time.time, 1));
    }

    private void ButtonSizePong()
    {
        StartCoroutine(WaitForPong());
    }

    private IEnumerator WaitForPong()
    {
        yield return new WaitForSeconds(2.5f);
        anim.SetBool("StartButton_anim", false);
        anim.SetBool("EnablePingPong", true);
    }

    public void PlayEffect(ParticleSystem effect, Vector3 pos, bool loop)
    {
        effect.gameObject.transform.position = pos;
        var peMain = effect.main;
        peMain.loop = loop;
        effect.Play();
    }

    public void CancelPe(ParticleSystem effect)
    {
        var peMain = effect.main;
        peMain.loop = false;

        var peEmit = effect.emission;
        peEmit.enabled = false;
    }

}
