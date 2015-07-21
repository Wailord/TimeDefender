using UnityEngine;
using Assets.Code.Units;
using System.Collections;
using Assets.Code.Behaviors;

namespace Assets.Code.Behaviors
{
    public class NeverAttacksBehavior : IAttackBehavior
    {
        public NeverAttacksBehavior()
        {
        }

        public float AttackDelay
        {
            get { return 0; }
        }

        public bool AllowedToMove
        { get { return true; } }

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

        public int AttackDamage
        {
            get { return 0; }
        }

        public int AttackRange
        {
            get { return 0; }
        }

        public void Service()
        {
            // NoOp
        }

        public void UpgradeAttackPower(int additive)
        {
            // NoOp
        }

        public void UpgradeAttackPower(float multiplicative)
        {
            // NoOp
        }

        public void UpgradeAttackCooldown(int additive)
        {
            // NoOp
        }

        public void UpgradeAttackCooldown(float multiplicative)
        {
            // NoOp
        }

        /// <summary>
        /// Reverts the unit's attack damage to its base damage.
        /// </summary>
        public void RevertAttackPower()
        {
            // NoOp
        }

        /// <summary>
        /// Reverts the unit's attack delay to its base attack delay.
        /// </summary>
        public void RevertAttackCooldown()
        {
            // NoOp
        }
    }
}
