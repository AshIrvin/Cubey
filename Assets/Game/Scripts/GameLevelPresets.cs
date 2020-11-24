using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Game.Scripts
{
    public class GameLevelPresets : MonoBehaviour
    {

        #region Public

        public GameManager gameManager;

        public int gold;
        public int silver;
        public int bronze;

        #endregion

        #region Private

        [SerializeField]
        [Tooltip("Finds Exit for level")]
        private GameObject exitObject;
        
        #endregion

        private void Start()
        {
            if (gameManager == null)
                gameManager = GetComponent<GameManager>();
        }

        private void LevelTimer(int n)
        {
            gameManager.useTimer = true;
            gameManager.timer.enableTimer = true;
            gameManager.timer.StartTimer(n);
            gameManager.jumpsText.text = "Seconds";
            gameManager.jumpText.gameObject.SetActive(false);
        }

        public void LoadLevelPresets(int c, int n)
        {
            gameManager.levelStats.LoadLevelStats(gameManager.levelNo);
            gameManager.camMovement = true;
            gameManager.leanForceRb.VelocityMultiplier = gameManager.cubeyJumpHeight;
            Physics.gravity = new Vector3(0, -9.81f, 0);
            gameManager.cubeyWings.SetActive(false);
            gameManager.deathWalls.SetActive(false);
            gameManager.allowFlight = false;
            VisualEffects.Instance.pePowerJump.gameObject.SetActive(true);
            gameManager.jumpCountReduces = true;
            gameManager.jumpCount = 10;
            gameManager.jumpText.text = gameManager.jumpCount.ToString();

            // set text back to jumps
            gameManager.jumpsText.text = "Jumps";
            gameManager.useTimer = false;
            gameManager.timer.enableTimer = false;
            gameManager.timer.timerText.text = "";

            gameManager.CountSweetsForLevel();
            VisualEffects.Instance.peExitSwirl.SetActive(false);

            // Christmas chapter
            if (c == 0)
            {
                VisualEffects.Instance.PlayEffectOverScreen(VisualEffects.Instance.peSnow);
                VisualEffects.Instance.PlayEffectOverScreen(VisualEffects.Instance.peSnowClose);

                gameManager.SetupExit(true, false);

                switch (n)
                {
                    case 0:
                        gold = 1;
                        silver = 2;
                        bronze = 10;
                        break;
                    case 1:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 2:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 3:
                        gold = 5;
                        silver = 6;
                        bronze = 10;
                        break;
                    case 4:
                        gold = 6;
                        silver = 7;
                        bronze = 10;
                        break;
                    case 5:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 6:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 7:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 8:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 9:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 10:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 11:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 12:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 13:
                        gold = 5;
                        silver = 6;
                        bronze = 10;
                        break;
                    case 14:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 15:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 16:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 17:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 18:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 19:
                        gold = 6;
                        silver = 7;
                        bronze = 10;
                        break;
                    case 20:
                        // enable timer - bonus level
                        LevelTimer(60);
                        gameManager.pickupsLeft = 50;
                        gameManager.itemText.text = gameManager.pickupsLeft + " Sweets left";
                        break;
                    case 21:
                        gold = 7;
                        silver = 8;
                        bronze = 10;
                        break;
                }
            }
            // Forest chapter
            else if (c == 1)
            {
                VisualEffects.Instance.PlayEffectOverScreen(VisualEffects.Instance.peLeaves);

                if (gameManager.xagon)
                    gameManager.xagonBg.SetActive(true);
                gameManager.treeRight.SetActive(true);

                BasicLevelSetup(c, n);

                switch (n)
                {
                    case 0:
                        //var ln = gameManager.levelNo + 1;
                        //UiManager.Instance.Tutorial(true, "Level " + ln + "!", "Welcome to\nCubey!");
                        UiManager.Instance.tutorialSketchImage.SetActive(true);
                        gold = 1;
                        silver = 2;
                        bronze = 10;
                        break;
                    case 1:
                        UiManager.Instance.TutorialPause(true);
                        gold = 1;
                        silver = 2;
                        bronze = 10;
                        break;
                    case 2:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 3:
                        gold = 3;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 4:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 5: //6
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 6:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 7: // 8
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 8:
                        gold = 5;
                        silver = 6;
                        bronze = 10;
                        break;
                    case 9: // 10
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 10:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 11: //12
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 12:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 13:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 14:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 15:
                        gold = 6;
                        silver = 7;
                        bronze = 10;
                        break;
                    case 16: //17
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 17:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 18:
                        gold = 6;
                        silver = 7;
                        bronze = 10;
                        break;
                    case 19:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 20:
                        gold = 7;
                        silver = 8;
                        bronze = 10;
                        break;
                    case 21:
                        LevelTimer(99);
                        UiManager.Instance.tutorialBonusSketch.SetActive(true);
                        gameManager.UpdateLevelString("Boss\nlevel!");
                        gameManager.xagonBg.SetActive(false);
                        gameManager.treeRight.SetActive(false);

                        break;

                }
            }
            // Beach chapter
            else if (c == 2)
            {
                BasicLevelSetup(c, n);

                switch (n)
                {
                    case 0:
                        //var ln = gameManager.levelNo + 1;
                        //UiManager.Instance.Tutorial(true, "Level " + ln + "!", "Welcome to\nCubey!");
                        UiManager.Instance.tutorialSketchImage.SetActive(true);
                        gold = 1;
                        silver = 2;
                        bronze = 10;
                        break;
                    case 1:
                        UiManager.Instance.TutorialPause(true);
                        gold = 1;
                        silver = 2;
                        bronze = 10;
                        break;
                    case 2:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 3:
                        gold = 3;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 4:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 5: //6
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 6:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 7: // 8
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 8:
                        gold = 5;
                        silver = 6;
                        bronze = 10;
                        break;
                    case 9: // 10
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 10:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 11: //12
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 12:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 13:
                        gold = 3;
                        silver = 4;
                        bronze = 10;
                        break;
                    case 14:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 15:
                        gold = 6;
                        silver = 7;
                        bronze = 10;
                        break;
                    case 16: //17
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 17:
                        gold = 4;
                        silver = 5;
                        bronze = 10;
                        break;
                    case 18:
                        gold = 6;
                        silver = 7;
                        bronze = 10;
                        break;
                    case 19:
                        gold = 2;
                        silver = 3;
                        bronze = 10;
                        break;
                    case 20:
                        gold = 7;
                        silver = 8;
                        bronze = 10;
                        break;
                    case 21:
                        //LevelTimer(99);
                        //UiManager.Instance.tutorialBonusSketch.SetActive(true);
                        //gameManager.UpdateLevelString("Boss\nlevel!");
                        //gameManager.xagonBg.SetActive(false);
                        //gameManager.treeRight.SetActive(false);

                        break;

                }

            }
            // Canyon chapter
            else if (c == 3)
            {

            }
            // Lollypop chapter
            else if (c == 4)
            {

            }
            // Alien planet chapter
            else if (c == 5)
            {

            }


        }

        private void BasicLevelSetup(int c, int n)
        {
            // set exit up
            if (ChaptersAndLevels.levelsList[n].transform.Find("Exit"))
            {
                exitObject = ChaptersAndLevels.levelsList[n].transform.Find("Exit").transform.GetChild(0).gameObject;
            }
            else if (ChaptersAndLevels.levelsList[n].transform.Find("MovingExitPlatform"))
            {
                exitObject = ChaptersAndLevels.levelsList[n].transform.Find("MovingExitPlatform").Find("Exit").transform.GetChild(0).gameObject;
                VisualEffects.Instance.peExitSwirl.transform.parent = ChaptersAndLevels.levelsList[n].transform.Find("MovingExitPlatform").transform;
            }
            else
            {
                print("missing exit");
            }

            // enable exit swirl
            VisualEffects.Instance.peExitSwirl.SetActive(true);
            var pePos = exitObject.transform.position;
            pePos.y += 0.65f;
            VisualEffects.Instance.peExitSwirl.transform.position = pePos;

            exitObject.SetActive(false);

            print("setting up chapter: " + c + ", level: " + n);
        }

    }
}