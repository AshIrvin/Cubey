using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChapterComplete : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI chapterNumberText;
    [SerializeField] private TextMeshProUGUI bronzeCollected;
    [SerializeField] private TextMeshProUGUI silverCollected;
    [SerializeField] private TextMeshProUGUI goldCollected;
    [SerializeField] private TextMeshProUGUI goldButtonCollected;
    [SerializeField] private TextMeshProUGUI goldLeftToCollect;
    [SerializeField] private GameObject gotThemAll;
    public GameObject goldStarButton;
    [SerializeField] private GameObject popup;
    

    [SerializeField] private ChapterList chapterList;
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
    }

    private void OnEnable()
    {
        var chapter = gameManager.SaveMetaData.LastChapterPlayed;
        chapterNumberText.text = chapter.ToString();
        var gold = chapterList[chapter].AwardsGold;
        goldCollected.text = gold.ToString();

        var levelCount = chapterList[chapter].LevelList.Count;
        goldLeftToCollect.text = levelCount - gold + " left to collect!";

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
