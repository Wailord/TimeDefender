using Assets.Code.Interfaces;
using UnityEngine;
using Assets.Code.Behaviors;
using Assets.Code.Enums;
using System.Collections;
using Assets.Code.Scores;
using Assets.Code.Player;
using Assets.Code.Controllers;

namespace Assets.Code.Units
{
    /// <summary>
    /// The different types of units in the game. 
    /// 
    /// ed units will grab their sprite
    /// from the Resources folder using the name provided here.
    /// </summary>
    public class Unit : IServiceable
    {
        /// <summary>
        /// The name of the given unit.
        /// </summary>
        public string Name
        { get { return _name; } }
        private string _name;

        public int Experience
        { get { return _experience; } }
        private int _experience;

        public int Resources
        { get { return _resources; } }
        private int _resources;

        /// <summary>
        /// The 'side' the unit is fighting for.
        /// </summary>
        public Faction UnitFaction
        { get { return _side; } }
        Faction _side;

        public int TilesUntilEnd
        {
            get
            {
                return MovementBehavior.TilesUntilEnd;
            }
        }

        /// <summary>
        /// The unit's UnitType, based on the UnitTypes enumeration.
        /// </summary>
        public UnitTypes UnitType
        {
            get { return _unitType; }
        }
        private UnitTypes _unitType;

        /// <summary>
        /// The current location of the unit in world coordinates,
        /// which refers to where the unit appears, not the square
        /// the unit is 'logically'.
        /// </summary>
        public Vector2 CurrentLocation
        { get { return _unitView.UnitPosition; } }

        // HACK this is so weird to have considering the above, someone please fix :(
        public Vector2 UnitWorldPosition
        { get { return _unitView.WorldPosition; } }

        /// <summary>
        /// The movement behavior of the unit, which handles
        /// things involving the traversing of the unit
        /// across the field, as well as data members
        /// involving movement, such as unit speed.
        /// </summary>
        public IMovementBehavior MovementBehavior
        { get { return _movementBehavior; } }
        IMovementBehavior _movementBehavior;

        /// <summary>
        /// The attack behavior of the unit, which handles
        /// things involving the attacking of the unit,
        /// as well as data members involving attacking,
        /// such as attack speed and damage.
        /// </summary>
        public IAttackBehavior AttackBehavior
        { get { return _attackBehavior; } }
        IAttackBehavior _attackBehavior;

        /// <summary>
        /// The target behavior of the unit, which handles
        /// things involving the targetability of the unit,
        /// as well as data members involving being targeted,
        /// such as hit points, shield points, or armor type.
        /// </summary>
        public ITargetBehavior TargetBehavior
        { get { return _targetBehavior; } }
        ITargetBehavior _targetBehavior;
        
        /// <summary>
        /// The actual view of the unit, which refers to 
        /// how and where the unit is drawn to the screen.
        /// The view is not used when calculating anything
        /// involving combat or combat logic; it simply is
        /// used to know how to draw the unit.
        /// </summary>
        public UnitView UnitView
        { get { return _unitView; } }
        UnitView _unitView;

        public Unit(string name, UnitTypes unitType, Faction side, IMovementBehavior movementBehavior, IAttackBehavior attackBehavior, ITargetBehavior targetBehavior)
        {
            _name = name;
            _experience = 50;
            _resources = 50;

            _side = side;

            _unitType = unitType;
            
            _movementBehavior = movementBehavior;
            _movementBehavior.Owner = this;

            _attackBehavior = attackBehavior;
            _attackBehavior.Owner = this;

            _targetBehavior = targetBehavior;
            _targetBehavior.Owner = this;

            _unitView = new UnitView(this, unitType);
        }

        /// <summary>
        /// Services one update cycle for the unit.
        /// </summary>
        public void Service()
        {
          //  Debug.Log("Servicing a unit ('" + _name + "')");
            if (!TargetBehavior.IsDead)
            {
                _movementBehavior.Service();                  
                _attackBehavior.Service();
                _targetBehavior.Service();
                _unitView.Service();
            }
        }

        /// <summary>
        /// Overrides the ToString() function so a Unit's string representation
        /// will be its name.
        /// </summary>
        /// <returns>The unit type's name</returns>
        public override string ToString()
        {
            return _name;
        }
        
        /// <summary>
        /// Kills the unit and marks it as dead, fading its sprite.
        /// </summary>
        public void KillUnit()
        {
            GameScore.GetInstance().AddResources(_resources);
            GameScore.GetInstance().IncrementEnemiesKilled();
            GamePlayer.GetInstance().AddExp(_experience);
            WaveController.GetInstance().RemoveEnemy();
            _unitView.UnitDied();
        }

        public void UpgradeAttackPower(int additive)
        {
            _attackBehavior.UpgradeAttackPower(additive);
        }

        public void UpgradeAttackPower(float multiplicative)
        {
            _attackBehavior.UpgradeAttackPower(multiplicative);
        }

        public void UpgradeAttackCooldown(int additive)
        {
            _attackBehavior.UpgradeAttackCooldown(additive);
        }
        
        public void UpgradeAttackCooldown(float multiplicative)
        {
            _attackBehavior.UpgradeAttackCooldown(multiplicative);
        }
    }
}