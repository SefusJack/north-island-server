namespace NorthIsland.Server.Features.Mall;

/// <summary>Per-session state of the mall.</summary>
public sealed class MallSessionState
{
    /// <summary>The rows sent to the player on login, which later purchases are matched against; null before they are sent.</summary>
    public IReadOnlyList<Protocol.Messages.Items.MallRow>? Rows { get; set; }
}
