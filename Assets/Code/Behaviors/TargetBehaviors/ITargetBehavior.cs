using System.Collections;
using Assets.Code.Units;
using Assets.Code.Interfaces;

namespace Assets.Code.Behaviors
{
    public interface ITargetBehavior : IServiceable
    {
        /// <summary>
        /// The amount of hit points the unit currently has.
        /// </summary>
        int CurrentHitPoints { get; }

        /// <summary>
        /// The maximum amount of hit points a unit can have.
        /// </summary>
        int MaximumHitPoints { get; }

        /// <summary>
        /// The armor type of the unit.
        /// </summary>
        UnitArmorType ArmorType { get; }

        /// <summary>
        /// Used to take an appropriate amount of damage away from a unit upon being struck.
        /// </summary>
        /// <param name="damage">The amount of hit points to remove.</param>
        void TakeDamage(int damage);

        /// <summary>
        /// Returns whether or not the given unit is dead.
        /// </summary>
        bool IsDead { get; }

        Unit Owner { get; set; }
    }

    /// <summary>
    /// Represents the different armor types in the game.
    /// </summary>
    public enum UnitArmorType
    { Squishy, Armored, NoArmor }
}