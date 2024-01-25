using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; set; }

    // TODO - move buttons to UiManager?
    [SerializeField] private GameObject restoreButton;
    [SerializeField] private GameObject demoButton;
    [SerializeField] private Text purchaseText;
    [SerializeField] private Text testPurchaseText;
    [SerializeField] private Text shopText;
    [SerializeField] private bool productionBuild;
    [SerializeField] private TextMeshProUGUI purchaseRestoredText;

    private MapManager mapManager;

    public static bool GamePurchased { get; private set; }

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
    {
        GamePurchased = false;

        purchaseText.transform.parent.gameObject.SetActive(true);
        testPurchaseText.transform.parent.gameObject.SetActive(true);
        SetPurchasedButtonsColours("Buy", "Shop");
        SaveLoadManager.SaveGameData();
    }

    internal static void SetGamePurchased(bool state)
    {
        GamePurchased = state;
    }

    private void PurchaseGame()
    {
        GamePurchased = true;
        Logger.Instance.ShowDebugLog("Game Purchased: " + GamePurchased);

        UnlockManager.UnlockAllChapters();

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

    private void SetPurchasedButtonsColours(string buttonText, string titleText)
    {
        purchaseText.text = buttonText;
        testPurchaseText.text = "Test " + buttonText;
        
        var purchaseTextColour = purchaseText.color;
        purchaseTextColour.a = 0.5f;
        purchaseText.color = purchaseTextColour;   
            
        var borderImageColour = new Color(0.9f, 0.9f, 0.9f, 1);
        purchaseText.transform.parent.transform.Find("border").GetComponent<Image>().color = borderImageColour;

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
