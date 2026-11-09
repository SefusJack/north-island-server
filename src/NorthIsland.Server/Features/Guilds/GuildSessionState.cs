namespace NorthIsland.Server.Features.Guilds;

/// <summary>Guild offers a session has received and not answered.</summary>
public sealed class GuildSessionState
{
    /// <summary>Login id of the player who invited them to a guild, 0 for none.</summary>
    public int InvitedBy { get; set; }

    /// <summary>The post (lord or vice) a guild lord offered them, with the offerer's id, or null when there is none.</summary>
    public (int FromId, bool Lord)? OfferedPost { get; set; }
}
