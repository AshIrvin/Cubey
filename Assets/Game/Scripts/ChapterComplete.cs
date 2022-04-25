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
        if (goldCollected == null) transform.Find("GoldCollected_text");
        if (goldLeftToCollect == null) transform.Find("MoreToCollectNumber_text");
        if (gotThemAll == null) transform.Find("YouGotThemAll_text");
        
        gotThemAll.gameObject.SetActive(false);
    }

    private void Start()
    {
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

        var chapter = SaveLoadManager.LastChapterPlayed;
        chapterNumberText.text = "Chapter " + chapter.ToString();
        
        var oneStar = SaveLoadManager.GetChapterAward(chapter, SaveLoadManager.Awards.OneStar); 
        var twoStars = SaveLoadManager.GetChapterAward(chapter, SaveLoadManager.Awards.TwoStars);
        var threeStars = SaveLoadManager.GetChapterAward(chapter, SaveLoadManager.Awards.ThreeStars);

        bronzeCollected.text = oneStar.ToString();
        silverCollected.text = twoStars.ToString();
        goldCollected.text = threeStars.ToString();
        goldButtonCollected.text = threeStars.ToString();

        var levelCount = chapterList[chapter].LevelList.Count;
        goldLeftToCollect.text = levelCount * 3 - threeStars + " stars to collect!";

        if (SaveLoadManager.GetLevelAward(29) > 0)
        {
            chapterNumberText.text = "Chapter " + chapter.ToString() + " Complete!";


            if (SaveLoadManager.GetChapterAward(chapter, SaveLoadManager.Awards.ThreeStars) == 30)
            {
                goldLeftToCollect.gameObject.SetActive(false);
                gotThemAll.gameObject.SetActive(true);
            
                // play PE
                // VisualEffects.Instance.PlayEffect();
            }
            else if (SaveLoadManager.GetChapterAward(chapter, SaveLoadManager.Awards.TwoStars) == 30)
            {
                goldLeftToCollect.gameObject.SetActive(false);
                gotThemAll.gameObject.SetActive(true);
            }
            else if (SaveLoadManager.GetChapterAward(chapter, SaveLoadManager.Awards.OneStar) == 30)
            {
                goldLeftToCollect.gameObject.SetActive(false);
                gotThemAll.gameObject.SetActive(true);
            }
        }
        

    }

    public void TogglePopup()
    {
        popup.SetActive(!popup.activeInHierarchy);
        goldStarButton.SetActive(!goldStarButton.activeInHierarchy);
    }
}
