using System.Collections;
using Assets.Code.GUICode;
using Assets.Code.Enums;
using Assets.Code.Interfaces;
using UnityEngine;

namespace Assets.Code.Units
{
    public class UnitView : IServiceable
    {
        /// <summary>
        /// The unit associated with this UnitView.
        /// </summary>
        public Unit Owner
        {
            get { return _owner; }
            set { _owner = value; } 
        }
        private Unit _owner;

        /// <summary>
        /// The GameObject that is instantiated. The sprite is a child of this GameObject.
        /// </summary>
        private UnitController _unit;

        public UnitView(Unit owner, UnitTypes unitType)
        {
            _owner = owner;

            _unit = GuiAPI.CreateUnit(UnitPosition.x, UnitPosition.y, unitType);
        }

        /// <summary>
        /// Called to service the visuals of a unit on each game tick.
        /// </summary>
        public void Service()
        {
            if(!_owner.TargetBehavior.IsDead)
                _unit.SetLocation(_currentPosition.x,_currentPosition.y);
        }

        /// <summary>
        /// Vector2 representing the unit's position in world coordinates.
        /// </summary>
        public Vector2 UnitPosition
        { get { return _currentPosition; } }
        private Vector2 _currentPosition;

        public Vector2 WorldPosition
        {
            get
            {
                return _unit.Interface.Unit.transform.localPosition;
            }
        }

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
        /// Fades the unit's sprite.
        /// </summary>
        public void UnitDied()
        {
            _unit.Kill();
        }
    }
}