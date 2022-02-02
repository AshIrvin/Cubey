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
    [SerializeField] private ChapterList chapterList;
    [SerializeField] private GameObject menuEnvironmentParent;
    [SerializeField] private List<GameObject> menuEnvironments;
    [SerializeField] private GameObject mapsParent;
    [SerializeField] private List<GameObject> chapterMaps;
    
    public GameObject mainMenu;
    public LeanConstrainToBox leanConstrainToBox;
    
    [Header("Extra screens")]
    [SerializeField] private GameObject chapterFinishScreen;
    [SerializeField] private bool deleteLastChapterFinishScreenData;

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
    // [SerializeField] private Text goldWon;

    // [SerializeField] public bool mapActive;
    // [SerializeField] private bool[] chapterComplete = new bool[5]; // todo this used or using the new ChapterUnlocked bool

    private Color c1, c2, c2b, c3;

    /*[Header("StartButton Movement")]*/
    private Vector3 pos1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 pos2 = new Vector3(1f, 1f, 1f);
    private Vector3 scale1 = new Vector3(0.95f, 0.95f, 0.95f);
    private Vector3 scale2 = new Vector3(0.95f, 0.95f, 0.95f);

    [SerializeField]
    private LeanCameraZoomSmooth leanZoom;

    private Animator anim;
    public int chapterUnlockedTo;
    
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
        SetNavButtons(false);
        
        if (visualEffects == null) visualEffects = GetComponent<VisualEffects>();
        if (audioManager == null) audioManager = GetComponent<AudioManager>();
        if (mapManager == null) mapManager = GetComponent<MapManager>();
        if (leanConstrainToBox == null) leanConstrainToBox = cameraManager.gameObject.GetComponent<LeanConstrainToBox>();
        mapManager.enabled = false;
        
        // find gameobjects
        if (chapterFinishScreen == null) chapterFinishScreen = GameObject.Find("ChapterFinishScreen");
        if (chapterButtons.Count == 0)
        {
            Debug.LogError("Assign chapter buttons to list!");
        }

        if (deleteLastChapterFinishScreenData)
        {
            PlayerPrefs.DeleteKey("chapterFinishScreenBronze" + saveMetaData.LastChapterPlayed);
            PlayerPrefs.DeleteKey("chapterFinishScreenSilver" + saveMetaData.LastChapterPlayed);
            PlayerPrefs.DeleteKey("chapterFinishScreenGold" + saveMetaData.LastChapterPlayed);
        }
    }

    private void SetNavButtons(bool on)
    {
        navButtons.SetActive(on);
    }

    // Start is called before the first frame update
    void Start()
    {
        versionNo.text = "v: " + Application.version;

        AddMenuEnvironments();
        SetColours();
        anim = startButton.GetComponent<Animator>();
        ButtonSizePong();
        SetMenuEnvironment(saveMetaData.LastChapterPlayed);
        chapterFinishScreen.SetActive(false);
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
        for (int i = 0; i < chapterList.Count; i++)
        {
            menuEnvironmentParent = Instantiate(chapterList[saveMetaData.LastChapterPlayed].MenuEnvironment);
        }
    }
    
    private void AddMenuEnvironments()
    {
        for (int i = 0; i < chapterList.Count; i++)
        {
            var menu = Instantiate(chapterList[i].MenuEnvironment, menuEnvironmentParent.transform);
            menuEnvironments.Add(menu);
            menu.SetActive(false);
        }
    }

    private void SetMenuEnvironment(int n)
    {
        DisableMenuEnv();
        
        menuEnvironments[n].SetActive(true);
        leanZoom.Zoom = chapterList[n].MenuZoomLevel;
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
        Destroy(menuEnvironmentParent);
    }

    public void ButtonClose(GameObject go)
    {
        go.SetActive(!go.activeInHierarchy);
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

    public void SetCollisionBox(string collisionName)
    {
        if (leanConstrainToBox.Target != null)
            leanConstrainToBox.Target.enabled = true;
        
        if (collisionName == "CollisionMenu")
        {
            leanConstrainToBox.Target = chapterList[saveMetaData.LastChapterPlayed].MenuEnvironment.transform.Find(collisionName).GetComponent<BoxCollider>();
        }
        else if (collisionName == "CollisionMap")
        {
            leanConstrainToBox.Target = chapterList[saveMetaData.LastChapterPlayed].ChapterMap.transform.Find(collisionName).GetComponent<BoxCollider>();
        }
        else
        {
            if (leanConstrainToBox.Target != null)
            {
                leanConstrainToBox.Target.enabled = false;
                leanConstrainToBox.Target = null;
            }
            Debug.Log("No Collision box found!");
        }
    }
    
    // Used in chapter menu buttons
    public void ShowMap(int n)
    {
        var chapter = saveMetaData.LastChapterPlayed = n;

        EditorUtility.SetDirty(saveMetaData);
        
        SetMenuEnvironment(chapter);
        DisableMenuScreens();

        // leanConstrainToBox.Target = allChapters[saveMetaData.LastChapterPlayed].ChapterMap.transform.Find("CollisionMap").GetComponent<BoxCollider>();
        SetCollisionBox("CollisionMap");
        
        leanZoom.enabled = true;
        SetNavButtons(true);
        DisableMenuEnv();

        mapManager.enabled = true;
        
        backButton.onClick.AddListener(() => LoadChapterScreen(true));
    }

    [SerializeField] private List<GameObject> chapterButtons;

    [SerializeField] private Color fadedButton = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] private Color unlockedButton = Color.white;
    
    // disable chapters that aren't unlocked?
    private void CycleThroughUnlockedChapters()
    {
        int c = 0;

        for (int i = 0; i < chapterList.Count; i++)
        {
            if (chapterList[i].ChapterUnlocked)
            {
                c = i;
                chapterButtons[c].GetComponent<Button>().interactable = true;
                var image = chapterButtons[i].transform.GetChild(0).GetChild(0).GetComponent<Image>();
                image.color = unlockedButton;
            }
            else
            {
                chapterButtons[i].GetComponent<Button>().interactable = false;
                var image = chapterButtons[i].transform.GetChild(0).GetChild(0).GetComponent<Image>();
                image.color = fadedButton;
            }
        }
        
        chapterUnlockedTo = c;
    }
    
    // for going back to chapter screen using back button on map and Start ui button
    public void LoadChapterScreen(bool enable)
    {
        CycleThroughUnlockedChapters();
        
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
    }

    private void MainMenuScreen()
    {
        mapManager.DisableMaps();
        SetMenuEnvironment(saveMetaData.LastChapterPlayed);
        SetCollisionBox("CollisionMenu");
        chapterFinishScreen.SetActive(false);
        chapterScreen.SetActive(false);
        SetNavButtons(false);
        mainMenuUi.SetActive(true);

        cameraManager.ResetCamPosition();
        leanZoom.enabled = true;
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
    
    public void TryChapterFinishScreen()
    {
        var chapterBronze = PlayerPrefs.GetInt("chapterFinishScreenBronze" + saveMetaData.LastChapterPlayed, 0);
        var chapterSilver = PlayerPrefs.GetInt("chapterFinishScreenSilver" + saveMetaData.LastChapterPlayed, 0);
        var chapterGold = PlayerPrefs.GetInt("chapterFinishScreenGold" + saveMetaData.LastChapterPlayed , 0);
        
        Debug.Log("bronze fin screen: " + chapterBronze);
        Debug.Log("silver fin screen: " + chapterSilver);
        Debug.Log("gold fin screen: " + chapterGold);
        
        // if all levels have been unlocked or at least bronze won, check for how many awards
        if (chapterList[saveMetaData.LastChapterPlayed].AwardsBronze == 29 && chapterBronze == 0)
        {
            // chapterFinishScreen.SetActive(true);
            chapterFinishScreen.GetComponent<ChapterComplete>().TogglePopup();
            
            if (saveMetaData.LastChapterPlayed < chapterList.Count)
            {
                Debug.Log("CHAPTER " + (saveMetaData.LastChapterPlayed + 1) + " UNLOCKED!");
                chapterList[saveMetaData.LastChapterPlayed + 1].ChapterUnlocked = true;
                EditorUtility.SetDirty(chapterList[saveMetaData.LastChapterPlayed]);
                // popup for next chapter?
            }
            else
            {
                Debug.LogError("Anomaly detected with last chapter / allChapters");
            }
            PlayerPrefs.SetInt("chapterFinishScreenBronze" + saveMetaData.LastChapterPlayed, 1);
        }
        else if (chapterList[saveMetaData.LastChapterPlayed].AwardsSilver == 29 && chapterSilver == 0)
        {
            chapterFinishScreen.SetActive(true);
            
            PlayerPrefs.SetInt("chapterFinishScreenSilver" + saveMetaData.LastChapterPlayed, 1);
        }
        else if (chapterList[saveMetaData.LastChapterPlayed].AwardsGold == 29 && chapterGold == 0)
        {
            chapterFinishScreen.SetActive(true);

            PlayerPrefs.SetInt("chapterFinishScreenGold" + saveMetaData.LastChapterPlayed, 1);
        }
    }

    public void EnableGoldAwardsButton(bool state)
    {
        chapterFinishScreen.GetComponent<ChapterComplete>().UpdateButtonAward();
        chapterFinishScreen.SetActive(state);
        // chapterFinishScreen.GetComponent<ChapterComplete>().goldStarButton.SetActive(state);
    }
    
    public void ResetSaves()
    {
        screenDeleteSaveData.SetActive(false);

        for (int i = 0; i < chapterList.Count; i++)
        {
            chapterList[i].AwardsBronze = 0;
            chapterList[i].AwardsSilver = 0;
            chapterList[i].AwardsGold = 0;
            for (int j = 0; j < chapterList[i].LevelList.Count; j++)
            {
                chapterList[i].LevelList[j].AwardsReceived = 0;
            }
        }

        MainMenuScreen();
    }
}
