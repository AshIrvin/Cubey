using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*using BayatGames.SaveGamePro;


namespace Game.Scripts
{
    public class PlayerLevelStats : MonoBehaviour
    {
        private int chapter;
        private bool chapterUnlocked;        
        private int levelNo;
        private bool unlocked;
        private float time, t;
        private int award, a;
        private int pickups, p;
        private int jumped, j;
        private int restarted, r;
        private int score, sc;


        private void SaveLevelStats(float time, int award, int pickups, int jumped, int restarted, int score)
        {
            this.time = time;
            this.award = award;
            this.pickups = pickups;
            this.jumped = jumped;
            this.restarted = restarted;
            this.score = score;
        }

        public void SaveLevel(int _levelNo, float _time, int _award, int _pickups, int _jumped, int _restarted, int _score)
        {
            var chapter = SaveLoadManager.LastChapterPlayed;
            var levelString = chapter + levelNo.ToString("00");

            if (int.TryParse(levelString, out _levelNo))
            {
                if (SaveGame.Exists("ChapterLevel" + _levelNo))
                {
                    time = CheckBest(
                        SaveGame.Load<float>("ChapterLevel" + _levelNo + "/Time", t),
                        _time);

                    award = (int)CheckBest(
                        SaveGame.Load<int>("ChapterLevel" + _levelNo + "/Award", a),
                        _award);

                    pickups = (int)CheckBest(
                        SaveGame.Load<int>("ChapterLevel" + _levelNo + "/Pickups", p),
                        _pickups);

                    jumped = (int)CheckBest(
                        SaveGame.Load<int>("ChapterLevel" + _levelNo + "/Jumped", j),
                        _jumped);

                    restarted = (int)CheckBest(
                        SaveGame.Load<int>("ChapterLevel" + _levelNo + "/Restarted", r),
                        _restarted);
                    score = (int)CheckBest(
                        SaveGame.Load<int>("ChapterLevel" + _levelNo + "/Score", sc),
                        _score);

                    SaveLevelStats(time, award, pickups, jumped, restarted, score);
                }
                else
                {
                    PlayerLevelStats playerLevelStats = new PlayerLevelStats
                    {
                        time = _time,
                        award = _award,
                        pickups = _pickups,
                        jumped = _jumped,
                        restarted = _restarted,
                        score = _score
                    };
                }
            }

            print("2b. Saved levelStats. Level: " + _levelNo + ", Time: " + _time + ", award: " + _award + ", pickups: " + _pickups +
                ", jumped: " + _jumped + ", restarted: " + _restarted + ", score: " + _score);

            SaveGame.Save("ChapterLevel" + _levelNo + "/Time", _time);
            SaveGame.Save("ChapterLevel" + _levelNo + "/Award", _award);
            SaveGame.Save("ChapterLevel" + _levelNo + "/Pickups", _pickups);
            SaveGame.Save("ChapterLevel" + _levelNo + "/Jumped", _jumped);
            SaveGame.Save("ChapterLevel" + _levelNo + "/Restarted", _restarted);
            SaveGame.Save("ChapterLevel" + _levelNo + "/Score", _score);
        }

        public void ClearStats()
        {
            SaveGame.Clear();

            time = 0;
            award = 0;
            pickups = 0;
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

        public void LoadLevelStats(int levelNo)
        {
            int chapter = ChaptersAndLevels.chapterNo;

            var levelString = chapter.ToString("0") + levelNo.ToString("00");

            if (int.TryParse(levelString, out levelNo))
            {
                if (SaveGame.Exists("ChapterLevel" + levelNo))
                {
                    // 101 = chapter 1, level 01

                    time = SaveGame.Load<float>("ChapterLevel" + levelNo + "/Time", t);
                    award = SaveGame.Load<int>("ChapterLevel" + levelNo + "/Award", a);
                    pickups = SaveGame.Load<int>("ChapterLevel" + levelNo + "/Pickups", p);
                    jumped = SaveGame.Load<int>("ChapterLevel" + levelNo + "/Jumped", j);
                    restarted = SaveGame.Load<int>("ChapterLevel" + levelNo + "/Restarted", r);
                    score = SaveGame.Load<int>("ChapterLevel" + levelNo + "/Score", sc);
                } else
                {
                    SaveGame.Save("ChapterLevel" + levelNo + "/Time", time);
                    SaveGame.Save("ChapterLevel" + levelNo + "/Star", award);
                    SaveGame.Save("ChapterLevel" + levelNo + "/Items", pickups);
                    SaveGame.Save("ChapterLevel" + levelNo + "/Jumped", jumped);
                    SaveGame.Save("ChapterLevel" + levelNo + "/Restarted", restarted);
                    SaveGame.Save("ChapterLevel" + levelNo + "/Score", score);

                    print("New Level saved: " + levelNo);
                }
            }
        }
    }
}*/