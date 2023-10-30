using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    #region Fields

    [Header("Finish Level text")]
    [SerializeField] private List<string> finishedInfoText;
    [SerializeField] private List<string> nearlyFinishedInfoText;
    [SerializeField] private List<string> failedFinishedInfoText;

    [Header("Game Screens1")]
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

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
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
}
