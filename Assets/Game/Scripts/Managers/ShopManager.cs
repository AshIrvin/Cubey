using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; set; }
    [SerializeField] private GameObject restoreButton;
    [SerializeField] private GameObject demoButton;
    [SerializeField] private Text purchaseText;
    [SerializeField] private Text testPurchaseText;
    [SerializeField] private Text shopText;
    [SerializeField] private bool productionBuild;
    [SerializeField] private TextMeshProUGUI purchaseRestoredText;

    private MapManager mapManager;
    private static bool gamePurchased = false;

    public static bool GamePurchased
    { // TODO - reassign everything to this
        get => gamePurchased;
        set => gamePurchased = value;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        UiManager.OnGamePurchased += PurchaseGame;
        UiManager.OnRestorePurchase += RestoreTransaction;
        UiManager.OnDemoMode += GameDemoMode;
    }

    private void OnEnable()
    {
        if (mapManager == null)
            mapManager = MapManager.Instance;

        if (GamePurchased)
        {
            SetPurchasedButtonsColours("Purchased", "Thank You!");
        }
        else
        {
            GameDemoMode();
        }
        
        if (productionBuild)
        {
            testPurchaseText.transform.parent.gameObject.SetActive(false);
            demoButton.SetActive(false);
            //AdSettings.ManyLevelsBeforeAds = 3;
        }
    }

    private void GameDemoMode()
    { // show purchase button

        GamePurchased = false;

        //purchaseText.text = "Purchase";
        //testPurchaseText.text = "Test Purchase";
        purchaseText.transform.parent.gameObject.SetActive(true);
        testPurchaseText.transform.parent.gameObject.SetActive(true);
        //shopText.text = "Purchase full game";
        SetPurchasedButtonsColours("Buy", "Shop");
        SaveLoadManager.SaveGameData();
    }

    private void PurchaseGame()
    {
        GamePurchased = true;
        Logger.Instance.ShowDebugLog("Game Purchased: " + GamePurchased);

        UnlockManager.UnlockAllChapters();
        UnlockManager.UnlockSeasonalChapter();

        MainMenuManager.Instance.CycleThroughUnlockedChapters();
        SetPurchasedButtonsColours("Purchased", "Thank You!");
        MainMenuManager.Instance.ToggleThankYouSign();
        SaveLoadManager.SaveGameData();
    }

    private void RestoreTransaction()
    {
        PurchaseGame();
        Logger.Instance.ShowDebugLog("Game Purchase restored: " + GamePurchased);
    }

    // TODO - move to UiManager
    public void SetPurchasedButtonsColours(string buttonText, string titleText)
    {
        purchaseText.text = buttonText;
        testPurchaseText.text = buttonText;
        
        var purchaseTextColour = purchaseText.color;
        purchaseTextColour.a = 0.5f;
        purchaseText.color = purchaseTextColour;   
            
        //var borderImageColour = purchaseText.transform.parent.transform.Find("border").GetComponent<Image>().color;
        var borderImageColour = new Color(0.9f, 0.9f, 0.9f, 1);
        purchaseText.transform.parent.transform.Find("border").GetComponent<Image>().color = borderImageColour;

        //testPurchaseText.transform.parent.gameObject.SetActive(false);
        shopText.text = titleText;
    }

    public void RestorePurchasesText()
    {
        restoreButton.GetComponent<Text>().text = "Restored";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("CubeyGame");
    }
}
