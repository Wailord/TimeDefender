using UnityEngine;
using Assets.Code.Controllers;
using Assets.Code.Behaviors;
using Assets.Code.Units;
using Assets.Code.Navigation;
using System.Collections.Generic;
using Assets.Code.Scores;

public class GenericMovementBehavior : IMovementBehavior
{
    public GenericMovementBehavior(int moveSpeed, UnitMoverType moveType, GridPoint spawnPos = null)
    {
        _moveSpeed = moveSpeed;
        _moveType = moveType;
        _currentLocation =
        _targetLocation = null;

        if (spawnPos == null) _currentLocation = NavigationController.Instance.FirstTile;
        else _currentLocation = spawnPos;

        // pathing stuff
        _pathToTraverse = null;
        _pendingPath = null;
        _ratioAlongCurrentStep = 0f;
        _currentPathNode = null;
        _currentStepLength = 0.0f;
        _nodesRemaining = -1;
    }

    private int _nodesRemaining;
    private float SQRT_2 = Mathf.Sqrt(2);

    /// <summary>
    /// The move speed of the unit.
    /// </summary>
    public int MoveSpeed
    { get { return _moveSpeed; } }
    private int _moveSpeed;

    public int TilesUntilEnd
    {
        get
        {
            if (_pathToTraverse != null)
                return _nodesRemaining;
            else
                return -1;
        }
    }

    /// <summary>
    /// The mover type of the unit based on the UnitMoverType enumeration.
    /// </summary>
    public UnitMoverType MoveType
    { get { return _moveType; } }
    private UnitMoverType _moveType;

    /// <summary>
    /// The current GridPoint location of the unit.
    /// </summary>
    public GridPoint CurrentLocation
    { get { return _currentLocation; } }
    private GridPoint _currentLocation;

    public GridPoint TargetLocation
    { get { return _targetLocation; } }
    private GridPoint _targetLocation;

    /// <summary>
    /// The unit associated with this movement behavior.
    /// </summary>
    public Unit Owner
    {
        get { return _owner; }
        set { _owner = value; }
    }
    private Unit _owner;

    /// <summary>
    /// The path that the unit is either traversing or set to traverse.
    /// </summary>
    public LinkedList<GridPoint> PathToTraverse
    {
        get { return _pathToTraverse; }
    }
    private LinkedList<GridPoint> _pathToTraverse;
    
    /// <summary>
    /// If a unit plans to path elsewhere after its current path, it is
    /// stored here.
    /// </summary>
    private LinkedList<GridPoint> _pendingPath;
    
    /// <summary>
    /// Percent of the way along the current path step that the unit has traveled.
    /// </summary>
    private float _ratioAlongCurrentStep;
    
    /// <summary>
    /// The current node that the unit is working with along its path of nodes.
    /// </summary>
    private LinkedListNode<GridPoint> _currentPathNode;
    
    /// <summary>
    /// The distance of the current path node.
    /// </summary>
    private float _currentStepLength;

    /// <summary>
    /// Services one update cycle of the movement behavior.
    /// </summary>
    public void Service()
    {
        // Debug.Log("Servicing a generic movement behavior...");

        // if you are on a path...
        if (_pathToTraverse != null && _owner.AttackBehavior.AllowedToMove)
        {
            // Debug.Log("Servicing a path...");
            // initalize some basic values
            bool destinationReached = false;

            // snap the distance along the current step in the path to zero if negative
            if (_ratioAlongCurrentStep < 0)
                _ratioAlongCurrentStep = 0;

            // if the current step completed on this update...
            if (_ratioAlongCurrentStep >= 1)
            {
                // and the step # is the second to last-step
                if (_currentPathNode == _pathToTraverse.Last.Previous || _currentPathNode == _pathToTraverse.Last)
                {
                    // ...we're done
                    destinationReached = true;
                    _ratioAlongCurrentStep = 1;
                }
                else
                {
                    // ... then increment the step # and path to the next step
                    if (_owner.AttackBehavior.AllowedToMove)
                    {
                        _currentPathNode = _currentPathNode.Next;
                        //Debug.Log(Owner.Name + " now at " + _currentPathNode.Value);
                        InitPathStep();
                        SetLocation(_currentPathNode.Value);
                    }
                    else
                    {
                        DestinationReached();
                        _pathToTraverse = null;
                    }
                }
            }

            float distanceToMove = MoveSpeed * TimeController.deltaTime;
            _ratioAlongCurrentStep += (distanceToMove / _currentStepLength);

            // get a reference to the current step's node
            LinkedListNode<GridPoint> fromNode = _currentPathNode;
            Vector3 fromPos = fromNode.Value.WorldPosition;

            // set the unit's location to that grid point
            SetLocation(fromNode.Value);

            // get the destination node (wtf?)
            LinkedListNode<GridPoint> toNode = (_pathToTraverse.Count > 1) ?
                _currentPathNode.Next
                : _pathToTraverse.First;
            Vector3 toPos = toNode.Value.WorldPosition;

            // if the associated unit has a view, update its position to the fromNode +/- a certain
            // amount of distance depending on how far it is to its next node
            if (Owner.UnitView != null)
            {
                // Debug.Log(Owner.Name + " @ " + (fromPos + _ratioAlongCurrentStep * (toPos - fromPos)));
                Owner.UnitView.SetViewPosition(fromPos + _ratioAlongCurrentStep * (toPos - fromPos));
            }

            // if the traverser reached its destination in this update...
            if (destinationReached)
            {
                SetLocation(toNode.Value);
                Owner.MovementBehavior.DestinationReached();
            }
            else
            {
                // TODO maybe update the associated unit's view to look at the next node in line
            }
        }
    }

    /// <summary>
    /// Finds a path to the provided GridPoint and sets the unit on its way there.
    /// </summary>
    /// <param name="dest">The GridPoint the unit should path to</param>
    public void PathTo(GridPoint dest)
    {
        // TODO: probably some other variables that need removed here, or checks to be done...
        _targetLocation = dest;
        FindAPath();
    }

    /// <summary>
    /// Finds a path from the unit's current location to the unit's target location.
    /// </summary>
    private void FindAPath()
    {
        // if we're finding a path, zero out the old one
        _pendingPath = null;

        // convert the destination GridPoint to its corresponding GridTile
        GridPoint destTile = _targetLocation;

        // force the destination to be walkable so we can path to that point
        destTile.SetWalkable();

        //fill the path
        _pendingPath = NavigationController.Instance.FindAPath(CurrentLocation, TargetLocation, MoveType);

        // undo whatever was changed when forcing the destination to be walkable
        destTile.RevertWalkable();

        // so long as we found a path...
        if (_pendingPath != null)
        {
            // remove the last node, as it was our destination
            if (!destTile.IsWalkable(MoveType))
                _pendingPath.RemoveLast();

            if (_pathToTraverse == null)
            {
                _pathToTraverse = _pendingPath;
                _pendingPath = null;
                if (_pathToTraverse != null)
                {
                    _currentPathNode = _pathToTraverse.First;
                    InitPathStep();
                    SetLocation(_currentPathNode.Value);
                    _nodesRemaining = _pathToTraverse.Count;
                }
            }
        }
        else
        {
            // if you hit this, there is no path to the destination tile, wah wah
            Debug.Log("impossible to path to " + destTile + " :(");
        }
    }

    /// <summary>
    /// Sets the GridPoint location of the unit.
    /// </summary>
    /// <param name="point">The point to set the unit's location to</param>
    public void SetLocation(GridPoint point)
    {
        // Debug.Log(" now resides at " + point);

        // when a traverser's location is set, first remove it from the GridPoint it resides on
        _currentLocation.Occupants.Remove(_owner);

        // set the traverser's position to the provided point
        _currentLocation = point;

        // finally, add the traverser to the GridPoint it resides on
        _currentLocation.Occupants.Add(_owner);
    }

    /// <summary>
    /// Prepares the unit to move from one node in the path to the next.
    /// </summary>
    private void InitPathStep()
    {
        // clip off completed part of the current step and get the leftover
        while (_ratioAlongCurrentStep >= 1)
            _ratioAlongCurrentStep -= 1;

        // get a reference to the current node of the traverser as its start position for the path
        LinkedListNode<GridPoint> fromNode = _currentPathNode;

        // get a reference to the next node in the traverser's path for its destination (step-wise) node
        LinkedListNode<GridPoint> toNode = _pathToTraverse.Count > 1 ?
            _currentPathNode.Next
            : _pathToTraverse.First;

        _nodesRemaining--;

        // get the length from the current node to the next node in the step
        // HACK null checks are bad
        if (fromNode == null || toNode == null || fromNode.Value.x == toNode.Value.x || fromNode.Value.y == toNode.Value.y) _currentStepLength = 1;
        else _currentStepLength = SQRT_2;
    }

    /// <summary>
    /// Called when a unit reaches its destination and nulls out its path as a result.
    /// </summary>
    public void DestinationReached()
    {
        Debug.Log(Owner.Name + " reached its destination of " + TargetLocation);
        //HACK
        GameScore.GetInstance().LoseLife();
        WaveController.GetInstance().RemoveEnemy();
        _pathToTraverse = null;
    }
}
