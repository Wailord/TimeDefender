using Assets.Code.Controllers;
using Assets.Code.Navigation;
using UnityEngine;
using System.Collections.Generic;
using Assets.Code.Units;
using System.Collections;
using Assets.Code.Behaviors;
using Assets.Code.Enums;

public class StopsToAttackBehavior : IAttackBehavior
{

    private int _currentAttackRange;
    private int _currentAttackDamage;
    private float _currentAttackDelay;
    
    public StopsToAttackBehavior(Faction fact, float delay, int damage, int range)
    {
        _attackDelay = delay;
        _timeUntilNextAttack = 0;
        _attackDamage = damage;
        _attackRange = range;
        _currentAttackRange = range;
        _currentAttackDamage = damage;
        _currentAttackDelay = delay;
        _faction = fact;
        _allowedToMove = true;
    }

    /// <summary>
    /// The associated unit's Faction.
    /// </summary>
    private Faction _faction;

    /// <summary>
    /// Returns whether or not the unit is allowed to move. Because this unit StopsToAttack,
    /// if the unit is currently attacking, the unit will return false.
    /// </summary>
    public bool AllowedToMove
    { get { return _allowedToMove; } }
    private bool _allowedToMove;

    /// <summary>
    /// The length a unit must wait between attacks in seconds.
    /// </summary>
    public float AttackDelay
    { get { return _currentAttackDelay; } }
    private float _attackDelay;

    /// <summary>
    /// The remaining attack cooldown of the unit in seconds.
    /// </summary>
    private float _timeUntilNextAttack;

    /// <summary>
    /// The unit that owns this attacker behavior.
    /// </summary>
    public Unit Owner
    {
        get { return _owner; }
        set { _owner = value; }
    }
    private Unit _owner;

    /// <summary>
    /// The current unit that this attacker is seeking to attack or attacking.
    /// </summary>
    public Unit Target
    {
        get { return _target; }
        set { _target = value; }
    }
    private Unit _target;

    /// <summary>
    /// The amount of damage done by one attack from this unit.
    /// </summary>
    public int AttackDamage
    {
        get { return _currentAttackDamage; }
    }
    private int _attackDamage;

    /// <summary>
    /// The distance from which this unit can attack. If a unit has an attack range of 3,
    /// it can attack anything in the 7x7 square surrounding around it.
    /// </summary>
    public int AttackRange
    {
        get { return _currentAttackRange; }
    }
    private int _attackRange;

    /// <summary>
    /// Services one update cycle for the attack behavior.
    /// </summary>
    public void Service()
    {
        //Debug.Log("Servicing attack behavior for " + _owner.Name);
        // find a new target
        ReacquireTarget();

        // unit attacking, so do attack stuff...
        if (_target != null)
            ServiceAttack();
    }

    /// <summary>
    /// Returns whether or not a given GridPoint is in range of the attacker.
    /// </summary>
    /// <param name="point">The point that will be compared to the attacker's location to check for in-rangeness.</param>
    /// <returns></returns>
    private bool IsInRange(GridPoint point)
    {
        return GridPoint.InRange(_owner.MovementBehavior.CurrentLocation, point, _attackRange);
    }

    void ReacquireTarget()
    {
        List<GridPoint> inRange = NavigationController.Instance.GetGridPointsInRange(_owner.MovementBehavior.CurrentLocation, _attackRange);
        _allowedToMove = true;
        Unit newTarget = null;
        int closest = -1;

        foreach (GridPoint pt in inRange)
        {
            // if we set the target at any point below, stop checking surrounding GridPoints;
            // this also means units will prioritize the top-left-most units
            foreach (Unit unitOnPt in pt.Occupants)
            {
                // if the unit is an enemy, set it as the new target
                if (unitOnPt != null && unitOnPt.UnitFaction != _faction)
                {
                    //Debug.Log(string.Format("{0} has {1} tiles left...", unitOnPt.Name, unitOnPt.TilesUntilEnd));
                    if (closest == -1 || unitOnPt.TilesUntilEnd < closest)
                    {
                        //Debug.Log(string.Format("maybe switching to {0}", unitOnPt.Name));
                        newTarget = unitOnPt;
                        closest = unitOnPt.TilesUntilEnd;
                    }
                    else
                    {
                        //Debug.Log(string.Format("... but there's a closer unit"));
                    }
                }
            }

            if (newTarget != null)
            {
                //Debug.Log(string.Format("{0} deemed closest this tick, attacking now", newTarget.Name));
                _target = newTarget;

                // because this is a StopsToAttackBehavior, it can't move so long as it has a living target
                _allowedToMove = false;
            }
        }
    }

    void ServiceAttack()
    {
        if (!_target.TargetBehavior.IsDead && IsInRange(_target.MovementBehavior.CurrentLocation))
        {
            // if cooldown isn't up, subtract from the time remaining
            if (_timeUntilNextAttack >= 0)
            {
                //Debug.Log(_owner.Name + " waiting to launch attack at " + _target.Name + "; " + _timeUntilNextAttack + " seconds remain");
                _timeUntilNextAttack -= TimeController.deltaTime;
            }
            else
            {
                //Debug.Log(_owner.Name + " launching attack at " + _target.Name);
                _timeUntilNextAttack = _attackDelay;
                CombatController.Instance.LaunchAttackAtUnit(_owner, _target);
            }
        }
        else if (_target.TargetBehavior.IsDead)
        {
            //Debug.Log(_target.Name + " died, " + _owner.Name + " is no longer attacking it");
            _target = null;
        }
        else
        {
            _target = null;
        }

        // if target dies and the unit's a lemming, maybe do the following line?
        // _owner.MovementBehavior.PathTo(NavigationController.Instance.LastTile);
    }

    /// <summary>
    /// Adjusts the unit's attack damage to the current damage plus the provided integer.
    /// </summary>
    /// <param name="additive">The number to add to the current attack damage.</param>
    public void UpgradeAttackPower(int additive)
    {
        _currentAttackDamage += additive;
    }

    /// <summary>
    /// Adjusts the unit's attack damage to the current damage multiplied by the provided
    /// multiplier.
    /// </summary>
    /// <param name="multiplicative">The number to multiply the current attack damage by.</param>
    public void UpgradeAttackPower(float multiplicative)
    {
        _currentAttackDamage = (int)(_currentAttackDamage * multiplicative);
    }

    /// <summary>
    /// Adjusts the units' attack delay to the current attack delay plus the provided integer.
    /// </summary>
    /// <param name="additive">The number to add to the current attack delay.</param>
    public void UpgradeAttackCooldown(int additive)
    {
        _currentAttackDelay += additive;
    }

    /// <summary>
    /// Adjusts the unit's attack delay to the current attack delay multiplied by the provided
    /// multiplier.
    /// </summary>
    /// <param name="multiplicative"></param>
    public void UpgradeAttackCooldown(float multiplicative)
    {
        _currentAttackDelay *= multiplicative;
    }

    /// <summary>
    /// Reverts the unit's attack damage to its base damage.
    /// </summary>
    public void RevertAttackPower()
    {
        _currentAttackDamage = _attackDamage;
    }

    /// <summary>
    /// Reverts the unit's attack delay to its base attack delay.
    /// </summary>
    public void RevertAttackCooldown()
    {
        _currentAttackDelay = _attackDelay;
    }
}
