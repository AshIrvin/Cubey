using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using UnityEditor;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

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
            PlayerPrefs.DeleteKey("chapterFinishScreenBronze" + SaveLoadManager.LastChapterPlayed);
            PlayerPrefs.DeleteKey("chapterFinishScreenSilver" + SaveLoadManager.LastChapterPlayed);
            PlayerPrefs.DeleteKey("chapterFinishScreenGold" + SaveLoadManager.LastChapterPlayed);
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
        SetMenuEnvironment(SaveLoadManager.LastChapterPlayed);
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
            menuEnvironmentParent = Instantiate(chapterList[SaveLoadManager.LastChapterPlayed].MenuEnvironment);
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

    /*private void EnableMap(bool on)
    {
        /*if (on)
        {
            PlayEffect(peSnowThrow1, peSnowThrow1.transform.position, true);
            PlayEffect(peSnowThrow2, peSnowThrow2.transform.position, true);
        } else
        {
            CancelPe(peSnowThrow1);
            CancelPe(peSnowThrow2);
        }#1#
    }*/

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
            leanConstrainToBox.Target = chapterList[SaveLoadManager.LastChapterPlayed].MenuEnvironment.transform.Find(collisionName).GetComponent<BoxCollider>();
        }
        else if (collisionName == "CollisionMap")
        {
            leanConstrainToBox.Target = chapterList[SaveLoadManager.LastChapterPlayed].ChapterMap.transform.Find(collisionName).GetComponent<BoxCollider>();
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
        int chapter = -1;

        chapter = SaveLoadManager.LastChapterPlayed = n;
        
        SetMenuEnvironment(chapter);
        DisableMenuScreens();
        SetCollisionBox("CollisionMap");
        
        leanZoom.enabled = true;
        SetNavButtons(true);
        DisableMenuEnv();

        mapManager.enabled = true;
        
        backButton.gameObject.SetActive(true);
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
            if (SaveLoadManager.GetChapterUnlocked(i))
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
        SetMenuEnvironment(SaveLoadManager.LastChapterPlayed);
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
        var bronzeFinishScreen = PlayerPrefs.GetInt("chapterFinishScreenBronze" + SaveLoadManager.LastChapterPlayed, 0);
        var silverFinishScreen = PlayerPrefs.GetInt("chapterFinishScreenSilver" + SaveLoadManager.LastChapterPlayed, 0);
        var goldFinishScreen = PlayerPrefs.GetInt("chapterFinishScreenGold" + SaveLoadManager.LastChapterPlayed , 0);
        
        // Debug.Log("bronze fin screen: " + bronzeFinishScreen);
        // Debug.Log("silver fin screen: " + silverFinishScreen);
        // Debug.Log("gold fin screen: " + goldFinishScreen);
        
        // if all levels have been unlocked or at least bronze won, check for how many awards
        // what if the user gets 29 awards in bronze, silver and gold???
        if (SaveLoadManager.GetChapterAward(SaveLoadManager.LastChapterPlayed, SaveLoadManager.Awards.Bronze) == 29 && bronzeFinishScreen == 0)
        {
            Debug.Log("Enable Finish Screen");
            chapterFinishScreen.GetComponent<ChapterComplete>().TogglePopup();
            
            if (SaveLoadManager.LastChapterPlayed < chapterList.Count)
            {
                Debug.Log("CHAPTER " + (SaveLoadManager.LastChapterPlayed + 1) + " UNLOCKED!");
                SaveLoadManager.UnlockChapter(SaveLoadManager.LastChapterPlayed + 1); // [SaveLoadManager.LastChapterPlayed + 1]
                // popup for next chapter?
            }
            else
            {
                Debug.LogError("Anomaly detected with last chapter / allChapters");
            }
            PlayerPrefs.SetInt("chapterFinishScreenBronze" + SaveLoadManager.LastChapterPlayed, 1);
        }
        else if (SaveLoadManager.GetChapterAward(SaveLoadManager.LastChapterPlayed, SaveLoadManager.Awards.Silver) == 29 && silverFinishScreen == 0)
        {
            chapterFinishScreen.GetComponent<ChapterComplete>().TogglePopup();
            
            PlayerPrefs.SetInt("chapterFinishScreenSilver" + SaveLoadManager.LastChapterPlayed, 1);
        }
        else if (SaveLoadManager.GetChapterAward(SaveLoadManager.LastChapterPlayed, SaveLoadManager.Awards.Gold) == 29 && goldFinishScreen == 0)
        {
            chapterFinishScreen.GetComponent<ChapterComplete>().TogglePopup();

            PlayerPrefs.SetInt("chapterFinishScreenGold" + SaveLoadManager.LastChapterPlayed, 1);
        }
    }

    public void EnableGoldAwardsButton(bool state)
    {
        chapterFinishScreen.GetComponent<ChapterComplete>().UpdateButtonAward();
        chapterFinishScreen.SetActive(state);
        // chapterFinishScreen.GetComponent<ChapterComplete>().goldStarButton.SetActive(state);
    }
    
    // TODO - Finish reset saves
    public void ResetSaves()
    {
        screenDeleteSaveData.SetActive(false);
        
        SaveLoadManager.ResetSaves();

        SceneManager.LoadScene("CubeyGame");
    }
}
