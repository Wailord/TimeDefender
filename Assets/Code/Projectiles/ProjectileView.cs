using System.Collections;
using Assets.Code.GUICode;
using Assets.Code.Enums;
using Assets.Code.Interfaces;
using Assets.Code.Units;
using UnityEngine;

namespace Assets.Code.Projectiles
{
    public class ProjectileView : IServiceable
    {
        /// <summary>
        /// The projectile associated with this UnitView.
        /// </summary>
        public Projectile Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }
        private Projectile _owner;

        /// <summary>
        /// The sprite that is drawn to the screen to represent the projectile.
        /// </summary>
        private SpriteRenderer _sprite;

        /// <summary>
        /// The GameObject that is instantiated. The sprite is a child of this GameObject.
        /// </summary>
        private ProjectileController _projectile;

        public ProjectileView(Projectile owner, Vector2 pos)
        {
            _owner = owner;

            _projectile = GuiAPI.CreateProjectile(pos.x, pos.y, ProjectileTypes.DummyProjectile);
        }

        /// <summary>
        /// Called to service the visuals of a unit on each game tick.
        /// </summary>
        public void Service()
        {
            // TODO: all of this
            // Debug.Log("Servicing a unit view");
            _projectile.SetLocation(_currentPosition);
        }

        /// <summary>
        /// Vector2 representing the unit's position in world coordinates.
        /// </summary>
        public Vector2 UnitPosition
        { get { return _currentPosition; } }
        private Vector2 _currentPosition;

        /// <summary>
        /// Sets the position of the unit on the screen.
        /// </summary>
        /// <param name="newPos">A vector representing the new position for the unit.</param>
        public void SetViewPosition(Vector2 newPos)
        {
            // Debug.Log("Setting UnitPosition to " + newPos);
            _currentPosition = newPos;
        }

        /// <summary>
        /// Destroys the GameObject representing the projectile.
        /// </summary>
        public void DestroyProjectile()
        {
            _projectile.Kill();
        }
    }
}