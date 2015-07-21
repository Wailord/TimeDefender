using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Assets.Code.Scores;
using Assets.Code.Controllers;
using Assets.Code.ErrorLog;

namespace Assets.Code.Maps
{
    class HighScoreMap : Map
    {
        List<Score> highscores;
        MapsDBController mapDBController = new MapsDBController();
        HighScoresDBController hsDBController = new HighScoresDBController();

        /// <summary>
        /// Creates a new HighScoreMap object and retrieves the maps data from the database
        /// </summary>
        /// <param name="mapID">The MapID for the map to be retrieved from the database</param>
        public HighScoreMap(int mapID)
        {
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
        }

        /// <summary>
        /// Loads the data retrieved from the database into the map object
        /// </summary>
        /// <param name="mapData">The DataSet retrieved from the database</param>
        private void LoadMap(DataSet mapData)
        {
            DataRow Coolio = mapData.Tables[0].Rows[0];

            mapID = Convert.ToInt32(Coolio["MapID"]);
            difficulty = Convert.ToInt32(Coolio["Difficulty"]);
            startingResources = Convert.ToInt32(Coolio["StartingResources"]);
            startingLives = Convert.ToInt32(Coolio["StartingLives"]);

            //ParseStringToMapData(Coolio["MapData"].ToString());

            RetrieveHighScores();
        }

        /// <summary>
        /// Loads all highscores for the loaded map into the list of HighScores
        /// </summary>
        private void RetrieveHighScores()
        {
            DataSet highscoreData = hsDBController.RetrieveScoresByMap(mapID);

            try
            {
                highscoreData = mapDBController.RetrieveSpecificMap(mapID);

                if (!(highscoreData.Tables[0].Rows.Count == 0))
                {
                    LoadHighScores(highscoreData);
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
        }

        /// <summary>
        /// Loads the maps highscores into the list of highscores
        /// </summary>
        /// <param name="highscoreData">The DataSet retured by the database containing all the highscores for the given map</param>
        private void LoadHighScores(DataSet highscoreData)
        {
            DataRow row;

            for (int x = 0; x < highscoreData.Tables[0].Rows.Count; x++)
            {
                row = highscoreData.Tables[0].Rows[x];

                int pID = Convert.ToInt32(row["PlayerID"]);
                int type = Convert.ToInt32(row["GameType"]);
                long res = Convert.ToInt32(row["Resources"]);
                int enemies = Convert.ToInt32(row["EnemiesKilled"]);
                int waves = Convert.ToInt32(row["WavesCompleted"]);
                int lives = Convert.ToInt32(row["LivesRemaining"]);
                double time = Convert.ToDouble(row["TimePlayed"]);
                string replay = row["ReplayData"].ToString();
                int totalScore = Convert.ToInt32(row["TotalScore"]);

                Score score = new Score(pID, mapID, res, enemies, waves, lives, type, time, totalScore, replay);
                highscores.Add(score);
            }
        }
    }
}
