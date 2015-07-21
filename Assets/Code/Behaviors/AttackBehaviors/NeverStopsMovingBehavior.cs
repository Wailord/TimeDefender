using UnityEngine;
using Assets.Code.Units;
using System.Collections;
using Assets.Code.Enums;
using Assets.Code.Behaviors;

public class NeverStopsMovingBehavior : IAttackBehavior
{
    public NeverStopsMovingBehavior(Faction faction, float delay, int damage, int range)
    {
        _attackDelay = delay;
        _attackDamage = damage;
        _attackRange = range;
        _currentAttackRange = range;
        _currentAttackDamage = damage;
        _currentAttackDelay = delay;
        _faction = faction;
    }

    private float _attackDelay;
    private Faction _faction;

    private int _currentAttackRange;
    private int _currentAttackDamage;
    private float _currentAttackDelay;

    /// <summary>
    /// Returns whether or not the unit is allowed to move. Because this unit StopsToAttack,
    /// if the unit is currently attacking, the unit will return false.
    /// </summary>
    public bool AllowedToMove
    { get { return true; } }

    public float AttackDelay
    {
        get { return _attackDelay; }
    }

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

    private int _attackDamage;
    public int AttackDamage
    {
        get { return _currentAttackDamage; }
    }

    private int _attackRange;
    public int AttackRange
    {
        get { return _currentAttackRange; }
    }

    public void Service()
    {
        //Debug.Log("Servicing attack behavior for " + _owner.Name);
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
