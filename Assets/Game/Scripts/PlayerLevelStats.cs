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
        private int levelNo;
        private float time, t;
        private int star, s;
        private int items, i;
        private int jumped, j;
        private int restarted, r;
        public int score, sc;

        public List<PlayerLevelStats> levelStats = new List<PlayerLevelStats>();

        private void Awake()
        {
            gameManager = gameObject.GetComponent<GameManager>();

            if (!menuManagerFound)
                menuManagerFound = GameObject.Find("MainMenuManager");
        }

        private void SaveLevelStats(float time, int star, int items, int jumped, int restarted, int score)
        {
            this.time = time;
            this.star = star;
            this.items = items;
            this.jumped = jumped;
            this.restarted = restarted;
            this.score = score;
        }

        public void SaveLevel(int _levelNo, float _time, int _star, int _items, int _jumped, int _restarted, int _score)
        {
            var chapter = MainMenuManager.Instance.chapter.ToString("0");
            var levelString = chapter + levelNo.ToString("00");

            if (int.TryParse(levelString, out _levelNo))
            {
                if (SaveGame.Exists("Level" + _levelNo))
                {
                    time = CheckBest(
                        SaveGame.Load<float>("Level" + levelNo + "/Time", t),
                        _time);

                    star = (int)CheckBest(
                        SaveGame.Load<int>("Level" + levelNo + "/Star", s),
                        _star);

                    items = (int)CheckBest(
                        SaveGame.Load<int>("Level" + levelNo + "/Items", i),
                        _items);

                    jumped = (int)CheckBest(
                        SaveGame.Load<int>("Level" + levelNo + "/Jumped", j),
                        _jumped);

                    restarted = (int)CheckBest(
                        SaveGame.Load<int>("Level" + levelNo + "/Restarted", r),
                        _restarted);
                    score = (int)CheckBest(
                        SaveGame.Load<int>("Level" + levelNo + "/Score", sc),
                        _score);

                    SaveLevelStats(time, star, items, jumped, restarted, score);
                }
                else
                {
                    PlayerLevelStats playerLevelStats = new PlayerLevelStats
                    {
                        time = _time,
                        star = _star,
                        items = _items,
                        jumped = _jumped,
                        restarted = _restarted,
                        score = _score
                    };

                }
            }

            print("2b. Saved levelStats. Level: " + _levelNo + ", Time: " + _time + ", star: " + _star + ", items: " + _items +
                ", jumped: " + _jumped + ", restarted: " + _restarted + ", score: " + _score);

            SaveGame.Save("Level" + _levelNo + "/Time", _time);
            SaveGame.Save("Level" + _levelNo + "/Star", _star);
            SaveGame.Save("Level" + _levelNo + "/Items", _items);
            SaveGame.Save("Level" + _levelNo + "/Jumped", _jumped);
            SaveGame.Save("Level" + _levelNo + "/Restarted", _restarted);
            SaveGame.Save("Level" + _levelNo + "/Score", _score);
            
        }

        public void ClearStats()
        {
            SaveGame.Clear();

            time = 0;
            star = 0;
            items = 0;
            jumped = 0;
            restarted = 0;
            score = 0;

            print("All Saves cleared.");
        }

        private float CheckBest(float current, float latest)
        {
            if (current > latest)
                return current;
            
            return latest;
        }

        // Todo - not used?
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
            int chapter = ChaptersAndLevels.chapterNo;

            var levelString = chapter.ToString("0") + levelNo.ToString("00");

            if (int.TryParse(levelString, out levelNo))
            {
                if (SaveGame.Exists("Level" + levelNo))
                {
                    //print("loading stats for level" + LevelNo);
                    // 101 = chapter 1, level 01

                    time = SaveGame.Load<float>("Level" + levelNo + "/Time", t);
                    star = SaveGame.Load<int>("Level" + levelNo + "/Star", s);
                    items = SaveGame.Load<int>("Level" + levelNo + "/Items", i);
                    jumped = SaveGame.Load<int>("Level" + levelNo + "/Jumped", j);
                    restarted = SaveGame.Load<int>("Level" + levelNo + "/Restarted", r);
                    score = SaveGame.Load<int>("Level" + levelNo + "/Score", sc);

                    //print("3. Load stats level" + LevelNo + " Time: " + Time + ", star: " + Star + ", items: " + Items +
                        //", jumped: " + Jumped + ", restarted: " + Restarted + ", score: " + Score);

                    if (gameManager != null)
                        gameManager.bestTime.text = "Best " + time.ToString("#0"); // "#0"
                } else
                {
                    SaveGame.Save("Level" + levelNo + "/Time", time);
                    SaveGame.Save("Level" + levelNo + "/Star", star);
                    SaveGame.Save("Level" + levelNo + "/Items", items);
                    SaveGame.Save("Level" + levelNo + "/Jumped", jumped);
                    SaveGame.Save("Level" + levelNo + "/Restarted", restarted);
                    SaveGame.Save("Level" + levelNo + "/Score", score);

                    print("New Level saved: " + levelNo);
                }

            }

        }

    }
}