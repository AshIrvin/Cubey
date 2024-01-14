using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;
    private AudioManager audioManager;

    #region Fields

    [Header("Finish Level text")]
    [SerializeField] private List<string> finishedInfoText;
    [SerializeField] private List<string> nearlyFinishedInfoText;
    [SerializeField] private List<string> failedFinishedInfoText;

    [Header("Game Screens")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject failedScreen;
    [SerializeField] private GameObject helpScreen;
    [SerializeField] private GameObject topUi;
    [SerializeField] private GameObject loadingScreen;

    [Header("UI Text")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text itemText;
    [SerializeField] private Text jumpAmountText;
    [SerializeField] private Text jumpText;
    [SerializeField] private Text awardToGet;
    [SerializeField] private Text bronzePodium;
    [SerializeField] private Text silverPodium;
    [SerializeField] private Text goldPodium;
    [SerializeField] private Text oneStarTutorialText;
    [SerializeField] private Text twoStarTutorialText;
    [SerializeField] private Text threeStarTutorialText;
    [SerializeField] private Text endScreenInfo;

    [Header("UI Pickups")]
    [SerializeField] private List<Image> pickupUiImages;

    [Header("UI Buttons")]
    [SerializeField] private Toggle analyticsButton;

    #endregion Fields

    #region Public Getters

    public Text LevelText => levelText;
    public Text ItemText => itemText;
    public Text JumpAmountText => jumpAmountText;
    public Text AwardToGet => awardToGet;
    public Text OneStarTutorialText => oneStarTutorialText;
    public Text TwoStarTutorialText => twoStarTutorialText;
    public Text ThreeStarTutorialText => threeStarTutorialText;
    public Text BronzePodium => bronzePodium;
    public Text SilverPodium => silverPodium;
    public Text GoldPodium => goldPodium;

    public GameObject TopUi => topUi;
    public GameObject PauseMenu => pauseMenu;
    public GameObject HelpScreen => helpScreen;
    public GameObject EndScreen => endScreen;
    public GameObject LoadingScreen => loadingScreen;
    public GameObject FailedScreen => failedScreen;

    #endregion Public Getters

    #region Actions

    public static Action<bool> AutoPanToggle;
    public static Action<bool> MusicToggle;
    public static Action<bool> SoundToggle;
    public static Action DeleteSaves;
    public static Action<bool> AnalyticsConsent;
    public static Action<int> OnLevelButtonPressed;
    public static Action<int> OnChapterButtonPressed;
    public static Action OnGamePurchased;
    public static Action OnDemoMode;
    public static Action OnRestorePurchase;
    public static Action<bool> OnLoadChapterScreen;
    public static Action<bool> OnLoadSettings;

    #endregion Actions

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;

        UGS_Analytics.AnalyticsConsent += GetAnalyticsConsentForButton;
    }

    public void SetGameLevelCanvases(bool state)
    {
        ShowTopUi(state);
        ShowPauseMenu(false);
        ShowEndScreen(false);
        ShowFailedScreen(false);
    }

    public void HideScreens()
    {
        ShowPauseMenu(false);
        ShowFailedScreen(false);
        ShowEndScreen(false);
        ShowTopUi(false);
    }

    public void ShowTopUi(bool state)
    {
        if (TopUi == null)
        {
            Logger.Instance.ShowDebugError("Missing TopUi in UiManager!");
            return;
        }

        TopUi.SetActive(state);
        PickupGraphic(LevelManager.LastChapterPlayed);
    }

    public void PickupText()
    {
        if (ItemText == null) return;

        ItemText.text = GameManager.Instance.PickupCountProperty + " X";
    }

    public void DisablePickupGraphics()
    {
        foreach (var image in pickupUiImages)
        {
            image.gameObject.SetActive(false);
        }
    }

    private void PickupGraphic(int n)
    {
        DisablePickupGraphics();

        pickupUiImages[n].gameObject.SetActive(true);
    }

    #region Main Menu

    public void StartButton()
    {
        OnLoadChapterScreen?.Invoke(true);
    }

    public void SettingsButton()
    {
        OnLoadSettings?.Invoke(true);
    }

    #endregion Main Menu

    #region Map

    // Attached to Level buttons on map
    public void GetLevelNoToLoad()
    {
        var levelButtonClicked = EventSystem.current.currentSelectedGameObject.gameObject.transform.Find("LevelText_no").GetComponent<Text>().text.ToString();
        int.TryParse(levelButtonClicked, out int levelNumber);
        levelNumber -= 1;

        OnLevelButtonPressed?.Invoke(levelNumber);
    }

    public void ShowMap(int n)
    {
        OnChapterButtonPressed?.Invoke(n);
    }

    #endregion Map

    #region Shop

    public void GamePurchasedButton()
    {
        OnGamePurchased?.Invoke();
    }

    public void DemoModeButton()
    {
        OnDemoMode?.Invoke();
    }

    public void RestorePurchase()
    {
        OnRestorePurchase?.Invoke();
    }

    #endregion Shop

    #region Pause/Options/End screen

    public void ShowPauseMenu(bool state)
    {
        GameManager.Instance.LaunchArc = !state;

        if (PauseMenu != null)
        {
            PauseMenu.SetActive(state);
        }

        Time.timeScale = state ? 0f : 1f;

        if (audioManager.allowMusic)
        {
            audioManager.MuteAudio(audioManager.gameMusic, state);
        }
    }

    public void ShowEndScreen(bool state)
    {
        if (EndScreen == null)
        {
            Logger.Instance.ShowDebugError("Missing EndScreen object on UiManager script");
            return;
        }

        EndScreen.SetActive(state);

        Time.timeScale = state ? 0.1f : 1f;
    }

    public void ModifyEndScreenInfoText(GameManager.FinishedInfo info)
    {
        switch (info)
        {
            case GameManager.FinishedInfo.Failed:
                endScreenInfo.text = failedFinishedInfoText[UnityEngine.Random.Range(0, failedFinishedInfoText.Count)];
                break;
            case GameManager.FinishedInfo.Nearly:
                endScreenInfo.text = nearlyFinishedInfoText[UnityEngine.Random.Range(0, nearlyFinishedInfoText.Count)];
                break;
            case GameManager.FinishedInfo.Completed:
                endScreenInfo.text = finishedInfoText[UnityEngine.Random.Range(0, finishedInfoText.Count)];
                break;
        }
    }

    public void ShowFailedScreen(bool on)
    {
        if (FailedScreen == null)
        {
            Logger.Instance.ShowDebugError("Missing failedScreen object on UiManager script");
            return;
        }

        FailedScreen.SetActive(on);

        Time.timeScale = on ? 0f : 1f;
    }

    public void RestartLevel()
    {
        Logger.Instance.ShowDebugLog("Restarting Level");

        LevelManager.OnLevelLoad?.Invoke();

        ShowTopUi(false);
        ShowTopUi(true);

        GameManager.Instance.ResetCubey();
    }

    public void QuitToMap()
    {
        GameManager.Instance.EnableCubeyLevelObject(false);
        LevelManager.Instance.LevelGameObject.SetActive(false);
        MapManager.Instance.QuitToMap();
    }

    public void QuitToMainMenu()
    {
        GameManager.Instance.EnableCubeyLevelObject(false);
        LevelManager.Instance.LevelGameObject.SetActive(false);
        HideScreens();
        MainMenuManager.Instance.LoadMainMenu();
    }

    #endregion Options/End screen

    #region Settings menu

    public void MusicButton()
    {
        bool clicked = EventSystem.current.currentSelectedGameObject.gameObject
            .GetComponent<Toggle>().isOn;

        MusicToggle?.Invoke(clicked);
    }

    public void SoundButton()
    {
        bool clicked = EventSystem.current.currentSelectedGameObject.gameObject
            .GetComponent<Toggle>().isOn;

        SoundToggle?.Invoke(clicked);
    }

    public void AutoPanButton()
    {
        bool clicked = EventSystem.current.currentSelectedGameObject.gameObject
            .GetComponent<Toggle>().isOn;

        AutoPanToggle?.Invoke(clicked);
    }

    public void DeleteSavesButton()
    {

    }

    public void DeleteSavesConfirmButton()
    {
        DeleteSaves?.Invoke();
    }

    public void AnalyticsConsentButton()
    {
        AnalyticsConsent?.Invoke(true);
    }

    public void AnalyticsOptOutButton()
    {       
        AnalyticsConsent?.Invoke(false);
    }

    public void EnableCloudSaving(bool state)
    {
        // This should be a switch in the settings UI. On as default
    }

    private void GetAnalyticsConsentForButton(bool state)
    {
        analyticsButton.GetComponent<Toggle>().isOn = state;
    }

    #endregion Settings menu

    #region Level End screen

    

    #endregion Level End screen
}
