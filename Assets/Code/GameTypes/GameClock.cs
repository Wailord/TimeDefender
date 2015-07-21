using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Enums;
using Assets.Code.Controllers;

namespace Assets.Code.GameTypes
{
    class GameClock
    {
        protected static WeakReference _instance = new WeakReference(null);
        protected static object _lock = new Object();

        public double StartTime
        { get { return startTime; } }
        protected static double startTime;
        protected static double timePlayed;
        protected static bool paused;

        /// <summary>
        /// Default constructor for GameClock
        /// </summary>
        protected GameClock()
        {
            startTime = 0;
            timePlayed = 0;
            paused = false;
        }

        /// <summary>
        /// Creates an instance of GameClock if one does not already exist
        /// </summary>
        /// <returns>The instance of the GameClock</returns>
        public static GameClock GetInstance()
        {
            GameClock strongReference = (GameClock)_instance.Target;
            if (strongReference == null)
            {
                lock (_lock)
                {
                    if (strongReference == null)
                    {
                        strongReference = new GameClock();
                        _instance = new WeakReference(strongReference);
                    }
                }
            }

            return strongReference;
        }

        /// <summary>
        /// Sets the starting time
        /// </summary>
        public void StartClock()
        {
            startTime = TimeController.Instance.GetUtcTime(0);
        }

        /// <summary>
        /// Pauses the total running time
        /// </summary>
        public void PauseClock(PauseTypes pause)
        {
            if (pause == PauseTypes.WavePause)
            {
                if (!paused)
                {
                    timePlayed -= TimeController.Instance.GetTimeDifferenceInSeconds(Convert.ToInt64(startTime));
                    paused = true;
                    CombatController.Instance.CurrentCombatState = CombatState.OutOfCombat;
                }
                else
                {
                    startTime = TimeController.Instance.GetUtcTime(0);
                    paused = false;
                    CombatController.Instance.CurrentCombatState = CombatState.InCombat;
                }
            }
            else if (pause == PauseTypes.GamePause)
            {
                if (!paused)
                {
                    timePlayed -= TimeController.Instance.GetTimeDifferenceInSeconds(Convert.ToInt64(startTime));
                    paused = true;
                    CombatController.Instance.CurrentCombatState = CombatState.PausedCombat;
                }
                else
                {
                    startTime = TimeController.Instance.GetUtcTime(0);
                    paused = false;
                    CombatController.Instance.CurrentCombatState = CombatState.InCombat;
                }
            }
            
        }

        /// <summary>
        /// Determines whether or not the clock is paused
        /// </summary>
        /// <returns>Returns the state of paused</returns>
        public bool IsPaused()
        {
            return paused;
        }

        /// <summary>
        /// Stores the total time played
        /// </summary>
        public double EndClock()
        {
            paused = true;
            timePlayed -= Convert.ToDouble(TimeController.Instance.GetTimeDifferenceInSeconds(Convert.ToInt64(startTime)));
            return timePlayed;
        }

        /// <summary>
        /// Returns the current amount of time spent in game (not including time spent paused)
        /// </summary>
        /// <returns>The time played as a string</returns>
        public string GetCurrentTimePlayed()
        {
            if (paused)
                return ConvertTimeToString(timePlayed);
            else
                return ConvertTimeToString(timePlayed - Convert.ToDouble(TimeController.Instance.GetTimeDifferenceInSeconds(Convert.ToInt64(startTime))));
        }

        public double GetCurrentTimePlayedAsDouble()
        {
            if (paused)
                return timePlayed;
            else
                return timePlayed - Convert.ToDouble(TimeController.Instance.GetTimeDifferenceInSeconds(Convert.ToInt64(startTime)));
        }

        /// <summary>
        /// Takes a double (the amount of seconds played) and converts it to a string in the 0:00:00.00 format
        /// </summary>
        /// <param name="time"></param>
        /// <returns>The time string</returns>
        private string ConvertTimeToString(double time)
        {
            // doesnt display this for some reason
            //TimeSpan ts = TimeSpan.FromTicks((long)time);
            //string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            //            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            //return elapsedTime;

            double hour = 0;
            double minute = 0;
            double second = 0;
            string timeAsString = "";

            if (Math.Floor((time / 60) / 60) >= 1)
                hour = Math.Floor(((time / 60) / 60));

            if (Math.Floor((time / 60)) >= 1)
            {
                minute = Math.Floor(time / 60);
                minute -= hour * 60;
            }

            second = time - (minute * 60) - (hour * 60 * 60);

            
            timeAsString = String.Format("{0:00}:{1:00}:{2:00}", hour, minute, second);
            //return timeAsString;

            return hour + ":" + minute + ":" + Math.Round(second, 2);
        }
    }
}
