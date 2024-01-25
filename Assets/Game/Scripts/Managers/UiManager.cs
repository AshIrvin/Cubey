using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class UiManager : MonoBehaviour
{
    internal static UiManager Instance;
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
    [SerializeField] private GameObject mainMenu;

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

    #region Internal Getters

    internal Text LevelText => levelText;
    internal Text ItemText => itemText;
    internal Text JumpAmountText => jumpAmountText;
    internal Text AwardToGet => awardToGet;
    internal Text OneStarTutorialText => oneStarTutorialText;
    internal Text TwoStarTutorialText => twoStarTutorialText;
    internal Text ThreeStarTutorialText => threeStarTutorialText;
    internal Text BronzePodium => bronzePodium;
    internal Text SilverPodium => silverPodium;
    internal Text GoldPodium => goldPodium;

    internal GameObject TopUi => topUi;
    internal GameObject PauseMenu => pauseMenu;
    internal GameObject HelpScreen => helpScreen;
    internal GameObject EndScreen => endScreen;
    internal GameObject LoadingScreen => loadingScreen;
    internal GameObject FailedScreen => failedScreen;
    internal GameObject MainMenu => mainMenu;

    #endregion Internal Getters

    #region Actions

    internal static Action<bool> AutoPanToggle;
    internal static Action<bool> MusicToggle;
    internal static Action<bool> SoundToggle;
    internal static Action DeleteSaves;
    internal static Action<bool> AnalyticsConsent;
    internal static Action<int> OnLevelButtonPressed;
    internal static Action<int> OnChapterButtonPressed;
    internal static Action OnGamePurchased;
    internal static Action OnDemoMode;
    internal static Action OnRestorePurchase;
    internal static Action<bool> OnLoadChapterScreen;
    internal static Action<bool> OnLoadSettings;

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

        SetJumpText(GameManager.Instance.JumpsLeft.ToString());
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

    internal void SetJumpText(string text)
    {
        JumpAmountText.text = text;
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

    private void ChangeTextColour(Text text, Color color)
    {
        text.color = color;
    }

    internal async Task WaitForLevelMetaData()
    {
        await Task.Run(() => new WaitUntil(() => GameManager.Instance.LevelMetaData != null));

        UpdateAwardsNeeded();
    }

    private void UpdateAwardsNeeded()
    {
        var levelMetaData = GameManager.Instance.LevelMetaData;
        int JumpsToStartWith = GameManager.JUMPS_TO_START;
        int JumpsLeft = GameManager.Instance.JumpsLeft;

        if (JumpsToStartWith - JumpsLeft <= levelMetaData.JumpsForGold) // 10 - 8 < 3
        {
            ChangeTextColour(JumpAmountText, ColourManager.starGold);
        }
        else if (JumpsToStartWith - JumpsLeft <= levelMetaData.JumpsForSilver)
        {
            ChangeTextColour(JumpAmountText, ColourManager.starSilver);
        }
        else if (JumpsToStartWith - JumpsLeft <= levelMetaData.JumpsForBronze) // 9 - 9 <= 9
        {
            ChangeTextColour(JumpAmountText, ColourManager.starBronze);
        }
        else
        {
            AwardToGet.text = "Need bronze for next level";
            ChangeTextColour(JumpAmountText, ColourManager.starDefault);
        }

        OneStarTutorialText.text = levelMetaData.JumpsForBronze.ToString();
        TwoStarTutorialText.text = levelMetaData.JumpsForSilver.ToString();
        ThreeStarTutorialText.text = levelMetaData.JumpsForGold.ToString();

        BronzePodium.text = levelMetaData.JumpsForBronze.ToString();
        SilverPodium.text = levelMetaData.JumpsForSilver.ToString();
        GoldPodium.text = levelMetaData.JumpsForGold.ToString();
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

    public void RefreshRateButton()
    {

    }

    #endregion Settings menu

    #region Level End screen



    #endregion Level End screen
}
