using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using Assets.Code.ErrorLog;

//disables the warning that comes from an exception behing created, but never used (line 267)
#pragma warning disable 0168

namespace Assets.Code.Controllers
{
    /// <summary>
    /// Stores local variables used in all inherited DatabaseController 
    /// classes as well as the methods to be used for reporting related errors
    /// </summary>
    class DatabaseController
    {
        protected string ConnectString;
        protected string QueryString;
        protected SqlConnection connect;

        /// <summary>
        /// Opens a SqlConnection, checks to make sure it opened properly.
        /// Attempts to reconnect if the connection was not opened properly.
        /// </summary>
        protected void OpenConnection()
        {
            int connectAttempts = 0;
            bool connected = false;

            do
            {
                connect.Close();
                connect.Open();

                if (connect.State == ConnectionState.Open)
                    connected = true;
                else
                    connectAttempts++;

            } while (connectAttempts < 5 && !connected);

            if (!connected)
            {
                connect.Close();
                throw new Exception("Could not establish a connection;");
            }
        }

        /// <summary>
        /// Encrypts the App.config files connection strings
        /// TODO: Remove this, place in seperate program that gets run during clients install
        /// </summary>
        public void EncryptConnectionString()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            ConnectionStringsSection section = config.GetSection("connectionStrings") as ConnectionStringsSection;

            if (section != null)
            {
                if (!section.SectionInformation.IsProtected)
                    section.SectionInformation.ProtectSection(null);
                else
                    section.SectionInformation.UnprotectSection();

                section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                section.SectionInformation.ForceSave = true;

                config.Save(ConfigurationSaveMode.Full);
            }
        }

        /// <summary>
        /// Adds the given SqlException's message to the general error logger.
        /// </summary>
        /// <param name="except">The sql exception storing the message that gets passed to the logger</param>
        protected void AddExceptToError(SqlException except)
        {
            if (except.Number == -2 || except.Number == 0)
            {
                GeneralLogger.GetInstance().AddError(except.Message);
            }
        }

        /// <summary>
        /// Adds the given Exception's message to the general error logger.
        /// </summary>
        /// <param name="except">The exception storing the message that gets passed to the logger</param>
        protected void AddExceptToError(Exception except)
        {
            GeneralLogger.GetInstance().AddError(except.Message);
        }

        /// <summary>
        /// Checks the stored session information to make sure that the login is still valid
        /// Prevents multiple logins
        /// </summary>
        /// <param name="playerID">The ID of the player attempting to access the database</param>
        /// <param name="session">The session character string associated with a login session</param>
        /// <returns></returns>
        protected bool VerifySession(int playerID, string session)
        {
            bool verified = false;

            try
            {
                OpenConnection();

                QueryString = "proc_checkSession @playerID, @session;";
                SqlCommand verifyCommand = new SqlCommand(QueryString, connect);
                verifyCommand.Parameters.AddWithValue("@playerID", playerID);
                verifyCommand.Parameters.AddWithValue("@session", session);

                verified = Convert.ToBoolean(verifyCommand.ExecuteScalar());
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return verified;
        }
    }

    /// <summary>
    /// Handles registration functions such as checking if 
    /// the desired username is available and storing the new account on the server.
    /// </summary>
    class Register : DatabaseController
    {
        public Register()
        {
            ConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["TimeDefender"].ConnectionString;
            connect = new SqlConnection();
            connect.ConnectionString = ConnectString;
        }

        /// <summary>
        /// Checks the given username to see if it already exists on the server.
        /// Returns false if username already exists or if there was a connection error.
        /// </summary>
        /// <param name="username">The desired username</param>
        /// <returns>True/False depending on whether or not the desired username is already in use</returns>
        private bool ChkUsernameAvailability(string username)
        {
            int UserAvailable = 0;

            try
            {
                OpenConnection();

                QueryString = "SELECT COUNT(Name) FROM Player WHERE Name = @User;";
                SqlCommand chkUser = new SqlCommand(QueryString, connect);
                chkUser.Parameters.AddWithValue("@User", username);

                UserAvailable = (Int32)chkUser.ExecuteScalar();

                if (Convert.ToBoolean(UserAvailable))
                {
                    throw new Exception("Username already taken, please choose another");
                }
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return !Convert.ToBoolean(UserAvailable);
        }

        /// <summary>
        /// If true is returned, stores the new account to the server and returns true. 
        /// If the above function returns false, or if there was a connection error, false is returned.
        /// </summary>
        /// <param name="username">The desired username</param>
        /// <param name="password">The desired password</param>
        /// <param name="email">The desired email address</param>
        /// <returns>True/False depending on whether or not the account was created</returns>
        public bool RegisterAccount(string username, string password, string email)
        {
            bool PlayerInserted = false;

            if (ChkUsernameAvailability(username))
            {
                try
                {
                    OpenConnection();
                    QueryString = "proc_inPlayer @User, @Pass, @Email;";
                    SqlCommand insertPlayer = new SqlCommand(QueryString, connect);
                    insertPlayer.Parameters.AddWithValue("@User", username);
                    insertPlayer.Parameters.AddWithValue("@Pass", password);
                    insertPlayer.Parameters.AddWithValue("@Email", email);

                    PlayerInserted = Convert.ToBoolean(insertPlayer.ExecuteScalar());
                }
                catch (SqlException except)
                {
                    AddExceptToError(except);
                }
                finally
                {
                    connect.Close();
                }
            }

            return PlayerInserted;
        }
    }

    /// <summary>
    /// Retrieves the specified account if both the username and password match what is stored in the database. 
    /// </summary>
    class Login : DatabaseController
    {
        private DataSet account;

        public Login()
        {
            ConnectString = ConfigurationManager.ConnectionStrings["TimeDefender"].ConnectionString;
            connect = new SqlConnection();
            connect.ConnectionString = ConnectString;
        }

        /// <summary>
        /// Sets up a new session for the user before login.
        /// Returns a null value if the session could not be set up, indicating
        /// a failed login.
        /// </summary>
        /// <param name="user">The username of the player attempting to log in</param>
        /// <returns></returns>
        public string GetNewSession(string user, string password)
        {
            string session = null;

            try
            {
                OpenConnection();

                QueryString = "proc_activateSession @user, @password;";
                SqlCommand getSessionCommand = new SqlCommand(QueryString, connect);
                getSessionCommand.Parameters.AddWithValue("@user", user);
                getSessionCommand.Parameters.AddWithValue("@password", password);

                session = (getSessionCommand.ExecuteScalar()).ToString();
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except) { /*Really don't care about this exception, but it needs to be caught.*/ }
            finally
            {
                connect.Close();
            }

            return session;
        }

        /// <summary>
        /// Retrieves a players email, experience, and playerID from the server
        /// based on the given username and password. Returns a DataSet, throws 
        /// an error if the DataSet is empty(no player with the matching username/password combination found).
        /// </summary>
        /// <param name="username">The username of the account to be logged in</param>
        /// <param name="password">The password of the account to be logged in</param>
        /// <param name="session">The session for the current login</param>
        /// <returns>A DataSet containing the account if username/password match</returns>
        public DataSet RetrieveAccount(string username, string password, string session)
        {
            account = new DataSet();
            try
            {
                OpenConnection();

                QueryString = "SELECT * FROM func_retPlayer(@User, @Pass, @Session);";
                SqlCommand getAccountCommand = new SqlCommand(QueryString, connect);
                getAccountCommand.Parameters.AddWithValue("@User", username);
                getAccountCommand.Parameters.AddWithValue("@Pass", password);
                getAccountCommand.Parameters.AddWithValue("@Session", session);

                SqlDataAdapter selectAccount = new SqlDataAdapter();
                selectAccount.SelectCommand = getAccountCommand;

                selectAccount.Fill(account);
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return account;
        }
    }

    /// <summary>
    /// Performs updates to the players account when they are logged-in. 
    /// </summary>
    class Update : DatabaseController
    {
        public Update()
        {
            ConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["TimeDefender"].ConnectionString;
            connect = new SqlConnection();
            connect.ConnectionString = ConnectString;
        }

        /// <summary>
        /// Updates the players account to reflect the experience they've gained.
        /// Adds the given experience to whatever experience is already stored on the server.
        /// </summary>
        /// <param name="playerID">The players unique ID</param>
        /// <param name="experience">The amount of experience to be added to the users account</param>
        /// <param name="session">The session for the current login</param>
        /// <returns>True/False depending on whether or not the experience was udpated</returns>
        public bool UpdateExperience(int playerID, int experience, string session)
        {
            bool updated_exp = false;

            try
            {
                if (VerifySession(playerID, session))
                {
                    OpenConnection();

                    QueryString = "proc_UpdateExperience @Player, @Exp;";

                    SqlCommand UpdateExp = new SqlCommand(QueryString, connect);
                    UpdateExp.Parameters.AddWithValue("@Player", playerID);
                    UpdateExp.Parameters.AddWithValue("@Exp", experience);

                    updated_exp = Convert.ToBoolean(UpdateExp.ExecuteNonQuery());

                    if (!updated_exp)
                    {
                        throw new Exception("Experience could not be updated");
                    }
                }
                else
                    throw new Exception("Invalid login session");
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return updated_exp;
        }

        /// <summary>
        /// Changes the email stored on the players account to the given email address. 
        /// </summary>
        /// <param name="playerID">The players unique ID</param>
        /// <param name="email">The desired new email address</param>
        /// <param name="session">The session for the current login</param>
        /// <returns>True/False depending on whether or not the email was updated</returns>
        public bool UpdateEmail(int playerID, string email, string session)
        {
            bool updated_email = false;

            try
            {
                if (VerifySession(playerID, session))
                {
                    OpenConnection();

                    QueryString = "proc_UpdateEmail @Player, @Email;";

                    SqlCommand UpdateEmail = new SqlCommand(QueryString, connect);
                    UpdateEmail.Parameters.AddWithValue("@Player", playerID);
                    UpdateEmail.Parameters.AddWithValue("@Email", email);

                    updated_email = Convert.ToBoolean(UpdateEmail.ExecuteNonQuery());

                    if (!updated_email)
                    {
                        throw new Exception("Email could not be updated");
                    }
                }
                else
                    throw new Exception("Login session not valid");
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return updated_email;
        }

        /// <summary>
        /// Changes the password for the account to the given value. 
        /// The old password and username must match the account before the password can be updated.
        /// </summary>
        /// <param name="playerID">The players unique ID</param>
        /// <param name="old_pass">The old password to verify that it is the actual user making the request</param>
        /// <param name="new_pass">The desired new password</param>
        /// <param name="session">The session for the current login</param>
        /// <returns>True/False depending on whether or not the password was updated</returns>
        public bool UpdatePassword(int playerID, string old_pass, string new_pass, string session)
        {
            bool updated_pass = false;

            try
            {
                if (VerifySession(playerID, session))
                {
                    OpenConnection();

                    QueryString = "proc_UpdatePassword @Player, @OldPass, @NewPass;";

                    SqlCommand UpdatePass = new SqlCommand(QueryString, connect);
                    UpdatePass.Parameters.AddWithValue("@Player", playerID);
                    UpdatePass.Parameters.AddWithValue("@NewPass", new_pass);
                    UpdatePass.Parameters.AddWithValue("@OldPass", old_pass);

                    updated_pass = Convert.ToBoolean(UpdatePass.ExecuteNonQuery());

                    if (!updated_pass)
                    {
                        throw new Exception("Password could not be updated");
                    }
                }
                else
                    throw new Exception("Login session not valid");
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return updated_pass;
        }
    }

    /// <summary>
    /// Contains all functions related to the storage and retrieval of high scores
    /// </summary>
    class HighScoresDBController : DatabaseController
    {
        public HighScoresDBController()
        {
            ConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["TimeDefender"].ConnectionString;
            connect.ConnectionString = ConnectString;
        }

        /// <summary>
        /// Checks the given highscore to see if it meets the requirements to be stored.
        /// </summary>
        /// <param name="mapID">The ID of the map</param>
        /// <param name="gameType">The type of game that was played</param>
        /// <param name="totalScore">The total score achieved by the player</param>
        /// <returns>True/False depending on whether or not the highscore is able to be stored</returns>
        private bool ChkHighScore(int mapID, int gameType, double totalScore)
        {
            try
            {
                OpenConnection();

                QueryString = "proc_checkHighscoreInput @MapID, @GameType, @TotalScore;";
                SqlCommand HighScoreRetriever = new SqlCommand(QueryString, connect);
                HighScoreRetriever.Parameters.AddWithValue("@MapID", mapID);
                HighScoreRetriever.Parameters.AddWithValue("@TotalScore", totalScore);
                HighScoreRetriever.Parameters.AddWithValue("@GameType", gameType);

                return Convert.ToBoolean(HighScoreRetriever.ExecuteScalar());
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return true;
        }

        /// <summary>
        /// Calls ChkHighScore() and then stores the highscore if it is valid.
        /// Stores the high score if ChkHighScore() returns true.
        /// </summary>
        /// <param name="mapID">The ID of the map the user played</param>
        /// <param name="playerID">The ID of the player</param>
        /// <param name="session">The session of the player</param>
        /// <param name="gameType">The type of game the player played</param>
        /// <param name="totalScore">The grand total score the player achieved</param>
        /// <param name="wavesCompleted">The number of waves completed in game</param>
        /// <param name="enemiesKilled">The number of enemies killed during the game</param>
        /// <param name="timePlayed">The amount of time the game was played</param>
        /// <param name="resources">The amount of resources collected during gameplay</param>
        /// <returns>True/False depending on whether or not the highscore was successfully stored</returns>
        public bool InsertHighScore(int mapID, int playerID, string session, int gameType, double totalScore, int wavesCompleted, int enemiesKilled, double timePlayed, long resources, int livesRemaining)
        {
            bool inserted = false;

            try
            {
                if (VerifySession(playerID, session))
                {
                    OpenConnection();

                    if (!ChkHighScore(mapID, gameType, totalScore))
                    {
                        throw new Exception("Score is not eligable for the highscore list");
                    }

                    QueryString = "proc_insertHighScore @MapID, @GameType, @PlayerID, @Session, @TotalScore, @WavesCompleted, @EnemiesKilled, @TimePlayed, @Resources, @LivesRemaining;";
                    SqlCommand insertHighScore = new SqlCommand(QueryString, connect);
                    insertHighScore.Parameters.AddWithValue("@MapID", mapID);
                    insertHighScore.Parameters.AddWithValue("@GameType", gameType);
                    insertHighScore.Parameters.AddWithValue("@PlayerID", playerID);
                    insertHighScore.Parameters.AddWithValue("@Session", session);
                    insertHighScore.Parameters.AddWithValue("@TotalScore", totalScore);
                    insertHighScore.Parameters.AddWithValue("@WavesCompleted", wavesCompleted);
                    insertHighScore.Parameters.AddWithValue("@EnemiesKilled", enemiesKilled);
                    insertHighScore.Parameters.AddWithValue("@TimePlayed", timePlayed);
                    insertHighScore.Parameters.AddWithValue("@Resources", resources);
                    insertHighScore.Parameters.AddWithValue("@LivesRemaining", livesRemaining);

                    inserted = Convert.ToBoolean(insertHighScore.ExecuteScalar());

                    if (!inserted)
                    {
                        throw new Exception("Highscore could not be added");
                    }
                }
                else
                    throw new Exception("Login session not valid");
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return inserted;
        }

        /// <summary>
        /// Retrieves all HighScoresDBController achieved by a given user.
        /// </summary>
        /// <param name="username">The username the player would like to retrieve HighScoresDBController for</param>
        /// <returns>A DataSet containing all the high scores for the given user</returns>
        public DataSet RetrieveScoresByUser(string username)
        {
            DataSet HighScoreList = new DataSet();

            try
            {
                OpenConnection();

                QueryString = "SELECT * FROM func_getUserHighScoresDBController(@Username);";
                SqlCommand retrieveHighScoresDBController = new SqlCommand(QueryString, connect);
                retrieveHighScoresDBController.Parameters.AddWithValue("@Username", username);

                SqlDataAdapter getter = new SqlDataAdapter();
                getter.SelectCommand = retrieveHighScoresDBController;

                getter.Fill(HighScoreList);
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return HighScoreList;
        }

        /// <summary>
        /// Retrieves all HighScoresDBController made on a given map.
        /// </summary>
        /// <param name="mapID">The ID of the map which the user wants highscore for</param>
        /// <returns>A DataSet containing all the high scores for the given map</returns>
        public DataSet RetrieveScoresByMap(int mapID)
        {
            DataSet HighScoreList = new DataSet();

            try
            {
                OpenConnection();

                QueryString = "SELECT * FROM func_getMapHighScoresDBController(@MapID);";
                SqlCommand retrieveHighScoresDBController = new SqlCommand(QueryString, connect);
                retrieveHighScoresDBController.Parameters.AddWithValue("@MapID", mapID);

                SqlDataAdapter getter = new SqlDataAdapter();
                getter.SelectCommand = retrieveHighScoresDBController;

                getter.Fill(HighScoreList);
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return HighScoreList;
        }
    }

    /// <summary>
    /// Contains all methods related to the storage and retrieval of maps.
    /// </summary>
    class MapsDBController : DatabaseController
    {
        public MapsDBController()
        {
            ConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["TimeDefender"].ConnectionString;
            connect.ConnectionString = ConnectString;
        }

        /// <summary>
        /// Retrieves a random map from the server.
        /// </summary>
        /// <returns>A DataSet containing a random map information</returns>
        public DataSet RetrieveRandomMap()
        {
            DataSet map = new DataSet();

            try
            {
                OpenConnection();

                QueryString = "SELECT * FROM func_retrieveRandMap();";

                SqlCommand MapRetriever = new SqlCommand(QueryString, connect);
                SqlDataAdapter getter = new SqlDataAdapter();
                getter.SelectCommand = MapRetriever;

                getter.Fill(map);
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return map;
        }

        /// <summary>
        /// Retrieves a specific map from the server.
        /// </summary>
        /// <param name="mapID">The ID of the map to be retrieved</param>
        /// <returns>A DataSet containing all map information</returns>
        public DataSet RetrieveSpecificMap(int mapID)
        {
            DataSet map = new DataSet();

            try
            {
                OpenConnection();

                QueryString = "SELECT * FROM func_retrieveSpecMap(@MapID);";
                SqlCommand MapRetriever = new SqlCommand(QueryString, connect);
                SqlDataAdapter getter = new SqlDataAdapter();
                getter.SelectCommand = MapRetriever;

                getter.Fill(map);
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return map;
        }

        /// <summary>
        /// Stores the given map to the server.
        /// </summary>
        /// <param name="map">A string which stores the map</param>
        /// <param name="difficulty">An int value that determines the difficulty of the map</param>
        /// <param name="seed">An int value to seed the Random() function to result in the same
        ///                    enemy waves each time the map is played</param>
        /// <returns>True/False depending on whether or not the map was successfully stored</returns>
        public int StoreMap(string map, int difficulty, int startingResources, int startingLives)
        {
            int inserted = 0;

            try
            {
                OpenConnection();

                QueryString = "proc_StoreMap @Map, @Difficulty, @StartingResources, @StartingLives;";
                SqlCommand InsertMap = new SqlCommand(QueryString, connect);
                InsertMap.Parameters.AddWithValue("@Map", map);
                InsertMap.Parameters.AddWithValue("@Difficulty", difficulty);
                InsertMap.Parameters.AddWithValue("@StartingLives", startingLives);
                InsertMap.Parameters.AddWithValue("@StartingResources", startingResources);

                inserted = Convert.ToInt32(InsertMap.ExecuteScalar());
            }
            catch (SqlException except)
            {
                AddExceptToError(except);
            }
            catch (Exception except)
            {
                AddExceptToError(except);
            }
            finally
            {
                connect.Close();
            }

            return inserted;
        }
    }
}

