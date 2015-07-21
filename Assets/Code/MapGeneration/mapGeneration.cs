/***********************************************************
* Primary author:           Anthony Nguyen
* Secondary author(s):      Luke Thompson
* Date Created:           	2/3/15
* Last Modification Date: 	4/30/15
* Filename:               	MapGeneration.cs
*
* Overview:
* 	This program will randomly generate a tower defense map that
 * 	has a walkable path, tower plots, and decoration
*
* Input:
* 	Desired number of towers, path length, resources, difficulty, split path
*
* Output:
* 	A tower defense map with the appropiate length, tower plots,
* 	and resources
************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Code.Enums;
using System.Diagnostics;
using Assets.Code.Constants;

namespace Assets.Code.MapGeneration
{
    /// <summary>
    /// Tile struct that will be the data type for the grid
    /// </summary>
    public struct Tile
    {
        // tile is given direction when placed
        public Direction dir;
        // grid's tile
        public TileType tile;
        // used in the recursive search
        public bool searched;
        // check if tile placed is split tile
        public bool split;
    }

    /// <summary>
    /// Available directions 
    /// </summary>
    public enum Direction
    {
        Up, Down, Left, Right, None
    };

    class MapGeneration
    {
        #region Member Variables
        /// <summary>
        /// Coordinate points for grid
        /// </summary>
        public struct Pair
        {
            public int x;
            public int y;

            // c'tor
            public Pair(int pt1, int pt2)
            {
                x = pt1;
                y = pt2;
            }
        }

        // random number generator
        static System.Random rand = new System.Random();

        // direction for map generation
        static Direction _dir;

        // position for map generation
        public static Pair _start;
        public static Pair _end;
        static Pair _position;
        static Pair _positionSplit;

        // array that contains map
        static Tile[,] grid = new Tile[Constant.GRID_WIDTH, Constant.GRID_HEIGHT];

        // flag for turn tiles
        public static bool _isTurnTile = false;

        // int array for counting distance till path tile
        public static int[] _emptyCount = new int[Constant.NUM_OF_DIRECTIONS];

        #endregion

        /// <summary>
        /// Generates a map on a 2D array of TileType. May include split pathing
        /// tower plots, and decoration.
        /// TODO: make it accept parameters
        /// </summary>
        /// <returns>a 2D array of TileType</returns>
        public static TileType[,] GenerateMap()
        {
            // Generate the map
            do
            {
                InitializeGrid();
            } while (!RandomMapGenerator(50, 300, 20));

            // Create split path
            SplitPath();

            // If not enough towers, place more
            while (TowerCount() < Constant.NUM_OF_TOWERS)
                SingleTower();

            // Place decorative objects
            PlaceDecor2(CalculateDecorAmount());

            TileType[,] tileMap = new TileType[Constant.GRID_WIDTH, Constant.GRID_HEIGHT];

            for (int x = 0; x < Constant.GRID_WIDTH; x++)
            {
                for (int y = 0; y < Constant.GRID_HEIGHT; y++)
                {
                    tileMap[x, y] = grid[x, y].tile;
                }
            }

            return tileMap;
        }

        #region Tile Path Methods
        /// <summary>
        /// Map generation algorithm
        /// </summary>
        /// <param name="min">Minimum path length</param>
        /// <param name="max">Maximum path length</param>
        /// <param name="tower_freq">Number of tiles per tower</param>
        /// <returns></returns>
        private static bool RandomMapGenerator(int min, int max, int tower_freq)
        {
            Pair start;             // start position
            int path_length;        // current path length
            int failures = 0;       // failed iterations
            bool generating = true; // flag for map generation
            bool put_tower = false; // flag for tower placement

            // start point on top of map
            if (rand.Next(0, 2) == 1)
            {
                start.x = rand.Next(0, Constant.GRID_WIDTH);
                start.y = 0;
                // set start tile
                grid[start.y, start.x].tile = TileType.StartPoint;
                _start.x = start.x;
                _start.y = start.y;
                start.y += 1;
                // set adjacent tile
                grid[start.y, start.x].tile = TileType.VertPath;
                // set direction
                _dir = Direction.Down;
            }
            // start point on side of map
            else
            {
                start.x = 0;
                start.y = rand.Next(0, Constant.GRID_HEIGHT);
                grid[start.y, start.x].tile = TileType.StartPoint;
                _start.x = start.x;
                _start.y = start.y;
                start.x += 1;
                grid[start.y, start.x].tile = TileType.HorizPath;
                _dir = Direction.Right;
            }

            // set current position to start point
            _position = start;
            path_length = 1;

            // generation loop
            while (generating)
            {
                if (PlaceTile()) // If a tile is successfully placed
                {
                    ++path_length;
                    if (path_length % tower_freq == 0) // Check if a tower needs placing
                        put_tower = true;
                    failures = 0; // reset failure count
                }
                else
                    failures++; // increment failure count

                if (failures > 30) // presumed catastrophic failure
                    return false;

                if (TowerCount() < Constant.NUM_OF_TOWERS && put_tower && PlaceTower()) // Attempt to place a tower if the put_tower flag is set
                    put_tower = false;         // Disable the flag if the placement is successful

                // Attempt to end the path
                if (path_length > min && (_position.x == 0 || _position.x == Constant.GRID_WIDTH - 1 || _position.y == 0 || _position.y == Constant.GRID_HEIGHT - 1))
                {
                    generating = false;
                    grid[_position.y, _position.x].tile = TileType.EndPoint;

                    // for navigation
                    _end = _position;
                }
            }
            if (path_length < max && !CheckTileAdjacency()) // If map is within bounds, return true
                return true;
            return false;
        }

        /// <summary>
        /// Outer switch statement that controls where to go according to '_dir' variable
        /// </summary>
        private static bool PlaceTile()
        {
            // set all of the tile search flags to false
            UnsearchGrid();
            // grid contains Direction datatype, so after a tile is placed and
            // variable '_dir' is adjusted, change direction for grid.
            grid[_position.y, _position.x].dir = _dir;
            grid[_position.y, _position.x].tile = TileType.CurPosition;

            // determine current direction of travel
            switch (_dir)
            {
                case Direction.Up:
                    return GoUp();
                case Direction.Down:
                    return GoDown();
                case Direction.Left:
                    return GoLeft();
                case Direction.Right:
                    return GoRight();
                default: return false;
            }
        }

        /// <summary>
        /// Changes values inside grid for path going up and considers every possible direction
        /// </summary>
        private static bool GoUp()
        {
            int rNum = rand.Next(0, 3);
            if (_isTurnTile) rNum = 0;

            // Randomly choose next possible direction
            switch (rNum)
            {
                // move up
                case 0:
                    // check if in bounds and tile is empty
                    if (_position.y > 0 && IsSafe(_position.y - 1, _position.x))
                    {
                        // place horizontal tile
                        grid[_position.y, _position.x].tile = TileType.VertPath;
                        // set turn tile flag, done before position is adjusted
                        _isTurnTile = IsTurnTile();
                        // adjust position after placing tile
                        _position.y -= 1;
                        // change direction to move up
                        _dir = Direction.Up;
                        return true;
                    }
                    break;
                // move left
                case 1:
                    // check if in bounds and tile is empty
                    if (_position.x > 0 && IsSafe(_position.y, _position.x - 1))
                    {
                        // place elbow right down tile
                        grid[_position.y, _position.x].tile = TileType.ElbowLeftDown;
                        // set turn tile flag, done before position is adjusted
                        _isTurnTile = IsTurnTile();
                        // adjust position after placing tile
                        _position.x -= 1;
                        // change direction to move left
                        _dir = Direction.Left;
                        return true;
                    }
                    break;
                // move right
                case 2:
                    // check if in bounds and tile is empty
                    if (_position.x < Constant.GRID_WIDTH - 1 && IsSafe(_position.y, _position.x + 1))
                    {
                        // place elbow left down tile
                        grid[_position.y, _position.x].tile = TileType.ElbowRightDown;
                        // set turn tile flag, done before position is adjusted
                        _isTurnTile = IsTurnTile();
                        // adjust position after placing tile
                        _position.x += 1;
                        // change direction to move right
                        _dir = Direction.Right;
                        return true;
                    }
                    break;
            }
            return false;
        }
        /// <summary>
        /// Changes values inside grid for path going down and considers every possible direction
        /// </summary>
        private static bool GoDown()
        {
            int rNum = rand.Next(0, 3);
            if (_isTurnTile) rNum = 0;

            // Randomly choose next possible direction
            switch (rNum)
            {
                // down
                case 0:
                    if (_position.y < Constant.GRID_HEIGHT - 1 && IsSafe(_position.y + 1, _position.x))
                    {
                        grid[_position.y, _position.x].tile = TileType.VertPath;
                        _isTurnTile = IsTurnTile();
                        _position.y += 1;
                        _dir = Direction.Down;
                        return true;
                    }
                    break;
                // left
                case 1:
                    if (_position.x > 0 && IsSafe(_position.y, _position.x - 1))
                    {
                        grid[_position.y, _position.x].tile = TileType.ElbowLeftUp;
                        _isTurnTile = IsTurnTile();
                        _position.x -= 1;
                        _dir = Direction.Left;
                        return true;
                    }
                    break;
                // right
                case 2:
                    if (_position.x < Constant.GRID_WIDTH - 1 && IsSafe(_position.y, _position.x + 1))
                    {
                        grid[_position.y, _position.x].tile = TileType.ElbowRightUp;
                        _isTurnTile = IsTurnTile();
                        _position.x += 1;
                        _dir = Direction.Right;
                        return true;
                    }
                    break;
            }
            return false;
        }
        /// <summary>
        /// Changes values inside grid for path going left and considers every possible direction
        /// </summary>
        private static bool GoLeft()
        {
            int rNum = rand.Next(0, 3);
            if (_isTurnTile) rNum = 2;

            // Randomly choose next possible direction
            switch (rNum)
            {
                // up
                case 0:
                    if (_position.y > 0 && IsSafe(_position.y - 1, _position.x))
                    {
                        grid[_position.y, _position.x].tile = TileType.ElbowRightUp;
                        _isTurnTile = IsTurnTile();
                        _position.y -= 1;
                        _dir = Direction.Up;
                        return true;
                    }
                    break;
                // down
                case 1:
                    if (_position.y < Constant.GRID_HEIGHT - 1 && IsSafe(_position.y + 1, _position.x))
                    {
                        grid[_position.y, _position.x].tile = TileType.ElbowRightDown;
                        _isTurnTile = IsTurnTile();
                        _position.y += 1;
                        _dir = Direction.Down;
                        return true;
                    }
                    break;
                // left
                case 2:
                    if (_position.x > 0 && IsSafe(_position.y, _position.x - 1))
                    {
                        grid[_position.y, _position.x].tile = TileType.HorizPath;
                        _isTurnTile = IsTurnTile();
                        _position.x -= 1;
                        _dir = Direction.Left;
                        return true;
                    }
                    break;
            }
            return false;
        }
        /// <summary>
        /// Changes values inside grid for path going right and considers every possible direction
        /// </summary>
        private static bool GoRight()
        {
            int rNum = rand.Next(0, 3);
            if (_isTurnTile) rNum = 2;

            // Randomly choose next possible direction
            switch (rNum)
            {
                // up
                case 0:
                    if (_position.y > 0 && IsSafe(_position.y - 1, _position.x))
                    {
                        grid[_position.y, _position.x].tile = TileType.ElbowLeftUp;
                        _isTurnTile = IsTurnTile();
                        _position.y -= 1;
                        _dir = Direction.Up;
                        return true;
                    }
                    break;
                // down
                case 1:
                    if (_position.y < Constant.GRID_HEIGHT - 1 && IsSafe(_position.y + 1, _position.x))
                    {
                        grid[_position.y, _position.x].tile = TileType.ElbowLeftDown;
                        _isTurnTile = IsTurnTile();
                        _position.y += 1;
                        _dir = Direction.Down;
                        return true;
                    }
                    break;
                // right
                case 2:
                    if (_position.x < Constant.GRID_WIDTH - 1 && IsSafe(_position.y, _position.x + 1))
                    {
                        grid[_position.y, _position.x].tile = TileType.HorizPath;
                        _isTurnTile = IsTurnTile();
                        _position.x += 1;
                        _dir = Direction.Right;
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Checks to see if there is a path from a given square to the edge of the map
        /// </summary>
        /// <param name="y"> y position value </param>
        /// <param name="x"> x position value </param>
        /// <returns> true if a path exists
        ///           false otherwise       </returns>
        private static bool IsSafe(int y, int x)
        {
            //Check if the tile itself is empty
            if (grid[y, x].tile != TileType.EmptySpace)
                return false;
            //Check if the current position is an edge tile
            if (y == 0 || y == Constant.GRID_HEIGHT - 1 || x == 0 || x == Constant.GRID_WIDTH - 1)
                return true;
            //Search the current tile
            if (grid[y, x].tile == TileType.EmptySpace)
            {
                grid[y, x].searched = true;

                //Recursively search surrounding tile
                if (grid[y + 1, x].tile == TileType.EmptySpace && !grid[y + 1, x].searched)
                    if (IsSafe(y + 1, x)) return true; //A path was found
                if (grid[y, x + 1].tile == TileType.EmptySpace && !grid[y, x + 1].searched)
                    if (IsSafe(y, x + 1)) return true;
                if (grid[y - 1, x].tile == TileType.EmptySpace && !grid[y - 1, x].searched)
                    if (IsSafe(y - 1, x)) return true;
                if (grid[y, x - 1].tile == TileType.EmptySpace && !grid[y, x - 1].searched)
                    if (IsSafe(y, x - 1)) return true;
            }
            return false;
        }
        /// <summary>
        /// Checks to see if there is a path from a given square to the edge of the map
        /// This will flag that a straight has to be placed
        /// </summary>
        /// <returns>true if a turn tile, otherwise false</returns>
        private static bool IsTurnTile()
        {
            TileType tile = grid[_position.y, _position.x].tile;

            // all turn tiles return true
            switch (tile)
            {
                case TileType.SplitUDL:
                case TileType.SplitUDR:
                case TileType.SplitULR:
                case TileType.SplitDLR:
                case TileType.ElbowLeftUp:
                case TileType.ElbowRightUp:
                case TileType.ElbowLeftDown:
                case TileType.ElbowRightDown:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Traverse the grid and check if tiles have adjacent tiles
        /// </summary>
        /// <returns></returns>
        private static bool CheckTileAdjacency()
        {
            for (int i = 0; i < Constant.GRID_HEIGHT; i++)
            {
                for (int j = 0; j < Constant.GRID_WIDTH; j++)
                {
                    if (grid[i, j].tile <= TileType.Split4Ways)
                        if (HasAdjacentTile(i, j))
                            // bad!
                            return true;
                }
            }
            // acceptable tile placement
            return false;
        }

        /// <summary>
        /// A simpler version of TileCheck() method.
        /// If a non-connecting tile is next to current tile then invalid tile is placed
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param> 
        /// <returns>true if non-connective tile is present, otherwise false</returns>
        private static bool HasAdjacentTile(int x, int y)
        {
            // checks for non-connective tiles above
            if (x > 0)
                if (grid[x - 1, y].tile == TileType.HorizPath || grid[x - 1, y].tile == TileType.ElbowLeftUp || grid[x - 1, y].tile == TileType.ElbowRightUp ||
                    grid[x - 1, y].tile == TileType.SplitULR)
                    return true;
            // checks for non-connective tiles below
            if (x < Constant.GRID_HEIGHT - 1)
                if (grid[x + 1, y].tile == TileType.HorizPath || grid[x + 1, y].tile == TileType.ElbowLeftDown || grid[x + 1, y].tile == TileType.ElbowRightDown ||
                    grid[x + 1, y].tile == TileType.SplitDLR)
                    return true;
            // checks for non-connective tiles to the left
            if (y > 0)
                if (grid[x, y - 1].tile == TileType.VertPath || grid[x, y - 1].tile == TileType.ElbowLeftDown || grid[x, y - 1].tile == TileType.ElbowLeftUp ||
                    grid[x, y - 1].tile == TileType.SplitUDL)
                    return true;
            // checks for non-connective tiles to the right
            if (y < Constant.GRID_WIDTH - 1)
                if (grid[x, y + 1].tile == TileType.VertPath || grid[x, y + 1].tile == TileType.ElbowRightDown || grid[x, y + 1].tile == TileType.ElbowRightUp ||
                    grid[x, y + 1].tile == TileType.SplitUDR)
                    return true;

            // acceptable tile placement
            return false;
        }

        /// <summary>
        /// Doesn't seem very necessary, but used for testing
        /// </summary>
        private static void InitializeGrid()
        {
            for (int i = 0; i < Constant.GRID_WIDTH; ++i)
                for (int j = 0; j < Constant.GRID_HEIGHT; ++j)
                {
                    grid[i, j].tile = TileType.EmptySpace;
                    grid[i, j].searched = false;
                    grid[i, j].split = false;
                }
        }

        private static void UnsearchGrid()
        {
            for (int i = 0; i < Constant.GRID_WIDTH; ++i)
                for (int j = 0; j < Constant.GRID_HEIGHT; ++j)
                    grid[i, j].searched = false;
        }

        /// <summary>
        /// Takes in an array of integers and display a map with corresponding path tiles.
        /// </summary>
        public static void Display()
        {
            // size 'Tower + 1' because Tower will always be last enum
            char[] tileset = new char[(int)TileType.Tower + 1];

            /*
            U = Up, D = Down, L = Left, R = Right
             
            End point        - 0
            Start point      - 1
            horizontal path  - 2
            vertical path    - 3
            elbow left/up    - 4
            elbow right/up   - 5
            elbow left/down  - 6
            elbow right/down - 7
            UDL split        - 8
            UDR split        - 9
            ULR split        - 10
            DLR split        - 11
            4 way split      - 12
            empty space      - 13
            decoration       - 14
            tower            - 15
            */

            tileset[(int)TileType.StartPoint] = 'A';
            tileset[(int)TileType.EndPoint] = 'Z';
            tileset[(int)TileType.HorizPath] = '─';
            tileset[(int)TileType.VertPath] = '│';
            tileset[(int)TileType.ElbowLeftUp] = '┘';
            tileset[(int)TileType.ElbowRightUp] = '└';
            tileset[(int)TileType.ElbowLeftDown] = '┐';
            tileset[(int)TileType.ElbowRightDown] = '┌';
            tileset[(int)TileType.SplitUDL] = '┤';
            tileset[(int)TileType.SplitUDR] = '├';
            tileset[(int)TileType.SplitULR] = '┴';
            tileset[(int)TileType.SplitDLR] = '┬';
            tileset[(int)TileType.Split4Ways] = '┼';
            tileset[(int)TileType.EmptySpace] = ' ';
            tileset[(int)TileType.CurPosition] = 'C';
            tileset[(int)TileType.Decor] = 'D';
            tileset[(int)TileType.Tower] = 'T';

            for (int x = 0; x < Constant.GRID_WIDTH; ++x)
            {
                for (int y = 0; y < Constant.GRID_HEIGHT; ++y)
                {
                    Console.Write("{0}", tileset[(int)grid[x, y].tile]);
                }
                Console.WriteLine();
            }
        }
        #endregion

        #region Split Path Methods
        /// <summary>
        /// Create a random split path on the randomly generated map
        /// </summary>
        private static void SplitPath()
        {
            // create two points
            Pair Start = new Pair(rand.Next(2, Constant.GRID_WIDTH - 3), rand.Next(2, Constant.GRID_HEIGHT - 3));
            Pair End = new Pair(rand.Next(2, Constant.GRID_WIDTH - 3), rand.Next(2, Constant.GRID_HEIGHT - 3));
            // outer while loop to check if split path is placed
            while (SplitTileCount() == 0)
            {
                // reset the position
                Start = new Pair(rand.Next(2, Constant.GRID_WIDTH - 3), rand.Next(2, Constant.GRID_HEIGHT - 3));
                End = new Pair(rand.Next(2, Constant.GRID_WIDTH - 3), rand.Next(2, Constant.GRID_HEIGHT - 3));

                // find a path tile that is not on the border
                while (grid[Start.y, Start.x].tile > TileType.Split4Ways)
                {
                    // x-coordinate can be anywhere, but the border
                    Start.x = rand.Next(2, Constant.GRID_WIDTH - 3);
                    // y-coordinate can be anywhere, but the border
                    Start.y = rand.Next(2, Constant.GRID_WIDTH - 3);
                }
                // find a path tile that is not on the border
                while (grid[End.y, End.x].tile > TileType.Split4Ways || (Start.y == End.y && Start.x == End.x))
                {
                    End.x = rand.Next(2, Constant.GRID_WIDTH - 3);
                    End.y = rand.Next(2, Constant.GRID_WIDTH - 3);
                }

                // set split attribute to true for start/end points
                grid[Start.y, Start.x].split = true;
                grid[End.y, End.x].split = true;

                // set position to start
                _positionSplit = Start;

                // create path between start/end points
                while (_positionSplit.y != End.y || _positionSplit.x != End.x)
                {
                    int rNum = rand.Next(0, Constant.NUM_OF_DIRECTIONS);
                    // steps are always toward end point
                    // randomly choose next step
                    switch (rNum)
                    {
                        // move up
                        case (int)Direction.Up:
                            // check the difference between next step position and end point
                            if (_positionSplit.y > 0 && Math.Abs(_positionSplit.y - 1 - End.y) < Math.Abs(_positionSplit.y - End.y))
                                _positionSplit.y -= 1;
                            break;
                        // move down
                        case (int)Direction.Down:
                            if (_positionSplit.y < Constant.GRID_HEIGHT - 1 && Math.Abs(_positionSplit.y + 1 - End.y) < Math.Abs(_positionSplit.y - End.y))
                                _positionSplit.y += 1;
                            break;
                        // move left
                        case (int)Direction.Left:
                            if (_positionSplit.x > 0 && Math.Abs(_positionSplit.x - 1 - End.x) < Math.Abs(_positionSplit.x - End.x))
                                _positionSplit.x -= 1;
                            break;
                        // move right
                        case (int)Direction.Right:
                            if (_positionSplit.x < Constant.GRID_WIDTH - 1 && Math.Abs(_positionSplit.x + 1 - End.x) < Math.Abs(_positionSplit.x - End.x))
                                _positionSplit.x += 1;
                            break;
                    }
                    // set temporary tile
                    grid[_positionSplit.y, _positionSplit.x].tile = TileType.Split4Ways;

                    // set split attribute to true
                    grid[_positionSplit.y, _positionSplit.x].split = true;
                }
                // reconfigure tiles
                PlaceSplitTile();
            }
        }

        /// <summary>
        /// Iterates through grid and check if split attribute 
        /// is set to true to reconfigure tile to split tile
        /// </summary>
        private static void PlaceSplitTile()
        {
            for (int x = 0; x < Constant.GRID_HEIGHT; x++)
            {
                for (int y = 0; y < Constant.GRID_WIDTH; y++)
                {
                    if (grid[x, y].split == true)
                        grid[x, y].tile = ConfigureTile(TileCheck(x, y));
                }
            }

            // make a 2nd run through because TileCheck modifies an adjacent tile's split attribute
            for (int x = 0; x < Constant.GRID_HEIGHT; x++)
            {
                for (int y = 0; y < Constant.GRID_WIDTH; y++)
                {
                    if (grid[x, y].split == true)
                        grid[x, y].tile = ConfigureTile(TileCheck(x, y));
                }
            }
        }

        /// <summary>
        /// Checks each side of the current tile for tile presence.
        /// If there is, then set bool to true and modify the adjacent tile's split 
        /// attribute.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>bool array where true means tile adjacency, otherwise false</returns>
        private static bool[] TileCheck(int x, int y)
        {
            // create an array of bools for each side of the current tile
            bool[] tileFlags = new bool[Constant.NUM_OF_DIRECTIONS];

            // check if a tile exist to the top
            if (x > 0)
                if (grid[x - 1, y].tile <= TileType.Split4Ways)
                {
                    // modify tile's split attribute
                    grid[x - 1, y].split = true;
                    // set bool to true if tile is found
                    tileFlags[0] = true;
                }
            // check if a tile exist to the bottom
            if (x < Constant.GRID_HEIGHT - 1)
                if (grid[x + 1, y].tile <= TileType.Split4Ways)
                {
                    grid[x + 1, y].split = true;
                    tileFlags[1] = true;
                }
            // check if a tile exist to the left
            if (y > 0)
                if (grid[x, y - 1].tile <= TileType.Split4Ways)
                {
                    grid[x, y - 1].split = true;
                    tileFlags[2] = true;
                }
            // check if a tile exist to the right
            if (y < Constant.GRID_WIDTH - 1)
                if (grid[x, y + 1].tile <= TileType.Split4Ways)
                {
                    grid[x, y + 1].split = true;
                    tileFlags[3] = true;
                }

            // return array of bools
            return tileFlags;
        }

        /// <summary>
        /// Takes in an array of bools and depending on which
        /// and how many bools are true, return the specific tile
        /// </summary>
        /// <param name="tileFlags">array of bools that represent each side</param>
        /// <returns>TileType</returns>
        private static TileType ConfigureTile(bool[] tileFlags)
        {
            // tileFlags[0] = up
            // tileFlags[1] = down
            // tileFlags[2] = left
            // tileFlags[3] = right

            // Split 4 Ways tile
            if (tileFlags[0] && tileFlags[1] && tileFlags[2] && tileFlags[3])
                return TileType.Split4Ways;

            // Split up down left
            else if (tileFlags[0] && tileFlags[1] && tileFlags[2])
                return TileType.SplitUDL;

            // Split up down right
            else if (tileFlags[0] && tileFlags[1] && tileFlags[3])
                return TileType.SplitUDR;

            // Split up left right
            else if (tileFlags[0] && tileFlags[2] && tileFlags[3])
                return TileType.SplitULR;

            // Split down left right
            else if (tileFlags[1] && tileFlags[2] && tileFlags[3])
                return TileType.SplitDLR;

            // Elbow left up
            else if (tileFlags[0] && tileFlags[2])
                return TileType.ElbowLeftUp;

            // Elbow right up
            else if (tileFlags[0] && tileFlags[3])
                return TileType.ElbowRightUp;

            // Elbow down left
            else if (tileFlags[1] && tileFlags[2])
                return TileType.ElbowLeftDown;

            // Elbow down right
            else if (tileFlags[1] && tileFlags[3])
                return TileType.ElbowRightDown;

            // Horiz path
            else if (tileFlags[2] && tileFlags[3])
                return TileType.HorizPath;

            // Vert path
            else if (tileFlags[0] && tileFlags[1])
                return TileType.VertPath;

            // Should never get called
            else
                return TileType.EmptySpace;
        }

        /// <summary>
        /// Counts the number of split tiles on grid
        /// </summary>
        /// <returns></returns>
        private static int SplitTileCount()
        {
            int count = 0;

            for (int x = 0; x < Constant.GRID_WIDTH; x++)
            {
                for (int y = 0; y < Constant.GRID_HEIGHT; y++)
                {
                    if (grid[x, y].tile >= TileType.SplitUDL && grid[x, y].tile <= TileType.Split4Ways)
                        count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Splits the grid into four quadrants 
        /// and then checks if a path tile exists.
        /// </summary>
        /// <param name="quandrant"></param>
        /// <returns>true if a non-start/end point exists</returns>
        private static bool QuandrantCheck(int quandrant)
        {
            // Quandrants
            //   4 | 1
            //   -----
            //   3 | 2

            switch (quandrant)
            {
                // check if path tile exists in first quadrant of grid
                case 1:
                    // lower half, start at 1 to exclude border
                    for (int y = 1; y < Constant.GRID_HEIGHT / 2; y++)
                    {
                        // upper half, subtract 1 to exclude border
                        for (int x = Constant.GRID_WIDTH / 2; x < Constant.GRID_WIDTH - 1; x++)
                        {
                            // I dont want to overwrite start/end points
                            if (grid[y, x].tile < TileType.EmptySpace && grid[y, x].tile > TileType.StartPoint)
                                return true;
                        }
                    }
                    break;
                // check if path tile exists in second quadrant of grid
                case 2:
                    // upper half
                    for (int y = Constant.GRID_HEIGHT / 2; y < Constant.GRID_HEIGHT - 1; y++)
                    {
                        // upper half
                        for (int x = Constant.GRID_WIDTH / 2; x < Constant.GRID_WIDTH - 1; x++)
                        {
                            if (grid[y, x].tile < TileType.EmptySpace && grid[y, x].tile > TileType.StartPoint)
                                return true;
                        }
                    }
                    break;
                // check if path tile exists in third quadrant of grid
                case 3:
                    // upper half
                    for (int y = Constant.GRID_HEIGHT / 2; y < Constant.GRID_HEIGHT - 1; y++)
                    {
                        // lower half
                        for (int x = 1; x < Constant.GRID_WIDTH / 2; x++)
                        {
                            if (grid[y, x].tile < TileType.EmptySpace && grid[y, x].tile > TileType.StartPoint)
                                return true;
                        }
                    }
                    break;
                // check if path tile exists in fourth quadrant of grid
                case 4:
                    // lower half
                    for (int y = 1; y < Constant.GRID_HEIGHT / 2; y++)
                    {
                        // lower half
                        for (int x = 1; x < Constant.GRID_WIDTH / 2; x++)
                        {
                            if (grid[y, x].tile < TileType.EmptySpace && grid[y, x].tile > TileType.StartPoint)
                                return true;
                        }
                    }
                    break;
            }
            return false;
        }
        #endregion

        #region Decoration Functions
        /// <summary>
        /// Does a calculation to determine the number
        /// of decorations to be placed
        /// </summary>
        /// <returns>number of decorations as an int</returns>
        private static int CalculateDecorAmount()
        {
            double numOfDecors = 0;
            double numOfEmpty = 0;

            // go through the whole grid and count all the empty spaces
            for (int x = 0; x < Constant.GRID_HEIGHT; x++)
            {
                for (int y = 0; y < Constant.GRID_WIDTH; y++)
                {
                    if (grid[x, y].tile == TileType.EmptySpace)
                        numOfEmpty++;
                }
            }

            // number of decorations will be a certain percentage of empty spaces.
            // *the percentage will probably be a constant*
            numOfDecors = numOfEmpty * .3f;
            
            return (int)numOfDecors;
        }

        /// <summary>
        /// Count decoration tiles
        /// </summary>
        /// <returns>Number of decorations</returns>
        private static int DecorCount()
        {
            int count = 0;

            // height of the grid
            for (int x = 0; x < Constant.GRID_HEIGHT; x++)
            {
                // width of the grid
                for (int y = 0; y < Constant.GRID_WIDTH; y++)
                {
                    if (grid[x, y].tile == TileType.Decor || grid[x, y].tile == TileType.Decor2)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Places decoration tiles according to specified amount
        /// </summary>
        /// <param name="num">Number of decorations desired</param>
        private static void PlaceDecor2(int num)
        {
            // number check
            while (DecorCount() < num)
            {
                PutDecor2(rand.Next(0, Constant.GRID_HEIGHT - 1), rand.Next(0, Constant.GRID_WIDTH - 1), TileType.Decor2);
                PutDecor(rand.Next(0, Constant.GRID_HEIGHT - 1), rand.Next(0, Constant.GRID_WIDTH - 1));
            }
        }

        /// <summary>
        /// Recursively place decoration tiles depending on amount of empty tiles in each direction
        /// </summary>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <param name="decor">Desired decoration tile</param>
        private static void PutDecor2(int x, int y, TileType decor)
        {
            if (grid[x, y].tile == TileType.EmptySpace)
            {
                // count number of empty tiles to the top
                EmptyTileCount(x, y, Direction.Up);
                // bottom
                EmptyTileCount(x, y, Direction.Down);
                // left
                EmptyTileCount(x, y, Direction.Left);
                // right
                EmptyTileCount(x, y, Direction.Right);

                // place first decoration
                grid[x, y].tile = decor;

                // recursively place decorations
                // up
                if (_emptyCount[0] > 2)
                {
                    // reset counter
                    _emptyCount[0] = 0;
                    // recursive decor placement
                    PutDecor2(x - 1, y, decor);
                }
                // bottom
                if (_emptyCount[1] > 2)
                {
                    _emptyCount[1] = 0;
                    PutDecor2(x + 1, y, decor);
                }
                // left
                if (_emptyCount[2] > 2)
                {
                    _emptyCount[2] = 0;
                    PutDecor2(x, y - 1, decor);
                }
                // right
                if (_emptyCount[3] > 2)
                {
                    _emptyCount[3] = 0;
                    PutDecor2(x, y + 1, decor);
                }

                // reset all counters
                for (int i = 0; i < Constant.NUM_OF_DIRECTIONS; i++)
                {
                    _emptyCount[i] = 0;
                }
            }
        }

        /// <summary>
        /// Recursively searches in one direction for empty tiles and increments counter
        /// </summary>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <param name="dir">The direction to search</param>
        private static void EmptyTileCount(int x, int y, Direction dir)
        {
            // check to the top
            if (x > 0 && dir == Direction.Up)
                if (grid[x - 1, y].tile == TileType.EmptySpace)
                {
                    _emptyCount[0]++;
                    EmptyTileCount(x - 1, y, Direction.Up);
                }
            // check to the bottom
            if (x < Constant.GRID_WIDTH - 1 && dir == Direction.Down)
                if (grid[x + 1, y].tile == TileType.EmptySpace)
                {
                    _emptyCount[1]++;
                    EmptyTileCount(x + 1, y, Direction.Down);
                }
            // check to the left
            if (y > 0 && dir == Direction.Left)
                if (grid[x, y - 1].tile == TileType.EmptySpace)
                {
                    _emptyCount[2]++;
                    EmptyTileCount(x, y - 1, Direction.Left);
                }
            // check to the right
            if (y < Constant.GRID_HEIGHT - 1 && dir == Direction.Right)
                if (grid[x, y + 1].tile == TileType.EmptySpace)
                {
                    _emptyCount[3]++;
                    EmptyTileCount(x, y + 1, Direction.Right);
                }
        }

        /// <summary>
        /// Given the position, place decoration tile if empty
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private static void PutDecor(int x, int y)
        {
            if (grid[x, y].tile == TileType.EmptySpace)
                grid[x, y].tile = TileType.Decor;
        }
        #endregion

        #region Tower Functions
        /// <summary>
        /// Randomly check for empty space on grid next to path; if empty, changes the tile to Tower 
        /// </summary>
        /// <param name="Constant.NUM_OF_TOWERS">number of desired towers</param>
        private static void SingleTower()
        {
            // flag to find new rower
            bool towerPlaced = false;

            // random row and column
            Pair randomPos = new Pair(rand.Next(0, Constant.GRID_WIDTH), rand.Next(0, Constant.GRID_HEIGHT));

            // loop that finds a valid tower placement
            while (!towerPlaced)
            {
                // if iterator reaches end and no matches made or tower placement flag raised, find new row
                if (grid[randomPos.x, randomPos.y].tile > TileType.Split4Ways || towerPlaced)
                {
                    randomPos.x = rand.Next(0, Constant.GRID_WIDTH);
                    randomPos.y = rand.Next(0, Constant.GRID_HEIGHT);
                    towerPlaced = false;
                }

                // check if position in bounds and if position is a path tile 
                if (grid[randomPos.x, randomPos.y].tile <= TileType.Split4Ways)
                {
                    // check to the left of the tile
                    if (randomPos.y > 0 && grid[randomPos.x, randomPos.y - 1].tile == TileType.EmptySpace)
                        grid[randomPos.x, randomPos.y - 1].tile = TileType.Tower;

                    // check to the right of tile, if empty place tower
                    else if (randomPos.y < Constant.GRID_HEIGHT - 1 && grid[randomPos.x, randomPos.y + 1].tile == TileType.EmptySpace)
                        grid[randomPos.x, randomPos.y + 1].tile = TileType.Tower;

                    // check to the bottom of the tile
                    else if (randomPos.x < Constant.GRID_WIDTH - 1 && grid[randomPos.x + 1, randomPos.y].tile == TileType.EmptySpace)
                        grid[randomPos.x + 1, randomPos.y].tile = TileType.Tower;

                    // check to the top of the tile
                    else if (randomPos.x > 0 && grid[randomPos.x - 1, randomPos.y].tile == TileType.EmptySpace)
                        grid[randomPos.x - 1, randomPos.y].tile = TileType.Tower;

                    // if all tiles occupied, break
                    else
                        break;

                    // once a tower is placed, raise flag to find new row
                    towerPlaced = true;
                }
            }
        }

        /// <summary>
        /// Randomly check for empty space on grid next to path; if empty, changes the tile to Tower.
        /// </summary>
        /// <param name="Constant.NUM_OF_TOWERS">number of desired towers</param>
        private static void PlaceTowers()
        {
            // flag to find new rower
            bool towerPlaced = false;

            // random row and column
            Pair randomPos = new Pair(rand.Next(0, Constant.GRID_WIDTH), rand.Next(0, Constant.GRID_HEIGHT));

            // loop that finds a valid tower placement
            while (TowerCount() != Constant.NUM_OF_TOWERS)
            {
                // if iterator reaches end and no matches made or tower placement flag raised, find new row
                if (grid[randomPos.x, randomPos.y].tile > TileType.Split4Ways || towerPlaced)
                {
                    randomPos.x = rand.Next(0, Constant.GRID_WIDTH);
                    randomPos.y = rand.Next(0, Constant.GRID_HEIGHT);
                    towerPlaced = false;
                }

                // check if position in bounds and if position is a path tile 
                if (grid[randomPos.x, randomPos.y].tile <= TileType.Split4Ways)
                {
                    // check to the left of the tile
                    if (randomPos.y > 0 && grid[randomPos.x, randomPos.y - 1].tile == TileType.EmptySpace)
                        grid[randomPos.x, randomPos.y - 1].tile = TileType.Tower;

                    // check to the right of tile, if empty place tower
                    else if (randomPos.y < Constant.GRID_HEIGHT - 1 && grid[randomPos.x, randomPos.y + 1].tile == TileType.EmptySpace)
                        grid[randomPos.x, randomPos.y + 1].tile = TileType.Tower;

                    // check to the bottom of the tile
                    else if (randomPos.x < Constant.GRID_WIDTH - 1 && grid[randomPos.x + 1, randomPos.y].tile == TileType.EmptySpace)
                        grid[randomPos.x + 1, randomPos.y].tile = TileType.Tower;

                    // check to the top of the tile
                    else if (randomPos.x > 0 && grid[randomPos.x - 1, randomPos.y].tile == TileType.EmptySpace)
                        grid[randomPos.x - 1, randomPos.y].tile = TileType.Tower;

                    // if all tiles occupied, break
                    else
                        break;

                    // once a tower is placed, raise flag to find new row
                    towerPlaced = true;
                }
            }
        }

        /// <summary>
        /// Count towers
        /// </summary>
        /// <returns></returns>
        private static int TowerCount()
        {
            int count = 0;

            // height of the grid
            for (int y = 0; y < Constant.GRID_HEIGHT; y++)
            {
                // width of the grid
                for (int x = 0; x < Constant.GRID_WIDTH; x++)
                {
                    if (grid[y, x].tile == TileType.Tower)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// PlaceTower(): Attempts to place a tower on a square diagonal to the current position. Returns true if
        /// successful, false otherwise
        /// </summary>
        /// <returns></returns>
        private static bool PlaceTower()
        {
            if (_position.y > 0 && _position.y < Constant.GRID_HEIGHT - 1 && _position.x > 0 && _position.x < Constant.GRID_WIDTH - 1)
            {
                switch (rand.Next(0, Constant.NUM_OF_DIRECTIONS))
                {
                    case 0: // Lower Right
                        if (grid[_position.y + 1, _position.x + 1].tile == TileType.EmptySpace)
                        {
                            grid[_position.y + 1, _position.x + 1].tile = TileType.Tower;
                            return true;
                        }
                        break;
                    case 1: // Lower Left
                        if (grid[_position.y + 1, _position.x - 1].tile == TileType.EmptySpace)
                        {
                            grid[_position.y + 1, _position.x - 1].tile = TileType.Tower;
                            return true;
                        }
                        break;
                    case 2: // Upper Right
                        if (grid[_position.y - 1, _position.x + 1].tile == TileType.EmptySpace)
                        {
                            grid[_position.y - 1, _position.x + 1].tile = TileType.Tower;
                            return true;
                        }
                        break;
                    case 3: // Upper Left
                        if (grid[_position.y - 1, _position.x - 1].tile == TileType.EmptySpace)
                        {
                            grid[_position.y - 1, _position.x - 1].tile = TileType.Tower;
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        #endregion
    }
}