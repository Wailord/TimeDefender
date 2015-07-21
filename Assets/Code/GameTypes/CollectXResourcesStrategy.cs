using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Scores;

namespace Assets.Code.GameTypes
{
    class CollectXResourcesStrategy : AbstractGameStrategy
    {
        private long winningResources;

        public long WinningResources
        {
            get { return winningResources; }
        }

        public CollectXResourcesStrategy(long resourcesToWin)
        {
            winningResources = resourcesToWin;
        }

        public override bool GameWon()
        {
            return (winningResources <= GameScore.GetInstance().Resources);
        }

        public override double CalcTotalScore()
        {
            throw new NotImplementedException();
        }
    }
}
