using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Data;
using System.Data.SqlTypes;
using Assets.Code.Controllers;
using Assets.Code.ErrorLog;
using System.Text.RegularExpressions;

namespace Assets.Code.Player
{
    /// <summary>
    /// Stores the default variables used in all derived player classes. 
    /// Also stores the methods to validate, hash, and
    /// store these variables as well as to add errors to the error list.
    /// </summary>
    class GenericPlayer
    {
        protected string username;
        protected string email;

        /// <summary>
        /// Simple check to ensure that the two passwords entered are the same.
        /// </summary>
        /// <param name="pass1">The password to be passed to the database</param>
        /// <param name="pass2">The password to check pass1 against</param>
        /// <returns>Returns true/false depending on whether or not the passwords match</returns>
        protected bool CheckPasswords(string pass1, string pass2)
        {
            return pass1 == pass2;
        }

        /// <summary>
        /// Creates a SHA256 object, passes in the password as bytes
        /// to the ComputeHash function, which returns the hashed-bytes.
        /// Converts the bytes back to a string in the foreach loop.
        /// </summary>
        /// <param name="password">The password that will be hashed</param>
        /// <returns>Returns the hashed password</returns>
        protected string HashPassword(string password)
        {
            SHA256Managed breakfast = new SHA256Managed();
            byte[] hashbrowns = breakfast.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));

            password = String.Empty;

            foreach (byte omnomnom in hashbrowns)
            {
                password += omnomnom.ToString("x2");
            }

            return password;
        }

        /// <summary>
        /// Checks the given username against the regular expression to ensure that it is valid.
        /// </summary>
        /// <param name="user">The username to be checked</param>
        /// <returns>True/False depending on whether or not the username is valid</returns>
        protected bool ValidateUsername(string user)
        {
            bool valid = false;
            string expression = @"[a-zA-Z][a-zA-Z0-9_\-]{5,15}$";
            Match match = Regex.Match(user, expression);

            if (match.Success)
                valid = true;

            return valid;
        }

        /// <summary>
        /// Uses a regular expression to validate the email address.
        /// </summary>
        /// <param name="e_mail">The email address to be validated</param>
        /// <returns>True/False depending on whether or not the email is valid</returns>
        protected bool ValidateEmail(string e_mail)
        {
            string expression = @".*@.*\..*";
            Match match = Regex.Match(e_mail, expression);

            return match.Success;
        }

        /// <summary>
        /// Uses a regular expression to validate the password.
        /// </summary>
        /// <param name="pass">The password to be validated</param>
        /// <returns>True/False depending on whether or not the password is valid</returns>
        protected bool ValidatePassword(string pass)
        {
            string expression = @"[a-zA-Z0-9@*#]{8,15}$";
            Match match = Regex.Match(pass, expression);

            return match.Success;
        }

        /// <summary>
        /// Adds the given Exception to the list of errors to be displayed at the presentation layer.
        /// </summary>
        /// <param name="except">The exception storing the message to be added to the general logger</param>
        protected void AddExceptToErrors(Exception except)
        {
            GeneralLogger.GetInstance().AddError(except.Message);
        }

        /// <summary>
        /// Removes all stored player data so that their account can not be accessed
        /// by the client.
        /// </summary>
        protected virtual void ClearData()
        {
            username = String.Empty;
            email = String.Empty;
        }
    }

    /// <summary>
    /// Stores an instance of the DatabaseController : Register class. 
    /// Validates the given data then submits it to the Register class to be stored in the database.
    /// </summary>
    class RegisterPlayer : GenericPlayer
    {
        private Register reg_instance;

        public RegisterPlayer()
        {
            reg_instance = new Register();
        }

        /// <summary>
        /// Submits the validated data to the Register class's RegisterAccount() method to be stored in the database.
        /// </summary>
        /// <param name="password">The hashed password to be stored in the account</param>
        /// <returns></returns>
        private bool SubmitRegistration(string password)
        {
            return reg_instance.RegisterAccount(username, password, email);
        }

        /// <summary>
        /// First, validates and stores the given data.
        /// If any of the data is not valid, returns false and goes no further.
        /// If all data is valid, calls SubmitRegistration().
        /// </summary>
        /// <param name="user">The desired username</param>
        /// <param name="pass1">The desired password</param>
        /// <param name="pass2">The password to verify that the desired password is correct</param>
        /// <param name="e_mail">The desired email address</param>
        /// <returns>True/False depending on whether or not the account was created</returns>
        public bool PrepData(string user, string pass1, string pass2, string e_mail)
        {
            bool ValidData = true;

            //Checks if the username is in a valid format
            try
            {
                if (!ValidateUsername(user))
                    throw new Exception("Username must be between 5 and 15 characters and only contain alpha-numeric characters as well as _ and -");
            }
            catch (Exception except)
            {
                AddExceptToErrors(except);
                ValidData = false;
            }
            finally
            {
                if (ValidData)
                    username = user;
            }

            //Checks if the two passwords entered are the same
            //and ensures they're a valid length
            try
            {
                if (!CheckPasswords(pass1, pass2))
                {
                    throw new Exception("Input passwords must be identical");
                }
                else if (!ValidatePassword(pass1))
                {
                    throw new Exception("Password is not valid, must be between 8-15 characters");
                }
                else
                {
                    pass1 = HashPassword(pass1);
                }
            }
            catch (Exception except)
            {
                AddExceptToErrors(except);
                ValidData = false;
            }

            //Checks if the email address is valid
            try
            {
                if (!ValidateEmail(e_mail))
                    throw new Exception("Email address is not valid");
            }
            catch (Exception except)
            {
                AddExceptToErrors(except);
                ValidData = false;
            }
            finally
            {
                if (ValidData)
                    email = e_mail;
            }

            //If all data is verified, proceed to SubmitRegistration()
            if (ValidData)
                ValidData = SubmitRegistration(pass1);

            ClearData();

            return ValidData;
        }
    }

    /// <summary>
    /// Stores a reference to a DatabaseController : Login instance.
    /// Checks username and password at login, and loads players data if given values are correct.
    /// Updates players data at end of games as well as game exit.
    /// </summary>
    class LoginPlayer : GenericPlayer
    {
        private Login log_instance;
        private string session;

        public LoginPlayer()
        {
            log_instance = new Login();
            session = null;
        }

        /// <summary>
        /// Initializes the players session
        /// Calls the stored procedure activatesession and returns the sessionID which will
        /// be used  to validate that this is a proper login, and that future account accesses
        /// are by a valid login session.
        /// </summary>
        /// <param name="user">The username of the player logging in</param>
        private bool InitializeSession(string user, string password)
        {
            session = log_instance.GetNewSession(user, password);

            return (session != null);
        }

        /// <summary>
        /// Stores the data within the given DataSet into the appropriate local variables.
        /// Creates a new GamePlayer instance which stores the given account information.
        /// </summary>
        /// <param name="user">The players username</param>
        /// <param name="player_info">The DataSet containing the remaining account information</param>
        private void LoadData(string user, DataSet player_info)
        {
            if (player_info.Tables[0].Rows.Count != 0)
            {
                DataRow player_row = player_info.Tables[0].Rows[0];

                username = user;
                email = player_row["Email"].ToString();
                int playerID = Convert.ToInt32(player_row["playerID"]);
                int experience = Convert.ToInt32(player_row["experience"]);

                GamePlayer.GetInstance().Initialize(playerID, username, email, experience, session);
            }
            else
                throw new Exception("Username and or password are invalid");
        }

        /// <summary>
        /// Calls the DatabaseController : Register class's 
        /// RetrieveAccount() method to get the players information from the server. 
        /// Calls LoadData() to store the retireved information
        /// into variables stored in Player and returns false if the account could not be retrieved.
        /// </summary>
        /// <param name="user">The username of the player to be retrieved from the database</param>
        /// <param name="password">The password of the account</param>
        /// <returns>A GamePlayer object containing the players account information if username/password match
        ///             or a null GamePlayer if they do not match</returns>
        public GamePlayer RetrievePlayer(string user, string password)
        {
            try
            {
                if (InitializeSession(user, HashPassword(password)))
                {
                    DataSet player_info = log_instance.RetrieveAccount(user, HashPassword(password), session);

                    if (player_info != null)
                    {
                        try
                        {
                            LoadData(user, player_info);
                        }
                        catch (Exception except)
                        {
                            AddExceptToErrors(except);
                        }
                    }
                    else
                        throw new Exception("Player username and password do not match");
                }
                else
                {
                    throw new Exception("Player username and password do not match");
                }
            }
            catch (Exception except)
            {
                AddExceptToErrors(except);
            }

            return GamePlayer.GetInstance();
        }
    }

    /// <summary>
    /// Stores a reference to a DatabaseController : Update instance.
    /// Changes players information based on input from
    /// the client, calls on the Update instance to store the new information on the server.
    /// </summary>
    class GamePlayer : GenericPlayer
    {
        protected static WeakReference _instance = new WeakReference(null);
        protected static object _lock = new Object();

        private Update update_instance;
        private int playerID;
        private int experience;
        private int exp_to_add;
        private int level;
        private string session;

        public string Username
        {
            get { return username; }
        }

        public string Email
        {
            get { return email; }
        }

        public int PlayerID
        {
            get { return playerID; }
        }

        public int Experience
        {
            get { return experience + exp_to_add; }
        }

        public int Level
        {
            get { return level; }
        }

        protected GamePlayer()
        {
            playerID = 0;
            username = "";
            email = "";
            experience = 0;
            session = "";
            SetLevel();
            update_instance = null;
        }

        /// <summary>
        /// Creates a GamePlayer object with the given player's data.
        /// Creates a new Databasae Controller : Update instance to
        /// handle all the database operations needed throughout gameplay.
        /// </summary>
        /// <param name="p_id">The users unique ID</param>
        /// <param name="user">The users username</param>
        /// <param name="e_mail">The users email address</param>
        /// <param name="exp">The users current experience</param>
        /// <param name="sess">The session ID for this login</param>
        public void Initialize(int p_id, string user, string e_mail, int exp, string sess)
        {
            playerID = p_id;
            username = user;
            email = e_mail;
            experience = exp;
            session = sess;
            SetLevel();
            update_instance = new Update();
        }

        public static GamePlayer GetInstance()
        {
            GamePlayer strongReference = (GamePlayer)_instance.Target;
            if (strongReference == null)
            {
                lock (_lock)
                {
                    if (strongReference == null)
                    {
                        strongReference = new GamePlayer();
                        _instance = new WeakReference(strongReference);
                    }
                }
            }

            return strongReference;
        }

        /// <summary>
        /// Calculates the players level based on their current total experience.
        /// </summary>
        private void SetLevel()
        {
            /// TODO: Insert some level calculation here
            level = 0;
        }

        /// <summary>
        /// Allows for the client to add to a running total of experience
        /// a player has earned during a game session. Once the game has
        /// been finished, call ChangeExperience(), to update the account.
        /// </summary>
        /// <param name="exp">The amount of experience to be added to the running total</param>
        public void AddExp(int exp)
        {
            exp_to_add += exp;
        }

        /// <summary>
        /// Allows for the players experience to be updated.
        /// Calls UpdateExperience(), which takes the amount the experience will be updated by.
        /// </summary>
        /// <returns>True/False depending on whether or not the experience was successfully updated</returns>
        public bool SubmitExpChange()
        {
            bool exp_changed = false;

            if (exp_to_add > 0)
            {
                exp_changed = update_instance.UpdateExperience(playerID, exp_to_add, session);

                if (exp_changed)
                {
                    experience += exp_to_add;
                    exp_to_add = 0;
                }
            }
            else
                exp_changed = true;

            return exp_changed;
        }

        /// <summary>
        /// Allows for the password to be updated.
        /// Throws an error if the two new passwords entered don't match.
        /// Calls UpdatePassword() to store the new password. 
        /// </summary>
        /// <param name="oldpass">The original password</param>
        /// <param name="pass1">The password which the user would like to update to</param>
        /// <param name="pass2">The password used to ensure that the new password is correct</param>
        /// <returns>True/False depending on whether the password was successfully updated</returns>
        public bool SubmitPasswordChange(string oldpass, string pass1, string pass2)
        {
            bool pass_changed = false;

            try
            {
                if (!CheckPasswords(pass1, pass2))
                    throw new Exception("Values entered for new password must match");

                if (!ValidatePassword(pass1))
                    throw new Exception("Password is not valid, must be 8-15 characters");

                oldpass = HashPassword(oldpass);
                pass1 = HashPassword(pass1);

                pass_changed = update_instance.UpdatePassword(playerID, oldpass, pass1, session);
            }
            catch (Exception except)
            {
                AddExceptToErrors(except);
            }

            return pass_changed;
        }

        /// <summary>
        /// Allows for the email address to be updated.
        /// Throws an error if new email address is not valid.
        /// Calls UpdateEmail() to change the email on the server,
        /// </summary>
        /// <param name="e_mail">The email which is to replace the old email</param>
        /// <returns>True/False depending on whether or not the email was updated</returns>
        public bool SubmitEmailChange(string e_mail)
        {
            bool email_changed = false;

            try
            {
                if (ValidateEmail(e_mail))
                {
                    email_changed = update_instance.UpdateEmail(playerID, e_mail, session);

                    if (email_changed)
                        email = e_mail;
                }
                else
                    throw new Exception("Email address is not valid");
            }
            catch (Exception except)
            {
                AddExceptToErrors(except);
            }

            return email_changed;
        }

        /// <summary>
        /// Clears all data from variables stored in the player class
        /// after updating the experience on the server.
        /// </summary>
        public void ReleaseAccount()
        {
            if (update_instance == null)
            {
                update_instance = new Update();
            }

            SubmitExpChange();

            ClearData();
        }

        /// <summary>
        /// Removes any data from stored variables to ensure that they
        /// are no longer available to the client.
        /// </summary>
        protected override void ClearData()
        {
            base.ClearData();
            playerID = 0;
            exp_to_add = 0;
            experience = 0;
            level = 0;
        }
    }
}

