using UnityEngine;
using Assets.Code.Controllers;
using Assets.Code.Units;
using System.Collections;
using Assets.Code.Interfaces;

namespace Assets.Code.Projectiles
{
    public class Projectile : IServiceable
    {
        /// <summary>
        /// The unit that this projectile is intended to hit/currently
        /// moving towards.
        /// </summary>
        public Unit Target
        { get { return _target; } }
        private Unit _target;

        /// <summary>
        /// Whether or not the projectile has been destroyed, meaning it either
        /// reached its target or its target destination (if the target died
        /// mid-flight).
        /// </summary>
        public bool Destroyed
        { get { return _destroy; } }
        private bool _destroy;
        
        /// <summary>
        /// Where the projectile is traveling. If its target dies mid-flight,
        /// this will be set to the last location of the target (pre-death).
        /// </summary>
        public Vector2 TargetPosition
        { get { return _targetPosition; } }
        private Vector2 _targetPosition;

        /// <summary>
        /// The unit that this project originated from.
        /// </summary>
        public Unit Attacker
        { get { return _attacker; } }
        private Unit _attacker;

        /// <summary>
        /// The current location of the projectile in world coordinates.
        /// </summary>
        private Vector2 _currentLocation;

        /// <summary>
        /// The handler for the drawing of the projectile.
        /// </summary>
        private ProjectileView _projectileView;

        /// <summary>
        /// The raw damage dealt by this projectile, ignoring armor
        /// or spell effects.
        /// </summary>
        public int Damage
        { get { return _damage; } }
        private int _damage;

        /// <summary>
        /// The movement speed of the projectile.
        /// </summary>
        public float Speed
        { get { return _speed; } }
        private float _speed;

        public Projectile(Unit attacker, Unit target, float speed)
        {
            // how do we want to get projectile names?
            _attacker = attacker;
            _target = target;
            _currentLocation = attacker.CurrentLocation;
            _damage = attacker.AttackBehavior.AttackDamage;
            _speed = speed;
            _targetPosition = _target.CurrentLocation;
            _destroy = false;

            //Debug.Log("attacker @ " + attacker.CurrentLocation + ", target @ " + target.CurrentLocation);

            _projectileView = new ProjectileView(this, _currentLocation);
        }

        /// <summary>
        /// Services one update cycle of the projectile.
        /// </summary>
        public void Service()
        {
            if (!_destroy)
            {
                Vector2 current = _currentLocation;
                Vector2 end;

                if (Target == null || Target.TargetBehavior.IsDead)
                    end = TargetPosition;
                else
                    end = Target.CurrentLocation;

                _targetPosition = end;

                float distanceTraveledThisTick = Speed * TimeController.deltaTime;

                float distanceToTarget = Vector2.Distance(current, end);

                float percentMovedThisTick = Mathf.Clamp(distanceTraveledThisTick / distanceToTarget, 0.0f, 1.0f);

                SetPosition(Vector2.Lerp(current, end, percentMovedThisTick));

                _projectileView.Service();

                if (percentMovedThisTick == 1.0f)
                {
                    if (Target != null)
                        CombatController.Instance.RegisterProjectileHit(this);
                }
            }
        }

        /// <summary>
        /// Moves the position on screen of the projectile.
        /// </summary>
        /// <param name="newPos">The position to move the projectile to.</param>
        public void SetPosition(Vector2 newPos)
        {
            _currentLocation = newPos;
            _projectileView.SetViewPosition(newPos);
        }

        /// <summary>
        /// Marks the projectile for destruction, meaning its view will no
        /// longer appear or update.
        /// </summary>
        public void MarkForDestruction()
        {
            _destroy = true;
            _projectileView.DestroyProjectile();
        }
    }
}