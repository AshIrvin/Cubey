using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Lean.Touch;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Todo Split this up - its become too big
namespace Assets.Game.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; set; }

        #region Public

        [Header("Scripts")]
        public Scene MainMenu;
        public LeanForceRigidbody leanForceRb;
        public Timer timer;
        public PlayerLevelStats levelStats;
        public GameLevelPresets gameLevelPresets;
        public ParticleSystem pe;
        public AudioManager audioManager;

        [Header("Level Music")]
        public AudioSource levelMusic;

        [Header("Custom level to play")]
        public bool playSingleLevel = false;

        [Header("GameObjects")]
        public GameObject playerCube;
        public GameObject pauseMenu;
        public GameObject endScreen;
        public GameObject failedScreen;
        public GameObject deathWalls;
        public GameObject tutorial;
        public GameObject helpScreen;
        public GameObject pauseTutorial;
        public GameObject cubeyWings;
        public GameObject dontDestroy;
        public GameObject xagonBg;
        public GameObject treeRight;
        public GameObject levelsGrp;
        public GameObject levelOpen;
        public GameObject loadingScreen;
        public GameObject levels;
        public GameObject environment;
        public GameObject topUi;

        [HideInInspector]
        public Rigidbody playerRb;

        [Header("Bools")]
        public bool playerStuck;
        public bool camMovement;
        public bool allowFlight;
        public bool jumpCountReduces;
        public bool xagon;
        public bool allowMovement;
        public bool useTimer;
        public bool onBreakablePlatform;
        public bool onMovingPlatform;

        public bool forceJump;

        [Header("Text")]
        public Text levelText;
        public Text itemText;
        public Text jumpText, jumpsText;
        public Text youLasted;
        public Text tutorialText;
        public Text bestTime;

        [Header("ints")]
        public int levelNo;
        public int chapterAndLevelNo;
        public int savedLevel;
        public int itemCount;
        public int jumpCount, jumpsToStartWith = 10;
        public int time, countdown = 60;

        public int pickupsLeft, maxSweets;
        public int award;

        public float distanceFromPlayer;
        public float cubeyJumpHeight = 2.6f;

        [Header("Environment")]
        public GameObject env_Bushes;
        public GameObject env_Ground, env_BushFront;

        [Header("Animation")]
        public Animator starGold_anim;
        public Animator starSilver_anim;
        public Animator starBronze_anim;

        [Header("Star Colours")]
        public Color starGold = new Color(0.95f, 0.95f, 0, 1);
        public Color starSilver = new Color(1, 0.86f, 0, 1);
        public Color starBronze = new Color(1, 0.5f, 0, 1); //
        public Color starDefault = new Color(1, 1, 1, 0.3f);

        [Header("Other")]
        public Vector3 cubeyPosition;
        public float CubeyMagnitude;

        #endregion

        #region Private

        private ChaptersAndLevels chaptersAndLevels;
        private GameObject exitPrezzie;
        private GameObject exitObject;
        private string pickupName = " Pickups";
        private int stat_Jumps;
        private Vector3 flip;

        #endregion


        private void Awake()
        {
            Instance = this;
            levelStats = gameObject.GetComponent<PlayerLevelStats>();
            chaptersAndLevels = gameObject.GetComponent<ChaptersAndLevels>();

            if (gameLevelPresets == null)
                gameLevelPresets = GetComponent<GameLevelPresets>();
        }

        private void Start()
        {
            //SetupLevels();

#if UNITY_EDITOR
            if (playSingleLevel)
            {
                print("loading custom level");
                LoadCustomLevel();
            }
#endif
            if (!playSingleLevel)
                LoadLevel();

            timer = GetComponent<Timer>();

            deathWalls.SetActive(false);

            if (!playerCube.activeSelf)
            {
                Instantiate(playerCube);
                ResetPlayerCube();
            }
            else
            {
                ResetPlayerCube();
            }

            // change if using multiple players
            playerRb = playerCube.GetComponent<Rigidbody>();
            RestartTimer();

            pauseMenu.SetActive(false);
            EndScreen(false);
            FailedScreen(false);
            
            flip = playerCube.transform.localScale;

            Physics.gravity = new Vector3(0, -9.81f, 0);

            UpdateLevelText(levelNo);

            // Switch off uneeded PE
            //blowingLeaves.Stop();

            if (ChaptersAndLevels.chapterNo == 0)
            {
                pickupName = " Sweets";
                // setup game music
                levelMusic = GameObject.Find("XmasMusic").GetComponent<AudioSource>();
                audioManager.levelMusic = levelMusic;
            }
            else
            {
                pickupName = " Pickups";
                levelMusic = GameObject.Find("LevelMusic").GetComponent<AudioSource>();
                audioManager.levelMusic = levelMusic;
            }
            audioManager.menuMusic = null;

            itemText.text = pickupsLeft + pickupName + " left";
        }

        public void Update()
        {
            if (leanForceRb.canJump && CheckJumpMagnitude() && !forceJump)
            {
                PlayerAllowedJump(false);
            }

            if (onMovingPlatform && playerRb.velocity.magnitude < 1f)
            {
                PlayerAllowedJump(true);
            }
            else if (playerRb.velocity.magnitude < 0.1f)
                PlayerAllowedJump(true);

            CubeyMagnitude = playerRb.velocity.magnitude;

            CheckJumpCount();


            cubeyPosition = playerCube.transform.position;
        }

        private void SetupLevels()
        {
            // find Levels go
            // find active folder start with Chapter
            // get all go in folder and add to list

            var LevelsParent = GameObject.Find("Chapters").gameObject;

            if (LevelsParent != null)
            {
                print("levels parent found: " + LevelsParent.name);
                for (int i = 0; i < 6; i++)
                {
                    // search for each gameobject with Chapter in its name to add it to the List
                    if (LevelsParent.transform.Find("Chapter_" + i))
                    {
                        GameObject chapterFolders = LevelsParent.transform.Find("Chapter_" + i).gameObject;
                        ChaptersAndLevels.chaptersList.Add(chapterFolders);
                    }
                }
            }

            foreach (var c in ChaptersAndLevels.chaptersList)
            {
                c.SetActive(false);
            }

            print("chapter_n: " + PlayerPrefs.GetInt("chapter_n"));

            if (PlayerPrefs.HasKey("chapter_n") && !playSingleLevel){
                var chapter = PlayerPrefs.GetInt("chapter_n");
                ChaptersAndLevels.chaptersList[chapter].SetActive(true);
                ChaptersAndLevels.chapterNo = chapter;
            }

            // take the chapters assigned to the array
            for (int i = 0; i < ChaptersAndLevels.chaptersList.Count; i++)
            {
                // 
                if (ChaptersAndLevels.chaptersList[i].activeInHierarchy)
                {
                    levelsGrp = ChaptersAndLevels.chaptersList[i].transform.Find("Levels").gameObject;
                    var levelsCount = levelsGrp.transform.childCount;

                    print("loading levels into array. Length: " + levelsCount);

                    for (int j = 0; j < levelsCount; j++)
                    {
                        ChaptersAndLevels.levelsList.Add(levelsGrp.transform.GetChild(j).gameObject);
                    }
                }
            }

            if (PlayerPrefs.HasKey("levels_n"))
            {
                levelNo = PlayerPrefs.GetInt("levels_n");
                print("Enabling level: " + levelNo);
                ChaptersAndLevels.levelsList[levelNo].gameObject.SetActive(true);
            }
        }

        private bool GetSavedLevelFromChapter()
        {
            if (PlayerPrefs.HasKey("chapter_n") && PlayerPrefs.HasKey("level_n") && !playSingleLevel)
            {
                // used to display chapter and level number
                chapterAndLevelNo = int.Parse(PlayerPrefs.GetInt("chapter_n").ToString() + 
                    PlayerPrefs.GetInt("level_n").ToString("00")); // 01, 02 etc

                chapterAndLevelNo--;
                ChaptersAndLevels.chapterNo = PlayerPrefs.GetInt("chapter_n"); // get from save
                levelNo = PlayerPrefs.GetInt("level_n");
                levelNo--;
                return true;
            }
            return false;
        }

        // set in SaveGameManager. Finds level to load with game_levels
        private void LoadLevel()
        {
            allowFlight = false;

            // load level from main menu
            if (GetSavedLevelFromChapter())
            {
                print("Get pPref level_n to load: " + savedLevel+1);
                HideAllLevels();

                if (ChaptersAndLevels.levelsList.Count > 0)
                {
                    levelOpen = ChaptersAndLevels.levelsList[levelNo].gameObject;
                    levelOpen.SetActive(true);
                }
                else
                {
                    print("ERROR - No levels list");
                }

                RestartTimer();
                print("load presets for chapter: " + ChaptersAndLevels.chapterNo + ", level: " + levelNo);
                gameLevelPresets.LoadLevelPresets(ChaptersAndLevels.chapterNo, levelNo);
            } 
            else
            {
                levelNo = 0;
                print("No level info saved");
            }
        }

        private void LoadCustomLevel()
        {
            ChaptersAndLevels.chapterNo = chaptersAndLevels.customChapter;

            print("Custom level being loaded");

            for(int i = 0; i < ChaptersAndLevels.levelsList.Count; i++)
            {
                if (ChaptersAndLevels.levelsList[i].activeSelf)
                {
                    levelOpen = ChaptersAndLevels.levelsList[i].gameObject;
                    PlayerPrefs.SetInt("level_n", i+1);
                    levelNo = i;
                    print("custom level opened: " + i);
                }
            }
            gameLevelPresets.LoadLevelPresets(ChaptersAndLevels.chapterNo, levelNo);
        }

        public void LoadHelpScreen(bool on)
        {
            helpScreen.SetActive(on);
        }

        private bool CheckJumpMagnitude()
        {
            if (playerRb.velocity.magnitude > 0.1f)
                return true;
            return false;
        }


        private void CheckJumpCount() 
        {
            // checks when jumpcount has reached 0 and not on a breakable platform
            if (jumpCountReduces && jumpCount == 0 && leanForceRb.canJump && !onBreakablePlatform && !CheckJumpMagnitude())
            {
                FailedScreen(true);
            }
            else if (jumpCountReduces && jumpCount == 0 && leanForceRb.canJump && onBreakablePlatform)
                StartCoroutine(DelayFailedScreen());
            
        }

        private IEnumerator DelayFailedScreen()
        {
            yield return new WaitForSeconds(4);
            FailedScreen(true);
        }

        private void RestartTimer()
        {
            time = countdown;
            var startTimer = GetComponent<Timer>();
        }

        private void HideScreens()
        {
            PauseMenu(false);
            FailedScreen(false);
            EndScreen(false);
            UiManager.Instance.Tutorial(false, "", "");
        }

        public void RestartLevel()
        {
            HideScreens();
            LoadingScene(true);
            SceneManager.LoadScene("Game_Levels");
        }

        // Used as failed screen button
        public void LoadMainMenu()
        {
            PauseMenu(false);
            LoadingScene(true);
            PlayerPrefs.DeleteKey("loadMap");
            SceneManager.LoadScene("MainMenu");
        }

        // loads from exiting or timer ending
        public void LoadEndScreen(bool won)
        {
            UiManager.Instance.continueButton.SetActive(false);
            if (!won)
            {
                // Time hits 0 - did not finish
                FailedScreen(true);
                if (allowFlight)
                {
                    youLasted.text = "You lasted\n" + GetStopwatchCount() + " seconds!";
                    // load continue button
                    UiManager.Instance.continueButton.SetActive(true);
                }
            }
            else
            {
                // change end text to COMPLETED 
                PE_Stars();
                // play audio
                audioManager.PlayAudio(audioManager.cubeyCelebtration);

                EndScreen(true);
                ShowStarsAchieved();
            }
        }

        private int GetStopwatchCount()
        {
            var stopwatch = GetComponent<Timer>().stopwatch;
            return (int)stopwatch;
        }

        private void PE_Stars()
        {
            pe.Play();
        }

        private void ResetPlayerCube()
        {
            GetPlayerSpawn();
            playerRb = playerCube.gameObject.GetComponent<Rigidbody>();
            playerRb.freezeRotation = true;
            playerRb.velocity = new Vector3(0, 0, 0);
            playerRb.freezeRotation = false;
            playerRb.useGravity = true;

            playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;

            if (allowFlight)
            {
                playerRb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
                for(int i = 0; i < ScrollingHorizontalLevel.Instance.spawnedPlatforms.Count; i++)
                {
                    Destroy(ScrollingHorizontalLevel.Instance.spawnedPlatforms[i].gameObject);
                }
                ScrollingHorizontalLevel.Instance.spawnedPlatforms.Clear();
            }

            RestartTimer();
        }

        public void UpdateLevelText(int n)
        {
            levelText.text = "Level " + (n+1);
        }

        public void UpdateLevelString(string n)
        {
            levelText.text = n;
            print("l: " + n);
        }

        public void PickupCount(GameObject item)
        {
            //if (item.name == "Friend")
            //{
            //    itemCount++;
            //    itemText.text = itemCount + " Friends";
            //}

            if (item.name.Contains("Sweet") || item.name.Contains("SweetRain(Clone)") || item.name.Contains("Butterfly"))
            {
                itemCount--;
                pickupsLeft--;
                itemText.text = pickupsLeft + pickupName + " left";

                if (pickupsLeft <= 0)
                {
                    OpenExit();
                    itemText.text = "Go to Exit";
                }

            }
        }

        public void CountSweetsForLevel()
        {
            GameObject pickupGroup = levelOpen.transform.Find("Pickups").gameObject;

            for (int i = 0; i < pickupGroup.transform.childCount; i++)
            {
                // counts the pickups in the level
                pickupsLeft++;
            }

            if (!useTimer)
            {
                if (pickupsLeft == 1)
                {
                    itemText.text = pickupsLeft + pickupName + " \nleft";
                } else
                    itemText.text = pickupsLeft + pickupName + " \nleft";
            } else
            {
                itemText.text = pickupsLeft + pickupName + " \ncollected";
            }
        }

        // exit PE is set up in GameLevelPresets
        private void OpenExit()
        {
            // count sweets in level
            if (ChaptersAndLevels.chapterNo == 0)
            {
                audioManager.PlayAudio(audioManager.cubeyExitOpen);
                SetupExit(false, true);
            }
            else //if (ChaptersAndLevels.chapterNo >= 1)
            {
                audioManager.PlayAudio(audioManager.cubeyExitOpen);

                // find exit in level
                if (ChaptersAndLevels.levelsList[levelNo].transform.Find("Exit"))
                    exitObject = ChaptersAndLevels.levelsList[levelNo].transform.Find("Exit").transform.GetChild(0).gameObject;
                else
                    exitObject = ChaptersAndLevels.levelsList[levelNo].transform.Find("MovingExitPlatform").Find("Exit").transform.GetChild(0).gameObject;

                var exitPePos = exitObject.transform.position;

                // play pe explosion
                VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peExitExplosion, exitPePos);
                VisualEffects.Instance.peExitSwirl.SetActive(false);

                // enable and scale up exit to full size
                exitObject.SetActive(true);
            }
        }

        // used only for Xmas level
        public void SetupExit(bool closed, bool open)
        {
            // cant find this in editor mode. Missing level no
            print("levelNo exit to unwrap: " + levelNo + ", folder name: " + ChaptersAndLevels.levelsList[levelNo].name);
            
            exitPrezzie = ChaptersAndLevels.levelsList[levelNo].transform.Find("XmasExit").transform.GetChild(0).gameObject;

            var prezzieClosed = exitPrezzie.transform.GetChild(0).gameObject;
            var prezzieFlat = exitPrezzie.transform.GetChild(1).GetComponent<SpriteRenderer>();
            var prezzieCollision = exitPrezzie.transform.GetChild(2).GetComponent<BoxCollider>();

            prezzieClosed.gameObject.SetActive(closed);
            prezzieCollision.enabled = closed;

            prezzieFlat.enabled = open;

            var newPePos = prezzieFlat.transform.position;
            newPePos.y += 2f;

            if (prezzieFlat.enabled)
                VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peExitOpened, newPePos);
        }

        private void HideAllLevels()
        {
            if (ChaptersAndLevels.levelsList.Count > 0)
            {
                foreach (GameObject go in ChaptersAndLevels.levelsList)
                    go.SetActive(false);
            }
        }

        public void PlayerAllowedJump(bool jump)
        {
            leanForceRb.canJump = jump;
        }

        public void PlayerVelocity(float n)
        {
            leanForceRb.VelocityMultiplier = n;
        }

        private void GetPlayerSpawn()
        {
            playerCube.transform.SetParent(null);

            for (int i = 0; i < ChaptersAndLevels.levelsList.Count; i++)
            {
                if (ChaptersAndLevels.levelsList[i].activeSelf)
                {
                    Transform go = ChaptersAndLevels.levelsList[i].transform.Find("StartPosition");
                    playerCube.gameObject.transform.SetPositionAndRotation(go.position, new Quaternion(0, 0, 0, 0));
                }
            }
        }

        /// <summary>
        /// LOAD MAP AFTER EACH LEVEL
        /// </summary>
        private void LoadMapInMenu()
        {
            // Set map depending on chapterNothat player is on
            PlayerPrefs.SetInt("loadMap", ChaptersAndLevels.chapterNo);
            PlayerPrefs.SetInt("lastLevel", levelNo);

            SceneManager.LoadScene("MainMenu");
        }

        private int StarsGiven()
        {
            var jumps = jumpsToStartWith - jumpCount;

            print("jumps: " + jumps + ", jumpsToStartWith: " + jumpsToStartWith);

            if (jumps <= gameLevelPresets.gold)
                return 3;
            else if (jumps <= gameLevelPresets.silver)
                return 2;
            else if (jumps <= gameLevelPresets.bronze)
                return 1;

            return 0;
        }

        // Shows stars on finished screen
        private void ShowStarsAchieved()
        {
            var s = StarsGiven();
            award = s;

            var sGrp = endScreen.transform.Find("StarsGrp");
            List<Image> starImages = new List<Image>();

            for (int i = 0; i < 3; i++)
            {
                starImages.Add(sGrp.transform.GetChild(i).GetComponent<Image>());
                starImages[i].color = starDefault;
            }

            switch (s)
            {
                case 1:
                    starImages[0].color = starBronze;

                    starBronze_anim.Play("StarBronze", 0);
                    starBronze_anim.speed = 1;

                    starSilver_anim.Play("StarSilver", 0, 0f);
                    starSilver_anim.speed = 0;

                    starGold_anim.Play("StarGold", 0, 0.4f);
                    starGold_anim.speed = 0;

                    starImages[2].color = starDefault;
                    break;
                case 2:
                    starImages[0].color = starBronze;
                    starImages[1].color = starSilver;

                    starBronze_anim.Play("StarBronze", 0);
                    starBronze_anim.speed = 0;

                    starSilver_anim.Play("StarSilver", 0, 0f);
                    starSilver_anim.speed = 1;

                    starGold_anim.Play("StarGold", 0, 0.4f);
                    starGold_anim.speed = 0;
                    starImages[2].color = starDefault;
                    break;
                case 3:
                    starImages[0].color = starBronze;
                    starImages[1].color = starSilver;
                    starImages[2].color = starGold;

                    starBronze_anim.speed = 0;
                    starSilver_anim.speed = 0;
                    starGold_anim.speed = 1;
                    break;
            }
        }

        private int GetFriendsFound()
        {
            return itemCount;
        }

        private void SaveLevelStats()
        {
            levelNo--;
            var time_ = timer.countdown;
            var star = StarsGiven();
            var items = GetFriendsFound();
            var jumped = jumpCount;
            var restarted = 0;
            var score = award;
            print("award: " + award);

            levelStats.SaveLevel(levelNo, time_, star, items, jumped, restarted, score);
        }

        private void LoadingScene(bool on)
        {
            topUi.SetActive(false);
            if (loadingScreen != null)
            {
                environment.SetActive(!on);
                loadingScreen.SetActive(on);
            }
        }

        // loads from end screen continue button
        public void NextLevel()
        {
            LoadingScene(true);
            allowFlight = false;
            HideAllLevels();

            levelNo = int.Parse(levelNo.ToString("00"));

            print("current chapter: " + ChaptersAndLevels.chapterNo + ", current levelNo: " + levelNo + ", levelList count: " + ChaptersAndLevels.levelsList.Count);

            if (levelNo++ < ChaptersAndLevels.levelsList.Count)
            {
                SaveLevelStats();
                levelNo++;
                SaveLevel();
            }

            // checks level against last one of the chapter
            if (levelNo == ChaptersAndLevels.levelsList.Count)
            {
                print("final chapterNoset");
                PlayerPrefs.SetInt("chapterComplete" + ChaptersAndLevels.chapterNo, 1);
            }

            ResetPlayerCube();
            EndScreen(false);
            FailedScreen(false);

            LoadMapInMenu();
        }

        private void SaveLevel()
        {
            var chapterLevel = int.Parse(ChaptersAndLevels.chapterNo.ToString("0") + levelNo.ToString("00"));

            // is it more than the savedLevel?
            // is the savedLevel showing up as another chapter?
            // need to get savedLevel for this chapter
            if (levelNo > GetSavedLevel())
                PlayerPrefs.SetInt(ChaptersAndLevels.chapterNo + "level", chapterLevel);

            print("SaveLevel: Chapter+Level: " + chapterLevel);
        }

        // When leaving game level
        private int GetSavedLevel()
        {
            // saving correct 3 digit number
            if (PlayerPrefs.HasKey(ChaptersAndLevels.chapterNo + "level"))
            {
                var chapterLevel = PlayerPrefs.GetInt(ChaptersAndLevels.chapterNo + "level"); // returns 3
                print("1. chapter and level saved previously: " + savedLevel);
                
                //savedLevel = chapterLevel;
                savedLevel = SaveLoadManager.Instance.GetLevelInChapter(chapterLevel); // returns 2 digits

                print("2. level saved previously: " + savedLevel);

                return savedLevel;
            }
            else
            {
                print("no saved level found");
            }
            return 0;
        }

        private void PauseMenu(bool on)
        {
            //audioManager.MuteAudio(audioManager.levelMusic, on);
            pauseMenu.SetActive(on);
            Time.timeScale = on ? 0f : 1f;
        }

        private void EndScreen(bool on)
        {
            endScreen.SetActive(on);
            Time.timeScale = on ? 0f : 1f;
        }

        public void FailedScreen(bool on)
        {
            failedScreen.SetActive(on);
            Time.timeScale = on ? 0f : 1f;
        }

        public void PlayerJumped()
        {
            audioManager.PlayAudio(audioManager.cubeyJump);

            if (jumpCountReduces)
            {
                if (jumpCount == 0)
                    jumpCount = 0;
                else
                    jumpCount--;
            }
            else
            {
                jumpCount++;
            }

            jumpText.text = jumpCount.ToString();
        }

        public void AddJump()
        {
            jumpCount++;
            jumpText.text = jumpCount.ToString();
        }

        public void PlayerFaceDirection(bool right)
        {
            flip.x = -1;

            if (right)
                flip.x = 1;

            flip.y = 1;
            flip.z = 1;

            playerRb.gameObject.transform.localScale = flip;
        }

        private void Stats_Load()
        {
            if (PlayerPrefs.HasKey("stat_Jumps"))
            {
                stat_Jumps = PlayerPrefs.GetInt("stat_Jumps");
            }
        }

        private void Stats_Save()
        {
            if (jumpCount > stat_Jumps)
            {
                PlayerPrefs.SetInt("stat_Jumps", stat_Jumps);
            }
        }

        public void HideGameObject(GameObject go)
        {
            StartCoroutine(HideObject(go));
        }

        private IEnumerator HideObject(GameObject go)
        {
            yield return new WaitForSeconds(3);
        }
    }
}