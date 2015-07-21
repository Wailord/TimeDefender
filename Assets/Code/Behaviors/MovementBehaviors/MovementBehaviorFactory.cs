using Assets.Code.Controllers;
using UnityEngine;
using Assets.Code.Navigation;
using System.Collections;

namespace Assets.Code.Behaviors
{
    public static class MovementBehaviorFactory
    {
        public static IMovementBehavior SpawnBehavior(UnitMoverType moverType, int speed, GridPoint spawnPos = null)
        {
            IMovementBehavior movementBehavior;
            switch (moverType)
            {
                case UnitMoverType.Flyer:
                    movementBehavior = new GenericMovementBehavior(speed, UnitMoverType.Flyer, spawnPos);
                    break;
                case UnitMoverType.Walker:
                    movementBehavior = new GenericMovementBehavior(speed, UnitMoverType.Walker, spawnPos);
                    break;
                case UnitMoverType.NoMovement:
                    movementBehavior = new NoMovementBehavior(spawnPos);
                    break;
                default:
                    movementBehavior = new GenericMovementBehavior(speed, UnitMoverType.Walker, spawnPos);
                    break;
            }

            return movementBehavior;
        }
    }
}