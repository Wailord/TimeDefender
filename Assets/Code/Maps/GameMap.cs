using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Scores;
using Assets.Code.MapGeneration;
using Assets.Code.ErrorLog;
using Assets.Code.Enums;
using Assets.Code.Controllers;
using Assets.Code.GameTypes;

namespace Assets.Code.Maps
{
    class GameMap : Map
    {
        protected static WeakReference _instance = new WeakReference(null);
        protected static object _lock = new Object();

        protected HighScoresDBController highscore_controller;

        private GameScore score;
        private TileType[,] map;

        /// <summary>
        /// Creates a new GameMap object
        /// </summary>
        protected GameMap()
        {
            difficulty = 0;
            mapID = 0;
            highscore_controller = null;
            gameType = 0;
            map = null;
            score = null;
        }

        /// <summary>
        /// Returns a reference to the GameMap instance. If no instance exists, throws an error.
        /// </summary>
        /// <returns>Returns a reference to the GameMap instance</returns>
        public static GameMap GetInstance()
        {
            GameMap strongReference = (GameMap)_instance.Target;
            if (strongReference == null)
            {
                lock (_lock)
                {
                    if (strongReference == null)
                    {
                        strongReference = new GameMap();
                        _instance = new WeakReference(strongReference);
                    }
                }
            }

            return strongReference;
        }

        /// <summary>
        /// Sets up the game map with all the map components
        /// </summary>
        /// <param name="diff">The maps difficulty</param>
        /// <param name="ID">The maps ID</param>
        /// <param name="type">The game type</param>
        /// <param name="startRes">The amount of starting resources</param>
        /// <param name="startLives">The amount of starting lives</param>
        /// <param name="mapSet">The array of tiles that are the map</param>
        /// <param name="strat">The strategy to be used to determine the total score calculation and win condition</param>
        public void Initialize(int diff, int ID, int type, int startRes, int startLives, TileType[,] mapTiles, AbstractGameStrategy strat)
        {
            difficulty = diff;
            mapID = ID;

            if (mapID == 0)
                highscore_controller = null;
            else
                highscore_controller = new HighScoresDBController();

            gameType = type;
            map = mapTiles;

            score = GameScore.GetInstance();
            score.Initialize(startingLives, startingResources, strat);
        }

        /// <summary>
        /// Provides the map TileType array for the MainController to use in the game
        /// </summary>
        /// <returns></returns>
        public TileType[,] GetMap()
        {
            return map;
        }

        /// <summary>
        /// Checks to see if the player is logged in, if so calculates the total score and attempts
        /// to store it on the server. If not, calculates the total score and displays a message prompting 
        /// the user to log in to store future scores.
        /// </summary>
        /// <param name="mapID">The ID for the map which the score is for</param>
        /// <param name="playerID">The ID for the player which the score is for</param>
        /// <param name="session">The session string for the player which the score is for</param>
        /// <returns></returns>
        public bool StoreHighScore(int playerID, string session)
        {
            bool scoreStored = false;

            if(highscore_controller != null)
            {
                try
                {
                    scoreStored = highscore_controller.InsertHighScore(mapID, playerID, session, gameType, score.TotalScore, score.WavesCompleted, score.EnemiesKilled, score.TimePlayed, score.Resources, score.LivesRemaining);

                    if (!scoreStored)
                        throw new Exception("Log in to an account to save your high scores!");
                }
                catch (Exception except)
                {
                    GeneralLogger.GetInstance().AddError(except.Message);
                }
            }
            
            return scoreStored;
        }
    }
}
