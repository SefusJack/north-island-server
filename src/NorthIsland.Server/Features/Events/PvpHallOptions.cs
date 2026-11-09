namespace NorthIsland.Server.Features.Events;

/// <summary>Settings of the PvP hall service.</summary>
public sealed class PvpHallOptions
{
    /// <summary>Whether the service sends the player to the PvP server.</summary>
    public bool Enabled { get; set; }
}
