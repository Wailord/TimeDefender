using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Enums;
using System.Data;
using Assets.Code.ErrorLog;
using Assets.Code.Controllers;
using Assets.Code.MapGeneration;
using Assets.Code.GameTypes;

namespace Assets.Code.Maps
{
    class MapInitializer : Map
    {
        private MapsDBController mapDBController;

        private GameMap map;
        private AbstractGameStrategy strat;

        public MapInitializer()
        {
            mapDBController = new MapsDBController();
            mapID = 0;
            difficulty = 0;
            startingResources = 0;
            startingLives = 0;
            mapTiles = new TileType[GRID_WIDTH, GRID_HEIGHT];
            strat = null;
            map = null;
        }

        /// <summary>
        /// Creates a new map given the parameters set by the user
        /// </summary>
        /// <returns>A boolean value depending on whether input was valid</returns>
        public bool CreateNewMap(int diff, int startRes, int startLives)
        {
            bool created = true;

            mapTiles = MapGeneration.MapGeneration.GenerateMap();

            //Ensures that the difficulty is valid
            try
            {
                if (diff <= 3 && diff >= 1)
                    difficulty = diff;
                else
                    throw new Exception("Invalid game difficulty");
            }
            catch (Exception except)
            {
                GeneralLogger.GetInstance().AddError(except.Message);
                created = false;
            }

            //Ensures that the starting resources is valid
            try
            {
                if (startRes > 0)
                    startingResources = startRes;
                else
                    throw new Exception("Starting resources must be greater than 0");
            }
            catch (Exception except)
            {
                GeneralLogger.GetInstance().AddError(except.Message);
                created = false;
            }

            //Ensures that the starting lives is valid
            try
            {
                if (startLives > 0)
                    startingLives = startLives;
                else
                    throw new Exception("Starting lives must be greater than 0");
            }
            catch (Exception except)
            {
                GeneralLogger.GetInstance().AddError(except.Message);
                created = false;
            }
            
            StoreMap(mapTiles);

            return created;
        }

        /// <summary>
        /// Sets the game type
        /// </summary>
        /// <param name="type">The game type</param>
        /// <param name="endRes">The amount of ending resources</param>
        /// <param name="endTime">The ending time</param>
        public bool SetGameType(int type, int endRes, long endTime)
        {
            gameType = type;
            bool success = false;

            switch (gameType)
            {
                case 1:
                    strat = new EndlessGameStrategy();
                    success = true;

                    break;
                case 2:
                    strat = new TimedGameStrategy(endTime);

                    if (endTime <= 0)
                    {
                        GeneralLogger.GetInstance().AddError("You must enter a valid time");
                    }
                    else
                        success = true;

                    break;
                case 3:
                    strat = new CollectXResourcesStrategy(endRes);

                    if (endRes <= startingResources)
                    {
                        GeneralLogger.GetInstance().AddError("Ending resources cannot be less than the starting resources for the map");
                    }
                    else
                        success = true;

                    break;
            }

            return success;
        }

        /// <summary>
        /// Creates the GameMap
        /// </summary>
        /// <returns>Returns the new GameMap</returns>
        public GameMap CreateGameMap()
        {
            map = GameMap.GetInstance();
            map.Initialize(difficulty, mapID, gameType, startingResources, startingLives, mapTiles, strat);

            return map;
        }

        /// <summary>
        /// Stores the newly created map in the database, assuming a connection could be established
        /// </summary>
        private void StoreMap(TileType[,] mapTiles)
        {
            int stored = 0;

            try
            {
                stored = mapDBController.StoreMap(mapTiles.ToString(), difficulty, startingResources, startingLives);

                if (!Convert.ToBoolean(stored))
                    throw new Exception("Could not store map. Please check your internet connection");
            }
            catch (Exception except)
            {
                GeneralLogger.GetInstance().AddError(except.Message);
            }

            mapID = stored;
        }

        /// <summary>
        /// Retrieves a specific map from the server and loads it for gameplay
        /// </summary>
        /// <param name="mapID">The MapID for the map to be loaded</param>
        /// <returns>A boolean value determined by whether or not the map could be loaded</returns>
        public bool RetrieveSpecificMap(int mapID)
        {
            bool retrieved = false;
            DataSet mapData = new DataSet();

            try
            {
                mapData = mapDBController.RetrieveSpecificMap(mapID);

                if (!(mapData.Tables[0].Rows.Count == 0))
                {
                    LoadMap(mapData);
                }
                else
                {
                    throw new Exception("Could not retrieve the specified map from the server.");
                }
            }
            catch (Exception except)
            {
                GeneralLogger.GetInstance().AddError(except.Message);
            }

            return retrieved;
        }

        /// <summary>
        /// Retrieves a random map from the server
        /// </summary>
        /// <returns>A boolean value determined by whether or not a random map could be loaded from the server</returns>
        public bool RetrieveRandomMap()
        {
            bool retrieved = false;
            DataSet mapData = new DataSet();

            try
            {
                mapData = mapDBController.RetrieveRandomMap();

                if (!(mapData.Tables[0].Rows.Count == 0))
                {
                    LoadMap(mapData);
                }
                else
                {
                    throw new Exception("Could not retrieve a random map from the server.");
                }
            }
            catch (Exception except)
            {
                GeneralLogger.GetInstance().AddError(except.Message);
            }

            return retrieved;
        }

        /// <summary>
        /// Loads the map retrieved from the server into a new GameMap object
        /// </summary>
        /// <param name="mapData">A DataSet containing the maps data retrieved from the server</param>
        private void LoadMap(DataSet mapData)
        {
            DataRow Coolio = mapData.Tables[0].Rows[0];

            mapID = Convert.ToInt32(Coolio["MapID"]);
            difficulty = Convert.ToInt32(Coolio["Difficulty"]);
            startingResources = Convert.ToInt32(Coolio["StartingResources"]);
            startingLives = Convert.ToInt32(Coolio["StartingLives"]);

            ParseStringToMapData(Coolio["MapData"].ToString());
        }
    }
}
