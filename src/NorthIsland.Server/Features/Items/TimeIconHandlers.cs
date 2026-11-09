using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Items;

/// <summary>
/// The two timer icons next to the double drop and spa bun effects: sends their state at login, on a click and when they run out, and handles
/// eating a Spa Bun.
/// </summary>
/// <param name="options">House settings, for the minutes and factor of a Spa Bun.</param>
/// <param name="logger">Logger.</param>
public sealed class TimeIconHandlers(
    IOptions<HouseOptions> options,
    ILogger<TimeIconHandlers> logger)
{
    /// <summary>The item id of the Spa Bun.</summary>
    public const ushort SpaBuns = 34117; // Spa Buns

    /// <summary>The kind name of the spa bun timed effect.</summary>
    public const string Spa = "spa";

    /// <summary>The lucky grass packet (<c>5/15</c>) with only a kind.</summary>
    /// <param name="kind">The state code.</param>
    /// <returns>The packet.</returns>
    private static byte[] Lucky(byte kind) => new PacketWriter(Action5.Code, Action5.LuckyGrass).U8(kind).ToArray();

    /// <summary>The lucky grass packet (<c>5/15</c>) with a kind and the minutes left.</summary>
    /// <param name="kind">The state code.</param>
    /// <param name="minutes">The minutes left.</param>
    /// <returns>The packet.</returns>
    private static byte[] Lucky(byte kind, int minutes) => new PacketWriter(Action5.Code, Action5.LuckyGrass).U8(kind).I32(minutes).ToArray();

    /// <summary>The spa bun packet (<c>5/16</c>) with only a kind.</summary>
    /// <param name="kind">The state code.</param>
    /// <returns>The packet.</returns>
    private static byte[] SpaPacket(byte kind) => new PacketWriter(Action5.Code, Action5.SpaBun).U8(kind).ToArray();

    /// <summary>The spa bun packet that shows the effect on, with the minutes left, the percentage of the limit used and the buns used.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The packet.</returns>
    private byte[] SpaOn(Character c, DateTime now)
    {
        var minutes = TimedEffects.Find(c, Spa, now) is { } e ? TimedEffects.Minutes(e, now) : 0;
        var bunMinutes = Math.Max(1, options.Value.HouseLife.SpaBunMinutes);
        var used = Math.Clamp((int)Math.Round(minutes * 100.0 / (bunMinutes * Math.Max(1, options.Value.HouseLife.SpaBunMax))), 0, 100);
        return new PacketWriter(Action5.Code, Action5.SpaBun)
            .U8(1)
            .I32(minutes)
            .U8((byte)used)
            .U8((byte)Math.Min(10, (minutes + bunMinutes - 1) / bunMinutes))
            .ToArray();
    }

    /// <summary>Sends the lucky grass icon with the minutes left when the double drop effect runs.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public static void SendLucky(GameSession session, Character c)
    {
        var now = TimedEffects.Now;
        if (TimedEffects.Find(c, TimedEffects.DoubleDrops, now) is { } e)
            session.Send(Lucky(1, TimedEffects.Minutes(e, now)));
    }

    /// <summary>At login, sends the icons of the effects that run.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public void SendOnLogin(GameSession session, Character c)
    {
        SendLucky(session, c);
        if (TimedEffects.Find(c, Spa, TimedEffects.Now) is not null)
            session.Send(SpaOn(c, TimedEffects.Now));
    }

    /// <summary>Switches off the icons of the double drop and spa effects that ran out.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="gone">The effects that ran out.</param>
    public static void Expired(GameSession session, IEnumerable<TimedEffect> gone)
    {
        foreach (var e in gone)
        {
            if (e.Kind == TimedEffects.DoubleDrops)
            {
                session.Send(Lucky(3)); // kind
                session.Send(Lucky(0));
            }
            else if (e.Kind == Spa)
                session.Send(SpaPacket(0));
        }
    }

    /// <summary>Handles a click on the lucky grass icon: answers with the minutes left, or that the effect is off.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body, which is not read.</param>
    internal static ValueTask OnLuckyClick(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c)
            return ValueTask.CompletedTask;
        var now = TimedEffects.Now;
        session.Send(TimedEffects.Find(c, TimedEffects.DoubleDrops, now) is { } e ? Lucky(4, TimedEffects.Minutes(e, now)) : Lucky(2)); // 4 kind, 2 kind
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles a click on the spa bun icon: answers with the state of the effect.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body, which is not read.</param>
    internal ValueTask OnSpaClick(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c)
            return ValueTask.CompletedTask;
        session.Send(TimedEffects.Find(c, Spa, TimedEffects.Now) is not null ? SpaOn(c, TimedEffects.Now) : SpaPacket(0));
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Eats a Spa Bun: adds its minutes to the spa effect, or refuses when the limit is reached.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The bag stack that was used.</param>
    /// <param name="item">The definition of the item.</param>
    /// <returns>True when the item is a Spa Bun and was handled; false for any other item.</returns>
    public bool TryUseBun(GameSession session, Character c, BagItem stack, ItemRecord item)
    {
        if (item.Id != SpaBuns)
            return false;
        var o = options.Value.HouseLife;
        var now = TimedEffects.Now;
        var cap = TimeSpan.FromMinutes((double)o.SpaBunMinutes * o.SpaBunMax);
        if (TimedEffects.Find(c, Spa, now) is { } running && running.Until + TimeSpan.FromMinutes(o.SpaBunMinutes) - now > cap)
        {
            session.Send(new PacketWriter(Action5.Code, Action5.SpaBun).U8(2).U8(0).ToArray());
            return true;
        }
        TimedEffects.Add(c, Spa, o.SpaFactor, 0, item.Id, TimeSpan.FromMinutes(o.SpaBunMinutes), now);
        stack.Count--;
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        session.Send(SpaOn(c, now));
        logger.LogInformation("[{Session}] {Name} eats a spa bun", session.Id, c.Name);
        return true;
    }

    /// <summary>How many times as much a bath restores.</summary>
    /// <param name="c">The player's character.</param>
    /// <returns>The factor while the spa effect runs, otherwise 1.</returns>
    public static double BathFactor(Character c) => TimedEffects.Find(c, Spa, TimedEffects.Now)?.Value ?? 1;
}
