using Assets.Code.Units;
using UnityEngine;
using Assets.Code.Enums;
using System.Collections;

namespace Assets.Code.Behaviors
{
    public static class AttackBehaviorFactory
    {
        // so this is the factory pattern. Notice every unit will have an "IAttackBehavior". Read up
        // on interfaces if you're unfamiliar with them. You pass this function/class a simple enum,
        // and it'll return an IAttackBehavior of a certain type, meaning it can react differently
        // to things. So, later, when we do something like unit.AttackBehavior.FindTarget(), we can do
        // that syntactically because all of the classes implement everything in the interface, but they
        // can each do different things. If something has an AttacksBuilding attack behavior, it may just scan
        // for buildings, while the same call on another unit will be a NeverAttacks behavior, which will contain
        // an empty function

        // TODO: implement the attack behaviors below
        public static IAttackBehavior SpawnBehavior(UnitAttackType attackType, Faction faction, float delay = 0, int damage = 0, int range = 0)
        {
            IAttackBehavior attackBehavior;
            switch (attackType)
            {
                case UnitAttackType.StopsToAttack:
                    attackBehavior = new StopsToAttackBehavior(faction, delay, damage, range);
                    break;
                case UnitAttackType.NeverStopsMoving:
                    attackBehavior = new NeverStopsMovingBehavior(faction, delay, damage, range);
                    break;
                case UnitAttackType.NeverAttacks:
                    attackBehavior = new NeverAttacksBehavior();
                    break;
                default:
                    attackBehavior = new NeverAttacksBehavior();
                    break;
            }

            return attackBehavior;
        }
    }
}