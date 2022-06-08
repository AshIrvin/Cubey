using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class StoryManager : MonoBehaviour
    {
        /*public static StoryManager Instance { get; set; }

        public CameraManager cameraManager;

        public GameObject[] StoryScreens;
        public GameObject[] StoryEndScreens;
        public GameObject closeButton, nextButton, previousButton;

        public bool[][] chapterStoryPlayed;

        private void Awake()
        {
            Instance = this;
        }
        
        void Start()
        {
            DisableStoryScreen();
            EnableDisableButtons(false);

            cameraManager.storyMode = true;
        }

        public bool CheckStoryPlayed(int story)
        {
            if (PlayerPrefs.HasKey("chapterStoryPlayed"))
            {
                var n = PlayerPrefs.GetInt("chapterStoryPlayed");
                switch (n)
                {
                    case 00: // start story
                        chapterStoryPlayed[0][0] = true;
                        break;
                    case 01: // end story
                        chapterStoryPlayed[0][1] = true;
                        break;
                    case 10:
                        chapterStoryPlayed[1][0] = true;
                        break;
                    default:
                        print("story case not set up");
                        break;
                }
                if (story == n)
                    return true;
            }


            return false;
        }

        public void LoadStory()
        {
            DisableStoryScreen();

            // MainMenuManager.Instance.chapterButtons.SetActive(true);

            StoryScreens[0].SetActive(true);

            EnableStoryButtons();

            EnableDisableButtons(true);

            
        }

        public void LoadStoryEnd()
        {
            //DisableStoryScreen();

            // MainMenuManager.Instance.chapterButtons.SetActive(true);

            StoryEndScreens[0].SetActive(true);

            EnableDisableButtons(true);

            DisableButton(nextButton.GetComponent<Button>());
        }

        public void DisableStoryScreen()
        {
            for (int i = 0; i < StoryScreens.Length; i++)
            {
                StoryScreens[i].SetActive(false);
                
            }
            StoryEndScreens[0].SetActive(false);

            EnableDisableButtons(false);
            // StartCoroutine(PanToLevelButton());
            StartCoroutine(CameraManager.Instance.PanToLevelButton());

            
        }

        

        public void CloseButton()
        {
            cameraManager.storyMode = false;
            DisableStoryScreen();
            // MainMenuManager.Instance.chapterButtons.SetActive(true);
            // MainMenuManager.Instance.ShowMap(1);

            cameraManager.playOnce = false;
            cameraManager.disableAutoPanMapCam = false;

            StartCoroutine(cameraManager.PanToLevelButton());
        }

        private void DisableButton(Button button)
        {
            //if (StoryScreens[n].activeInHierarchy)
            if (StoryScreens[0].activeInHierarchy || StoryScreens[StoryScreens.Length-1].activeInHierarchy
                || StoryEndScreens[0].activeInHierarchy)
            {
                button.interactable = false;
            } 
        }

        private void EnableStoryButtons()
        {
            var pButton = previousButton.GetComponent<Button>();
            var nButton = nextButton.GetComponent<Button>();

            if (StoryScreens[0].activeInHierarchy)
            {
                //print("prev button false. next button true");
                pButton.interactable = false;
                nButton.interactable = true;
            } else if (StoryScreens[StoryScreens.Length - 1].activeInHierarchy)
            {
                //print("prev button true. next button false");
                pButton.interactable = true;
                nButton.interactable = false;
            } else
            {
                pButton.interactable = true;
                nButton.interactable = true;
            }
        }


        public void BackButton()
        {
            for (int i = 0; i < StoryScreens.Length; i++)
            {
                if (StoryScreens[i].activeInHierarchy && !StoryScreens[0].activeInHierarchy)
                {
                    StoryScreens[i].SetActive(false);
                    StoryScreens[i-1].SetActive(true);
                    EnableStoryButtons();
                    //DisableButton(previousButton.GetComponent<Button>());
                    return;
                }
            }
        }

        public void NextButton()
        {
            for (int i = 0; i < StoryScreens.Length - 1; i++)
            {
                if (StoryScreens[i].activeInHierarchy)
                {
                    StoryScreens[i].SetActive(false);
                    StoryScreens[i + 1].SetActive(true);
                    EnableStoryButtons();
                    //DisableButton(nextButton.GetComponent<Button>());
                    return;
                }
            }
        }

        private void EnableDisableButtons(bool on)
        {
            //if (StoryScreens[0].activeInHierarchy)
            //{
                nextButton.SetActive(on);
                previousButton.SetActive(on);
                closeButton.SetActive(on);
                cameraManager.storyMode = on;
            //} else
            //{
            //    nextButton.SetActive(on);
            //    previousButton.SetActive(on);
            //    closeButton.SetActive(on);
            //    cameraManager.storyMode = on;
            //}
        }*/
    }
}