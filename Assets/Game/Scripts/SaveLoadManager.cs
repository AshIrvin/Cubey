using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BayatGames.SaveGamePro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;

namespace Assets.Game.Scripts
{
    public class SaveLoadManager : MonoBehaviour
    {
        #region Public Variables

        public static SaveLoadManager Instance { get; set; }

        public PlayerLevelStats levelStats;
        public AudioManager audioManager;

        [Header("Levels")]
        public int level;
        public int lastLevelVisited, levelUnlocked, levelsPlayed;

        [Header("Buttons")]
        public List<GameObject> levelButtons = new List<GameObject>();
        public List<GameObject> chapterButtons = new List<GameObject>();
        public Button storyButtonEnd;
        public Button buttonGold;

        [Header("Lists")]
        public List<GameObject> listOfMaps = new List<GameObject>();
        public GameObject[] stars;
        public Image[] lock_img;
        public List<int> chapterLevelSaved;

        [Header("Map")]
        public GameObject chapterMap;

        public GameObject starGrp;
        public GameObject levelSweet, cubey, levelButterfly, levelPickup;

        [Header("Screens")]
        public GameObject loadingScreen;
        public GameObject screenGoldAward;
        public GameObject screenChapterComplete;

        [Header("Lock Buttons for Editor")]
        public GameObject buttonLockAll, buttonUnlockAll;

        [Header("Buttons")]


        [Header("Star Colours")]
        public Vector3 lastLevelButtonPos;
        public Vector3 nextLevel;

        [Header("Star Colours")]
        public Color starGold = new Color(0.95f, 0.95f, 0, 1);
        public Color starSilver = new Color(1, 0.86f, 0, 1);
        public Color starBronze = new Color(1, 0.5f, 0, 1); //
        public Color starDefault = new Color(1, 1, 1, 0.3f);

        public Vector3 cubeyOffset = new Vector3(0, -1.8f, 0);

        [Header("ints")]
        public int gold;
        public int silver, bronze;
        public bool goldScreenPlayed;

        #endregion

        #region Private Variables

        private int totalLevels;
        
        private Vector3 s1 = new Vector3(0.9f, 0.9f, 0.9f);
        private Vector3 s2 = new Vector3(1f, 1f, 1f);
        private Animator starAnim;
        private GameObject lockImg;
        private GameObject mapPickup;
        #endregion


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            screenChapterComplete.SetActive(false);

            Time.timeScale = 1f;

            if (loadingScreen.activeInHierarchy)
                LoadingScreen(false);

#if UNITY_EDITOR
            buttonLockAll.SetActive(true);
            buttonUnlockAll.SetActive(true);
#elif !UNITY_EDITOR
            buttonLockAll.SetActive(false);
            buttonUnlockAll.SetActive(false);
#endif

            //SetButtonColour(buttonGold);

            if (PlayerPrefs.HasKey("goldScreenPlayed"))
            {
                var g = PlayerPrefs.GetInt("goldScreenPlayed");
                if (g == 1)
                    goldScreenPlayed = true;
            }

            totalLevels = levelButtons.Count;

            for (int i = 0; i < chapterButtons.Count; i++)
            {
                if (PlayerPrefs.HasKey(i + "level"))
                    chapterLevelSaved.Add(PlayerPrefs.GetInt(i + "level"));
            }
        }

        public void LoadChapterComplete()
        {
            CloseWindow(screenChapterComplete);
        }

        public void CloseWindow(GameObject go)
        {
            go.SetActive(!go.activeInHierarchy);
        }

        private void CheckLastLevelVisited()
        {
            var chapter = MainMenuManager.Instance.chapter;
            if (PlayerPrefs.HasKey(chapter + "lastLevelVisited"))
            {
                lastLevelVisited = PlayerPrefs.GetInt(chapter + "lastLevelVisited");

                var newLevel = GetLevelInChapter(lastLevelVisited);

                print("singleLevel: " + newLevel + ", lastLevelVisited: " + lastLevelVisited);
                //var newLevel = singleLevel;
                newLevel++;

                lastLevelButtonPos = levelButtons[lastLevelVisited-1].transform.position;
                if (newLevel < levelButtons.Count)
                    nextLevel = levelButtons[newLevel].transform.position;

                SetCubeyPosition(lastLevelButtonPos);
            }
        }

        // comes from level buttons on map
        public void GetLevelNoToLoad()
        {
            var levelButtonClicked = EventSystem.current.currentSelectedGameObject.gameObject.transform.Find("LevelText_no").GetComponent<Text>().text.ToString();
            var chapter = MainMenuManager.Instance.chapter;

            int levelToLoad;
            if (int.TryParse(levelButtonClicked, out levelToLoad))
            {
                levelToLoad += chapter * 100;
                //levelToLoad = int.Parse(chapter.ToString("0")) + int.Parse();
            }

            print("Get Level No To Load: " + levelToLoad);

            LoadLevel(levelToLoad);
        }

        private void AssignPickup(int n)
        {
            if (n == 0)
            {
                levelPickup = levelSweet;
            } else
            {
                levelPickup = levelButterfly;
            }
            print("level pickup set to: " + n);
        }

        public void AssignLevelButtonsToList()
        {
            // take the chapters assigned to the array
            if (levelButtons.Count == 0)
            {
                for (int i = 0; i < listOfMaps.Count; i++)
                {
                    // 
                    if (listOfMaps[i].activeInHierarchy)
                    {
                        chapterMap = listOfMaps[i].transform.Find("Canvas_Map").transform.GetChild(1).transform.GetChild(0).gameObject;

                        var length = chapterMap.transform.childCount;

                        print("loading levels into array. Length: " + length);

                        for (int j = 0; j < length; j++)
                        {
                            var level = chapterMap.transform.GetChild(j).name.Contains("Level");
                            if (level)
                            {
                                var levelText = chapterMap.transform.GetChild(j).transform.GetChild(2).GetComponent<Text>();
                                levelText.text = j.ToString();
                                levelButtons.Add(chapterMap.transform.GetChild(j).gameObject);
                            }
                        }
                    }
                }
            }

            GetLevel();
            CheckLevelUnlocks();

            SetStarsToMapButtons();

            // before this, level buttons needs added to a list
            SetStarsForEachLevel();
        }

        // placement of stars
        private void SetStarsToMapButtons()
        {
            var t = new Vector3(0, -180f, 0);
            var s = new Vector3(1.3f, 1.3f, 1.3f);
            var lvlSweet = new Vector3(-100, 179, 0);

            if(MainMenuManager.Instance.chapter == 0)
                AssignPickup(0);
            else
                AssignPickup(1);


            print("Assigning stars to map buttons");

            foreach (var starred in levelButtons)
            {
                bool found = (starred.transform.Find("StarsGrp(Clone)") != null ||
                         starred.transform.Find("LevelSweet(Clone)") != null);

                if (!found)
                {
                    var starGameObject = Instantiate(starGrp, starred.transform);
                    var sweetGameObject = Instantiate(levelPickup, starred.transform);

                    starGameObject.transform.localPosition = t;
                    starGameObject.transform.localScale = s;

                    sweetGameObject.transform.localPosition = lvlSweet;
                }

            }

            cubey = chapterMap.transform.Find("CubeyOnMap").gameObject;

        }

        // Set STARS for each level button
        private void SetStarsForEachLevel()
        {
            storyButtonEnd.interactable = false;
            storyButtonEnd.gameObject.SetActive(false);

            for (int i = 0; i < levelButtons.Count; i++)
            {
                var b = levelButtons[i].GetComponent<Button>();
                var sGrp = levelButtons[i].transform.Find("StarsGrp(Clone)");

                if (MainMenuManager.Instance.chapter == 0)
                {
                    mapPickup = levelButtons[i].transform.Find("LevelSweet(Clone)").gameObject;
                }
                else
                    mapPickup = levelButtons[i].transform.Find("LevelButterfly(Clone)").gameObject;

                if (b.interactable && i < level)
                {
                    mapPickup.SetActive(false);
                }
                else
                {
                    mapPickup.SetActive(true);
                }


                // load level stats
                levelStats.LoadLevelStats(i);

                // set score with stars
                var s = levelStats.Score;
                CheckAwards(s);

                List<SpriteRenderer> starImages = new List<SpriteRenderer>();

                for (int j = 0; j < 3; j++)
                {
                    starImages.Add(sGrp.transform.GetChild(j).GetComponent<SpriteRenderer>());
                    starImages[j].color = starDefault;
                }

                switch (s)
                {
                    case 1:
                        starImages[0].color = starBronze;
                        break;
                    case 2:
                        starImages[0].color = starBronze;
                        starImages[1].color = starSilver;
                        break;
                    case 3:
                        starImages[0].color = starBronze;
                        starImages[1].color = starSilver;
                        starImages[2].color = starGold;
                        if (level == i)
                            starImages[2].transform.localScale = Vector3.Lerp(s1, s2, Mathf.PingPong(Time.time, 1));
                        break;
                    default:
                        break;
                }
            }

            if (MainMenuManager.Instance.chapter == 0)
            {
                if (levelButtons[20].GetComponent<Button>().interactable)
                {
                    storyButtonEnd.gameObject.SetActive(true);
                    storyButtonEnd.interactable = true;
                }
            }

            CheckLastLevelVisited();
        }

        public void EnableBonusLevel(Button button)
        {
            // saved after end story
            if (PlayerPrefs.HasKey("goldScreenPlayed"))
            {
                var g = PlayerPrefs.GetInt("goldScreenPlayed");
                if (g == 1)
                    goldScreenPlayed = true;
            }

            if (gold >= totalLevels - 2 && !goldScreenPlayed)
            {
                screenGoldAward.SetActive(true);
                button.interactable = true;
                goldScreenPlayed = true;
                lastLevelButtonPos = buttonGold.transform.position;
                SetCubeyPosition(lastLevelButtonPos);

                SetButtonColour(button);

                PlayerPrefs.SetInt("goldScreenPlayed", 1);
            } else if (gold >= totalLevels - 2 && goldScreenPlayed)
            {
                button.interactable = true;
                SetButtonColour(button);

                lastLevelButtonPos = buttonGold.transform.position;
                SetCubeyPosition(lastLevelButtonPos);
            }
            else
            {
                button.interactable = false;
                screenGoldAward.SetActive(false);
                CheckLastLevelVisited();
            }

        }

        private void SetButtonColour(Button button)
        {
            Image image = button.GetComponent<Image>();
            var screenShot = button.transform.Find("Mask").transform.Find("Screenshot").gameObject;
            Image screen = screenShot.GetComponent<Image>();
            var colour = screen.color;
            colour = Color.white;
            screen.color = colour;
        }

        public void SetCubeyPosition(Vector3 pos)
        {
            print("Cubey set to: " + pos + ", level: " + level);

            cubeyOffset = new Vector3(1.2f, -1.6f, 0);

            cubey.transform.position = pos + cubeyOffset;
        }

        private void CheckAwards(int s)
        {
            if (s == 1)
                bronze += 1;
            else if (s == 2)
                silver += 1;
            else if (s == 3)
                gold += 1;
        }

        private void LoadingScreen(bool on)
        {
            MainMenuManager.Instance.chapterButtons.SetActive(false);
            if (loadingScreen != null)
                loadingScreen.SetActive(on);
        }

        private void CheckLevelsPlayed()
        {
            if (PlayerPrefs.HasKey("levelsPlayed"))
            {
                var n = PlayerPrefs.GetInt("levelsPlayed");
                levelsPlayed += n;
                levelsPlayed++;
                PlayerPrefs.SetInt("levelsPlayed", levelsPlayed);

                if (n == 5)
                {
                    PlayerPrefs.SetInt("levelsPlayed", 0);
                    MainMenuManager.Instance.ShowAd();
                }
            }
            else
                PlayerPrefs.SetInt("levelsPlayed", 1);



            //MainMenuManager.Instance.CheckAdLoad();
        }

        // comes from map buttons
        public void LoadLevel(int level_n)
        {
            LoadingScreen(true);

            CheckLevelsPlayed(); // for Ads

            if (audioManager != null && audioManager.allowSounds)
                audioManager.PlayMusic(audioManager.menuStartLevel);

            // all needed?
            var s = level_n.ToString();

            var chapterN = 0;
            var levelN = 0;
            var levelS = "";
            
            if (s.Length > 2)
            {
                chapterN = int.Parse(s[0].ToString());
                print(s.Length + " chapterN: " + chapterN);
                //if (chapterN > 0)
                levelS = s[1].ToString() + s[2].ToString();
                levelN = int.Parse(levelS);
            } else
            {
                // if only 1, 2 digits return, it's chapter 0
                levelN = level_n;
                //chapterN = 0;
            }

            print("1. Save to pPrefs. From button: " + level_n + ". Set chapter_n: " + chapterN + ", level_n: " + levelN);

            PlayerPrefs.SetInt("chapter_n", chapterN);
            PlayerPrefs.SetInt("level_n", levelN);

            // this only gets set once
            PlayerPrefs.SetInt(chapterN + "lastLevelVisited", levelN);
            print("lastLevelVisited: " + lastLevelVisited);

            SceneManager.LoadScene("Game_Levels");
        }

        // Get level from save
        private void GetLevel()
        {
            if (levelButtons.Count > 0)
            {
                var chapter = MainMenuManager.Instance.chapter;

                if (PlayerPrefs.HasKey(chapter + "level"))
                {
                    level = PlayerPrefs.GetInt(chapter + "level");
                    print("1. Get level: " + level);

                    var newLevel = GetLevelInChapter(level);
                    //newLevel++;

                    // to show public only?
                    if (newLevel < levelButtons.Count)
                        nextLevel = levelButtons[newLevel-1].transform.position;

                    lastLevelButtonPos = levelButtons[lastLevelVisited].transform.position;

                    // this has been -1
                    level = newLevel;
                }
                else
                {
                    print("No save found");
                    level = 0;
                    lastLevelButtonPos = levelButtons[level].transform.position;
                }
            }
            else
            {
                print("No map level buttons found");
            }

            print("2. get level: " + level);
        }

        public int GetLevelInChapter(int level)
        {
            int singleLevel = 0;
            print("get level in chapter: " + level);
            if (level >= 0)
            {
                // convert back to single level form
                string s = level.ToString("000");
                if (s.Length == 3)
                {
                    string lvl = s[1] + s[2].ToString();
                    singleLevel = int.Parse(lvl);
                }
                else if (s.Length == 4)
                {
                    string lvl = s[2] + s[3].ToString();
                    singleLevel = int.Parse(lvl);
                }
                else
                {
                    var c = MainMenuManager.Instance.chapter;
                    s = c + level.ToString("000");

                    string lvl = s[1] + s[2].ToString();
                    singleLevel = int.Parse(lvl);
                }
                print("return single level: " + singleLevel);
            }
            //else
            //    singleLevel = 00;

            return singleLevel;
        }

        public void UnlockNLevels(int n)
        {
            level = n;
            print("unlock n levels: " + level);
            PlayerPrefs.SetInt(MainMenuManager.Instance.chapter + "level", level);

            CheckLevelUnlocks();
            SetUnlockGold(level);

            SetCubeyPosition(levelButtons[n].transform.position);
        }

        public void UnlockAllLevels()
        {
            level = levelButtons.Count; // 3
            print("unlock All levels: " + level);
            PlayerPrefs.SetInt("level", level);

            print("levels unlocked: " + level);

            CheckLevelUnlocks();
            //CheckChapterUnlocks();
            SetUnlockGold(level);
        }

        private void SetUnlockGold(int count)
        {
            for (int i = 0; i < count+1; i++)
            {
                CheckAwards(3);
            }
        }

        // comes from Lock all levels chapter button
        public void LockAllLevels()
        {
            SaveGame.Clear();
            gold = 0;

            for (int i = 0; i < 10; i++)
            {
                PlayerPrefs.DeleteKey(i + "level");
                PlayerPrefs.DeleteKey(i + "lastLevelVisited");
            }

            CheckLevelUnlocks();
            PlayerPrefs.DeleteKey("loadMap");

            // reset cubey to 1st level
            if (levelButtons.Count > 0)
                SetCubeyPosition(levelButtons[0].transform.position);
        }

        void CheckChapterUnlocks()
        {
            CycleButtonLocks(chapterButtons);
        }

        void CheckLevelUnlocks() 
        {
            CycleButtonLocks(levelButtons);
        }

        // does it get used?
        /*
        public void SaveLevel(int n)
        {
            GetLevel();

            print("aa SAVE LEVEL: " + n);

            // check chapter
            for (int i = 0; i < 5; i++)
            {
                if (MainMenuManager.Instance.chapter == i)
                {
                    var checkLevel = i + level.ToString("00");

                    if (n > level)
                    {
                        int newLevel;
                        var c = MainMenuManager.Instance.chapter;
                        print("chapter: " + MainMenuManager.Instance.chapter + ", level: " + level);

                        var stringLevel = c.ToString() + level.ToString("00");
                        if (int.TryParse(stringLevel, out newLevel))
                            n = newLevel;

                        PlayerPrefs.SetInt("level", n);
                        print("Level " + n + " saved");
                    } else
                        print("new level save error. n:" + n + ", level:" + level);
                }
            }


        }
        */

        // Check which levels are unlocked inside the chapter
        void CycleButtonLocks(List<GameObject> buttons)
        {
            // get correct saved level
            GetLevel();

            print("Unlock to latest Saved level: " + level);

            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i].GetComponent<Button>();

                lockImg = button.gameObject.transform.Find("Lock_image").gameObject;

                var screenShot = button.transform.Find("Mask").transform.Find("Screenshot").gameObject;

                lockImg.gameObject.SetActive(false);

                // check against saved level and unlock the next level
                button.interactable = i < (level + 1);

                // save last level unlocked?
                if (button.interactable)
                    levelUnlocked = level;

                totalLevels = levelButtons.Count - 1;

                if (lock_img.Length > 0)
                {
                    Image img = lock_img[i].GetComponent<Image>();
                }

                Image screen = screenShot.GetComponent<Image>();
                var colour = screen.color;
                colour = Color.white;
                screen.color = colour;

                if (!button.interactable)
                {
                    colour.a = 0.3f;
                } else
                {
                    colour.a = 1f;
                }
                screen.color = colour;
            }
        }
    }
}