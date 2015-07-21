using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code.Interfaces;
using UnityEngine;
using Assets.Code.Enums;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Assets.Code.Controllers;
using Assets.Code.Navigation;
using Assets.Code.Scores;

namespace Assets.Code.GUICode
{
    /// <summary>
    /// This static class is used by the business layer to update the display.
    /// </summary>
    public static class GuiAPI
    {
        #region Fields
        // list of gui items being managed by the API
        public static List<IGuiItem> GuiItemsList = new List<IGuiItem>();
        private static List<MapTileController> _mapTiles = new List<MapTileController>();
        private static List<UnitController> _units;
        private static List<GameTextBoxes> _displayTextBoxeses = new List<GameTextBoxes>();

        private static float _minBound = -182.5f;
        private static float _posScaleFactor = 18.25f;
        private static float _sizeScaleFactor = 28.6f;

        private static HudInterface _hudInterface;

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
        public static List<MapTileController> MapTiles
        {
            get { return _mapTiles; }
            set { _mapTiles = value; }
        }

        public static List<UnitController> Units
        {
            get { return _units; }
            set { _units = value; }
        }

        public static HudInterface HUD
        {
            get { return _hudInterface; }
            set
            {
                if (value != _hudInterface)
                {
                    _hudInterface = value;
                    SetTextBoxes();
                }
            }
        }

        private static void SetTextBoxes()
        {
            _displayTextBoxeses.Clear();
            _displayTextBoxeses.Add(new GameTextBoxes("Money", 0, 0, "Time Fragments: ", "", TextBoxType.UpdateRight, _hudInterface.MoneyText, "GameInfoPanel"));
            _displayTextBoxeses.Add(new GameTextBoxes("Lives", 0, 0, "Lives: ", "", TextBoxType.UpdateRight, _hudInterface.LivesText, "GameInfoPanel"));
            _displayTextBoxeses.Add(new GameTextBoxes("Waves", 0, 0, "Waves: ", "", TextBoxType.UpdateRight, _hudInterface.WaveText, "GameInfoPanel"));
            _displayTextBoxeses.Add(new GameTextBoxes("Score", 0, 0, "Score: ", "", TextBoxType.UpdateRight, _hudInterface.ScoreText, "GameInfoPanel"));
            _displayTextBoxeses.Add(new GameTextBoxes("Next", 0, 0, "", "", TextBoxType.UpdateWhole, _hudInterface.NextText, "GameInfoPanel"));
            _displayTextBoxeses.Add(new GameTextBoxes("Time", 0, 0, "", "", TextBoxType.UpdateRight, _hudInterface.TimeText, "GameInfoPanel"));
        }

        #endregion

        /// <summary>
        /// Creates a volitile button with the information passed.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="btnX"></param>
        /// <param name="btnY"></param>
        /// <param name="btnWidth"></param>
        /// <param name="btnHeight"></param>
        /// <param name="text"></param>
        public static void CreateVolButton(IGuiCommand command, float btnX, float btnY, float btnWidth, float btnHeight, String text)
        {
            // get values for location
            float width = btnWidth / 100 * Screen.width;
            float height = btnHeight / 100 * Screen.height;
            float x = btnX;// / 100 * Screen.width + width / 2;
            float y = btnY;// / 100 * Screen.height + height / 2;

            //button created. derived from IGuiButton. 
            VolitileGuiButton newButton = new VolitileGuiButton(command, x, y, width, height, text);
            Button buttonObj = (Button)Object.Instantiate(Resources.Load<Button>("ButtonPrefab"));
            buttonObj.onClick.AddListener(command.Action);
            buttonObj.transform.position = new Vector3(x, y);
            buttonObj.transform.parent = GameObject.Find("UICanvas").GetComponent<Transform>();
            newButton.ButtonObject = buttonObj;

            GuiItemsList.Add(newButton);
        }

        /// <summary>
        /// This function call removes the passed game object from the unity scene.
        /// 
        /// Used by the DestroyGuiScript.
        /// </summary>
        /// <param name="obj"></param>
        public static void DeleteGuiItem(Object obj)
        {
            Object.Destroy(obj);
        }

        /// <summary>
        /// Function for removal of GuiItems by the business layer.
        /// </summary>
        /// <param name="guiItem"></param>
        public static void Destroy(IGuiItem guiItem)
        {
            guiItem.Delete();
        }

        /// <summary>
        /// Adds the controller for the map tile sprite to the list to be accessed later.
        /// </summary>
        /// <param name="mapTileController"></param>
        public static void RegisterMapTile(MapTileController mapTileController)
        {
            MapTiles.Add(mapTileController);
        }

        public static void DrawGrid(TileType[,] map)
        {
            // why do we have two different types of enums here? TileTypes and Tile?
            int rows = map.GetLength(0);
            int cols = map.GetLength(1);

            try
            {
                for (int r = 0; r < rows; ++r)
                {
                    for (int c = 0; c < cols; ++c)
                    {
                        CreateMapTile(map[c, r], c, r);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public static void DrawGrid(int[,] map)
        {
            int rows = map.GetLength(0);
            int cols = map.GetLength(1);

            int current = 0;

            try
            {
                for (int r = 0; r < rows; ++r)
                {
                    for (int c = 0; c < cols; ++c)
                    {
                        current = map[r, c];
                        if (current == 0)
                        {
                            CreateMapTile(TileType.EmptySpace, c, r);
                        }
                        else if (current == 2)
                        {
                            CreateMapTile(TileType.Tower, c, r);
                        }
                        else
                        {
                            //check for path above
                            bool above = (r == 0 || map[r - 1, c] == 1);
                            bool below = (r == rows - 1 || map[r + 1, c] == 1);
                            bool left = (c == 0 || map[r, c - 1] == 1);
                            bool right = (c == cols - 1 || map[r, c + 1] == 1);

                            if (above && below)
                                CreateMapTile(TileType.VertPath, c, r);
                            else if (above && left)
                                CreateMapTile(TileType.ElbowRightDown, c, r);
                            else if (above && right)
                                CreateMapTile(TileType.ElbowLeftDown, c, r);
                            else if (below && left)
                                CreateMapTile(TileType.ElbowRightUp, c, r);
                            else if (below && right)
                                CreateMapTile(TileType.ElbowLeftUp, c, r);
                            else if (left && right)
                                CreateMapTile(TileType.HorizPath, c, r);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public static bool CreateMapTile(TileType tile, int posX, int posY)
        {
            try
            {
                MapTileController newTileController = new MapTileController(posX, posY, tile);
                //Debug.Log(String.Format("Tile created at {0}x{1}", posX, posY));
            }
            catch (Exception e)
            {
                Debug.Log(String.Format("There was an error creating tile at location {0}x{1} -- exception: {2}", posX, posY, e));
                return false;
            }
            return true;
        }
        public static UnitController CreateUnit(float posX, float posY, UnitTypes unit)
        {
            UnitController newUnitController = null;
            try
            {
                newUnitController = new UnitController(posX, posY, unit);
                RegisterUnit(newUnitController);
                //Debug.Log(String.Format("UnitController created at {0}x{1}", posX, posY));
            }
            catch (Exception e)
            {
                //Debug.Log(String.Format("There was an error creating UnitController at location {0}x{1}", posX, posY));
            }
            return newUnitController;
        }

        public static void RegisterUnit(UnitController unitController)
        {
            Units.Add(unitController);
        }

        public static ProjectileController CreateProjectile(float posX, float posY, ProjectileTypes projectile)
        {
            ProjectileController projectileController = null;
            try
            {
                projectileController = new ProjectileController(posX, posY, projectile);
            }
            catch (Exception)
            {
                // prefab loading failed.
            }
            return projectileController;
        }

        public static Vector3 UnitySpaceToGame(Vector3 position)
        {
            position.x = (MinBound + (position.x / PosScaleFactor));
            position.y = -(MinBound + (position.y / PosScaleFactor));
            return position;
        }
        public static Vector3 GameToUnitySpace(Vector3 position)
        {
            position.x = (MinBound + (position.x * PosScaleFactor));
            position.y = -(MinBound + (position.y * PosScaleFactor));
            return position;
        }

        public static void ClickGrid(Vector3 location)
        {
            foreach (MapTileController tile in MapTiles)
            {
                if (tile.WorldPosMatch(location))
                {
                    CombatController.Instance.SpawnCombatantOfType(UnitTypes.DummyTower, Faction.TowerSide, new GridPoint(tile.PosX, tile.PosY));
                    break;
                }
            }
        }

        /// <summary>
        /// Creates a tower at the location in the business layer.
        /// </summary>
        /// <param name="posX">X Position of tower</param>
        /// <param name="posY">Y Position of tower</param>
        /// <returns>True on success</returns>
        public static bool CreateTower(int posX, int posY) // TODO: This should also accept the tower type
        {
            GridPoint tile = NavigationController.Instance.GetGridPoint(posX, posY);

            if (tile.TileType != TileType.Tower)
                return false;
            
            bool moneySpent = GameScore.GetInstance().RemoveResources(200); // TODO: Remove hardcoded cost

            if (moneySpent)
            {
                try
                {
                    CombatController.Instance.SpawnCombatantOfType(UnitTypes.DummyTower, Faction.TowerSide,
                        NavigationController.Instance.GetGridPoint(posX, posY));
                }
                catch (Exception)
                {
                    // prefab loading failed.
                }
            }

            return moneySpent;
        }

        public static GameTextBoxes GetTextBox(string name)
        {
            return _displayTextBoxeses.FirstOrDefault(x => x.Name == name);
        }
    }
}
