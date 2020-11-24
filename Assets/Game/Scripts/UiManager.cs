using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Scripts
{
    public class UiManager : MonoBehaviour
    {
        public static UiManager Instance { get; set; }

        public Toggle debugConsoleToggle;

        public GameObject debugConsole;
        public GameObject tutorial, tutorialTitleImage, tutorialSketchImage; 
        public GameObject tutorialBonusSketch, continueButton;
        public GameObject tutorialPause;

        public Text tutorialText, titleText;

        public float tutorialTimer = 4f;
        bool timerEnabled;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            //CloseScreens();
            if (debugConsole == null)
                debugConsole = GameObject.Find("IngameDebugConsole");
            tutorial.SetActive(false);

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            tutorialTimer -= Time.fixedDeltaTime;
            if (tutorialTimer < 0 && timerEnabled)
            {
                CloseScreens();
                timerEnabled = false;
                tutorialTimer = 4f;
            }
        }

        public void DebugConsole(bool on)
        {
            //GameObject console = GameObject.Find("IngameDebugConsole");
            debugConsole.SetActive(on);
        }

        public void TutorialScreen(bool on)
        {
            Tutorial(on, "", "");
        }

        public void Tutorial(bool on, string title, string text)
        {
            CloseScreens();
            //print("tutorial loaded");
            tutorial.SetActive(on);
            //ChooseTutorialImage();
            timerEnabled = true;
            titleText.text = title;
            tutorialText.text = text;
            Time.timeScale = on ? 0f : 1f;
        }

        public void TutorialPause(bool on)
        {
            timerEnabled = true;
            tutorialPause.SetActive(on);
        }

        //void ChooseTutorialImage()
        //{
        //    CloseScreens();


        //    if (GameManager.Instance.levelNo == 0)
        //    {
        //        print("level 0");
        //        tutorialTitleImage.SetActive(true);
        //    }
        //    else if (GameManager.Instance.levelNo == 5)
        //    {
        //        print("level 5 bonus");
        //        tutorialSketchImage.SetActive(true);
        //    } else
        //    {
        //        CloseScreens();
        //    }
        //}

        void CloseScreens()
        {
            //print("close tut screens");
            tutorial.SetActive(false);
            tutorialTitleImage.SetActive(false);
            tutorialSketchImage.SetActive(false);
            tutorialPause.SetActive(false);
        }

    }
}