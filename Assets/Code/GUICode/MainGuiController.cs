using System;
using Assets.Code.Enums;
using Assets.Code.GameTypes;
using Assets.Code.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.GUICode
{
    public class MainGuiController : MonoBehaviour
    {
        public LoginInfoHandler LoginInfoHandler;

        public bool IsPaused = false;
        
        public int ScreenWidth;
        public int ScrrenHeight;
        
        // Update is called once per frame
        void Update ()
        {
            ScreenWidth = Screen.width;
            ScrrenHeight = Screen.height;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                IsPaused = !IsPaused;
                Debug.Log(String.Format("Update pause: {0}", IsPaused));
                PauseToggle();
            }
        }

        public void StartGame()
        {
            // Load the game scene
            Application.LoadLevel(1);
        }

        public void ShowMainMenu()
        {
            // load the main menu
            Application.LoadLevel(0);
        }

        public void ShowLogin()
        {
            // load login scene
            Application.LoadLevel(2);
        }

        public void ShowRegister()
        {
            // load registration scene
            Application.LoadLevel(3);
        }

        public void Login()
        {
            LoginPlayer newPlayer = new LoginPlayer();
            GamePlayer gamePlayer = newPlayer.RetrievePlayer(LoginInfoHandler.Username, LoginInfoHandler.Password);
            if (gamePlayer.PlayerID == 0)
            {
                LoginInfoHandler.ErrorMessage = "Login failed"; // TODO: get error from general logger
            }
            // TODO: Give GamePlayer reference to the business layer
        }

        public void Register()
        {
            RegisterPlayer registerPlayer = new RegisterPlayer();
            bool registerSuccessful = registerPlayer.PrepData(LoginInfoHandler.Username, LoginInfoHandler.Password,
                LoginInfoHandler.PasswordConfirm, LoginInfoHandler.Email);
            if (registerSuccessful == false)
            {
                LoginInfoHandler.ErrorMessage = "Registration failed"; // TODO: get error from general logger
            }
            // TODO: Possibly give reference to business layer. If weak ref.
        }

        public void PauseToggle()
        {
            Debug.Log(String.Format("Pause Toggle: {0}", IsPaused));
            if (IsPaused)
            {
                GameClock.GetInstance().PauseClock(PauseTypes.WavePause);
                Debug.Log("Pausing!");
            }
            else
            {
                GameClock.GetInstance().StartClock();
                Debug.Log("Un-pausing!");
            }
        }

    }
}
