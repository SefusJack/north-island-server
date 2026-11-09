namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action 11: battles.</summary>
public static class Action11
{
    /// <summary>Action code 11.</summary>
    public const byte Code = 11;

    /// <summary>Client to server: starts an event battle.</summary>
    public const byte StartEventBattle = 1;

    /// <summary>Client to server: asks to start or watch a battle.</summary>
    public const byte BattleRequest = 2;

    /// <summary>Client to server: asks to join a battle; the server answers with the same sub code and a reason byte.</summary>
    public const byte JoinBattle = 3;

    /// <summary>Server to client: the fighter in a cell leaves the battle.</summary>
    public const byte FighterLeave = 1;

    /// <summary>Server to client: fighters enter a running battle.</summary>
    public const byte Join = 5;

    /// <summary>Server to client: sets the battle type.</summary>
    public const byte BattleType = 10;

    /// <summary>Server to client: the weapon look of fighters.</summary>
    public const byte EquipLook = 251;

    /// <summary>Server to client: the battle starts with its background and fighters.</summary>
    public const byte FightStart = 250;

    /// <summary>Server to client: a player leaves the battle.</summary>
    public const byte PlayerLeft = 0;

    /// <summary>Server to client: tells the other players on the map the fight state of a player.</summary>
    public const byte FightStates = 4;

    /// <summary>Battle type value the server gives to player-versus-player battles.</summary>
    public const byte PkBattleType = 3;

    /// <summary>Fighter state value the server gives to a spectator's own record.</summary>
    public const byte WatchState = 4;
}
