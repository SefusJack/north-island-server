using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.World;

/// <summary>Counts how long an account has been online and tells the client whether the required online time is reached.</summary>
public static class OnlineTime
{
    /// <summary>Adds the time since the session's last count to the account's online seconds and moves the count mark forward.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="now">The current time.</param>
    public static void Bank(GameSession session, DateTime now)
    {
        if (session.Account is not { } account || session.OnlineSinceUtc is not { } since)
            return;
        var seconds = (long)Math.Max(0, (now - since).TotalSeconds);
        account.OnlineSeconds += seconds;
        session.OnlineSinceUtc = since.AddSeconds(seconds);
    }

    /// <summary>The account's total online time: the saved seconds plus the current stretch.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The total; zero without an account.</returns>
    public static TimeSpan Total(GameSession session, DateTime now) =>
        session.Account is null
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds(session.Account.OnlineSeconds) + (session.OnlineSinceUtc is { } since ? now - since : TimeSpan.Zero);

    /// <summary>Whether the total online time reaches a number of hours; always true when the hours are 0 or less.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="hours">The required hours.</param>
    /// <param name="now">The current time.</param>
    /// <returns>True when enough time is online.</returns>
    public static bool Enough(GameSession session, double hours, DateTime now) => hours <= 0 || Total(session, now) >= TimeSpan.FromHours(hours);

    /// <summary>Tells the client whether the requirement is met and, when it is not, tells it again once the time is reached.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="world">The online world, to check the session is still the one online.</param>
    /// <param name="hours">The required hours.</param>
    public static void Announce(GameSession session, WorldState world, double hours)
    {
        var now = GameClock.UtcNow;
        var enough = Enough(session, hours, now);
        session.Send(TradeMessages.OnlineTime(enough));
        if (enough || session.Account is not { } account)
            return;
        _ = Task.Run(async () =>
        {
            while (TimeSpan.FromHours(hours) - Total(session, GameClock.UtcNow) is var left && left > TimeSpan.Zero)
            {
                await Task.Delay(left + TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
                if (world.SessionOf(account.LoginId) is not { } current || !ReferenceEquals(current, session))
                    return;
            }
            Bank(session, GameClock.UtcNow);
            session.Send(TradeMessages.OnlineTime(true));
        });
    }
}
