using System;
using System.Collections;
using System.Collections.Generic;
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

    
    private void OnEnable()
    {
        // check if game has been purchased and update shop buttons
        if (SaveLoadManager.GamePurchased)
        {
            GamePurchased();
        }
        else
        {
            GameDemoMode();
        }
        
        if (productionBuild)
        {
            testPurchaseText.transform.parent.gameObject.SetActive(false);
            demoButton.SetActive(false);
        }
    }

    private void GameDemoMode()
    {
        // show purchase button
        purchaseText.text = "Purchase";
        testPurchaseText.text = "Test Purchase";
        purchaseText.transform.parent.gameObject.SetActive(true);
        testPurchaseText.transform.parent.gameObject.SetActive(true);
        restoreButton.SetActive(false);
        shopText.text = "Purchase full game";


    }
    
    public void GamePurchased()
    {
        // remove purchase text, add purchased and restore text and button
        purchaseText.text = "Purchased";
        testPurchaseText.text = "Purchased";
        purchaseText.transform.parent.gameObject.SetActive(false);
        testPurchaseText.transform.parent.gameObject.SetActive(false);
        restoreButton.SetActive(true);
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
