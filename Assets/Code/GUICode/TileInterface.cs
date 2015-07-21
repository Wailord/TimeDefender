using Assets.Code.Units;
using Assets.Code.Controllers;
using Assets.Code.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Code.GUICode
{
    /// <summary>
    /// This class acts as the Interface between the Unity side of logic and the game logic.
    /// 
    /// Currently it is used for position translation between our grid and the Unity local position.
    /// </summary>
    public class TileInterface : MonoBehaviour
    {
        public GameObject Tile;

        #region Fields
        public int PosX;
        public int PosY;

        // used for time fragment interest
        private static bool _moneySpent = false;

        // Used for checking if location changed between updates.
        private int lastX;
        private int lastY;

        private static float _minBound = -182.5f;
        private static float _posScaleFactor = 18.25f;
        private static float _sizeScaleFactor = 28.6f;
        private bool _hasTower = false;

        #endregion

        #region Properties

        public static float MinBound
        {
            get { return _minBound; }
            set { _minBound = value; }
        }

        public static float PosScaleFactor
        {
            get { return _posScaleFactor; }
            set { _posScaleFactor = value; }
        }

        public static float SizeScaleFactor
        {
            get { return _sizeScaleFactor; }
            set { _sizeScaleFactor = value; }
        }

        public Vector3 WorldPos
        {
            get { return Tile.transform.position; }
        }

        public bool HasTower
        {
            get { return _hasTower; }
            set { _hasTower = value; }
        }

        public static bool MoneySpent
        {
            get { return _moneySpent; }
            set { _moneySpent = value; }
        }

        #endregion
        void Update()
        {
            // Update the position of the tile
            UpdatePosition();

        }

        /// <summary>
        /// Click handler for the tile
        /// </summary>
        private void OnMouseDown()
        {
            Click();
        }

        /// <summary>
        /// Private click function. Used to support multiple ways to clicking the tile (maybe touch later?)
        /// </summary>
        private void Click()
        {
            if (!HasTower)
            {
                CreateTower();
            }
            else
                UpgradeTower();
        }

        private void UpgradeTower()
        {
            Unit tower = CombatController.Instance.GetTowerAt(PosX, PosY);
            
            // TODO: smarter upgrades, not hardcoded
            if (tower != null)
            {
                Debug.Log(string.Format("Upgrading tower @ ({0}, {1})", PosX, PosY));
                tower.UpgradeAttackPower(10.0f);
            }
            else
                Debug.Log(string.Format("error: tower does not exist at ({0}, {1})", PosX, PosY));
        }

        /// <summary>
        /// Constructor for things derived from mono
        /// </summary>
        void Awake()
        {
            lastX = -1;
            lastY = -1;
        }

        /// <summary>
        /// Updates the position of the tile in Unity local space 
        /// according to the currently set position.
        /// </summary>
        private void UpdatePosition()
        {
            // If no changes happened, skip the work.
            if (PosX == lastX && PosY == lastY) return;

            float X = (MinBound + (PosX * PosScaleFactor));
            float Y = -(MinBound + (PosY * PosScaleFactor));

            Tile.transform.localPosition = new Vector3(X, Y);
            Tile.transform.localScale = new Vector2(SizeScaleFactor, SizeScaleFactor);

            lastX = PosX;
            lastY = PosY;
        }

        public void CreateTower()
        {
            if (GuiAPI.CreateTower(PosX, PosY))
            {
                HasTower = true;
                MoneySpent = true;
                Debug.Log("Created tower at:" + PosX + "," + PosY);
            }
        }
    }
}
