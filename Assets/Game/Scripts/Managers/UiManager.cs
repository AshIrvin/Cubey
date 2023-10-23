//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    // public static UiManager Instance { get; set; }
    // No longer used?
    
    // old
    [SerializeField] private Toggle debugConsoleToggle;
    [SerializeField] private GameObject debugConsole;
    [SerializeField] private GameObject tutorial;
    [SerializeField] private GameObject tutorialTitleImage; 
    [SerializeField] private GameObject tutorialSketchImage;
    [SerializeField] private GameObject tutorialBonusSketch;
    [SerializeField] private GameObject tutorialPause;

    [SerializeField] private Text tutorialText;
    [SerializeField] private Text titleText;

    [SerializeField] private float tutorialTimer = 4f;
    [SerializeField] private bool timerEnabled;
    
    public static GameObject continueButton;

    private void Awake()
    {
        // Instance = this;
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

    private void Tutorial(bool on, string title, string text)
    {
        CloseScreens();
        tutorial.SetActive(on);
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


    private void CloseScreens()
    {
        //print("close tut screens");
        tutorial.SetActive(false);
        tutorialTitleImage.SetActive(false);
        tutorialSketchImage.SetActive(false);
        tutorialPause.SetActive(false);
    }

}
