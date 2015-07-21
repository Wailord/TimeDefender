using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.ErrorLog
{
    /// <summary>
    /// A general logger for error messages that are generated through the DatabaseController,
    /// player, Map, and HighScore classes.
    /// </summary>
    class GeneralLogger
    {
        List<string> ErrorList = new List<string>();
        static WeakReference _instance = new WeakReference(null);
        protected static object _lock = new Object();

        private GeneralLogger()
        {
            ErrorList = new List<string>();
        }

        /// <summary>
        /// Ensures that there is only every one instance of the general logger.
        /// </summary>
        /// <returns>Returns a reference to the sole instance of the general logger</returns>
        public static GeneralLogger GetInstance()
        {
            GeneralLogger strongReference = (GeneralLogger)_instance.Target;
            if (strongReference == null)
            {
                lock (_lock)
                {
                    if (strongReference == null)
                    {
                        strongReference = new GeneralLogger();
                        _instance = new WeakReference(strongReference);
                    }
                }
            }

            return strongReference;
        }

        /// <summary>
        /// Adds the given message to the error list.
        /// </summary>
        /// <param name="error">The error message to be added to the list</param>
        public void AddError(string error)
        {
            ErrorList.Add(error);
        }

        /// <summary>
        /// If error messages exist in the logger, returns the first one in the list.
        /// Deletes the returned message.
        /// </summary>
        /// <returns>The first message stored in the list of errors, or a null value if none exist</returns>
        public string RetrieveError()
        {
            string msg = null;

            if (ErrorList.Count > 0)
            {
                msg = ErrorList.First();
                ErrorList.RemoveAt(0);
            }

            return msg;
        }
    }
}
