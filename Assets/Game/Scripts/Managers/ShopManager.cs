using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject restoreButton;
    [SerializeField] private GameObject demoButton;
    [SerializeField] private Text purchaseText;
    [SerializeField] private Text testPurchaseText;
    [SerializeField] private Text shopText;
    [SerializeField] private bool productionBuild;
    [SerializeField] private TextMeshProUGUI purchaseRestoredText;

    private MapManager mapManager;
    
    public static bool GamePurchased
    {
        get => SaveLoadManager.GamePurchased;
        set => SaveLoadManager.SaveGamePurchased(value);
    }
    
    private void OnEnable()
    {
        if (mapManager == null)
            mapManager = MapManager.Instance;

        if (SaveLoadManager.GamePurchased)
        {
            GameIsPurchased();
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
        purchaseText.text = "Purchase";
        testPurchaseText.text = "Test Purchase";
        purchaseText.transform.parent.gameObject.SetActive(true);
        testPurchaseText.transform.parent.gameObject.SetActive(true);
        shopText.text = "Purchase full game";
    }

    public void PurchaseGameButton()
    {
        GamePurchased = true;
        Logger.Instance.ShowDebugLog("Game Purchased: " + GamePurchased);
        // TODO - Needs logic here to open chapter without restarting

    }
    
    public void DemoMode()
    {
        GamePurchased = false;
        Logger.Instance.ShowDebugLog("Demo mode. Purchased: " + GamePurchased);
    }

    public static void RestoreTransaction()
    {
        GamePurchased = true;
        Logger.Instance.ShowDebugLog("Game Purchase restored: " + GamePurchased);
        SceneManager.LoadScene("CubeyGame");
    }
    
    public void GameIsPurchased()
    {
        purchaseText.text = "Purchased";
        testPurchaseText.text = "Purchased";
        //purchaseText.transform.parent.GetComponent<IAPButton>().interactable = false;
        var tc = purchaseText.color;
        purchaseText.color = new Color(tc.r, tc.g, tc.b, 0.5f);   
            
        var c = purchaseText.transform.parent.transform.GetChild(0).GetComponent<Image>().color;
        purchaseText.transform.parent.transform.GetChild(0).GetComponent<Image>().color =
            new Color(c.r, c.g, c.b, 0.5f);            
        testPurchaseText.transform.parent.gameObject.SetActive(false);
        shopText.text = "Game Purchased";
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
