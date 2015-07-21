using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.GameTypes
{
    public abstract class AbstractGameStrategy
    {
        /// <summary>
        /// Determines whether or not the win condition has been met
        /// </summary>
        /// <returns>A boolean value dependant on whether or not the win condition has been met</returns>
        public abstract bool GameWon();

        /// <summary>
        /// Calculates the total score based off of the GameScore singletons member variables
        /// </summary>
        /// <returns>The total score as a double value</returns>
        public abstract double CalcTotalScore();
    }
}
