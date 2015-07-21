using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Enums
{
    /// <summary>
    /// The different types of tiles that can be placed on the map
    /// </summary>
    public enum TileType
    {
        EndPoint,
        StartPoint,
        HorizPath,
        VertPath,
        ElbowLeftUp,
        ElbowRightUp,
        ElbowLeftDown,
        ElbowRightDown,
        SplitUDL,
        SplitUDR,
        SplitULR,
        SplitDLR,
        Split4Ways,
        EmptySpace,
        CurPosition,
        Decor,
        Decor2,
        Tower
    }

    public enum UnitTypes
    {
        FlyingKappa, WalkingKappa, DummyTower, Knight, Caveman, Swat, Soldier, Tric, Boss
    };

    public enum ProjectileTypes
    {
        DummyProjectile
    };

    /// <summary>
    /// The different 'teams' or 'sides' in a combat sequence.
    /// </summary>
    public enum Faction
    {
        TowerSide, LemmingSide,
        NoFaction
    };

    public enum PauseTypes
    {
        WavePause, GamePause
    };

    public enum MapDifficulty
    {
        Easy = 1, Medium, Hard
    }
}
