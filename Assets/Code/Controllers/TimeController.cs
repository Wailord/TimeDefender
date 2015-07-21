using UnityEngine;
using System;

namespace Assets.Code.Controllers
{
    /// <summary>
    /// Singleton that provides services related to time comparisons, time math, etc. all in UTC time.
    /// </summary>
    public class TimeController
    {
        private static TimeController _instance;

        /// <summary>
        /// Public access to the singleton.
        /// </summary>
        public static TimeController Instance
        {
            get { return _instance ?? (_instance = new TimeController()); }
        }

        /// <summary>
        /// Private constructor for singleton.
        /// </summary>
        private TimeController()
        {
            Timescale = 1.0f;
        }

        /// <summary>
        /// The time modifier that gets applied to every time operation. The default is 1.0 (normal speed). Set to 2.0 for double speed, .5 for half speed, and so on.
        /// </summary>
        public static float Timescale { get; set; }

        /// <summary>
        /// The time differential since the last update tick, measured in SECONDS.
        /// </summary>
        public static float deltaTime { get { return Instance.GetDeltaTime(); } }

        public float GetDeltaTime()
        { return Time.deltaTime * Timescale; }

        /// <summary>
        /// Gets the current UTC time, adds the given number of seconds, and returns the result in ticks. If offsetInSeconds is 0, returns the current UTC time.
        /// </summary>
        /// <param name="offsetInSeconds">The number of seconds from which to compute the future time.</param>
        /// <returns>The future UTC time, in ticks.</returns>
        public long GetUtcTime(int offsetInSeconds)
        {
            if (offsetInSeconds == 0) return DateTime.UtcNow.Ticks;
            return DateTime.UtcNow.Ticks + new TimeSpan(0, 0, offsetInSeconds).Ticks;
        }

        public int GetUtcTimeInSeconds(int offsetInSeconds)
        {
            if (offsetInSeconds == 0) return DateTime.UtcNow.Second;
            return new TimeSpan(0, 0, offsetInSeconds).Seconds - DateTime.UtcNow.Second;
        }

        /// <summary>
        /// Gets the time difference between UTC now and the supplied time.
        /// </summary>
        /// <param name="time">The time that is to be compared with the current time. This time is expected to be a UTC time in ticks.</param>
        /// <returns>Returns a TimeSpan that represents the difference between the supplied time and now.</returns>
        public TimeSpan GetTimeDifference(long time)
        {
            return new DateTime(time) - DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the time difference between UTC now and the supplied time as a TimeSpan object.
        /// </summary>
        /// <param name="time">The time that is to be compared with the current time. This time is expected to be a UTC time.</param>
        /// <returns>Returns a TimeSpan that represents the difference between the supplied time and now.</returns>
        public TimeSpan GetTimeDifference(DateTime time)
        {
            return time - DateTime.UtcNow;
        }

        /// <summary>
        /// Convenience method that gets the time difference between UTC now and the supplied time, in seconds.
        /// </summary>
        /// <param name="time">The time that is to be compared with the current time. This time is expected to be a UTC time in ticks.</param>
        /// <returns>Returns the number of seconds (negative or positive) between the supplied time and now.</returns>
        public double GetTimeDifferenceInSeconds(long time)
        {
            TimeSpan span = new DateTime(time) - DateTime.UtcNow;
            return span.TotalSeconds;
        }

        /// <summary>
        /// Convenience method that gets the time difference between UTC now and the supplied time, in seconds.
        /// </summary>
        /// <param name="time">The time that is to be compared with the current time. This time is expected to be a UTC time.</param>
        /// <returns>Returns a TimeSpan representing the difference between the supplied time and now.</returns>
        public int GetTimeDifferenceInSeconds(DateTime time)
        {
            TimeSpan span = time - DateTime.UtcNow;
            return Convert.ToInt32(span.TotalSeconds);
        }
    }
}
