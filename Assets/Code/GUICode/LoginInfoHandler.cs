using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.GUICode
{
    public class LoginInfoHandler : MonoBehaviour
    {
        #region Fields
        private static String username;
        private static String email;
        private static String password;
        private static String passwordConfirm;
        private static String errorMessage;

        public InputField UsernameField;
        public InputField EmailField;
        public InputField PasswordField;
        public InputField PasswordConfirmField;
        public Text ErrorText;
        #endregion

        #region Properties
        public static string Username
        {
            get { return username; }
            set { username = value; }
        }

        public static string Email
        {
            get { return email; }
            set { email = value; }
        }

        public static string Password
        {
            get { return password; }
            set { password = value; }
        }

        public static string PasswordConfirm
        {
            get { return passwordConfirm; }
            set { passwordConfirm = value; }
        }

        public static string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }
        #endregion

        // Update is called once per frame
        void Update ()
        {
            username = UsernameField.text;
            email = EmailField.text;
            password = PasswordField.text;
            passwordConfirm = PasswordConfirmField.text;
            ErrorText.text = errorMessage;
        }
    }
}
