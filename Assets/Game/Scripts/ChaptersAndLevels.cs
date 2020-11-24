using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
{
    public class ChaptersAndLevels : MonoBehaviour
    {
        #region Public

        public GameManager gameManager;

        public static List<GameObject> chaptersList = new List<GameObject>();
        public static List<GameObject> levelsList = new List<GameObject>();
        public static int chapterNo;
        
        [Header("Set chapter")]
        public int customChapter;

        #endregion

        private void Awake()
        {
            if (gameManager == null)
                gameManager = gameObject.GetComponent<GameManager>();

            levelsList.Clear();
        }

        private void Start()
        {
            SetupLevels();
        }

        private void SetupLevels()
        {
            // find Levels GO
            // find active folder start with Chapter
            // get all GO in folder and add to list

            var levelsParent = GameObject.Find("Chapters").gameObject;

            chaptersList.Clear();

            if (levelsParent != null && chaptersList.Count == 0)
            {
                //print("levels parent found: " + levelsParent.name);

                for (var i = 0; i < 6; i++)
                {
                    // search for each gameobject with Chapter in its name to add it to the List
                    GameObject chapterFolders = levelsParent.transform.Find("Chapter_" + i).gameObject;

                    if (chapterFolders.name.Contains("Chapter_"))
                        chaptersList.Add(chapterFolders);
                }
            }

            for (var i = 0; i < chaptersList.Count; i++)
            {
                chaptersList[i].SetActive(false);
            }

            print("chapter_n: " + PlayerPrefs.GetInt("chapter_n"));

            if (PlayerPrefs.HasKey("chapter_n") && !gameManager.playSingleLevel)
            {
                var chapter = PlayerPrefs.GetInt("chapter_n");
                chaptersList[chapter].SetActive(true);
                chapterNo = chapter;
            } else if (gameManager.playSingleLevel)
            {
                chapterNo = customChapter;
                chaptersList[customChapter].SetActive(true);
            }

            // take the chapters assigned to the array
            for (var i = 0; i < chaptersList.Count; i++)
            {
                if (chaptersList[i].activeInHierarchy)
                {
                    gameManager.levelsGrp = chaptersList[i].transform.Find("Levels").gameObject;
                    var levelsCount = gameManager.levelsGrp.transform.childCount;

                    for (var j = 0; j < levelsCount; j++)
                    {
                        levelsList.Add(gameManager.levelsGrp.transform.GetChild(j).gameObject);
                    }
                }
            }

            // change all these '_n's
            if (PlayerPrefs.HasKey("levels_n"))
            {
                gameManager.levelNo = PlayerPrefs.GetInt("levels_n");
                levelsList[gameManager.levelNo].gameObject.SetActive(true);
            }
        }
    }
}