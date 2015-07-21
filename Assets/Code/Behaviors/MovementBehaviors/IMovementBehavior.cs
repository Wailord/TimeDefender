using System.Collections;
using Assets.Code.Units;
using Assets.Code.Navigation;
using Assets.Code.Interfaces;
using System.Collections.Generic;

namespace Assets.Code.Behaviors
{
    public interface IMovementBehavior : IServiceable
    {
        /// <summary>
        /// The speed of a unit. In units I'm not really sure of right now.
        /// TODO: actually, like, find out what the # represents - Ryan
        /// </summary>
        int MoveSpeed { get; }

        /// <summary>
        /// The tiles remaining in the unit's current path.
        /// </summary>
        int TilesUntilEnd { get; }

        /// <summary>
        /// The mover type of the unit. Used to determine how the unit can traverse (flying, walking).
        /// </summary>
        UnitMoverType MoveType { get; }

        /// <summary>
        /// The current location of the unit on the map.
        /// </summary>
        GridPoint CurrentLocation { get; }

        /// <summary>
        /// The location that the unit is currently pathing to.
        /// </summary>
        GridPoint TargetLocation { get; }

        /// <summary>
        /// The current path that the movement behavior is traversing.
        /// </summary>
        LinkedList<GridPoint> PathToTraverse { get; }

        /// <summary>
        /// Called when a unit reaches its destination GridPoint.
        /// </summary>
        void DestinationReached();

        void PathTo(GridPoint dest);

        Unit Owner { get; set; }
    }

    /// <summary>
    /// Represents the different mover types in the game (flying units, walking units, etc).
    /// </summary>
    public enum UnitMoverType
    { Walker, Flyer, NoMovement }
}