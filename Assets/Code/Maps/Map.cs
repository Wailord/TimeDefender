using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Enums;
using Assets.Code.MapGeneration;
using Assets.Code.ErrorLog;

namespace Assets.Code.Maps
{
    class Map
    {
        public const int GRID_WIDTH = 20;
        public const int GRID_HEIGHT = 20;

        protected int mapID;
        protected int difficulty;
        protected int gameType;
        protected int startingResources;
        protected int startingLives;
        protected TileType[,] mapTiles;

        public int MapID
        {
            get { return mapID; }
        }

        public int Difficulty
        {
            get { return difficulty; }
        }

        public int GameType
        {
            get { return gameType; }
        }

        public int StartingResources
        {
            get { return startingResources; }
        }

        public int StartingLives
        {
            get { return startingLives; }
        }

        public TileType[,] MapTiles
        {
            get { return mapTiles; }
        }

         //<summary>
         //Takes a string of mapData from the server and parses it to an array of TileTypes
         //</summary>
         //<param name="mapData">The string of map data</param>
        protected void ParseStringToMapData(string mapData)
        {
            
            //TODO: Redo this
        }

         //<summary>
         //Takes an array of TileTypes and parses it into a string
         //</summary>
         //<returns>The string containing the map data</returns>
        protected string ParseMapDataToString()
        {
            string mapData = "";

            //TODO: redo this

            return mapData;
        }
    }
}
