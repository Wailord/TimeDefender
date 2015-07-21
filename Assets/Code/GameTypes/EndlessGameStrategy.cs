using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.GameTypes
{
    public class EndlessGameStrategy : AbstractGameStrategy
    {
        public override bool GameWon()
        {
            return false;
        }

        public override double CalcTotalScore()
        {
            throw new NotImplementedException();
        }
    }
}
