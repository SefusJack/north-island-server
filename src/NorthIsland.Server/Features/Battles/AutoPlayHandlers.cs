using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Battles;

/// <summary>Server side of the Remote Control (auto play) item: it only records the on/off state and the leave request.</summary>
/// <param name="logger">Logs the state changes.</param>
public sealed class AutoPlayHandlers(
    ILogger<AutoPlayHandlers> logger)
{
    /// <summary>Handles <c>5/8</c>: the Remote Control asks to leave the game; auto play is switched off and the connection is closed.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code (not read).</param>
    internal ValueTask OnLeave(GameSession session, ReadOnlyMemory<byte> body)
    {
        var result = Set(session, false, "stopped (system form)");
        if (session.State == SessionState.InWorld && session.ActiveCharacter is { } c)
        {
            logger.LogInformation("[{Session}] {Name}: leaves the game (the Remote Control's leave rule)", session.Id, c.Name);
            session.Close();
        }
        return result;
    }

    /// <summary>Sets the session's auto play flag when <paramref name="on"/> has a value and logs the change.</summary>
    /// <param name="session">The player's session; nothing happens unless they are in the world.</param>
    /// <param name="on">The new auto play state, or null to leave it unchanged.</param>
    /// <param name="what">Text for the log line that says what happened.</param>
    internal ValueTask Set(GameSession session, bool? on, string what)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c)
            return ValueTask.CompletedTask;
        if (on is { } state)
            session.AutoPlay = state;
        logger.LogInformation("[{Session}] {Name}: auto play {What}", session.Id, c.Name, what);
        return ValueTask.CompletedTask;
    }
}
