namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action 19: the active pet.</summary>
public static class Action19
{
    /// <summary>Action code 19.</summary>
    public const byte Code = 19;

    /// <summary>Client to server: sets the active pet; server to client: the pet appears and follows.</summary>
    public const byte JoinCombat = 1;

    /// <summary>Client to server: the active pet rests.</summary>
    public const byte Rest = 2;

    /// <summary>Server to client: the selected pet, set silently at login.</summary>
    public const byte Selected = 4;

    /// <summary>Server to client: a player's pet leaves the scene.</summary>
    public const byte PetHidden = 7;
}
