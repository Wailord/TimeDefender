using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Scores;

namespace Assets.Code.GameTypes
{
    public class TimedGameStrategy : AbstractGameStrategy
    {
        private long winTime;

        public long WinTime
        {
            get { return winTime; }
        }

        public TimedGameStrategy(long timeToWin)
        {
            winTime = timeToWin;
        }

        public override bool GameWon()
        {
            return (winTime <= GameScore.GetInstance().TimePlayed);
        }

        public override double CalcTotalScore()
        {
            throw new NotImplementedException();
        }
    }
}
