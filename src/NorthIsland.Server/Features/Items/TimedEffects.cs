using System.Collections.Immutable;
using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Items;

/// <summary>Helpers for the timed effects on a character, such as experience potions, stat buffs and no-encounter food.</summary>
public static class TimedEffects
{
    /// <summary>The kind name of the experience rate effect; its value is the rate.</summary>
    public const string Exp = "exp";

    /// <summary>The kind name of the effect that keeps a player from losing experience.</summary>
    public const string Protect = "protect";

    /// <summary>The kind name of the double drop effect.</summary>
    public const string DoubleDrops = "drops2";

    /// <summary>The kind name of the effect that blocks encounters in the field.</summary>
    public const string NoEncounters = "noenc";

    /// <summary>The kind names of the four stat buffs, in the order of the stat buff rows.</summary>
    public static readonly ImmutableArray<string> StatKinds = ["stat3", "stat4", "stat5", "stat6"];

    /// <summary>The current game time.</summary>
    public static DateTime Now => GameClock.Now;

    /// <summary>Whether an effect still runs.</summary>
    /// <param name="e">The effect.</param>
    /// <param name="now">The current time.</param>
    /// <returns>True when its end is after <paramref name="now"/>.</returns>
    public static bool IsLive(TimedEffect e, DateTime now) => e.Until > now;

    /// <summary>Finds the running effect of a kind.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="kind">The kind name.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The effect, or null when none of that kind runs.</returns>
    public static TimedEffect? Find(Character c, string kind, DateTime now) => c.Effects.Find(e => e.Kind == kind && IsLive(e, now));

    /// <summary>The experience rate of the player.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The rate of the running experience effect, or 1.</returns>
    public static double ExpMultiplier(Character c, DateTime now) => Find(c, Exp, now)?.Value ?? 1;

    /// <summary>Whether the player is protected from losing experience.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>True when the protection runs.</returns>
    public static bool ExpProtected(Character c, DateTime now) => Find(c, Protect, now) is not null;

    /// <summary>Whether field encounters are blocked for the player.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>True when the no-encounter effect runs.</returns>
    public static bool EncountersBlocked(Character c, DateTime now) => Find(c, NoEncounters, now) is not null;

    /// <summary>Whether the player gets double drops.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>True when the double drop effect runs.</returns>
    public static bool DoubleDrop(Character c, DateTime now) => Find(c, DoubleDrops, now) is not null;

    /// <summary>The bonus of a stat buff.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="index">The buff, 0 to 3 in the order of <see cref="StatKinds"/>.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The bonus, 0 when the buff does not run or the index is out of range.</returns>
    public static int StatBonus(Character c, int index, DateTime now) =>
        index is >= 0 and <= 3 ? (int)(Find(c, StatKinds[index], now)?.Value ?? 0) : 0;

    /// <summary>The mask of the running stat buffs: a four-bit field per buff holding its level.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The mask.</returns>
    public static uint StatMask(Character c, DateTime now)
    {
        uint mask = 0;
        for (var i = 0; i < StatKinds.Length; i++)
            if (Find(c, StatKinds[i], now) is { } e)
                mask |= (uint)(e.Level & 0xF) << (4 * (i + 2));
        return mask;
    }

    /// <summary>The minutes left of each stat buff.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>Five entries; the first four are the buffs in the order of <see cref="StatKinds"/>, 0 for a buff that does not run.</returns>
    public static ushort[] StatMinutes(Character c, DateTime now)
    {
        var minutes = new ushort[5];
        for (var i = 0; i < StatKinds.Length; i++)
            if (Find(c, StatKinds[i], now) is { } e)
                minutes[i] = (ushort)Math.Min(ushort.MaxValue, Minutes(e, now));
        return minutes;
    }

    /// <summary>The packet that shows the experience rate icon.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The packet, with the icon off when the rate is 1.</returns>
    public static byte[] IconPacket(Character c, DateTime now)
    {
        var m = ExpMultiplier(c, now);
        return m == 1 ? PlayerMessages.ExpRate(false, 0) : PlayerMessages.ExpRate(true, (byte)Math.Clamp((int)Math.Round(m * 10), 1, 255));
    }

    /// <summary>The minutes an effect has left, rounded up.</summary>
    /// <param name="e">The effect.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The minutes, 0 when it ended.</returns>
    public static int Minutes(TimedEffect e, DateTime now) => (int)Math.Max(0, Math.Ceiling((e.Until - now).TotalMinutes));

    /// <summary>Removes the effects that ran out.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The effects that were removed.</returns>
    public static List<TimedEffect> Prune(Character c, DateTime now)
    {
        var gone = c.Effects.Where(e => !IsLive(e, now)).ToList();
        foreach (var e in gone)
            c.Effects.Remove(e);
        return gone;
    }

    /// <summary>
    /// Starts an effect, or extends it by the duration when one of the same kind, value and level runs; one of the same kind with another value or
    /// level is replaced.
    /// </summary>
    /// <param name="c">The player's character.</param>
    /// <param name="kind">The kind name.</param>
    /// <param name="value">The value of the effect, such as a rate or a bonus.</param>
    /// <param name="level">The level of the effect.</param>
    /// <param name="item">The item id that started it.</param>
    /// <param name="duration">How long it runs.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The running effect.</returns>
    public static TimedEffect Add(
        Character c,
        string kind,
        double value,
        int level,
        ushort item,
        TimeSpan duration,
        DateTime now)
    {
        var running = Find(c, kind, now);
        if (running is not null && running.Value == value && running.Level == level)
        {
            running.Until += duration;
            return running;
        }
        if (running is not null)
            c.Effects.Remove(running);
        var effect = new TimedEffect { Kind = kind, Value = value, Level = level, Item = item, Until = now + duration };
        c.Effects.Add(effect);
        return effect;
    }
}
