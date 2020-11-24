using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;
using UnityEngine.Advertisements;


namespace Assets.Game.Scripts
{
    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance { get; set; }

        public VisualEffects visualEffects;
        public AudioManager audioManager;

        [Header("GameObjects")]
        public GameObject[] chapter_maps;
        public GameObject chapterScreen;
        public GameObject mainMenuUi;
        public GameObject environmentBg;
        public GameObject chapterButtons;
        public GameObject screenDeleteSaveData;

        [Header("Buttons")]
        public Button backButton;
        public Button startButton;

        [Header("Particle Effects")]
        public ParticleSystem peLvlStars;
        public ParticleSystem peSnowThrow1, peSnowThrow2;

        [Header("Other")]
        public int chapter;

        public Text startText;
        public Text versionNo;
        public Text goldWon;

        public bool mapActive;
        public bool[] chapterComplete = new bool[5];

        Color c1, c2, c2b, c3;
        //private Image image;

        [Header("StartButton Movement")]
        public Vector3 pos1 = new Vector3(0.95f, 0.95f, 0.95f);
        public Vector3 pos2 = new Vector3(1f, 1f, 1f);
        public Vector3 scale1 = new Vector3(0.95f, 0.95f, 0.95f);
        public Vector3 scale2 = new Vector3(0.95f, 0.95f, 0.95f);

        private GameObject cam;
        private LeanCameraZoomSmooth leanZoom;



        private Animator anim;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            cam = GameObject.Find("Camera").gameObject;
            leanZoom = cam.GetComponent<LeanCameraZoomSmooth>();

            if (!mapActive)
                leanZoom.enabled = false;
            else
                leanZoom.enabled = true;

            versionNo.text = "v: " + Application.version;

            ChapterSelectScreen(false);


            if (PlayerPrefs.HasKey("loadMap"))// && GameObject.Find("DontDestroy(Clone)") != null)
            {
                var n = PlayerPrefs.GetInt("loadMap");
                print("loading map after game");
                PlayerPrefs.DeleteKey("loadMap");
                ShowMap(n);

            }

            c1 = new Color(0, 0, 0, 0); // clear
            c2 = new Color(0.1f, 0.3f, 0.7f, 1); // blue
            c2b = new Color(0.2f, 0.4f, 0.9f, 1); // blue
            c3 = new Color(1, 1, 1, 1); // white

            //s1 = new Vector3(0.95f, 0.95f, 0.95f);
            //s2 = new Vector3(1f, 1f, 1f);

            anim = startButton.GetComponent<Animator>();
            ButtonSizePong();

        }

        //public void CheckAdLoad()
        //{
        //    if (PlayerPrefs.HasKey("levelsPlayed"))
        //    {
        //        var n = PlayerPrefs.GetInt("levelsPlayed");
        //        if (n == 5)
        //        {
        //            PlayerPrefs.SetInt("levelsPlayed", 0);
        //            ShowAd();
        //        }
        //    }
        //}

        public void ShowAd()
        {
            Advertisement.Show();
        }

        public void ButtonClose(GameObject go)
        {
            go.SetActive(false);

            if (go.name == "ScreenGoldAward")
            {
                PlayerPrefs.SetInt("goldScreenPlayed", 1);
                SaveLoadManager.Instance.goldScreenPlayed = true;
            }
        }

        private void EnabledisableSnowPe(bool on)
        {
            if (on)
            {
                PlayEffect(peSnowThrow1, peSnowThrow1.transform.position, true);
                PlayEffect(peSnowThrow2, peSnowThrow2.transform.position, true);
            } else
            {
                CancelPe(peSnowThrow1);
                CancelPe(peSnowThrow2);
            }
        }

        public void ShowMap(int n)
        {
            //n -= 1;
            DisableScreens();
            print("chapter map: " + n);
            for (int i = 0; i < chapter_maps.Length; i++)
            {
                chapter_maps[i].SetActive(false);
                if (n == i)
                {
                    leanZoom.enabled = true;
                    chapter_maps[i].SetActive(true);
                    mapActive = true;
                    chapter = i;

                    SaveLoadManager.Instance.AssignLevelButtonsToList();
                    peLvlStars.Play();
                }
            }

            switch (chapter)
            {
                case 0:
                    //if (SaveLoadManager.Instance.level == 0)
                    //    StoryManager.Instance.LoadStory();
                    var lastLevel = SaveLoadManager.Instance.levelButtons.Count;
                    print("lastLevel: " + (lastLevel-1));
                    if (SaveLoadManager.Instance.level == (lastLevel-1) && 
                        !SaveLoadManager.Instance.goldScreenPlayed)
                        StoryManager.Instance.LoadStoryEnd();
                    break;
                case 1:
                    //if (SaveLoadManager.Instance.level == 21 && chapterComplete[1])
                    //{
                    //    // && !SaveLoadManager.Instance.goldScreenPlayed)
                    //    SaveLoadManager.Instance.screenChapterComplete.SetActive(true);
                    //    chapterComplete[1] = false;
                    //}
                    // check if theres at least bronze in every level? Means completed

                    if (PlayerPrefs.HasKey("chapterComplete" + chapter))
                    {
                        var complete = PlayerPrefs.GetInt("chapterComplete" + chapter);
                        print("final level completed: " + complete);
                        if (complete == 1)
                        {
                            goldWon.text = SaveLoadManager.Instance.gold.ToString();
                            SaveLoadManager.Instance.LoadChapterComplete();
                            PlayerPrefs.DeleteKey("chapterComplete" + chapter);
                        }
                    }
                    print("No story");
                    break;
            }

            chapterButtons.SetActive(true);
            backButton.GetComponent<Button>().onClick.AddListener(delegate { ChapterSelectScreen(true); });

            // where is this set if level isn't 0?
            if (SaveLoadManager.Instance.level == 0)
                SaveLoadManager.Instance.SetCubeyPosition(SaveLoadManager.Instance.levelButtons[0].transform.position);
        }


        // 
        public void ChapterSelectScreen(bool enable)
        {
            // delete all starGrp
            //if (SaveLoadManager.Instance.levelButtons.Count > 0 && enable)
            if (enable)
            {
                print("deleting all stars and sweets from map");
                foreach (var starred in SaveLoadManager.Instance.levelButtons)
                {
                    var star = starred.transform.Find("StarsGrp(Clone)").gameObject;
                    var sweet = starred.transform.Find("LevelSweet(Clone)").gameObject;

                    if (star != null)
                        Destroy(star);
                    else
                        print("can't find star object");
                    if (sweet != null)
                        Destroy(sweet);
                    else
                       print("can't find sweet object");
                }
            }

            StoryManager.Instance.DisableStoryScreen();
            //audioManager.PlayAudio(audioManager.menuButtonForward);
            //EnabledisableSnowPe(enable);
            chapterScreen.SetActive(enable);
            chapterButtons.SetActive(enable);
            mainMenuUi.SetActive(!enable);
            environmentBg.SetActive(true);
            mapActive = false;
            //leanZoom.enabled = false;
            peLvlStars.Stop();
            DisableMaps();
            leanZoom.enabled = false;
            SaveLoadManager.Instance.levelButtons.Clear();
            SaveLoadManager.Instance.level = 0;
            SaveLoadManager.Instance.lastLevelVisited = 0;
            SaveLoadManager.Instance.levelUnlocked = 0;


            backButton.GetComponent<Button>().onClick.AddListener(delegate { MainMenuScreen(); });
        }

        private void DisableMaps()
        {
            for (int i = 0; i < chapter_maps.Length; i++)
                chapter_maps[i].SetActive(false);
        }

        public void DisableScreens()
        {
            chapterScreen.SetActive(false);
            mainMenuUi.SetActive(false);
            environmentBg.SetActive(false);
        }

        public void BackButton()
        {
            if (mapActive)
            {
                CameraManager.Instance.panToLevel = false;
                CameraManager.Instance.disableAutoPanMapCam = false;

                backButton.GetComponent<Button>().onClick.AddListener(delegate { ChapterSelectScreen(true); });
                //print("map screen back button");

            }
            else
            {
                CameraManager.Instance.panToLevel = false;
                CameraManager.Instance.disableAutoPanMapCam = false;
                backButton.GetComponent<Button>().onClick.AddListener(delegate { MainMenuScreen(); });
                //print("chapter screen back button");
            }
        }

        private void MainMenuScreen()
        {
            //EnabledisableSnowPe(true);
            environmentBg.SetActive(true);
            ChapterSelectScreen(false);
            CameraManager.Instance.ResetCamPosition();

        }

        private void FadeInStartButton()
        {
            startText.color = Color.Lerp(c2, c2b, Mathf.PingPong(Time.time, 1));
            //image.color = Color.Lerp(c1, c3, Mathf.PingPong(Time.time, 1));
        }

        private void ButtonSizePong()
        {
            StartCoroutine(WaitForPong());
        }

        private IEnumerator WaitForPong()
        {
            //while (startButton.transform.localScale.x != 1)
            //    startButton.transform.localScale = Vector3.Lerp(pos2, pos1, Mathf.Lerp(0, 1, 1)); //Mathf.PingPong(Time.time, 1));

            yield return new WaitForSeconds(2.5f);
            // switch off animation
            
            //anim.enabled = false;
            //anim.StopPlayback();
            anim.SetBool("StartButton_anim", false);
            anim.SetBool("EnablePingPong", true);
            //anim.Play("ButtonPingPong");
            // continue pulse animation
            //startButton.transform.localPosition = Vector3.Lerp(pos2, pos1, Mathf.PingPong(Time.time, 1));

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
}