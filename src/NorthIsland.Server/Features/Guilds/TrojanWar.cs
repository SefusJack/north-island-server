using Microsoft.Extensions.Options;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Guilds;

/// <summary>The weekly Trojan War schedule and the messages the clients get about it.</summary>
/// <param name="options">The Trojan War options.</param>
public sealed class TrojanWar(
    IOptions<GuildFeatureOptions> options)
{
    /// <summary>The war message numbers of the war packet <c>70/1</c>.</summary>
    public const byte FiveMinutesToStart = 4, Started = 5, FiveMinutesToEnd = 6, Ended = 7, EventBegins = 19;

    /// <summary>Builds the war packet <c>70/1</c> with a message number.</summary>
    /// <param name="message">The message number.</param>
    /// <returns>The packet.</returns>
    public static byte[] Message(byte message) => new PacketWriter(Action70.Code, Action70.War).U8(message).ToArray();

    /// <summary>Works out the war state for a time and the messages to announce, so the ticker can call it repeatedly.</summary>
    /// <param name="now">The current time.</param>
    /// <param name="wasOn">Whether the war was on at the last call.</param>
    /// <param name="warned">Whether the start warning was already sent.</param>
    /// <param name="warnedEnd">Whether the end warning was already sent.</param>
    /// <returns>Whether the war is on, the messages to send, and the new warned and warned-end values.</returns>
    public (bool On, IReadOnlyList<byte> Messages, bool Warned, bool WarnedEnd) Phase(DateTime now, bool wasOn, bool warned, bool warnedEnd)
    {
        var o = options.Value.TrojanWar;
        var start = now.Date.AddHours(o.StartHour);
        var end = now.Date.AddHours(o.EndHour);
        var sunday = now.DayOfWeek == o.Day;
        var on = sunday && Enabled && now >= start && now < end;
        var messages = new List<byte>();
        var nowWarned = warned;
        var nowWarnedEnd = warnedEnd;
        if (Enabled && sunday && !on && !wasOn && now < start && now >= start.AddMinutes(-o.WarnMinutes) && !warned)
        {
            messages.Add(FiveMinutesToStart);
            nowWarned = true;
        }
        if (on && !wasOn)
        {
            messages.Add(Started);
            nowWarnedEnd = false;
        }
        if (on && now >= end.AddMinutes(-o.WarnMinutes) && !warnedEnd)
        {
            messages.Add(FiveMinutesToEnd);
            nowWarnedEnd = true;
        }
        if (!on && wasOn)
        {
            messages.Add(Ended);
            nowWarned = false;
        }
        return (on, messages, nowWarned, nowWarnedEnd);
    }

    /// <summary>Whether the Trojan War is switched on in the options.</summary>
    private bool Enabled => options.Value.TrojanWar.Enabled;
}
