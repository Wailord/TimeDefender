using UnityEngine;
using Assets.Code.Interfaces;
using Assets.Code.Behaviors;
using Assets.Code.Navigation;
using System.Collections.Generic;


namespace Assets.Code.Controllers
{

    public class NavigationController
    {
        /// <summary>
        /// Our Unity-style singleton to ensure we have just one
        /// NavigationController in the game.
        /// </summary>
        public static NavigationController Instance
        {
            get { return _instance ?? (_instance = new NavigationController()); }
        }
        private static NavigationController _instance;

        /// <summary>
        /// The class used to manage our pathfinding. Any calls to pathfind go through
        /// this data member.
        /// </summary>
        private static SpatialAStar<GridPoint, UnitMoverType> _pather;

        /// <summary>
        /// The height (in grid squares) of the map.
        /// </summary>
        public const int GRID_HEIGHT = 20;
        /// <summary>
        /// The width (in grid squares) of the map.
        /// </summary>
        public const int GRID_WIDTH = 20;

        /// <summary>
        /// A reference to the globally used navigation grid of GridPoints.
        /// </summary>
        public GridPoint[,] NavigationGrid
        { get { return _navGrid; } }
        private GridPoint[,] _navGrid;

        public GridPoint FirstTile { get; set; }
        public GridPoint LastTile { get; set; }

        public GridPoint GetGridPoint(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < GRID_WIDTH && y < GRID_HEIGHT) return _navGrid[x, y];

            return null;
        }

        public NavigationController()
        {
            //Debug.Log("Creating navigation grid...");
            _navGrid = new GridPoint[GRID_HEIGHT, GRID_WIDTH];
            for (int h = 0; h < GRID_HEIGHT; h++)
                for (int w = 0; w < GRID_WIDTH; w++)
                    _navGrid[h, w] = new GridPoint(h, w);

            _pather = new SpatialAStar<GridPoint, UnitMoverType>(_navGrid);
        }

        public LinkedList<GridPoint> FindAPath(GridPoint start, GridPoint dest, UnitMoverType moveType)
        {
            return _pather.Search(start, dest, moveType);
        }

        public List<GridPoint> GetGridPointsInRange(GridPoint source, int range)
        {
            List<GridPoint> inRange = new List<GridPoint>();
            GridPoint curPoint = null;

            for (int x = source.x - range; x <= source.x + range; x++)
            {
                for (int y = source.y - range; y <= source.y + range; y++)
                {
                    curPoint = GetGridPoint(x, y);
                    if (curPoint != null) inRange.Add(curPoint);
                }
            }

            return inRange;
        }
    }
}