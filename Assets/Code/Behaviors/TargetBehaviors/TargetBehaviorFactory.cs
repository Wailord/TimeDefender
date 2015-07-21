using UnityEngine;
using System.Collections;

namespace Assets.Code.Behaviors
{
    public static class TargetBehaviorFactory
    {
        public static ITargetBehavior SpawnBehavior(UnitArmorType armorType)
        {
            ITargetBehavior targetBehavior;
            switch (armorType)
            {
                case UnitArmorType.Squishy:
                case UnitArmorType.Armored:
                case UnitArmorType.NoArmor:
                default:
                    targetBehavior = new GenericTargetBehavior(100, UnitArmorType.NoArmor);
                    break;
            }

            return targetBehavior;
        }
    }
}