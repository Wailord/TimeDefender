using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Controllers;

namespace Assets.Code.Scores
{
    class Score
    {
        protected long resources;
        protected int enemiesKilled;
        protected int wavesCompleted;
        protected int livesRemaining;
        protected int gameType;
        protected double timePlayed;
        protected string replay;
        protected double totalScore;

        public Score()
        {

        }

        public Score(int pID, int mID, long res, int enemy, int wave, int life, int type, double time, double score, string replay)
        {

        }

        public long Resources
        {
            get { return resources; }
        }

        public int EnemiesKilled
        {
            get { return enemiesKilled; }
        }

        public int WavesCompleted
        {
            get { return wavesCompleted; }
        }

        public int LivesRemaining
        {
            get { return livesRemaining; }
        }

        public int GameType
        {
            get { return gameType; }
        }

        public double TimePlayed
        {
            get { return timePlayed; }
        }

        public double TotalScore
        {
            get { return totalScore; }
        }
    }
}
