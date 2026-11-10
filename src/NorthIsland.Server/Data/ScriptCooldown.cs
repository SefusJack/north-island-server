namespace NorthIsland.Server.Data;

/// <summary>A cooldown a game script started for a character, such as the wait before a gathering spot gives more.</summary>
public sealed class ScriptCooldown
{
    /// <summary>The id the script gave the cooldown.</summary>
    public ushort Id { get; set; }

    /// <summary>When the cooldown ends (UTC).</summary>
    public DateTime UntilUtc { get; set; }
}
