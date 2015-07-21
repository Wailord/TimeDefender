///Documentation to come
///GameScore object to be used to store the current games components, which will later be used to calculate the total score
///Game components (such as resources, and lives) will also be used to determine if a player can build a tower or has lost the game
///The timePlayed component can also be used to determine if the game has ended depending on the game type

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.ErrorLog;
using Assets.Code.Controllers;
using Assets.Code.GameTypes;
using Assets.Code.GUICode;

namespace Assets.Code.Scores
{
    class GameScore : Score
    {
        protected static WeakReference _instance = new WeakReference(null);
        protected static object _lock = new Object();

        public delegate bool CheckWinCondition();
        public delegate double CalcTotalScore();

        public CheckWinCondition WinCheck;
        public CalcTotalScore TotalScoreCalc;

        private double _interestTimer = 0;
        private double _baseInterestTime = 0;
        private double _baseDifference = 0;
        private double _interestDifference = 0;

        /// <summary>
        /// Protected initial constructor, should be called at the start of the game
        /// </summary>
        /// <param name="type">The game type</param>
        /// <param name="lives">The number of starting lives</param>
        /// <param name="res">The amount of starting resources</param>
        protected GameScore()
        {
            WinCheck = null;
            TotalScoreCalc = null;
            resources = 0;
            enemiesKilled = 0;
            wavesCompleted = 0;
            livesRemaining = 0;
            timePlayed = 0;
            replay = "";
            totalScore = 0.0;
        }

        /// <summary>
        /// Gets the current GameScore instance
        /// </summary>
        /// <returns>Returns an instance of the GameScore object</returns>
        public static GameScore GetInstance()
        {
            GameScore strongReference = (GameScore)_instance.Target;
            if (strongReference == null)
            {
                lock (_lock)
                {
                    if (strongReference == null)
                    {
                        strongReference = new GameScore();
                        _instance = new WeakReference(strongReference);
                    }
                }
            }

            return strongReference;
        }

        /// <summary>
        /// Initializes GameScore object with the starting values for the game as 
        /// well as the functions to calculate total score to check for the win condition
        /// </summary>
        /// <param name="lives">The amount of lives the player will start with</param>
        /// <param name="res">The amount of resources the player will start with</param>
        /// <param name="strat">The strategy to be used to calculate score and check for the win condition</param>
        public void Initialize(int lives, int res, AbstractGameStrategy strat)
        {
            livesRemaining = lives;
            resources = res;
            WinCheck = strat.GameWon;
            wavesCompleted = 1;
            TotalScoreCalc = strat.CalcTotalScore;
        }

        /// <summary>
        /// Removes resources when a player builds a tower
        /// </summary>
        /// <param name="res">The amount of resources to be removed</param>
        /// <returns></returns>
        public bool RemoveResources(int res)
        {
            bool removed = false;

            if ((resources - res) >= 0)
            {
                resources -= res;
                removed = true;
            }

            return removed;
        }

        /// <summary>
        /// Adds resources when an enemy is killed
        /// </summary>
        /// <param name="res">The number of resources to be added</param>
        public void AddResources(int res)
        {
            resources += res;
        }

        /// <summary>
        /// Increments the number of enemies killed
        /// </summary>
        public void IncrementEnemiesKilled()
        {
            enemiesKilled++;
        }

        /// <summary>
        /// Increments the wave the player is currently on
        /// </summary>
        public void IncrementWavesCompleted()
        {
            wavesCompleted++;
        }

        /// <summary>
        /// Deducts a life from the player, to be called when an enemy reaches the end of the path
        /// </summary>
        public void LoseLife()
        {
            if (livesRemaining > 0)
                livesRemaining--;
        }

        /// <summary>
        /// Determines whether or not the player has lost
        /// </summary>
        /// <returns>True or false depending on whether or not the amount of lives remaining is greater than 0</returns>
        public bool LossCheck()
        {
            return livesRemaining <= 0;
        }

        /// <summary>
        /// Adds the players move into the replay string
        /// </summary>
        /// <param name="move">A string that denotes an event during gameplay along with a timestamp</param>
        public void AddMove(string move)
        {
            replay = replay + ',' + move + ' ' + GameClock.GetInstance().GetCurrentTimePlayed();
        }

        /// <summary>
        /// User passively increases money in bank for not spending it. As more time goes by, more money is accrued.
        /// Currently caps at 10TF/3sec
        /// </summary>
        public void PassiveMoneyGain()
        {
            // check if user spent money
            if (TileInterface.MoneySpent)
            {
                // reset base time
                _baseInterestTime = GameClock.GetInstance().GetCurrentTimePlayedAsDouble();
                // reset money spent flag
                TileInterface.MoneySpent = false;
            }
            // use this to measure how long it's been since user has spent money
            _baseDifference = GameClock.GetInstance().GetCurrentTimePlayedAsDouble() - _baseInterestTime;
            // use this to add resources at a certain amount
            _interestDifference = GameClock.GetInstance().GetCurrentTimePlayedAsDouble() - _interestTimer;

            // makes call if < 30 and > 4 every 2 seconds
            if (_baseDifference <= 30 && _baseDifference > 4 && _interestDifference > 2)
            {
                // user gets 1 Time Fragment every 2 seconds
                GameScore.GetInstance().AddResources(1);
                // reset TF incrementer
                _interestTimer = GameClock.GetInstance().GetCurrentTimePlayedAsDouble();
            }
            // makes call if > 30 and <= 60 every 3 seconds
            else if (_baseDifference > 30 && _baseDifference <= 60 && _interestDifference > 3)
            {
                GameScore.GetInstance().AddResources(5);
                _interestTimer = GameClock.GetInstance().GetCurrentTimePlayedAsDouble();
            }
            // makes call if > 60 every 3 seconds
            else if (_baseDifference > 30 && _interestDifference > 3)
            {
                GameScore.GetInstance().AddResources(10);
                _interestTimer = GameClock.GetInstance().GetCurrentTimePlayedAsDouble();
            }
        }
    }
}
