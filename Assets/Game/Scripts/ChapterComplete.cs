using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterComplete : MonoBehaviour
{
    [SerializeField] private Text chapterNumberText;
    [SerializeField] private Text bronzeCollected;
    [SerializeField] private Text silverCollected;
    [SerializeField] private Text goldCollected;
    [SerializeField] private Text goldButtonCollected;
    [SerializeField] private Text goldLeftToCollect;
    [SerializeField] private GameObject gotThemAll;
    [SerializeField] private GameObject popup;
    [SerializeField] private ChapterList chapterList;
    
    public GameObject goldStarButton;

    private GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();        
        if (chapterNumberText == null) transform.Find("");
        if (bronzeCollected == null) transform.Find("BronzeCollected_text");
        if (silverCollected == null) transform.Find("SilverCollected_text");
        if (goldCollected == null) transform.Find("GoldCollected_text");
        // if (goldButtonCollected == null) transform.Find("GoldCollected_text");
        if (goldLeftToCollect == null) transform.Find("MoreToCollectNumber_text");
        if (gotThemAll == null) transform.Find("YouGotThemAll_text");
        
        gotThemAll.gameObject.SetActive(false);

        UpdateButtonAward();
    }

    private void OnEnable()
    {
        goldStarButton.SetActive(true);
    }

    public void UpdateButtonAward()
    {
        if (gameManager == null)
        {
            return;
        }
        var chapter = gameManager.SaveMetaData.LastChapterPlayed;
        
        chapterNumberText.text = "Chapter " + chapter.ToString();

        var bronze = chapterList[chapter].AwardsBronze;
        var silver = chapterList[chapter].AwardsSilver;
        var gold = chapterList[chapter].AwardsGold;

        bronzeCollected.text = bronze.ToString();
        silverCollected.text = silver.ToString();
        goldCollected.text = gold.ToString();
        
        goldButtonCollected.text = gold.ToString();

        var levelCount = chapterList[chapter].LevelList.Count;
        goldLeftToCollect.text = levelCount - gold + " gold to collect!";

        if (bronze >= levelCount)
        {
            chapterNumberText.text = "Chapter " + chapter.ToString() + " Complete!";
        }
        
        if (levelCount == gold)
        {
            goldLeftToCollect.gameObject.SetActive(false);
            gotThemAll.gameObject.SetActive(true);
            
            // play PE
            // VisualEffects.Instance.PlayEffect();
        }
    }

    public void TogglePopup()
    {
        popup.SetActive(!popup.activeInHierarchy);
        goldStarButton.SetActive(!goldStarButton.activeInHierarchy);
    }
}
