using Assets.Code.Units;
using Assets.Code.GUICode;
using System.Collections;
using Assets.Code.Enums;
using System.Collections.Generic;
using Assets.Code.Behaviors;
using UnityEngine;
using Assets.Code.MapGeneration;

namespace Assets.Code.Navigation
{
    public class GridPoint : IPathNode<UnitMoverType>
    {
        private List<Unit> _unitsOnSquare;
        public List<Unit> Occupants
        { get { return _unitsOnSquare; } }

        /// <summary>
        /// The number of blockers to walking mover types
        /// currently on this GridPoint.
        /// </summary>
        public int WalkerBlockers
        {
            get { return _walkerBlockers; }
            set { _walkerBlockers = value; }
        }
        private int _walkerBlockers;
        private int _tempWalkerBlockers;

        public TileType TileType
        { get; set; }

        /// <summary>
        /// The number of blockers to flying mover types
        /// currently on this GriDdoint.
        /// </summary>
        public int FlyerBlockers
        {
            get { return _flyerBlockers; }
            set { _flyerBlockers = value; }
        }
        private int _flyerBlockers;
        private int _tempFlyerBlockers;
        
        /// <summary>
        /// The GridPoint's X coordinate.
        /// </summary>
        public int x
        {
            get { return _x; }
        }
        private int _x;

        /// <summary>
        /// The GridPoints Y coordinate.
        /// </summary>
        public int y
        {
            get { return _y; }
        }
        private int _y;

        /// <summary>
        /// The position on the screen that the given GridPoint is to be drawn.
        /// </summary>
        public Vector2 WorldPosition
        { get { return _worldPos; } }
        private Vector2 _worldPos;

        public GridPoint(int x, int y)
        {
            _x = x;
            _y = y;
            _walkerBlockers = 0;
            _tempWalkerBlockers = 0;
            _flyerBlockers = 0;

            _unitsOnSquare = new List<Unit>();

            _worldPos = new Vector2(x, y);
            
            _tempFlyerBlockers = 0;
        }

        public bool IsWalkable(UnitMoverType moverType)
        {
            bool walkable;

            switch(moverType)
            {
                case UnitMoverType.Flyer:
                    walkable = (_flyerBlockers == 0);
                    break;
                case UnitMoverType.Walker:
                    walkable = (_walkerBlockers == 0);
                    break;
                default:
                    walkable = false;
                    break;
            }

            return walkable;
        }

        /// <summary>
        /// Sets the GridPoint to be traversable by all mover types.
        /// </summary>
        public void SetWalkable()
        {
            _tempFlyerBlockers = _flyerBlockers;
            _flyerBlockers = 0;

            _tempWalkerBlockers = _walkerBlockers;
            _walkerBlockers = 0;
        }

        /// <summary>
        /// Undoes the most recent call to SetWalkable(). NOTE: This will
        /// not necessarily cause the GridPoint to be unwalkable; it simply
        /// returns the GridPoint to whatever its status was before the last
        /// call to SetWalkable().
        /// </summary>
        public void RevertWalkable()
        {
            _walkerBlockers = _tempWalkerBlockers;
            _flyerBlockers = _tempFlyerBlockers;
        }

        public override string ToString()
        {
            return string.Format("(" + x + ", " + y + ")");
        }

        public static bool InRange(GridPoint a, GridPoint b, float range)
        {
            Vector2 av = new Vector2(a.x, a.y);
            Vector2 bv = new Vector2(b.x, b.y);
            float dist = Vector2.Distance(av, bv);

            return dist <= range;
        }

        internal void AddToTile(Unit testUnit)
        {
            _unitsOnSquare.Add(testUnit);
        }
    }
}