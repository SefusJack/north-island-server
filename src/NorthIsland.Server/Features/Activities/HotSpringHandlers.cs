using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NorthIsland.Server.Features.Activities;

/// <summary>Gives a player bonus HP and SP points for the time spent standing in a hot spring map's water.</summary>
/// <param name="world">The world state, whose leave event stops the bathing.</param>
/// <param name="combat">The combat stats, resent after a point is added.</param>
/// <param name="options">The activity options giving the hot spring settings.</param>
/// <param name="logger">Logger for the bathing messages.</param>
public sealed class HotSpringHandlers(
    WorldState world,
    CombatStats combat,
    IOptions<ActivityOptions> options,
    ILogger<HotSpringHandlers> logger)
{
    /// <summary>Subscribes to the world's leave event so a leaving player stops bathing.</summary>
    internal void Start()
    {
        world.Leaving += Stop;
    }

    /// <summary>Stops the sender's bathing when the client reports leaving the water.</summary>
    /// <param name="s">The player's session.</param>
    /// <param name="body">The packet body, not read.</param>
    internal static ValueTask OnLeave(GameSession s, ReadOnlyMemory<byte> body)
    {
        Stop(s);
        return ValueTask.CompletedTask;
    }

    /// <summary>Cancels the player's bathing loop, if any.</summary>
    /// <param name="session">The player's session.</param>
    public static void Stop(GameSession session)
    {
        if (session.Activities.Spring is not { } spring)
            return;
        session.Activities.Spring = null;
        spring.Cancel();
        spring.Dispose();
    }

    /// <summary>Starts the bathing loop when the player steps into the water of a hot spring map.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body, not read.</param>
    internal ValueTask OnEnter(GameSession session, ReadOnlyMemory<byte> body)
    {
        var o = options.Value.HotSpring;
        if (!o.Enabled
            || session.State != SessionState.InWorld
            || session.ActiveCharacter is not { } character
            || o.MinutesPerPoint <= 0
            || o.MaxPerVisit <= 0
            || !o.Maps.Contains(character.MapId))
            return ValueTask.CompletedTask;
        Stop(session);
        var cts = new CancellationTokenSource();
        session.Activities.Spring = cts;
        logger.LogInformation(
            "[{Session}] {Name} steps into the hot spring on map {Map} ({Bonus} points so far)",
            session.Id,
            character.Name,
            character.MapId,
            character.SpringBonus);
        _ = BatheAsync(session, character.MapId, cts.Token);
        return ValueTask.CompletedTask;
    }

    /// <summary>The bathing loop: after each interval adds the bonus points until the visit's maximum, a map change or cancellation.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="map">The map the player entered the water on.</param>
    /// <param name="token">Cancels the loop.</param>
    private async Task BatheAsync(GameSession session, ushort map, CancellationToken token)
    {
        var o = options.Value.HotSpring;
        try
        {
            for (var step = 1; step <= o.MaxPerVisit; step++)
            {
                await Task.Delay(TimeSpan.FromMinutes(o.MinutesPerPoint), token).ConfigureAwait(false);
                if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } character || character.MapId != map)
                    return;
                character.SpringBonus += o.PointsPerStep;
                combat.Send(session);
                logger.LogInformation(
                    "[{Session}] {Name}: hot spring point {Step}/{Max}, bonus now {Bonus}",
                    session.Id,
                    character.Name,
                    step,
                    o.MaxPerVisit,
                    character.SpringBonus);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
