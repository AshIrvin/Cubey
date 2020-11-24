using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGamePro;


namespace Assets.Game.Scripts
{
    public class PlayerLevelStats : MonoBehaviour
    {
        public static PlayerLevelStats Instance { get; set; }

        private GameManager gameManager;
        private bool menuManagerFound;

        [Header("Save info")]
        public int LevelNo;
        public float Time, t;
        public int Star, s;
        public int Items, i;
        public int Jumped, j;
        public int Restarted, r;
        public int Score, sc;

        public List<PlayerLevelStats> levelStats = new List<PlayerLevelStats>();

        private void Awake()
        {
            gameManager = gameObject.GetComponent<GameManager>();

            menuManagerFound = GameObject.Find("MainMenuManager");
        }

        private void SaveLevelStats(float time, int star, int items, int jumped, int restarted, int score)
        {
            Time = time;
            Star = star;
            Items = items;
            Jumped = jumped;
            Restarted = restarted;
            Score = score;
        }

        public void SaveLevel(int levelNo, float time, int star, int items, int jumped, int restarted, int score)
        {
            var chapter = MainMenuManager.Instance.chapter.ToString("0");
            var levelString = chapter + levelNo.ToString("00");

            print("2. saving level: " + levelString);

            if (int.TryParse(levelString, out LevelNo))
            {
                if (SaveGame.Exists("Level" + LevelNo))
                {
                    time = CheckBest(
                        SaveGame.Load<float>("Level" + LevelNo + "/Time", t),
                        time);

                    star = (int)CheckBest(
                        SaveGame.Load<int>("Level" + LevelNo + "/Star", s),
                        star);

                    items = (int)CheckBest(
                        SaveGame.Load<int>("Level" + LevelNo + "/Items", i),
                        items);

                    jumped = (int)CheckBest(
                        SaveGame.Load<int>("Level" + LevelNo + "/Jumped", j),
                        jumped);

                    restarted = (int)CheckBest(
                        SaveGame.Load<int>("Level" + LevelNo + "/Restarted", r),
                        restarted);

                    score = (int)CheckBest(
                        SaveGame.Load<int>("Level" + LevelNo + "/Score", sc),
                        score);

                    SaveLevelStats(time, star, items, jumped, restarted, score);
                }
                else
                {
                    PlayerLevelStats playerLevelStats = new PlayerLevelStats
                    {
                        Time = time,
                        Star = star,
                        Items = items,
                        Jumped = jumped,
                        Restarted = restarted,
                        Score = score
                    };

                }
            }

            print("2b. Saved levelStats. Level: " + LevelNo + ", Time: " + time + ", star: " + star + ", items: " + items +
                ", jumped: " + jumped + ", restarted: " + restarted + ", score: " + score);

            SaveGame.Save("Level" + LevelNo + "/Time", Time);
            SaveGame.Save("Level" + LevelNo + "/Star", Star);
            SaveGame.Save("Level" + LevelNo + "/Items", Items);
            SaveGame.Save("Level" + LevelNo + "/Jumped", Jumped);
            SaveGame.Save("Level" + LevelNo + "/Restarted", Restarted);
            SaveGame.Save("Level" + LevelNo + "/Score", Score);

            

            
        }

        public void ClearStats()
        {
            SaveGame.Clear();

            Time = 0;
            Star = 0;
            Items = 0;
            Jumped = 0;
            Restarted = 0;
            Score = 0;

            print("All Saves cleared.");
        }

        private float CheckBest(float current, float latest)
        {
            if (current > latest)
                return current;
            
            return latest;
        }

        public int FindActiveChapter()
        {
            GameObject chapterFolder = GameObject.Find("Chapters").gameObject;
            
            // get all children and check if name starts with chapter_
            for (int i = 0; i < chapterFolder.transform.childCount; i++)
            {

            }

            // if chapter_ is active, return

            return 0;
        }

        public void LoadLevelStats(int levelNo)
        {
            //PlayerLevelStats playerLevelStats = new PlayerLevelStats();
            //int chapter = 0;
            //if (menuManagerFound)
            //    chapter = MainMenuManager.Instance.chapter;
            //else
            //    chapter = customChapter;
            //else
            //    chapter = transform.parent.
            int chapter = ChaptersAndLevels.chapterNo;

            var levelString = chapter.ToString("0") + levelNo.ToString("00");

            if (int.TryParse(levelString, out LevelNo))
            {
                if (SaveGame.Exists("Level" + LevelNo))
                {
                    //print("loading stats for level" + LevelNo);
                    // 101 = chapter 1, level 01
                    //"Level" + levelNo + ""
                    Time = SaveGame.Load<float>("Level" + LevelNo + "/Time", t);
                    Star = SaveGame.Load<int>("Level" + LevelNo + "/Star", s);
                    Items = SaveGame.Load<int>("Level" + LevelNo + "/Items", i);
                    Jumped = SaveGame.Load<int>("Level" + LevelNo + "/Jumped", j);
                    Restarted = SaveGame.Load<int>("Level" + LevelNo + "/Restarted", r);
                    Score = SaveGame.Load<int>("Level" + LevelNo + "/Score", sc);

                    //print("3. Load stats level" + LevelNo + " Time: " + Time + ", star: " + Star + ", items: " + Items +
                        //", jumped: " + Jumped + ", restarted: " + Restarted + ", score: " + Score);

                    if (gameManager != null)
                        gameManager.bestTime.text = "Best " + Time.ToString("#0");
                } else
                {
                    SaveGame.Save("Level" + LevelNo + "/Time", Time);
                    SaveGame.Save("Level" + LevelNo + "/Star", Star);
                    SaveGame.Save("Level" + LevelNo + "/Items", Items);
                    SaveGame.Save("Level" + LevelNo + "/Jumped", Jumped);
                    SaveGame.Save("Level" + LevelNo + "/Restarted", Restarted);
                    SaveGame.Save("Level" + LevelNo + "/Score", Score);

                    print("New Level saved: " + levelNo);
                }

            }

        }

    }
}