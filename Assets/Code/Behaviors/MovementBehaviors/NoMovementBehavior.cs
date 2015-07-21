using UnityEngine;
using Assets.Code.Controllers;
using Assets.Code.Behaviors;
using Assets.Code.Units;
using Assets.Code.Navigation;
using System.Collections.Generic;

namespace Assets.Code.Behaviors
{
    public class NoMovementBehavior : IMovementBehavior
    {
        public NoMovementBehavior(GridPoint spawnPos = null)
        {
            if (spawnPos == null) _currentLocation = NavigationController.Instance.FirstTile;
            else _currentLocation = spawnPos;
        }

        public int MoveSpeed
        { get { return 0; } }

        public int TilesUntilEnd
        {
            get { return -1; }
        }

        public UnitMoverType MoveType
        { get { return UnitMoverType.NoMovement; } }

        private GridPoint _currentLocation;
        public Assets.Code.Navigation.GridPoint CurrentLocation
        { get { return _currentLocation; } }

        public Assets.Code.Navigation.GridPoint TargetLocation
        { get { return null; } }

        public Unit Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }
        private Unit _owner;

        public LinkedList<GridPoint> PathToTraverse
        {
            get { return null; }
        }

        public void Service()
        {
            if (Owner.UnitView != null)
                Owner.UnitView.SetViewPosition(_currentLocation.WorldPosition);
        }

        public void PathTo(GridPoint dest)
        {
            // NoOp
        }

        public void SetLocation(GridPoint point)
        {
            //Debug.Log(" now resides at " + point);

            // when a traverser's location is set, first remove it from the GridPoint it resides on
            _currentLocation.Occupants.Remove(_owner);

            // set the traverser's position to the provided point
            _currentLocation = point;

            // TODO finally, add the traverser to the GridPoint it resides on
            _currentLocation.Occupants.Add(_owner);
        }

        public void DestinationReached()
        {
            // NoOp
        }
    }

}