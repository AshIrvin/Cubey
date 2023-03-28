using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private GameObject restoreButton;
    [SerializeField] private GameObject demoButton;
    [SerializeField] private Text purchaseText;
    [SerializeField] private Text testPurchaseText;
    [SerializeField] private Text shopText;
    [SerializeField] private bool productionBuild;
    [SerializeField] private TextMeshProUGUI purchaseRestoredText;
    
    public static bool GamePurchased
    {
        get => SaveLoadManager.GamePurchased;
        set => SaveLoadManager.SaveGamePurchased(value);
    }
    
    private void OnEnable()
    {
        // check if game has been purchased and update shop buttons
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
            mapManager.manyLevelsBeforeAds = 3;
        }
    }

    private void GameDemoMode()
    {
        // show purchase button
        purchaseText.text = "Purchase";
        testPurchaseText.text = "Test Purchase";
        purchaseText.transform.parent.gameObject.SetActive(true);
        testPurchaseText.transform.parent.gameObject.SetActive(true);
        // restoreButton.SetActive(false);
        shopText.text = "Purchase full game";
    }

    public void PurchaseGameButton()
    {
        GamePurchased = true;
        Debug.Log("Game Purchased: " + GamePurchased);
    }
    
    public void DemoMode()
    {
        GamePurchased = false;
        Debug.Log("Demo mode. Purchased: " + GamePurchased);
    }

    public static void RestoreTransaction()
    {
        GamePurchased = true;
        Debug.Log("Game Purchase restored: " + GamePurchased);
        SceneManager.LoadScene("CubeyGame");
    }
    
    public void GameIsPurchased()
    {
        // remove purchase text, add purchased and restore text and button
        purchaseText.text = "Purchased";
        testPurchaseText.text = "Purchased";
        // purchaseText.transform.parent.gameObject.SetActive(false);
        purchaseText.transform.parent.GetComponent<Button>().interactable = false;
        var tc = purchaseText.color;
        purchaseText.color = new Color(tc.r, tc.g, tc.b, 0.5f);   
            
        var c = purchaseText.transform.parent.transform.GetChild(0).GetComponent<Image>().color;
        purchaseText.transform.parent.transform.GetChild(0).GetComponent<Image>().color =
            new Color(c.r, c.g, c.b, 0.5f);            
        testPurchaseText.transform.parent.gameObject.SetActive(false);
        // restoreButton.SetActive(true);
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
