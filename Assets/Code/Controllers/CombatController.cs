using UnityEngine;
using Assets.Code.Behaviors;
using Assets.Code.Navigation;
using System.Collections.Generic;
using Assets.Code.Projectiles;
using System;
using Assets.Code.Interfaces;
using Assets.Code.Units;
using Assets.Code.Enums;
using Assets.Code.MapGeneration;
using Assets.Code.Scores;
using Assets.Code.GameTypes;
using Assets.Code.GUICode;
using Assets.Code.Sound;

namespace Assets.Code.Controllers
{
    /// <summary>
    /// The possible states for the CombatController.
    /// </summary>
    public enum CombatState
    {
        OutOfCombat, PreCombat, InCombat, PostCombat, PausedCombat
    }

    public class CombatController : IServiceable
    {
        /// <summary>
        /// The current state that combat is in. Can be
        /// OutOfCombat, PreCombat, InCombat, or PostCombat.
        /// </summary>
        public CombatState CurrentCombatState
        { get { return _combatState; } set { _combatState = value; } }
        private CombatState _combatState;

        /// <summary>
        /// Keeps a running count of how many instances of each unit type have been spawned.
        /// </summary>
        private Dictionary<UnitTypes, int> _spawnedUnits;

        /// <summary>
        /// Keeps a running count of how many instances of each unit type have been killed.
        /// </summary>
        private Dictionary<UnitTypes, int> _slainUnits;

        /// <summary>
        /// A collection of all projectiles currently in flight.
        /// </summary>
        private List<Projectile> _activeProjectiles;

        /// <summary>
        /// A collection of all projectiles that have been created, but aren't yet managed or serviced
        /// by the combat controller.
        /// </summary>
        private List<Projectile> _pendingProjectiles;

        /// <summary>
        /// A collection of all projectiles that have been destroyed.
        /// </summary>
        private List<Projectile> _deadProjectiles;

        /// <summary>
        /// A collection of all units currently on the field.
        /// </summary>
        private List<Unit> _activeUnits;

        /// <summary>
        /// A collection of all units that have been created, but aren't yet managed or serviced
        /// by the combat controller.
        /// </summary>
        private List<Unit> _pendingUnits;

        /// <summary>
        /// A collection of all units that have been killed.
        /// </summary>
        private List<Unit> _deadUnits;

        /// <summary>
        /// A public reference to the CombatController singleton.
        /// </summary>
        public static CombatController Instance
        {
            get { return _instance ?? (_instance = new CombatController()); }
        }
        private static CombatController _instance;

        private CombatController()
        {
            _spawnedUnits = new Dictionary<UnitTypes, int>();
            _slainUnits = new Dictionary<UnitTypes, int>();

            foreach (UnitTypes type in Enum.GetValues(typeof(UnitTypes)))
            {
                _spawnedUnits[type] = 0;
                _slainUnits[type] = 0;
            }

            _activeProjectiles = new List<Projectile>();
            _pendingProjectiles = new List<Projectile>();
            _deadProjectiles = new List<Projectile>();

            _activeUnits = new List<Unit>();
            _pendingUnits = new List<Unit>();
            _deadUnits = new List<Unit>();

            _combatState = CombatState.PreCombat;

            // TODO: hook this up to a button
            StartCombat();
        }

        /// <summary>
        /// Services one tick of combat.
        /// </summary>
        public void Service()
        {
            if (_combatState == CombatState.InCombat)
            {
                Faction winner = CheckVictoryCondition();

                // check victory condition
                if (winner == Faction.NoFaction)
                {
                    ServiceProjectiles();
                    ServiceUnits();
                }
                else if (winner == Faction.TowerSide)
                {
                    Debug.Log("############################################# yay, tower side won");
                    _combatState = CombatState.PostCombat;
                }
                else
                {
                    Debug.Log("############################################# boo, lemming side won");
                    _combatState = CombatState.PostCombat;
                }
            }
            else if (_combatState == CombatState.OutOfCombat)
            {
               ServiceProjectiles();
               ServiceOnlyTowers();
            }
        }

        /// <summary>
        /// Services all projectiles during gameplay and in between waves
        /// </summary>
        private void ServiceProjectiles()
        {
            // add pending projectiles
            foreach (Projectile p in _pendingProjectiles)
            {
                _activeProjectiles.Add(p);
            }

            // clear out the pending projectile list
            _pendingProjectiles.Clear();

            // service all projectiles
            foreach (Projectile p in _activeProjectiles)
            {
                p.Service();
                if (p.Destroyed) _deadProjectiles.Add(p);
            }

            // remove any useless projectiles
            foreach (Projectile p in _deadProjectiles)
                _activeProjectiles.Remove(p);
        }

        /// <summary>
        /// Services all units during non-paused gameplay
        /// </summary>
        private void ServiceUnits()
        {
            // add pending units
            foreach (Unit u in _pendingUnits)
                _activeUnits.Add(u);

            _pendingUnits.Clear();

            // service all units
            foreach (Unit u in _activeUnits)
                u.Service();
        }

        /// <summary>
        /// Services the towers in between waves, allows for the players to place towers on
        /// the map during this time
        /// </summary>
        private void ServiceOnlyTowers()
        {
            //Temporary container to hold the towers that will be removed from the pending units
            List<Unit> _temp = new List<Unit>();

            foreach (Unit u in _pendingUnits)
            {
                //Gets all the tower units from pending units and adds them to active units and the temp container
                if (u.UnitFaction == Faction.TowerSide)
                {
                    _activeUnits.Add(u);
                    _temp.Add(u);
                }
            }

            //Removes all the towers from pending units
            foreach (Unit u in _temp)
                _pendingUnits.Remove(u);

            //Services the towers
            foreach (Unit u in _activeUnits)
                u.Service();
        }

        /// <summary>
        /// Spawns a new projectile and adds it to the list of pending projectiles in the
        /// CombatController.
        /// </summary>
        /// <param name="attacker">The unit that will spawn the projectile</param>
        /// <param name="target">The unit that the projectile will travel towards</param>
        public void LaunchAttackAtUnit(Unit attacker, Unit target)
        {
            // TODO where do we want to store projectile speed?
            Projectile newProjectile = new Projectile(attacker, target, 30);

            _pendingProjectiles.Add(newProjectile);
            //Debug.Log(attacker + " launched an attack at " + target + "!");

			SoundPlayer.getInstance ().Play (sounds.light_projectile);
        }

        /// <summary>
        /// Registers a hit by the provided projectile. If the unit that the projectile hit
        /// is killed by the projectile, that is handled in this function.
        /// </summary>
        /// <param name="p">The projectile that struck its target</param>
        public void RegisterProjectileHit(Projectile p)
        {
            if (!p.Target.TargetBehavior.IsDead)
            {
                // Debug.Log(p.Target + " was hit by an attack for " + p.Damage + " damage!");
                p.Target.TargetBehavior.TakeDamage(p.Damage);
                if (p.Target.TargetBehavior.IsDead)
                {
                    // TODO: this block fires when a unit is killed by a projectile, maybe do particle stuff
                    p.Target.KillUnit();
                    p.Target.MovementBehavior.CurrentLocation.Occupants.Remove(p.Target);
                    _slainUnits[p.Target.UnitType] = 5;
                    _activeUnits.Remove(p.Target);

					SoundPlayer.getInstance ().Play (sounds.death);
                }
            }

            p.MarkForDestruction();
            _deadProjectiles.Add(p);
        }

        /// <summary>
        /// Resets the CombatController to a fresh state.
        /// </summary>
        public void ResetCombat()
        {
            _activeProjectiles.Clear();
            _activeUnits.Clear();
            _pendingProjectiles.Clear();
            _pendingUnits.Clear();
            _deadProjectiles.Clear();
            _deadUnits.Clear();

            foreach (UnitTypes type in Enum.GetValues(typeof(UnitTypes)))
            {
                _spawnedUnits[type] = 0;
                _slainUnits[type] = 0;
            }
        }

        /// <summary>
        /// Spawns a unit with the given parameters and adds it to battle.
        /// </summary>
        /// <param name="type">The type of unit to spawn</param>
        /// <param name="side">The Faction that the unit should 'fight for'</param>
        /// <param name="spawnPos">The GridPoint at which the created unit should spawn</param>
        public void SpawnCombatantOfType(UnitTypes type, Faction side, GridPoint spawnPos = null)
        {
            IMovementBehavior movement = null;
            IAttackBehavior attack = null;
            ITargetBehavior target = null;
            Unit testUnit = null;

            switch (type)
            {
                case UnitTypes.Caveman:
                    // movement
                    movement = MovementBehaviorFactory.SpawnBehavior(UnitMoverType.Walker, 1, spawnPos);
                    // attack
                    attack = AttackBehaviorFactory.SpawnBehavior(UnitAttackType.NeverAttacks, side);
                    // target
                    target = TargetBehaviorFactory.SpawnBehavior(UnitArmorType.Armored);
                    break;
                case UnitTypes.Knight:
                    // movement
                    movement = MovementBehaviorFactory.SpawnBehavior(UnitMoverType.Walker, 1, spawnPos);
                    // attack
                    attack = AttackBehaviorFactory.SpawnBehavior(UnitAttackType.NeverAttacks, side);
                    // target
                    target = TargetBehaviorFactory.SpawnBehavior(UnitArmorType.Armored);
                    break;
                case UnitTypes.Soldier:
                    // movement
                    movement = MovementBehaviorFactory.SpawnBehavior(UnitMoverType.Walker, 1, spawnPos);
                    // attack
                    attack = AttackBehaviorFactory.SpawnBehavior(UnitAttackType.NeverAttacks, side);
                    // target
                    target = TargetBehaviorFactory.SpawnBehavior(UnitArmorType.Armored);
                    break;
                case UnitTypes.Swat:
                    // movement
                    movement = MovementBehaviorFactory.SpawnBehavior(UnitMoverType.Walker, 1, spawnPos);
                    // attack
                    attack = AttackBehaviorFactory.SpawnBehavior(UnitAttackType.NeverAttacks, side);
                    // target
                    target = TargetBehaviorFactory.SpawnBehavior(UnitArmorType.Armored);
                    break;
                case UnitTypes.Tric:
                    // movement
                    movement = MovementBehaviorFactory.SpawnBehavior(UnitMoverType.Walker, 1, spawnPos);
                    // attack
                    attack = AttackBehaviorFactory.SpawnBehavior(UnitAttackType.NeverAttacks, side);
                    // target
                    target = TargetBehaviorFactory.SpawnBehavior(UnitArmorType.Armored);
                    break;
                case UnitTypes.Boss:
                    // movement
                    movement = MovementBehaviorFactory.SpawnBehavior(UnitMoverType.Walker, 1, spawnPos);
                    // attack
                    attack = AttackBehaviorFactory.SpawnBehavior(UnitAttackType.NeverAttacks, side);
                    // target
                    target = TargetBehaviorFactory.SpawnBehavior(UnitArmorType.Armored);
                    break;
                case UnitTypes.FlyingKappa:
                    // movement
                    movement = MovementBehaviorFactory.SpawnBehavior(UnitMoverType.Flyer, 1, spawnPos);
                    // attack
                    attack = AttackBehaviorFactory.SpawnBehavior(UnitAttackType.NeverAttacks, side);
                    // target
                    target = TargetBehaviorFactory.SpawnBehavior(UnitArmorType.Armored);
                    break;
                case UnitTypes.WalkingKappa:
                    // movement
                    movement = MovementBehaviorFactory.SpawnBehavior(UnitMoverType.Walker, 2, spawnPos);
                    // attack
                    attack = AttackBehaviorFactory.SpawnBehavior(UnitAttackType.NeverAttacks, side);
                    // target
                    target = TargetBehaviorFactory.SpawnBehavior(UnitArmorType.Armored);
                    break;
                case UnitTypes.DummyTower:
                    // movement
                    movement = MovementBehaviorFactory.SpawnBehavior(UnitMoverType.NoMovement, 5, spawnPos);
                    // attack
                    attack = AttackBehaviorFactory.SpawnBehavior(UnitAttackType.StopsToAttack, side, .5f, 10, 10);
                    // target
                    target = TargetBehaviorFactory.SpawnBehavior(UnitArmorType.Armored);
                    break;
                default:
                    Debug.LogError("Did you forget to add a unit type to SpawnCombatant?");
                    break;
            }

            if (movement == null || attack == null || target == null) Debug.LogError("Unit didn't instantiate correctly :(");

            _spawnedUnits[type]++;

            if (spawnPos == null) spawnPos = NavigationController.Instance.FirstTile;

            // TODO: this is here until units can decide where to go themselves
            movement.PathTo(NavigationController.Instance.LastTile);

            testUnit = new Unit(String.Format(type.ToString() + "-" + _spawnedUnits[type]), type, side, movement, attack, target);

            spawnPos.AddToTile(testUnit);
            AddCombatantToActiveBattle(testUnit);

			if (type == UnitTypes.DummyTower)
				SoundPlayer.getInstance ().Play (sounds.tower);
			else
				SoundPlayer.getInstance().Play (sounds.spawn);
        }

        /// <summary>
        /// Adds the provided to the CombatController pending unit list.
        /// </summary>
        /// <param name="u">The unit to be added to combat</param>
        private void AddCombatantToActiveBattle(Unit u)
        {
            _pendingUnits.Add(u);
        }

        /// <summary>
        /// Checks to see if the combat sequence should end.
        /// </summary>
        /// <returns>The winning faction (if applicable), or NoFaction if no side has won</returns>
        private Faction CheckVictoryCondition()
        {
            Faction winner = Faction.NoFaction;

            if (GameScore.GetInstance().WinCheck())
            {
                winner = Faction.TowerSide;
                GameClock.GetInstance().EndClock();
            }
            else if (GameScore.GetInstance().LossCheck())
            {
                winner = Faction.LemmingSide;
                GameClock.GetInstance().EndClock();
            }

            return winner;
        }

        /// <summary>
        /// Starts a combat sequence in the CombatController.
        /// </summary>
        public void StartCombat()
        {
            _combatState = CombatState.InCombat;

            GridPoint[,] map = NavigationController.Instance.NavigationGrid;
        }

        internal Unit GetTowerAt(int PosX, int PosY)
        {
            GridPoint tile = NavigationController.Instance.GetGridPoint(PosX, PosY);
            Unit tower = tile.Occupants.Find(o => o.UnitType == UnitTypes.DummyTower);

            return tower;
        }
    }
}