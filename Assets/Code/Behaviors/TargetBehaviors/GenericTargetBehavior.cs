using Assets.Code.Units;
using UnityEngine;
using Assets.Code.Behaviors;
using System.Collections;

public class GenericTargetBehavior : ITargetBehavior
{
    public GenericTargetBehavior(int maxHP, UnitArmorType armorType)
    {
        _currentHitPoints = maxHP;
        _maxHitPoints = maxHP;
        _armorType = armorType;
    }

    /// <summary>
    /// The unit associated with the target behavior.
    /// </summary>
    public Unit Owner
    {
        get { return _owner; }
        set { _owner = value; }
    }
    private Unit _owner;

    /// <summary>
    /// Whether or not the unit has run out of health.
    /// </summary>
    public bool IsDead
    { get { return _currentHitPoints <= 0; } }

    /// <summary>
    /// How many hit points remain for the unit.
    /// </summary>
    public int CurrentHitPoints
    { get { return _currentHitPoints; } }
    private int _currentHitPoints;

    /// <summary>
    /// How many hit points the unit spawns with.
    /// </summary>
    public int MaximumHitPoints
    { get { return _maxHitPoints; } }
    private int _maxHitPoints;
   
    // TODO: integrate armor types into damage calculations
    /// <summary>
    /// The armor type of the given unit, based on the UnitArmorType enumeration.
    /// </summary>
    public UnitArmorType ArmorType
    { get { return _armorType; } }
    private UnitArmorType _armorType;

    // TODO: this should probably take in a projectile so it can check attack types
    // to compare to armor types
    /// <summary>
    /// Subtracts the provided amount of damage from the unit's hit points.
    /// </summary>
    /// <param name="damage">The amount of damage to subtract</param>
    public void TakeDamage(int damage)
    {
        _currentHitPoints -= damage;
        //Debug.Log(_owner.Name + " hit for " + damage + ", " + _currentHitPoints + " remaining!");
    }

    /// <summary>
    /// Services one update cycle for the target behavior.
    /// </summary>
    public void Service()
    {
        // TODO: if anything needs serviced (like a shield cooldown or something), do it here
    }
}
