using UnityEngine;
using UnityEngine.UI;

public class ChapterComplete : MonoBehaviour
{
    [SerializeField] private Text chapterNumberText;
    [SerializeField] private Text goldCollected;
    [SerializeField] private Text goldButtonCollected;
    [SerializeField] private Text goldLeftToCollect;
    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject completedChapter100;
    [SerializeField] private ChapterList chapterList;
    [SerializeField] private ParticleSystem peChapterCompleteStars;
    
    public GameObject goldStarButton;

    private GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();        
        if (goldCollected == null) transform.Find("GoldCollected_text");
        if (goldLeftToCollect == null) transform.Find("MoreToCollectNumber_text");
    }

    private void OnEnable()
    {
        goldStarButton.SetActive(true);
    }

    public void ShowCompleteScreen(bool state)
    {
        if (gameManager == null) return;

        var chapter = LevelManager.LastChapterPlayed;
        chapterNumberText.text = "Chapter " + chapter.ToString();
        
        var threeStars = AwardManager.GetChapterAward(chapter);

        goldCollected.text = threeStars.ToString();
        goldButtonCollected.text = threeStars.ToString();

        var levelCount = chapterList[chapter].LevelList.Count;
        goldLeftToCollect.text = levelCount * 3 - threeStars + " stars to collect!";

        CheckCompletedChapter(chapter, state);
    }

    private void CheckCompletedChapter(int chapter, bool state)
    {
        if (AwardManager.GetChapterAward(chapter) >= 90)
        {
            bool completedGame = true;

            // also check other chapters if player has finished the game
            for (int i = 0; i < LevelManager.ChapterAmount; i++)
            {
                if (AwardManager.GetChapterAward(i) < 90)
                {
                    completedGame = false;
                    break;
                }
            }

            // TODO add completed game popup
            if (completedGame)
            {
                // player has completed the whole game! Show new popup

            }
            else
            {
                completedChapter100.SetActive(state);
                popup.SetActive(false);
                VisualEffects.Instance.PlayEffect(peChapterCompleteStars);
            }
        }
        else
        {
            if (LevelManager.LastLevelPlayed == 29 && AwardManager.GetLevelAward(29) > 0)
            {
                popup.SetActive(state);
                completedChapter100.SetActive(false);

                chapterNumberText.text = "Chapter " + chapter.ToString() + " Complete!";
            }
        }
    }

    public void ClosePopup()
    {
        popup.SetActive(false);
        completedChapter100.SetActive(false);
        goldStarButton.SetActive(true);
    }

    public void OpenPopup()
    {
        if (AwardManager.GetChapterAward(LevelManager.LastChapterPlayed) >= 90)
        {
            completedChapter100.SetActive(true);
            popup.SetActive(false);
        }
        else
        {
            popup.SetActive(true);
            completedChapter100.SetActive(false);
        }
        
        goldStarButton.SetActive(false);
    }
}
