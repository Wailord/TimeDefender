using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.GameTypes;
using Assets.Code.Interfaces;
using Assets.Code.Enums;
using Assets.Code.Scores;
using Assets.Code.GUICode;
using Assets.Code.Maps;
using UnityEngine;

namespace Assets.Code.Controllers
{
    class WaveController : IServiceable
    {
        protected static WeakReference _instance = new WeakReference(null);
        protected static object _lock = new System.Object();

        /// <summary>
        /// The list of units for the current wave
        /// </summary>
        public List<UnitTypes> UnitList
        { get { return unitList; } }
        private List<UnitTypes> unitList;

        /// <summary>
        /// Returns the number of enemies left in the wave
        /// </summary>
        public int EnemiesRemaining
        { get { return unitList.Count; } }

        /// <summary>
        /// Returns the number of enemies currently spawned on the map (RemoveEnemy() decrements this value)
        /// </summary>
        public int EnemiesInGame
        { get { return enemiesInGame; } }
        private int enemiesInGame;

        /// <summary>
        /// Called to dedeuct from the amount of enemies in the game (when an enemy dies or reaches the end of the map)
        /// </summary>
        public void RemoveEnemy()
        { enemiesInGame--; }

        /// <summary>
        /// seed is the number which is specific for each map that will be used
        /// to generate the same random enemy waves for that map.
        /// </summary>
        public int Seed
        { get { return seed; } }
        private int seed;

        /// <summary>
        /// Keeps track of the lower and upper bounds for the number of
        /// enemies that can be generated for the wave, as well as the time
        /// til the next enemy spawn.
        /// </summary>
        private double lowerWaveBound;
        private double upperWaveBound;

        /// <summary>
        /// Keeps track of the time the player completes a wave and the time
        /// until the next enemy is spawned.
        /// </summary>
        private double startOfHoldPeriod;
        private double timeTilNextEnemySpawn;

        /// <summary>
        /// Returns the time until the next wave
        /// </summary>
        public double TimeTilNextWave
        { get { return TIME_TIL_NEXT_WAVE + Convert.ToDouble(TimeController.Instance.GetTimeDifferenceInSeconds(Convert.ToInt64(startOfHoldPeriod))); } }

        System.Random rand;
        private static double MAX_TIME_BETWEEN_SPAWNS = 3.0;
        private static double MIN_TIME_BETWEEN_SPAWNS = 0.0;
        private static int START_LOWER_WAVE_BOUND = 1;
        private static int START_UPPER_WAVE_BOUND = 4;
        private static double TIME_TIL_NEXT_WAVE = 5.0;

        protected WaveController()
        {

        }

        /// <summary>
        /// If no instance of the wave controller exists, creates a new one
        /// </summary>
        /// <returns>Returns an instance of the wave controller</returns>
        public static WaveController GetInstance()
        {
            WaveController strongReference = (WaveController)_instance.Target;
            if (strongReference == null)
            {
                lock (_lock)
                {
                    if (strongReference == null)
                    {
                        strongReference = new WaveController();
                        _instance = new WeakReference(strongReference);
                    }
                }
            }

            return strongReference;
        }

        /// <summary>
        /// Initializes the wave controller with a seed value for the randon number generator.s
        /// The seed value will be the same every playthrough of one map, therefore each time one
        /// map is played, the waves will be the same.
        /// </summary>
        /// <param name="mapSeed">The maps seed value</param>
        public void Initialize(int mapSeed)
        {
            seed = mapSeed;
            unitList = new List<UnitTypes>();

            lowerWaveBound = START_LOWER_WAVE_BOUND;
            upperWaveBound = START_UPPER_WAVE_BOUND;
            timeTilNextEnemySpawn = 0.0;

            rand = new System.Random(seed);
        }

        /// <summary>
        /// Sets the waves difficulty multiplier based off of the maps difficulty and the wave number
        /// </summary>
        /// <param name="difficulty">The maps difficulty</param>
        /// <param name="wave">The current wave number</param>
        private void GenerateWaveMultiplier(MapDifficulty difficulty, int wave)
        {

        }

        /// <summary>
        /// Generates a list of UnitTypes that will be spawned during the current wave
        /// </summary>
        public void GenerateWave()
        {
            //Boolean value to determine if the enemy spawned should be a boss or not
            bool first = true;

            //Retrieves difficulty from the GameMap object
            //Hardcoded value until we start using the GameMap objects
            //MapDifficulty diff = (MapDifficulty)(GameMap.GetInstance().Difficulty);
            MapDifficulty diff = MapDifficulty.Easy;

            //Retrieves the wave number from the GameScore object
            int wave = GameScore.GetInstance().WavesCompleted;

            //Generates the wave multiplier used to scale up enemies
            GenerateWaveMultiplier(diff, wave);

            //Calculates the number of enemies the current wave will be populated with
            int numberOfEnemies = rand.Next((int)Math.Ceiling(lowerWaveBound), (int)Math.Ceiling(upperWaveBound));

            //Stores the values that will be used to determine the next waves number of enemies
            double temp = lowerWaveBound;
            lowerWaveBound = (double)numberOfEnemies;
            upperWaveBound = (upperWaveBound + temp) * ((double)diff / 3) + (lowerWaveBound / 1.1);

            //Debug.Log("Lower wave bound: " + lowerWaveBound);
            //Debug.Log("Upperwave bound: " + upperWaveBound);

            //If it is a boss wave, and the first enemy is being added, makes that enemy a boss
            if (wave % 10 == 0 && first == true)
            {
                unitList.Add(UnitTypes.Boss);
                first = false;
            }

            //All this logic will change
            //Randomly adds an enemy to the enemy list, decrements the number of enemies left to be added
            for (; numberOfEnemies > 0; numberOfEnemies--)
            {
                int nextEnemy = rand.Next(7);

                switch (nextEnemy)
                {
                    case 6:
                        unitList.Add(UnitTypes.Tric);
                        numberOfEnemies -= 4;
                        break;
                    case 5:
                        unitList.Add(UnitTypes.Swat);
                        numberOfEnemies -= 3;
                        break;
                    case 4:
                        unitList.Add(UnitTypes.Soldier);
                        numberOfEnemies -= 2;
                        break;
                    case 3:
                        unitList.Add(UnitTypes.Knight);
                        numberOfEnemies -= 1;
                        break;
                    case 2:
                        unitList.Add(UnitTypes.Caveman);
                        break;
                    case 1:
                        unitList.Add(UnitTypes.WalkingKappa);
                        break;
                    case 0:
                        unitList.Add(UnitTypes.FlyingKappa);
                        break;
                }
            }

            //Sets the enemies in game to the size of the enemy list
            enemiesInGame = unitList.Count;
        }

        /// <summary>
        /// Stores the time at which the player completes a wave so that the
        /// next wave can be started TIME_TIL_NEXT_WAVE seconds from that point
        /// </summary>
        public void SetTimeTilNextWave()
        {
            startOfHoldPeriod = TimeController.Instance.GetUtcTime(0);
        }

        /// <summary>
        /// Sets the delay between enemy spawns
        /// </summary>
        public void SetTimeForNextEnemy()
        {
            //This function needs work
            //Have not decided on an algorithm to determine the time between enemy spawns yet
            //* (Math.Pow(MAX_TIME_BETWEEN_SPAWNS, -1 * (double)MapDifficulty.Easy))
            timeTilNextEnemySpawn = GameClock.GetInstance().GetCurrentTimePlayedAsDouble() + (rand.NextDouble() * ((MAX_TIME_BETWEEN_SPAWNS / GameScore.GetInstance().WavesCompleted) - MIN_TIME_BETWEEN_SPAWNS) + MIN_TIME_BETWEEN_SPAWNS);
            if (timeTilNextEnemySpawn < 0.01) { timeTilNextEnemySpawn = 0.01; }
        }

        /// <summary>
        /// Called each tick, spawns an enemy if the game time is at or beyond the timeTilNextEnemySpawn
        /// </summary>
        public void Service()
        {
            //So long as the list of enemies has enemies to be spawned, and so long as the CombatController state is InCombat
            if (unitList.Count != 0 && CombatController.Instance.CurrentCombatState == CombatState.InCombat)
            {
                //If the current time is at or past the time til next enemy spawn, spawns the enemy and removes it from the enemy list
                if (timeTilNextEnemySpawn <= GameClock.GetInstance().GetCurrentTimePlayedAsDouble())
                {
                    CombatController.Instance.SpawnCombatantOfType(unitList[0], Faction.LemmingSide, null);
                    unitList.RemoveAt(0);
                    SetTimeForNextEnemy();
                }
            }
        }
    }
}
