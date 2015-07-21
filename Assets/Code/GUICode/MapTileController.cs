using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Code.Enums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Code.GUICode
{
    /// <summary>
    /// This class creates and acts as a controller for the sections of the map that are displayed
    /// </summary>
    public class MapTileController
    {

        #region Fields
        private int _posX;
        private int _posY;
        #endregion

        #region Properties
        /// <summary>
        ///  Location property for the x-axis. Points given are in matrix notation
        /// </summary>
        public int PosX
        {
            get { return _posX; }
            set
            {
                if(_posX == value) return; // value has not changed
                _posX = value;
                Interface.PosX = value;
            }
        }
        /// <summary>
        /// Location property for the y-axis. Points given are in matrix notation
        /// </summary>
        public int PosY 
        {
            get { return _posY; }
            set
            {
                if (_posY == value) return;
                _posY = value;
                Interface.PosY = value;
            }
        }
        /// <summary>
        /// The interface with the tile. Deals with all Unity properties
        /// </summary>
        public TileInterface Interface { get; set; }

        /// <summary>
        /// Reference to the tile on the screen. Will be used for deletion
        /// </summary>
        public GameObject Sprite { get; set; }

        #endregion
        /// <summary>
        /// Creates a new controller at posx,posy with the tile type of tile passed.
        /// Location is in matrix notation
        /// </summary>
        /// <param name="posX">X position on map grid</param>
        /// <param name="posY">Y position on map grid</param>
        /// <param name="tile">Tile to display</param>
        public MapTileController(int posX, int posY, TileType tile)
        {
            switch (tile)
            {
                case TileType.VertPath:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadUpDown"));
                    break;
                case TileType.ElbowLeftUp:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadUpLeft"));
                    break;
                case TileType.ElbowRightUp:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadUpRight"));
                    break;
                case TileType.ElbowLeftDown:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadDownLeft"));
                    break;
                case TileType.ElbowRightDown:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadDownRight"));
                    break;
                case TileType.HorizPath:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadLeftRight"));
                    break;
                case TileType.EmptySpace:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("GrassTile"));
                    break;
                case TileType.Tower:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("TowerPlot"));
                    break;
                // 3 and 4-way splits =======
                case TileType.Split4Ways:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadCross"));
                    break;
                case TileType.SplitUDL:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadSplitUDL"));
                    break;
                case TileType.SplitUDR:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadSplitUDR"));
                    break;
                case TileType.SplitULR:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadSplitULR"));
                    break;
                case TileType.SplitDLR:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("RoadSplitDLR"));
                    break;
                case TileType.Decor:
                    // maybe randomly pick a decoration?
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("Decor"));
                    break;
                    // used by additional decoration placement algorithm
                case TileType.Decor2:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("DecorMtn"));
                    break;
                case TileType.EndPoint:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("EndPoint"));
                    break;
                case TileType.StartPoint:
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("StartPoint"));
                    break;
                // =================
                default:
                    Debug.Log("couldn't find a matching tile, placing a grass tile instead");
                    Sprite = (GameObject)Object.Instantiate(Resources.Load("GrassTile"));
                    break;
            }

            // Make the new sprite a child of the UI Canvas
            Sprite.transform.parent = GameObject.Find("MapCanvas").GetComponent<Transform>();

            // Get the Interface with the new sprite
            Interface = Sprite.GetComponent<TileInterface>();

            // HACK dear god guys we need to stop using y,x and just unify x,y everywhere
            SetLocation(posY,posX);

            // Add the sprite to a list so that the map may be altered.
            GuiAPI.RegisterMapTile(this);
        }

        /// <summary>
        /// Sets the location of the tile. Points are in matrix notation.
        /// </summary>
        /// <param name="xLoc"></param>
        /// <param name="yLoc"></param>
        public void SetLocation(int xLoc, int yLoc)
        {
            Interface.PosX = xLoc;
            Interface.PosY = yLoc;
        }

        public bool WorldPosMatch(Vector3 location)
        {
            Vector3 tolerence = new Vector3(20,20,20);
            return (Interface.WorldPos.x - location.x < tolerence.x) && (Interface.WorldPos.y - location.y < tolerence.y);
        }
    }
}
