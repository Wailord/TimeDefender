using System.Collections;
using Assets.Code.Units;
using Assets.Code.Interfaces;

namespace Assets.Code.Behaviors
{
    public interface IAttackBehavior : IServiceable
    {
        /// <summary>
        /// The delay, in milliseconds, between each projectile from this attacker.
        /// </summary>
        float AttackDelay { get; }

        /// <summary>
        /// The damage done by this attacker with each projectile.
        /// </summary>
        int AttackDamage { get; }

        /// <summary>
        /// The range of this attacker in world units.
        /// </summary>
        int AttackRange { get; }

        /// <summary>
        /// The unit associated with this attack behavior.
        /// </summary>
        Unit Owner { get; set; }

        /// <summary>
        /// The current unit under attack by this unit.
        /// </summary>
        Unit Target { get; set; }

        /// <summary>
        /// Indicates whether or not the unit is currently allowed to move.
        /// </summary>
        bool AllowedToMove { get; }

        // TODO: projectile prefab stuff...

        /// <summary>
        /// Adjusts the unit's attack damage to the current damage plus the provided integer.
        /// </summary>
        /// <param name="additive">The number to add to the current attack damage.</param>
        void UpgradeAttackPower(int additive);

        /// <summary>
        /// Adjusts the unit's attack damage to the current damage multiplied by the provided
        /// multiplier.
        /// </summary>
        /// <param name="multiplicative">The number to multiply the current attack damage by.</param>
        void UpgradeAttackPower(float multiplicative);

        /// <summary>
        /// Adjusts the units' attack delay to the current attack delay plus the provided integer.
        /// </summary>
        /// <param name="additive">The number to add to the current attack delay.</param>
        void UpgradeAttackCooldown(int additive);

        /// <summary>
        /// Adjusts the unit's attack delay to the current attack delay multiplied by the provided
        /// multiplier.
        /// </summary>
        /// <param name="multiplicative"></param>
        void UpgradeAttackCooldown(float multiplicative);

        /// <summary>
        /// Reverts the unit's attack damage to its base damage.
        /// </summary>
        void RevertAttackPower();

        /// <summary>
        /// Reverts the unit's attack delay to its base attack delay.
        /// </summary>
        void RevertAttackCooldown();
    }

    public enum UnitAttackType { StopsToAttack, NeverStopsMoving, NeverAttacks }
}